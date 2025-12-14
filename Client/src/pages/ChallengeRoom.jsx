import React, { useEffect, useState, useRef } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getChallengeById, getTaskById, leaveChallenge, advanceChallenge, API_BASE } from '../services/api.js'
import CommentSection from '../components/CommentSection.jsx'

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

export default function ChallengeRoom() {
  const { challengeId } = useParams()
  const navigate = useNavigate()
  const userId = Number(localStorage.getItem('userId') || 0)
  
  const [challenge, setChallenge] = useState(null)
  const [task, setTask] = useState(null)
  const [submissions, setSubmissions] = useState([])
  const [leaderboard, setLeaderboard] = useState([])
  const [activeTab, setActiveTab] = useState('submit')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [tasksForChallenge, setTasksForChallenge] = useState([])
  const [challengeTasksMeta, setChallengeTasksMeta] = useState([])
  const [countdownByTask, setCountdownByTask] = useState({})
  const [stageCountdown, setStageCountdown] = useState('')
  const [submitTaskId, setSubmitTaskId] = useState('')
  const [voteTaskId, setVoteTaskId] = useState('')
  
  const [photoFile, setPhotoFile] = useState(null)
  const [previewUrl, setPreviewUrl] = useState(null)
  const [uploading, setUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)
  
  const scrollContainerRef = useRef(null)
  const refreshTimeoutRef = useRef(null)

  const fmtVilnius = new Intl.DateTimeFormat(undefined, {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
    timeZone: 'Europe/Vilnius'
  })
  function formatVilnius(msOrDateLike) {
    try {
      const d = typeof msOrDateLike === 'number' ? new Date(msOrDateLike) : new Date(msOrDateLike)
      return fmtVilnius.format(d)
    } catch {
      return String(msOrDateLike)
    }
  }

  const participants = challenge?.members ?? challenge?.Participants ?? challenge?.participants ?? challenge?.participantsList ?? []
  const isAdmin = Array.isArray(participants) && participants.some(p => 
    Number(p.userId ?? p.UserId ?? p.id ?? p.Id) === userId && Number(p.role ?? p.Role ?? 0) === 1)

  useEffect(() => {
    loadChallengeData()
  }, [challengeId])

  useEffect(() => {
    if (challenge) {
      const status = Number(challenge.status ?? 0)
      if (status === 0) setActiveTab('submit')
      else if (status === 1) setActiveTab('vote')
      else if (status === 2) setActiveTab('leaderboard')
    }
  }, [challenge])

  // Poll challenge status to detect auto-advance
 useEffect(() => {
   const timer = setInterval(async () => {
     try {
       const res = await fetch(`${API_BASE}/api/challenge/${challengeId}`)
       if (res.ok) {
         const data = await res.json()
         setChallenge(data)
       }
     } catch {}
   }, 15000) // check every 15 seconds
   return () => clearInterval(timer)
 }, [challengeId])

 // Stage countdown updater
 useEffect(() => {
   function parseAsMs(s) {
     if (!s) return null
     const str = String(s)
     const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(str)
     const normalized = hasTz ? str : (str.endsWith('Z') ? str : str + 'Z')
     const ms = Date.parse(normalized)
     return Number.isFinite(ms) ? ms : null
   }
   
   function fmtCountdown(endMs) {
     const diff = endMs - Date.now()
     if (diff <= 0) return 'Stage ended'
     const total = Math.floor(diff / 1000)
     const d = Math.floor(total / 86400)
     const h = Math.floor((total % 86400) / 3600)
     const m = Math.floor((total % 3600) / 60)
     const s = total % 60
     return d > 0 ? `${d}d ${h}h ${m}m` : h > 0 ? `${h}h ${m}m ${s}s` : `${m}m ${s}s`
   }

   function update() {
     const status = Number(challenge?.status ?? 0)
     if (status === 0) {
       const endMs = parseAsMs(challenge?.submissionEndsAt ?? challenge?.SubmissionEndsAt)
       setStageCountdown(endMs ? `This phase ends in: ${fmtCountdown(endMs)}` : '')
     } else if (status === 1) {
       const endMs = parseAsMs(challenge?.votingEndsAt ?? challenge?.VotingEndsAt)
       setStageCountdown(endMs ? `This phase ends in: ${fmtCountdown(endMs)}` : '')
     } else {
       setStageCountdown('')
     }
   }
    if (challenge) {
     update()
     const timerId = setInterval(update, 1000)
     return () => clearInterval(timerId)
   }
 }, [challenge])

  async function loadChallengeData() {
    setLoading(true)
    setError('')
    try {
      const cRes = await getChallengeById(challengeId)
      if (!cRes.ok) {
        setError('Challenge not found')
        return
      }

      const challengeData = cRes.data
      setChallenge(challengeData)

      // load all tasks for this challenge (ChallengeTasks relation)
      const refs = Array.isArray(challengeData?.challengeTasks ?? challengeData?.ChallengeTasks)
        ? (challengeData.challengeTasks ?? challengeData.ChallengeTasks)
        : []
      setChallengeTasksMeta(refs)
      const ids = refs.map(t => Number(t.taskId ?? t.TaskId ?? t.task?.id ?? t.task?.Id ?? 0)).filter(Boolean)
      if (ids.length > 0) {
        const fetched = await Promise.all(ids.map(id => getTaskById(id)))
        const valid = fetched.filter(r => r?.ok).map(r => r.data)
        setTasksForChallenge(valid)
        // default preview/task card to the first task
        setTask(valid[0] ?? null)
        setSubmitTaskId(String(valid[0]?.id ?? valid[0]?.Id ?? ''))
      } else {
        setTasksForChallenge([])
        setTask(null)
        setSubmitTaskId('')
      }

      const status = Number(challengeData.status ?? 0)
      if (status === 1) {
        // If voting stage, default voteTaskId to first task and load its submissions
        if (ids.length > 0) {
          setVoteTaskId(String(ids[0]))
          await loadSubmissionsByTask(ids[0])
        } else {
          await loadSubmissions()
        }
      } else if (status === 2) {
        await loadLeaderboard()
      }
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  // Countdown updater for each task in the challenge
  useEffect(() => {
    let timerId = null
    function parseDeadlineUtc(dl) {
      if (!dl) return null
      const s = String(dl)
      const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
      const normalized = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
      const ms = Date.parse(normalized)
      return Number.isFinite(ms) ? ms : null
    }
    function fmtCountdown(endTimeMs) {
      const diffMs = endTimeMs - Date.now()
      if (diffMs <= 0) return 'Expired'
      const totalSeconds = Math.floor(diffMs / 1000)
      const days = Math.floor(totalSeconds / 86400)
      const hours = Math.floor((totalSeconds % 86400) / 3600)
      const minutes = Math.floor((totalSeconds % 3600) / 60)
      const seconds = totalSeconds % 60
      if (days > 0) return `${days}d ${hours}h ${minutes}m`
      if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`
      return `${minutes}m ${seconds}s`
    }
    function updateAll() {
      const map = {}
      for (const ct of challengeTasksMeta) {
        const tid = String(ct.taskId ?? ct.TaskId)
        const endMs = parseDeadlineUtc(ct.deadline ?? ct.Deadline)
        if (endMs === null) continue
        map[tid] = fmtCountdown(endMs)
      }
      setCountdownByTask(map)
    }
    updateAll()
    timerId = setInterval(updateAll, 1000)
    return () => { if (timerId) clearInterval(timerId) }
  }, [challengeTasksMeta])

// load submissions for a specific task (used in voting)
  async function loadSubmissionsByTask(taskId) {
    try {
      const tid = Number(taskId)
      if (!tid) { setSubmissions([]); return }
      // use explicit task route to avoid ambiguity
      const res = await fetch(`${API_BASE}/api/photosubmissions/task/${tid}`)
      if (!res.ok) return
      const data = await res.json()
      const arr = Array.isArray(data) ? data : []
      const normalized = arr.map(s => ({
        id: s.id ?? s.Id,
        userId: s.userId ?? s.UserId,
        userName: s.userName ?? s.UserName ?? 'Unknown',
        photoUrl: (() => {
          const p = (s.photoUrl ?? s.PhotoUrl ?? '').toString()
          if (!p) return ''
          if (p.startsWith('http://') || p.startsWith('https://')) return p
          const base = (API_BASE || '').replace(/\/$/, '')
          return base ? `${base}${p.startsWith('/') ? '' : '/'}${p}` : p
        })(),
        votes: s.votes ?? s.Votes ?? 0,
        photoDataUrl: null
      }))
      setSubmissions(normalized)

      // fetch blobs and patch submissions by id (safer than using the index)
      normalized.forEach(async (item) => {
        if (!item.photoUrl) return
        try {
          const r = await fetch(item.photoUrl)
          if (!r.ok) return
          const blob = await r.blob()
          const reader = new FileReader()
          reader.onloadend = () => {
            setSubmissions(prev => {
              const copy = prev.map(p => p.id === item.id ? { ...p, photoDataUrl: reader.result } : p)
              return copy
            })
          }
          reader.readAsDataURL(blob)
        } catch {}
      })
    } catch {}
  }

  async function loadSubmissions() {
    // require a selected vote task — show empty until user picks one
    if (!voteTaskId) { setSubmissions([]); return }
    await loadSubmissionsByTask(Number(voteTaskId))
  }

  async function loadLeaderboard() {
    try {
      let res = await fetch(`${API_BASE}/api/leaderboard/challenge/${challengeId}`)
      if (res.ok) {
        const data = await res.json()
        const entries = Array.isArray(data) ? data : (Array.isArray(data?.entries) ? data.entries : [])
        setLeaderboard(assignDenseRanks(entries, e => Number(e.wins ?? e.Wins ?? e.totalVotes ?? e.TotalVotes ?? e.votes ?? e.Votes ?? 0)))
        return
      }

      res = await fetch(`${API_BASE}/api/photosubmissions?challengeId=${challengeId}`)
      if (!res.ok) return
      const subs = await res.json()
      const arr = Array.isArray(subs) ? subs : []

      const map = new Map()
      arr.forEach(s => {
        const uid = s.userId ?? s.UserId ?? 0
        const votes = Number(s.votes ?? s.Votes ?? 0)

        if (!map.has(uid)) {
          map.set(uid, {
            userId: uid,
            userName: s.userName ?? s.UserName ?? `User ${uid}`,
            votes
          })
        } else {
          const item = map.get(uid)
          item.votes += votes
        }
      })

      const out = Array.from(map.values()).sort((a, b) => (b.votes || 0) - (a.votes || 0))
      setLeaderboard(assignDenseRanks(out, e => Number(e.votes ?? e.Votes ?? 0)))
    } catch {}
  }


  async function handleVote(subId) {
    try {
      // Optimistically update the vote count immediately (like YouTube)
      setSubmissions(prev => prev.map(s => 
        s.id === subId ? { ...s, votes: (s.votes ?? 0) + 1 } : s
      ))
      
      const res = await fetch(`${API_BASE}/api/photosubmissions/${subId}/vote?userId=${userId}`, { method: 'POST' })
      if (!res.ok) {
        // Revert optimistic update on error
        setSubmissions(prev => prev.map(s => 
          s.id === subId ? { ...s, votes: Math.max(0, (s.votes ?? 1) - 1) } : s
        ))
        const raw = await res.text()
        let msg = raw
        try {
          const j = JSON.parse(raw)
          msg = j.error || j.message || raw
        } catch {}
        setMessage(`Vote failed: ${msg}`)
        return
      }
      setMessage('Vote recorded!')
      setTimeout(() => setMessage(''), 2000)
      
      // Refresh in background after a delay to sync with server (so other users see updates)
      // Clear any existing timeout
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current)
      }
      refreshTimeoutRef.current = setTimeout(async () => {
        const savedScroll = window.scrollY || document.documentElement.scrollTop
        if (voteTaskId) {
          await loadSubmissionsByTask(voteTaskId)
        } else {
          await loadSubmissions()
        }
        // Restore scroll position after refresh
        requestAnimationFrame(() => {
          window.scrollTo({ top: savedScroll, behavior: 'instant' })
          setTimeout(() => window.scrollTo({ top: savedScroll, behavior: 'instant' }), 100)
        })
        refreshTimeoutRef.current = null
      }, 1500) // Refresh after 1.5 seconds
    } catch (e) {
      setMessage(String(e))
    }
  }

  async function handleLeave() {
    if (!confirm('Leave this challenge?')) return
    setMessage('Leaving...')
    const res = await leaveChallenge(challengeId, userId)
    if (!res.ok) {
      setMessage('Error leaving challenge')
      return
    }
    navigate('/my-challenges')
  }

  async function handleAdvance() {
    if (!confirm('Advance to next stage?')) return
    
    const status = Number(challenge?.status ?? 0)
    
    // If we're in voting stage (status 1), sync all votes before advancing
    if (status === 1) {
      setMessage('Syncing votes...')
      
      // Cancel any pending refresh timeouts
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current)
        refreshTimeoutRef.current = null
      }
      
      // Refresh submissions for all tasks to ensure votes are synced
      const taskIds = tasksForChallenge.map(t => t.id ?? t.Id).filter(Boolean)
      const savedScroll = window.scrollY || document.documentElement.scrollTop
      
      try {
        // Refresh all tasks' submissions
        await Promise.all(taskIds.map(taskId => loadSubmissionsByTask(taskId)))
        
        // Restore scroll position
        window.scrollTo({ top: savedScroll, behavior: 'instant' })
        
        // Small delay to ensure server has processed all votes
        await new Promise(resolve => setTimeout(resolve, 500))
      } catch (e) {
        console.error('Error syncing votes:', e)
      }
    }
    
    setMessage('Advancing...')
    const res = await advanceChallenge(challengeId, userId)
    if (!res.ok) {
      setMessage('Error advancing challenge')
      return
    }
    setMessage('Stage advanced!')
    setTimeout(() => {
      setMessage('')
      loadChallengeData()
    }, 1000)
  }

  function handleFileChange(e) {
    const file = e.target.files[0]
    if (!file) {
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif']
    if (!allowedTypes.includes(file.type)) {
      setMessage('Only JPG, PNG, and GIF images are allowed')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    if (file.size > 10_000_000) {
      setMessage('File size must be less than 10MB')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    setPhotoFile(file)
    setMessage('')

    const reader = new FileReader()
    reader.onloadend = () => setPreviewUrl(reader.result)
    reader.readAsDataURL(file)
  }

  async function handleSubmitPhoto(e) {
    e.preventDefault()
    if (!photoFile) {
      setMessage('Please select a photo')
      return
    }
    if (!submitTaskId) {
      setMessage('Please choose a task to submit to.')
      return
    }

    setUploading(true)
    setUploadProgress(0)

    const formData = new FormData()
    formData.append('challengeId', challengeId)
    formData.append('taskId', submitTaskId)
    formData.append('userId', userId)
    formData.append('file', photoFile)

    try {
      const xhr = new XMLHttpRequest()
      
      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          setUploadProgress(Math.round((e.loaded / e.total) * 100))
        }
      }

      const result = await new Promise((resolve, reject) => {
        xhr.onload = () => {
          const text = xhr.responseText
          let data = null
          try { data = text ? JSON.parse(text) : null } catch { data = text }
          resolve({
            ok: xhr.status >= 200 && xhr.status < 300,
            status: xhr.status,
            data,
            text
          })
        }
        xhr.onerror = () => reject(new Error('Network error'))
        xhr.open('POST', `${API_BASE}/api/photosubmissions/upload`)
        xhr.send(formData)
      })

      if (!result.ok) {
        setMessage('Upload failed')
        return
      }

      setMessage('✅ Photo submitted successfully!')
      setPhotoFile(null)
      setPreviewUrl(null)
      setTimeout(() => setMessage(''), 3000)
      // refresh submissions if currently in vote tab for this task
      if (activeTab === 'vote' && submitTaskId) {
        setVoteTaskId(submitTaskId)
        await loadSubmissionsByTask(submitTaskId)
      }
    } catch (err) {
      setMessage('Network error')
    } finally {
      setUploading(false)
    }
  }

  if (loading) {
    return (
      <div style={{
        minHeight: 'calc(100vh - 70px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        <div style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: 18 }}>
          Loading challenge...
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div style={{
        minHeight: 'calc(100vh - 70px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 40
      }}>
        <div className="card" style={{ textAlign: 'center', maxWidth: 500 }}>
          <div className="error-message">{error}</div>
          <button onClick={() => navigate('/my-challenges')} style={{ marginTop: 16 }}>
            Back to My Challenges
          </button>
        </div>
      </div>
    )
  }

  const status = Number(challenge?.status ?? 0)
  const statusInfo = status === 0 ? { text: 'Submission Phase', color: '#51cf66' } :
                     status === 1 ? { text: 'Voting Phase', color: '#ffd43b' } :
                     { text: 'Completed', color: '#868e96' }

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      padding: '40px 40px 80px'
    }}>
      <div style={{ maxWidth: 1200, margin: '0 auto' }}>
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 32
        }}>
          <div>
            <h1 style={{ fontSize: 36, color: 'white', marginBottom: 8 }}>
              {challenge?.name ?? challenge?.Name ?? 'Challenge'}
            </h1>
            <div style={{ display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
              <div style={{
                display: 'inline-block',
                background: statusInfo.color,
                color: statusInfo.color === '#ffd43b' ? '#1a1a2e' : 'white',
                padding: '6px 16px',
                borderRadius: 16,
                fontSize: 14,
                fontWeight: 600
              }}>
                {statusInfo.text}
              </div>
              {stageCountdown && (
               <div style={{ color: 'rgba(255,255,255,0.7)', fontSize: 14 }}>
                 {stageCountdown}
               </div>
             )}

              {(challenge?.isPrivate ?? challenge?.IsPrivate ?? challenge?.private ?? false) && (challenge?.joinCode ?? challenge?.JoinCode) && (
                <div style={{
                  background: 'rgba(100, 108, 255, 0.2)',
                  color: 'white',
                  padding: '6px 16px',
                  borderRadius: 16,
                  fontSize: 14,
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8
                }}>
                  <span>🔑</span>
                  <span>Code: {challenge?.joinCode ?? challenge?.JoinCode}</span>
                </div>
              )}
              {!(challenge?.isPrivate ?? challenge?.IsPrivate ?? challenge?.private ?? false) && (
                <div style={{
                  background: 'rgba(81, 207, 102, 0.2)',
                  color: '#51cf66',
                  padding: '6px 16px',
                  borderRadius: 16,
                  fontSize: 14,
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8
                }}>
                  <span>🌍</span>
                  <span>Public</span>
                </div>
              )}
            </div>
          </div>
          <div style={{ display: 'flex', gap: 12 }}>
            {isAdmin && status < 2 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
              <button onClick={handleAdvance} style={{
                background: '#51cf66',
                padding: '10px 20px'
              }}>
                {status === 0 ? '→ Move to Voting' : 'Complete Challenge'}
              </button>
              {stageCountdown && (
                 <div style={{ fontSize: 12, color: 'rgba(255,255,255,0.6)', textAlign: 'center' }}>
                   Auto-advances when phase timer ends
                 </div>
               )}
             </div>
            )}
            <button onClick={handleLeave} style={{
              background: '#ff6b6b',
              padding: '10px 20px'
            }}>
              Leave Challenge
            </button>
          </div>
        </div>

        {/* Task summary / list
              - show single task preview in submit/vote phases
              - in leaderboard phase show all tasks that were part of the challenge
        */}
        {activeTab === 'leaderboard' ? (
          tasksForChallenge && tasksForChallenge.length > 0 ? (
            <div className="card" style={{ marginBottom: 32 }}>
              <h3 style={{ color: 'white', marginBottom: 12 }}>📋 Tasks in this challenge</h3>
              <ul style={{ margin: 0, paddingLeft: 18 }}>
                {tasksForChallenge.map((t) => (
                  <li key={t.id ?? t.Id} style={{ marginBottom: 8 }}>
                    <div style={{ color: 'white', fontWeight: 600 }}>{t.description ?? t.Description}</div>
                    {(() => {
                      // Prefer per-challenge deadline
                      const meta = challengeTasksMeta.find(ct => String(ct.taskId ?? ct.TaskId) === String(t.id ?? t.Id))
                      const dl = meta?.deadline ?? meta?.Deadline
                      if (dl) {
                        const countdown = countdownByTask[String(t.id ?? t.Id)] || ''
                        const normalized = (() => {
                          const s = String(dl)
                          const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
                          const normalizedStr = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
                          const ms = Date.parse(normalizedStr)
                          return Number.isFinite(ms) ? ms : null
                        })()
                        return (
                          <div style={{ color: 'rgba(255,255,255,0.6)', fontSize: 13 }}>
                            Deadline: {normalized !== null ? formatVilnius(normalized) : formatVilnius(dl)} {countdown && `(${countdown})`}
                          </div>
                        )
                      }
                      const tdl = t.deadline ?? t.Deadline
                      if (tdl) {
                        // Global deadline fallback
                        const normalizedTask = (() => {
                          const s = String(tdl)
                          const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
                          const normalizedStr = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
                          const ms = Date.parse(normalizedStr)
                          return Number.isFinite(ms) ? ms : null
                        })()
                        return (
                          <div style={{ color: 'rgba(255,255,255,0.6)', fontSize: 13 }}>
                            Deadline: {normalizedTask !== null ? formatVilnius(normalizedTask) : formatVilnius(tdl)}
                          </div>
                        )
                      }
                      return null
                    })()}
                  </li>
                ))}
              </ul>
            </div>
          ) : null
        ) : (
          task && (
            <div className="card" style={{ marginBottom: 32 }}>
              <h3 style={{ color: 'white', marginBottom: 12 }}>📋 Task</h3>
              <p style={{ color: 'rgba(255, 255, 255, 0.8)', fontSize: 16 }}>
                {task.description ?? task.Description}
              </p>
              {(() => {
                const meta = challengeTasksMeta.find(ct => String(ct.taskId ?? ct.TaskId) === String(task.id ?? task.Id))
                const dl = meta?.deadline ?? meta?.Deadline
                if (dl) {
                  const countdown = countdownByTask[String(task.id ?? task.Id)] || ''
                  const normalized = (() => {
                    const s = String(dl)
                    const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
                    const normalizedStr = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
                    const ms = Date.parse(normalizedStr)
                    return Number.isFinite(ms) ? ms : null
                  })()
                  return (
                    <p style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: 14, marginTop: 8 }}>
                      Deadline: {normalized !== null ? formatVilnius(normalized) : formatVilnius(dl)} {countdown && `(${countdown})`}
                    </p>
                  )
                }
                const tdl = task.deadline ?? task.Deadline
                if (tdl) {
                  const normalizedTask = (() => {
                    const s = String(tdl)
                    const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
                    const normalizedStr = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
                    const ms = Date.parse(normalizedStr)
                    return Number.isFinite(ms) ? ms : null
                  })()
                  return (
                    <p style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: 14, marginTop: 8 }}>
                      Deadline: {normalizedTask !== null ? formatVilnius(normalizedTask) : formatVilnius(tdl)}
                    </p>
                  )
                }
                return null
              })()}
            </div>
          )
        )}

        {message && (
          <div className={message.includes('Error') || message.includes('failed') ? 'error-message' : 'success-message'}>
            {message}
          </div>
        )}

        <div style={{
          display: 'flex',
          gap: 16,
          marginBottom: 24,
          borderBottom: '2px solid rgba(100, 108, 255, 0.2)',
          paddingBottom: 0
        }}>
          {status === 0 && (
            <button
              onClick={() => setActiveTab('submit')}
              style={{
                background: activeTab === 'submit' ? 'rgba(100, 108, 255, 0.2)' : 'transparent',
                borderBottom: activeTab === 'submit' ? '2px solid #646cff' : 'none',
                borderRadius: '8px 8px 0 0',
                padding: '12px 24px',
                marginBottom: -2,
                boxShadow: 'none'
              }}
            >
              Submit Photo
            </button>
          )}
          {status === 1 && (
            <button
              onClick={() => setActiveTab('vote')}
              style={{
                background: activeTab === 'vote' ? 'rgba(100, 108, 255, 0.2)' : 'transparent',
                borderBottom: activeTab === 'vote' ? '2px solid #646cff' : 'none',
                borderRadius: '8px 8px 0 0',
                padding: '12px 24px',
                marginBottom: -2,
                boxShadow: 'none'
              }}
            >
              Vote
            </button>
          )}
          {status === 2 && (
            <button
              onClick={() => setActiveTab('leaderboard')}
              style={{
                background: activeTab === 'leaderboard' ? 'rgba(100, 108, 255, 0.2)' : 'transparent',
                borderBottom: activeTab === 'leaderboard' ? '2px solid #646cff' : 'none',
                borderRadius: '8px 8px 0 0',
                padding: '12px 24px',
                marginBottom: -2,
                boxShadow: 'none'
              }}
            >
              Leaderboard
            </button>
          )}
        </div>

        <div className="card">
          {activeTab === 'submit' && status === 0 && (
            <form onSubmit={handleSubmitPhoto}>
              <h3 style={{ color: 'white', marginBottom: 24 }}>Upload Your Photo</h3>
              
              {/* Task selector for submit */}
              {tasksForChallenge.length > 0 && (
                <div style={{ marginBottom: 12 }}>
                  <label style={{ display: 'block', marginBottom: 6 }}>Choose Task</label>
                  <select value={submitTaskId} onChange={e => {
                      const v = e.target.value
                      setSubmitTaskId(v)
                      const t = tasksForChallenge.find(x => String(x.id ?? x.Id) === v)
                      if (t) setTask(t)
                    }} disabled={uploading} style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}>
                    <option value="">-- choose a task --</option>
                    {tasksForChallenge.map(t => <option key={t.id ?? t.Id} value={t.id ?? t.Id}>{t.description ?? t.Description}</option>)}
                  </select>
                </div>
              )}
              <input
                type="file"
                accept="image/jpeg,image/png,image/gif"
                onChange={handleFileChange}
                disabled={uploading}
                style={{ marginBottom: 20 }}
              />

              {previewUrl && (
                <div style={{
                  marginBottom: 20,
                  textAlign: 'center',
                  borderRadius: 8,
                  overflow: 'hidden'
                }}>
                  <img
                    src={previewUrl}
                    alt="Preview"
                    style={{
                      maxWidth: '100%',
                      maxHeight: 400,
                      objectFit: 'contain'
                    }}
                  />
                </div>
              )}

              {uploading && (
                <div style={{ marginBottom: 20 }}>
                  <div style={{ fontSize: 14, marginBottom: 8, color: 'rgba(255, 255, 255, 0.7)' }}>
                    {uploadProgress}%
                  </div>
                  <div style={{
                    width: '100%',
                    height: 8,
                    background: 'rgba(100, 108, 255, 0.2)',
                    borderRadius: 4,
                    overflow: 'hidden'
                  }}>
                    <div style={{
                      width: `${uploadProgress}%`,
                      height: '100%',
                      background: '#646cff',
                      transition: 'width 0.3s'
                    }} />
                  </div>
                </div>
              )}

              <button
                type="submit"
                disabled={!photoFile || uploading}
                style={{ width: '100%', padding: '14px' }}
              >
                {uploading ? `Uploading... ${uploadProgress}%` : 'Submit Photo'}
              </button>
            </form>
          )}

          {activeTab === 'vote' && status === 1 && (
            <div>
              <h3 style={{ color: 'white', marginBottom: 24 }}>Vote for Submissions</h3>
              {/* task selector for voting */}
              {tasksForChallenge.length > 0 && (
                <div style={{ marginBottom: 12 }}>
                  <label style={{ display: 'block', marginBottom: 6 }}>Select Task to Vote</label>
                  <select value={voteTaskId} onChange={async e => {
                      const v = e.target.value
                      setVoteTaskId(v)
                      const t = tasksForChallenge.find(x => String(x.id ?? x.Id) === v)
                      if (t) setTask(t)
                      if (v) await loadSubmissionsByTask(Number(v))
                      else setSubmissions([])
                    }} style={{ width: '100%', padding: 8, boxSizing: 'border-box', marginBottom: 12 }}>
                    <option value="">-- choose a task --</option>
                    {tasksForChallenge.map(t => <option key={t.id ?? t.Id} value={t.id ?? t.Id}>{t.description ?? t.Description}</option>)}
                  </select>
                </div>
              )}
              {submissions.length === 0 ? (
                <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.6)', padding: 40 }}>
                  No submissions yet
                </div>
              ) : (
                <div ref={scrollContainerRef} style={{ display: 'grid', gap: 24 }}>
                  {submissions.map(s => (
                    <div key={s.id} style={{
                      background: 'rgba(100, 108, 255, 0.05)',
                      borderRadius: 12,
                      padding: 20
                    }}>
                      <div style={{ fontWeight: 600, color: 'white', marginBottom: 12 }}>
                        {s.userName}
                      </div>
                      {(s.photoDataUrl || s.photoUrl) && (
                        <div style={{ textAlign: 'center', marginBottom: 16 }}>
                          <img
                            src={s.photoDataUrl || s.photoUrl}
                            alt={`Submission by ${s.userName}`}
                            style={{
                              maxWidth: '100%',
                              maxHeight: 400,
                              objectFit: 'contain',
                              borderRadius: 8
                            }}
                          />
                        </div>
                      )}
                      <div style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between'
                      }}>
                        <div style={{ color: 'rgba(255, 255, 255, 0.7)' }}>
                          Votes: {s.votes}
                        </div>
                        <button onClick={() => handleVote(s.id)}>
                          Vote
                        </button>
                      </div>
                      <CommentSection submissionId={s.id} currentUserId={userId} />
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'leaderboard' && status === 2 && (
            <div>
              <h3 style={{ color: 'white', marginBottom: 24 }}>Final Results</h3>
              {(() => {
                const hasAnyVotes = leaderboard.some(e => Number(e.votes ?? e.wins ?? e.totalVotes ?? 0) > 0)
                if (!leaderboard.length || !hasAnyVotes) {
                  return (
                    <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.7)', padding: 40 }}>
                      No submissions received any votes. There are no winners.
                    </div>
                  )
                }
                return (
                  <ol style={{ listStyle: 'none', padding: 0, margin: 0 }}>
                    {leaderboard.map((entry, index) => {
                      const total = Number(entry.votes ?? entry.wins ?? entry.totalVotes ?? 0)
                      if (total === 0) return null
                      const rank = entry.rank ?? (index + 1)
                      const medal = rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : `#${rank}`
                    return (
                      <li key={entry.userId ?? index} style={{
                        background: 'rgba(100, 108, 255, 0.05)',
                        borderRadius: 12,
                        padding: 20,
                        marginBottom: 12,
                        display: 'flex',
                        alignItems: 'center',
                        gap: 20
                      }}>
                        <div style={{
                          fontSize: 32,
                          fontWeight: 700,
                          width: 60,
                          textAlign: 'center'
                        }}>
                          {medal || `#${index + 1}`}
                        </div>
                        <div style={{ flex: 1 }}>
                          <div style={{ color: 'white', fontWeight: 600, fontSize: 18 }}>
                            {entry.userName}
                          </div>
                          <div style={{ color: '#51cf66', fontWeight: 600, fontSize: 20, marginTop: 4 }}>
                            {entry.votes ?? entry.wins ?? entry.totalVotes ?? 0} votes
                          </div>
                        </div>
                      </li>
                    )
                  })}
                </ol>
              )})()}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}