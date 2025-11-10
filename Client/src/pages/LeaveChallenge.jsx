import React, { useEffect, useState } from 'react'

export default function LeaveChallenge() {
  const userId = Number(localStorage.getItem('userId') || 0)
  const [challenges, setChallenges] = useState([])
  const [selected, setSelected] = useState('')
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState('')

  useEffect(() => {
    loadMyChallenges()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function loadMyChallenges() {
    setMessage(''); setLoading(true)
    try {
      // try a dedicated endpoint first
      let res = await fetch(`/api/challenge/mine?userId=${userId}`)
      if (!res.ok) {
        // fallback: fetch all and filter by participant list if available
        res = await fetch('/api/challenge?publicOnly=false')
        if (!res.ok) throw new Error(`Failed to load challenges (${res.status})`)
        const all = await res.json()
        const mine = (Array.isArray(all) ? all : []).filter(c => {
          const parts = c.participants ?? c.Participants ?? c.participantsList ?? []
          return Array.isArray(parts) && parts.some(p => Number(p.userId ?? p.UserId ?? p.id ?? p.Id) === userId)
        })
        setChallenges(mine)
        setSelected(mine.length ? String(mine[0].id ?? mine[0].Id) : '')
        return
      }
      const data = await res.json()
      setChallenges(Array.isArray(data) ? data : [])
      setSelected((Array.isArray(data) && data.length) ? String(data[0].id ?? data[0].Id) : '')
    } catch (err) {
      setMessage(String(err))
    } finally {
      setLoading(false)
    }
  }

  async function handleLeave() {
    if (!selected) { setMessage('No challenge selected.'); return }
    setLoading(true); setMessage('')
    try {
      const payload = { ChallengeId: Number(selected), UserId: Number(userId) }
      // try common leave endpoints in order
      let res = await fetch('/api/challenge/leave', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
      if (!res.ok) {
        // try path-based fallback
        res = await fetch(`/api/challenge/${payload.ChallengeId}/leave?userId=${payload.UserId}`, { method: 'POST' })
      }
      if (!res.ok) {
        // try delete-style fallback
        res = await fetch(`/api/challenge/${payload.ChallengeId}/participants/${payload.UserId}`, { method: 'DELETE' })
      }
      if (!res.ok) {
        const text = await res.text()
        throw new Error(text || `Leave failed (${res.status})`)
      }
      setMessage('Left challenge successfully.')
      // refresh list
      await loadMyChallenges()
    } catch (err) {
      setMessage(String(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Leave Challenge</h2>
      {!userId && <div style={{ color: 'crimson' }}>You must be logged in to leave a challenge.</div>}
      {message && <div style={{ margin: '8px 0', color: message.startsWith('Left') ? 'green' : 'crimson' }}>{message}</div>}
      {loading && <div>Loading…</div>}

      {!loading && challenges.length === 0 && <div>You are not a member of any active challenges.</div>}

      {!loading && challenges.length > 0 && (
        <div>
          <label>
            Choose challenge to leave:{' '}
            <select value={selected} onChange={e => setSelected(e.target.value)}>
              <option value="">-- choose --</option>
              {challenges.map(c => (
                <option key={c.id ?? c.Id} value={c.id ?? c.Id}>
                  {c.name ?? c.Name} (id:{c.id ?? c.Id}) {c.joinCode ? `— code: ${c.joinCode}` : ''}
                </option>
              ))}
            </select>
          </label>
          <div style={{ marginTop: 12 }}>
            <button onClick={handleLeave} disabled={!selected || loading}>Leave selected challenge</button>
          </div>
        </div>
      )}
    </div>
  )
}