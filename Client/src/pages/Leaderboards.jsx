import React, { useEffect, useState } from 'react'

export default function Leaderboards() {
  const [challenges, setChallenges] = useState([])
  const [selected, setSelected] = useState(null)
  const [board, setBoard] = useState(null)
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

  async function loadLeaderboard(challengeId) {
    setLoading(true); setError(''); setBoard(null)
    try {
      // try direct leaderboard endpoint per-challenge first
      let res = await fetch(`/api/leaderboard/challenge/${challengeId}`)
      if (res.ok) {
        const data = await res.json()
        setBoard({ source: 'leaderboard', entries: data })
        return
      }

      // fallback: fetch submissions for the challenge and build simple leaderboard
      res = await fetch(`/api/photosubmissions?challengeId=${challengeId}`)
      if (!res.ok) throw new Error(`No leaderboard or submissions endpoint (status ${res.status})`)
      const subs = await res.json()
      // aggregate by user
      const map = new Map()
      (Array.isArray(subs) ? subs : []).forEach(s => {
        const uid = s.userId ?? s.userId ?? s.UserId
        const votes = Number(s.votes ?? s.Votes ?? 0)
        if (!map.has(uid)) map.set(uid, { userId: uid, userName: s.userName ?? s.userName ?? `User ${uid}`, wins: 0, votes })
        else map.get(uid).votes += votes
      })
      const arr = Array.from(map.values()).sort((a,b) => (b.votes||0)-(a.votes||0))
      setBoard({ source: 'computed', entries: arr })
    } catch (e) {
      setError(String(e))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Leaderboards</h2>
      {error && <div style={{ color: 'crimson' }}>{error}</div>}
      <div style={{ marginBottom: 12 }}>
        <label>Select finalized challenge: </label>
        <select
          value={selected ?? ''}
          onChange={e => {
            const id = e.target.value || null
            setSelected(id)
            if (id) loadLeaderboard(id)
            else { setBoard(null) }
          }}
        >
          <option value="">-- choose --</option>
          {challenges
            .filter(c => Number(c.status) === 2) // status 2 = Completed / finalized
            .map(c => (
              <option key={c.id} value={c.id}>
                {c.name} (id:{c.id}) — {c.joinCode ? `code: ${c.joinCode}` : ''}
              </option>
            ))}
        </select>
      </div>

      {loading && <div>Loading leaderboard…</div>}

      {board && (
        <div>
          <h3>Leaderboard ({board.source})</h3>
          <ol>
            {board.entries.map((e, i) => (
              <li key={e.userId ?? i}>
                {e.userName ?? e.userId} — {e.wins ?? e.votes ?? 0} {board.source === 'computed' ? 'votes' : 'wins'}
                <button
                  style={{ marginLeft: 12 }}
                  onClick={async () => {
                    // open submissions page for this challenge
                    window.location.href = `/submit?challengeId=${selected}`
                  }}
                >
                  Show submissions
                </button>
              </li>
            ))}
          </ol>
        </div>
      )}

      {!loading && !board && <div>No leaderboard selected.</div>}
    </div>
  )
}