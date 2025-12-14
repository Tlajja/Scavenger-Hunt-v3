import React, { useEffect, useState, useRef } from 'react'
import { API_BASE } from '../services/api.js'
import CommentSection from '../components/CommentSection.jsx'

export default function Vote() {
  const [challenges, setChallenges] = useState([])
  const [selected, setSelected] = useState(null)
  const [subs, setSubs] = useState([])
  const [userVotes, setUserVotes] = useState({})
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [tasksForChallenge, setTasksForChallenge] = useState([])
  const [selectedTask, setSelectedTask] = useState(null)
  const userId = Number(localStorage.getItem('userId') || 0)
  const scrollContainerRef = useRef(null)
  const refreshTimeoutRef = useRef(null)

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

  async function loadSubmissionsByTask(taskId) {
    setLoading(true); setError('')
    try {
      const res = await fetch(`/api/photosubmissions?taskId=${taskId}`)
      if (!res.ok) throw new Error(`Failed to load submissions (${res.status})`)
      const data = await res.json()
      const arr = Array.isArray(data) ? data : []
      const normalized = arr.map(s => ({
        id: s.id ?? s.Id,
        userId: s.userId ?? s.UserId,
        userName: s.userName ?? s.UserName ?? s.user?.name ?? null,
        photoUrl: s.photoUrl ?? s.PhotoUrl ?? '',
        photoFullUrl: (() => {
          const p = (s.photoUrl ?? s.PhotoUrl ?? '').toString()
          if (!p) return ''
          if (p.startsWith('http://') || p.startsWith('https://')) return p
          const base = (API_BASE || '').replace(/\/$/, '')
          return base ? `${base}${p.startsWith('/') ? '' : '/'}${p}` : p
        })(),
        photoDataUrl: null,
        votes: s.votes ?? s.Votes ?? 0,
        challengeId: s.challengeId ?? s.ChallengeId
      }))
      setSubs(normalized)

      // Fetch user votes for this task
      const votesRes = await fetch(`/api/votes/task/${taskId}?userId=${userId}`)
      if (votesRes.ok) {
        const votesData = await votesRes.json()
        setUserVotes(votesData)
      }

      // fetch image blobs
      normalized.forEach(async (item, idx) => {
        if (!item.photoFullUrl) return
        try {
          const r = await fetch(item.photoFullUrl)
          if (!r.ok) return
          const blob = await r.blob()
          const reader = new FileReader()
          reader.onloadend = () => {
            setSubs(prev => {
              const copy = [...prev]
              if (copy[idx]) copy[idx] = { ...copy[idx], photoDataUrl: reader.result }
              return copy
            })
          }
          reader.readAsDataURL(blob)
        } catch {}
      })
    } catch (e) {
      setError(String(e))
    } finally {
      setLoading(false)
    }
  }

  async function onChallengeChange(challengeId) {
    setSelected(challengeId)
    setTasksForChallenge([])
    setSelectedTask(null)
    if (!challengeId) { setSubs([]); return }
    try {
      const cres = await fetch(`/api/challenge/${challengeId}`)
      if (!cres.ok) return
      const ch = await cres.json()
      const refs = Array.isArray(ch?.challengeTasks ?? ch?.ChallengeTasks) ? (ch.challengeTasks ?? ch.ChallengeTasks) : []
      const ids = refs.map(t => Number(t.taskId ?? t.TaskId ?? t.task?.id ?? t.task?.Id ?? 0)).filter(Boolean)
      const tasks = await Promise.all(ids.map(id => fetch(`/api/tasks/${id}`).then(r=>r.ok? r.json(): null)))
      const good = tasks.filter(Boolean)
      setTasksForChallenge(good)
    } catch {}
  }

  function onSelectTask(tid) {
    setSelectedTask(tid)
    if (tid) loadSubmissionsByTask(tid)
    else setSubs([])
  }

  async function handleVote(subId) {
    try {
      const hasVoted = userVotes[subId]
      
      if (hasVoted) {
        // Remove vote
        const res = await fetch(`/api/votes/${subId}?userId=${userId}`, { method: 'DELETE' })
        if (!res.ok) {
          const raw = await res.text()
          let msg = raw
          try {
            const j = JSON.parse(raw)
            msg = j.error || j.message || raw
          } catch {}
          throw new Error(`Remove vote failed: ${msg}`)
        }
        
        // Update local state
        setSubs(prev => prev.map(s => 
          s.id === subId ? { ...s, votes: Math.max(0, (s.votes ?? 1) - 1) } : s
        ))
        setUserVotes(prev => ({ ...prev, [subId]: false }))
      } else {
        // Add vote
        const res = await fetch(`/api/votes/${subId}?userId=${userId}`, { method: 'POST' })
        if (!res.ok) {
          const raw = await res.text()
          let msg = raw
          try {
            const j = JSON.parse(raw)
            msg = j.error || j.message || raw
          } catch {}
          throw new Error(`Vote failed: ${msg}`)
        }
        
        // Update local state
        setSubs(prev => prev.map(s => 
          s.id === subId ? { ...s, votes: (s.votes ?? 0) + 1 } : s
        ))
        setUserVotes(prev => ({ ...prev, [subId]: true }))
      }
      
      // Refresh in background after a delay
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current)
      }
      refreshTimeoutRef.current = setTimeout(async () => {
        if (selectedTask) {
          const savedScroll = window.scrollY || document.documentElement.scrollTop
          await loadSubmissionsByTask(selectedTask)
          requestAnimationFrame(() => {
            window.scrollTo({ top: savedScroll, behavior: 'instant' })
            setTimeout(() => window.scrollTo({ top: savedScroll, behavior: 'instant' }), 100)
          })
        }
        refreshTimeoutRef.current = null
      }, 1500)
    } catch (e) {
      setError(String(e.message || e))
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Vote</h2>
      {error && <div style={{ color: 'crimson' }}>{error}</div>}

      <div style={{ marginBottom: 12 }}>
        <label>Challenges in voting stage: </label>
        <select value={selected ?? ''} onChange={e => onChallengeChange(e.target.value)}>
          <option value="">-- choose --</option>
          {challenges
            .filter(c => Number(c.status) === 1)
            .map(c => (
              <option key={c.id} value={c.id}>
                {c.name} (id:{c.id})
              </option>
            ))}
        </select>
        {tasksForChallenge.length > 0 && (
          <div style={{ marginTop: 8 }}>
            <label>Task: </label>
            <select value={selectedTask ?? ''} onChange={e => onSelectTask(e.target.value)}>
              <option value="">-- choose task --</option>
              {tasksForChallenge.map(t => <option key={t.id ?? t.Id} value={t.id ?? t.Id}>{t.description ?? t.Description}</option>)}
            </select>
          </div>
        )}
      </div>

      {loading && <div>Loading submissions…</div>}

      {!loading && subs.length === 0 && selected && <div>No submissions yet for this challenge.</div>}

      <div ref={scrollContainerRef} style={{ display: 'grid', gap: 12 }}>
        {subs.map(s => {
          const hasVoted = userVotes[s.id]
          return (
            <div key={s.id} style={{ border: '1px solid #ddd', padding: 8, maxWidth: 620 }}>
              <div>
                <strong>{s.userName ?? `User ${s.userId}`}</strong>
              </div>
              <div style={{ marginTop: 8, textAlign: 'center' }}>
                {(s.photoDataUrl ?? s.photoFullUrl ?? s.photoUrl) ? (
                  <img src={s.photoDataUrl ?? s.photoFullUrl ?? s.photoUrl} alt={`Submission ${s.id}`} style={{ maxWidth: '100%', maxHeight: 420, objectFit: 'contain', borderRadius: 4 }} />
                ) : (
                  <div>No image</div>
                )}
              </div>
              <div style={{ marginTop: 8, display: 'flex', alignItems: 'center', gap: 12 }}>
                <div>Votes: {s.votes ?? s.Votes ?? 0}</div>
                {s.userId === userId ? (
                  <span style={{ color: 'rgba(255, 255, 255, 0.5)', fontSize: 14, fontStyle: 'italic' }}>
                    You can't vote for your own submission
                  </span>
                ) : (
                  <>
                    <button 
                      onClick={() => handleVote(s.id ?? s.Id)}
                      style={{
                        background: hasVoted ? '#ff6b6b' : '#646cff',
                      }}
                    >
                      {hasVoted ? 'Remove Vote' : 'Vote'}
                    </button>
                    {hasVoted && <span style={{ color: '#51cf66', fontSize: 14 }}>✓ Voted</span>}
                  </>
                )}
              </div>
              <CommentSection submissionId={s.id} currentUserId={userId} />
            </div>
          )
        })}
      </div>
    </div>
  )
}