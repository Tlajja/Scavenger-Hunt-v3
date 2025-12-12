import React, { useEffect, useState } from 'react'
import { API_BASE } from '../services/api.js'

function assignDenseRanks(items, valueSelector) {
  if (!Array.isArray(items)) return []
  const arr = [...items].sort((a,b) => (valueSelector(b) || 0) - (valueSelector(a) || 0))
  let prevVal = null
  let prevRank = 0
 let nextRank = 1
  return arr.map(it => {
    const val = valueSelector(it) ?? 0
    if (prevVal === null || val !== prevVal) {
      prevRank = nextRank
      prevVal = val
      nextRank += 1
    }
    return { ...it, rank: prevRank }
  })
}

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
        const entries = Array.isArray(data) ? data : (Array.isArray(data?.entries) ? data.entries : [])
        // ensure each entry has a photoUrl when server provides it (if available)
        setBoard({ source: 'leaderboard', entries: assignDenseRanks(entries, e => Number(e.wins ?? e.Wins ?? e.totalVotes ?? e.TotalVotes ?? 0)) })
        return
      }

      // fallback: fetch submissions for the challenge and build leaderboard with top-photo per user
      res = await fetch(`/api/photosubmissions?challengeId=${challengeId}`)
      if (!res.ok) throw new Error(`No leaderboard or submissions endpoint (status ${res.status})`)
      const subs = await res.json()
      const arr = Array.isArray(subs) ? subs : []

      // aggregate by user and pick user's top submission (by votes) to show image
      const map = new Map()
      arr.forEach(s => {
        const uid = s.userId ?? s.UserId ?? s.user?.id ?? s.User?.Id ?? 0
        const votes = Number(s.votes ?? s.Votes ?? 0)
        const subId = s.id ?? s.Id
        const rawPhoto = s.photoUrl ?? s.PhotoUrl ?? s.photo?.url ?? s.PhotoUrl ?? ''
        // build absolute URL: use raw if already absolute, otherwise prefix API_BASE
        const photoUrl = rawPhoto
          ? (rawPhoto.startsWith('http://') || rawPhoto.startsWith('https://')
              ? rawPhoto
              : ((API_BASE || '').replace(/\/$/, '') + (rawPhoto.startsWith('/') ? rawPhoto : '/' + rawPhoto)))
          : null

        if (!map.has(uid)) {
          map.set(uid, {
            userId: uid,
            userName: s.userName ?? s.UserName ?? s.user?.name ?? `User ${uid}`,
            votes,
            topSubmission: { id: subId, photoUrl, votes }
          })
        } else {
          const item = map.get(uid)
          item.votes += votes
          // update topSubmission if this submission has more votes
          if ((item.topSubmission?.votes ?? 0) < votes) {
            item.topSubmission = { id: subId, photoUrl, votes }
          }
        }
      })

      const out = Array.from(map.values())
        .map(x => ({
          userId: x.userId,
          userName: x.userName,
          votes: x.votes,
          photoUrl: x.topSubmission?.photoUrl ?? null,
          submissionId: x.topSubmission?.id ?? null
        }))
        .sort((a, b) => (b.votes || 0) - (a.votes || 0))

      setBoard({ source: 'computed', entries: assignDenseRanks(out, e => Number(e.votes ?? e.Votes ?? 0)) })
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

      {board && board.entries.length === 0 ? (
        <div style={{ textAlign: 'center', color: 'rgba(0, 0, 0, 0.6)', padding: 40 }}>
          No submissions or all submissions have zero votes. There are no winners.
        </div>
      ) : board ? (
        <div>
          <h3>Leaderboard ({board.source})</h3>
          {(Array.isArray(board.entries) ? board.entries : []).every(e => (e.votes ?? e.totalVotes ?? 0) === 0) ? (
            <div style={{ textAlign: 'center', color: 'rgba(0, 0, 0, 0.6)', padding: 40 }}>
              No submissions or all submissions have zero votes. There are no winners.
            </div>
          ) : (
          <ol style={{ paddingLeft: 18 }}>
            {(Array.isArray(board.entries) ? board.entries : []).map((e, i) => {
              const display = board.source === 'computed' ? (e.votes ?? 0) : (e.wins ?? e.totalVotes ?? 0)
              return (
                <li key={e.userId ?? i} style={{ marginBottom: 12 }}>
                  <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                    <div style={{ width: 46, textAlign: 'right', fontWeight: 700 }}>{medal}</div>
                    <div style={{ width: 120, height: 80, background: '#f7f7f7', display: 'flex', alignItems: 'center', justifyContent: 'center', border: '1px solid #eee' }}>
                      {e.photoUrl ? (
                        <img src={e.photoUrl} alt={`User ${e.userName} photo`} style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} />
                      ) : (
                        <div style={{ fontSize: 12, color: '#666' }}>No image</div>
                      )}
                    </div>
                    <div>
                      <div style={{ fontWeight: 600 }}>{e.userName ?? `User ${e.userId}`}</div>
                      <div style={{ fontSize: 13, color: '#444' }}>{display} {board.source === 'computed' ? 'votes' : 'wins'}</div>
                    </div>
                  </div>
                </li>
              )
            })}
          </ol>
          )}
        </div>
      ) : null}

      {!loading && !board && <div>No leaderboard selected.</div>}
    </div>
  )
}