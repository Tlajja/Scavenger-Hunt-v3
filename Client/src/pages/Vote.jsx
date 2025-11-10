import React, { useEffect, useState } from 'react'

export default function Vote() {
  const [challenges, setChallenges] = useState([])
  const [selected, setSelected] = useState(null)
  const [subs, setSubs] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function load() {
      setError('')
      try {
        const res = await fetch('/api/challenge?publicOnly=false')
        if (!res.ok) throw new Error(`Failed to load challenges (${res.status})`)
        const data = await res.json()
        setChallenges(Array.isArray(data) ? data : [])
      } catch (e) {
        setError(String(e))
      }
    }
    load()
  }, [])

  async function loadSubmissions(challengeId) {
    setLoading(true); setError(''); setSubs([])
    try {
      const res = await fetch(`/api/photosubmissions?challengeId=${challengeId}`)
      if (!res.ok) throw new Error(`Failed to load submissions (${res.status})`)
      const data = await res.json()
      // normalize property names to frontend-friendly shape
      const arr = Array.isArray(data) ? data : []
      const normalized = arr.map(s => ({
        id: s.id ?? s.Id,
        userId: s.userId ?? s.UserId,
        userName: s.userName ?? s.UserName ?? s.user?.name ?? null,
        photoUrl: s.photoUrl ?? s.PhotoUrl ?? s.photoUrl ?? s.PhotoUrl,
        votes: s.votes ?? s.Votes ?? 0,
        challengeId: s.challengeId ?? s.ChallengeId
      }))
      setSubs(normalized)
    } catch (e) {
      setError(String(e))
    } finally {
      setLoading(false)
    }
  }

  async function vote(subId) {
    try {
      const id = subId ?? (typeof subId === 'object' ? subId.id : null)
      if (!id) throw new Error('Invalid submission id')
      const res = await fetch(`/api/photosubmissions/${id}/vote`, { method: 'POST' })
      if (!res.ok) {
        const txt = await res.text()
        throw new Error(`Vote failed (${res.status}): ${txt}`)
      }
      // refresh
      if (selected) await loadSubmissions(selected)
    } catch (e) {
      setError(String(e))
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Vote</h2>
      {error && <div style={{ color: 'crimson' }}>{error}</div>}

      <div style={{ marginBottom: 12 }}>
        <label>Challenges in voting stage: </label>
        <select
          value={selected ?? ''}
          onChange={e => {
            const id = e.target.value || null
            setSelected(id)
            if (id) loadSubmissions(id)
            else setSubs([])
          }}
        >
          <option value="">-- choose --</option>
          {challenges
            .filter(c => Number(c.status) === 1) // status 1 = Closed / voting stage per your design
            .map(c => (
              <option key={c.id} value={c.id}>
                {c.name} (id:{c.id})
              </option>
            ))}
        </select>
      </div>

      {loading && <div>Loading submissions…</div>}

      {!loading && subs.length === 0 && selected && <div>No submissions yet for this challenge.</div>}

      <div style={{ display: 'grid', gap: 12 }}>
        {subs.map(s => (
          <div key={s.id} style={{ border: '1px solid #ddd', padding: 8, maxWidth: 420 }}>
            <div>
              <strong>{s.userName ?? `User ${s.userId}`}</strong>
            </div>
            <div style={{ marginTop: 8 }}>
              {s.photoUrl ? <img src={s.photoUrl} alt="" style={{ maxWidth: '100%' }} /> : <div>No image</div>}
            </div>
            <div style={{ marginTop: 8 }}>
              Votes: {s.votes ?? s.Votes ?? 0}
              <button style={{ marginLeft: 12 }} onClick={() => vote(s.id ?? s.Id)}>Vote</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}