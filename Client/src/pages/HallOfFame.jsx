import React, { useEffect, useState } from 'react'
import { getHallOfFame } from '../services/api.js'

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

export default function HallOfFame() {
  const [entries, setEntries] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  
  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await getHallOfFame()
        let data = null

        if (res instanceof Response) {
          if (!res.ok) throw new Error(await res.text() || res.statusText)
          data = await res.json()
        } else {
          if (!res.ok) throw new Error(res.text || `Error ${res.status}`)
          data = res.data
        }

        if (mounted) {
          const items = Array.isArray(data) ? data : []
          setEntries(assignDenseRanks(items, e => Number(e.wins ?? e.Wins ?? e.totalVotes ?? e.TotalVotes ?? 0)))
        }
      } catch (err) {
        if (mounted) setError(String(err))
      } finally {
        if (mounted) setLoading(false)
      }
    }

    load()
    return () => { mounted = false }
  }, [])

  if (loading) return <div>Loading hall of fame…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error: {error}</div>
  if (!entries || entries.length === 0) return <div>No hall of fame entries yet.</div>

  return (
    <div>
      <h2>Hall of Fame</h2>
      <table style={{ borderCollapse: 'collapse', width: '100%', maxWidth: 720 }}>
        <thead>
          <tr>
            <th style={{ textAlign: 'left', padding: 8 }}>Rank</th>
            <th style={{ textAlign: 'left', padding: 8 }}>User</th>
            <th style={{ textAlign: 'right', padding: 8 }}>Wins</th>
          </tr>
        </thead>
        <tbody>
          {entries.map((e, i) => {
            const rank = e.rank ?? (i + 1)
            const medal = rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : `#${rank}`
            return (
              <tr key={e.userId ?? i} style={{ borderTop: '1px solid #eee' }}>
                <td style={{ padding: 8 }}>{medal}</td>
                <td style={{ padding: 8 }}>{e.userName ?? e.userName ?? 'Unknown'}</td>
                <td style={{ padding: 8, textAlign: 'right' }}>{Number(e.wins ?? e.Wins ?? e.totalVotes ?? e.TotalVotes ?? 0)}</td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}