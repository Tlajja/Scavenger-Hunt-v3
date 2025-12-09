import React, { useState, useEffect } from 'react'
import { getChallenges, getChallengeById, getTaskById } from '../services/api.js'

export default function SubmitPhoto() {
  const [photoFile, setPhotoFile] = useState(null)
  const [previewUrl, setPreviewUrl] = useState(null)
  const [message, setMessage] = useState('')
  const [challengeId, setChallengeId] = useState('')
  const [challenges, setChallenges] = useState([])
  const [selectedTask, setSelectedTask] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [uploading, setUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)
  const userId = localStorage.getItem('userId') || ''
  const [tasksForChallenge, setTasksForChallenge] = useState([])
  const [selectedTaskId, setSelectedTaskId] = useState('')
  const [challengeTasksMeta, setChallengeTasksMeta] = useState([])
  const [countdownText, setCountdownText] = useState('')

  function parseDeadlineUtc(dl) {
    if (!dl) return null
    const s = String(dl)
    // If the deadline string has no timezone, assume UTC and append 'Z'
    const hasTz = /([zZ]|[+-]\d{2}:?\d{2})$/.test(s)
    const normalized = hasTz ? s : (s.endsWith('Z') ? s : s + 'Z')
    const ms = Date.parse(normalized)
    return Number.isFinite(ms) ? ms : null
  }

  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const cres = await getChallenges(false)
        if (!mounted) return
        const raw = Array.isArray(cres.data) ? cres.data : []
        const open = raw.filter(c => Number(c.status ?? c.Status ?? 0) === 0)
        setChallenges(open)
      } catch (err) {
        if (!mounted) return
        setError(String(err))
      } finally {
        if (mounted) {
          setLoading(false)
        }
      }
    }
    load()
    return () => { mounted = false }
  }, [])

  useEffect(() => {
    let mounted = true
    async function loadPreview() {
      setSelectedTask(null)
      setTasksForChallenge([])
      setSelectedTaskId('')
      if (!challengeId) return
      try {
        const cres = await getChallengeById(Number(challengeId))
        if (!mounted || !cres.ok) return
        const ch = cres.data
        // collect task ids from challenge -> fetch each task to show to user
        const taskRefs = Array.isArray(ch?.challengeTasks ?? ch?.ChallengeTasks)
          ? ch.challengeTasks ?? ch.ChallengeTasks
          : (ch?.ChallengeTasks ?? []).map(x => x)
        setChallengeTasksMeta(taskRefs)
        const ids = taskRefs.map(t => Number(t.taskId ?? t.TaskId ?? t.task?.id ?? t.task?.Id ?? 0)).filter(Boolean)
        if (ids.length === 0) return
        const tasks = await Promise.all(ids.map(id => getTaskById(id)))
        const valid = tasks.filter(r => r?.ok).map(r => r.data)
        if (!mounted) return
        setTasksForChallenge(valid)
        // default the preview to first task
        setSelectedTask(valid[0] ?? null)
        setSelectedTaskId(String(valid[0]?.id ?? valid[0]?.Id ?? ''))
      } catch {}
    }
    loadPreview()
    return () => { mounted = false }
  }, [challengeId])

  // when user picks a task in the UI
  function handleSelectTask(e) {
    const tid = e.target.value
    setSelectedTaskId(tid)
    const t = tasksForChallenge.find(x => String(x.id ?? x.Id) === tid)
    setSelectedTask(t ?? null)
  }

  useEffect(() => {
    let timerId = null
    function updateCountdown() {
      if (!selectedTaskId) { setCountdownText(''); return }
      const meta = challengeTasksMeta.find(ct => String(ct.taskId ?? ct.TaskId) === String(selectedTaskId))
      const dl = meta?.deadline ?? meta?.Deadline
      const end = parseDeadlineUtc(dl)
      if (end === null) { setCountdownText(''); return }
      const now = Date.now()
      const diffMs = end - now
      if (diffMs <= 0) { setCountdownText('Expired'); return }
      const totalSeconds = Math.floor(diffMs / 1000)
      const days = Math.floor(totalSeconds / 86400)
      const hours = Math.floor((totalSeconds % 86400) / 3600)
      const minutes = Math.floor((totalSeconds % 3600) / 60)
      const seconds = totalSeconds % 60
      const fmt = days > 0 ? `${days}d ${hours}h ${minutes}m` : (hours > 0 ? `${hours}h ${minutes}m ${seconds}s` : `${minutes}m ${seconds}s`)
      setCountdownText(fmt)
    }
    updateCountdown()
    timerId = setInterval(updateCountdown, 1000)
    return () => { if (timerId) clearInterval(timerId) }
  }, [selectedTaskId, challengeTasksMeta])

  function handleFileChange(e) {
    const file = e.target.files[0]
    if (!file) {
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif']
    if (!allowedTypes.includes(file.type)) {
      setMessage('Only JPG, PNG, and GIF images are allowed.')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    if (file.size > 10_000_000) {
      setMessage('File size must be less than 10MB.')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    setPhotoFile(file)
    setMessage('')

    const reader = new FileReader()
    reader.onloadend = () => {
      setPreviewUrl(reader.result)
    }
    reader.readAsDataURL(file)
  }

  function clearPreview() {
    setPhotoFile(null)
    setPreviewUrl(null)
    setUploadProgress(0)
    setMessage('')
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('')

    if (!userId) { 
      setMessage('You must be logged in to submit.')
      return 
    }
    if (!challengeId) { 
      setMessage('Please select a challenge.')
      return 
    }
    if (!photoFile) { 
      setMessage('Please select a photo file.')
      return 
    }
    if (!selectedTaskId) {
      setMessage('Please choose which task to submit to.')
      return
    }

    setUploading(true)
    setUploadProgress(0)

    try {
      const result = await uploadWithProgress(challengeId, selectedTaskId, userId, photoFile)

      if (!result.ok) {
        const err = (result.data && (result.data.message || result.data.error)) || result.text || `Error ${result.status}`
        setMessage(`Error: ${err}`)
        return
      }

      setMessage('✅ Photo uploaded successfully!')

      setTimeout(() => {
        clearPreview()
        setChallengeId('')
        e.target.reset()
      }, 2000)

    } catch (err) {
      console.error('Upload error:', err)
      setMessage(`Network error: ${err.message}`)
    } finally {
      setUploading(false)
    }
  }

  // update uploadWithProgress to accept challengeId name and only send challengeId
  function uploadWithProgress(challengeId, taskId, userId, file) {
    return new Promise((resolve, reject) => {
      const formData = new FormData()
      formData.append('challengeId', challengeId)
      formData.append('taskId', taskId)
      formData.append('userId', userId)
      formData.append('file', file)

      const xhr = new XMLHttpRequest()
      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          const percentComplete = Math.round((e.loaded / e.total) * 100)
          setUploadProgress(percentComplete)
        }
      }

      xhr.onload = () => {
        const text = xhr.responseText
        let data = null
        try { data = text ? JSON.parse(text) : null } catch { data = text }

        resolve({
          ok: xhr.status >= 200 && xhr.status < 300,
          status: xhr.status,
          data: data,
          text: text
        })
      }

      xhr.onerror = () => reject(new Error('Network error occurred'))
      xhr.onabort = () => reject(new Error('Upload cancelled'))

      xhr.open('POST', 'http://localhost:5248/api/photosubmissions/upload')
      xhr.send(formData)
    })
  }

  if (loading) return <div>Loading tasks…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error loading tasks: {error}</div>

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 480, margin: '0 auto' }}>
      <h2>Submit Photo</h2>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Select Challenge</label>
        <select
          value={challengeId}
          onChange={e => setChallengeId(e.target.value)}
          disabled={uploading}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        >
          <option value="">-- choose a challenge --</option>
          {challenges.map(c => (
            <option key={c.id ?? c.Id} value={c.id ?? c.Id}>
              {`${c.name ?? c.Name ?? '(no name)'} — task:${c.taskId ?? c.TaskId ?? ''} — code:${c.joinCode ?? c.JoinCode ?? ''}`}
            </option>
          ))}
        </select>
        <div style={{ marginTop: 8, fontSize: 12, color: '#666' }}>
          Select a challenge to preview its task before uploading.
        </div>
      </div>

      {/* Task preview */}
      {selectedTask && (
        <div style={{ marginBottom: 12, padding: 10, border: '1px solid #eee', borderRadius: 4 }}>
          <div style={{ fontWeight: 600 }}>Task preview</div>
          <div style={{ marginTop: 6 }}>{selectedTask.description ?? selectedTask.Description}</div>
          {/* Show per-challenge effective deadline with countdown if present */}
          {(() => {
            const meta = challengeTasksMeta.find(ct => String(ct.taskId ?? ct.TaskId) === String(selectedTaskId))
            const dl = meta?.deadline ?? meta?.Deadline
            if (dl) {
              return (
                <div style={{ marginTop: 6, fontSize: 12, color: '#666' }}>
                  Deadline: {new Date(dl).toLocaleString()} ({countdownText})
                </div>
              )
            }
            // fallback: show task global deadline if any
            const tdl = selectedTask.deadline ?? selectedTask.Deadline
            if (tdl) {
              return (
                <div style={{ marginTop: 6, fontSize: 12, color: '#666' }}>
                  Deadline: {new Date(tdl).toLocaleString()}
                </div>
              )
            }
            return null
          })()}
        </div>
      )}
       {/* rest of the form (file input, preview, upload) */}

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Choose Task</label>
        <select value={selectedTaskId} onChange={handleSelectTask} disabled={uploading || tasksForChallenge.length===0} style={{ width:'100%', padding:8, boxSizing:'border-box' }}>
          <option value="">-- choose a task --</option>
          {tasksForChallenge.map(t => (
            <option key={t.id ?? t.Id} value={t.id ?? t.Id}>
              {t.description ?? t.Description}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Upload Photo</label>
        <input
          type="file"
          accept="image/jpeg,image/png,image/gif"
          onChange={handleFileChange}
          disabled={uploading}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        />
      </div>

      {/* Image Preview */}
      {previewUrl && (
        <div style={{ 
          marginBottom: 15,
          borderRadius: 4,
          overflow: 'hidden',
          backgroundColor: 'inherit', // use default background
          textAlign: 'center'
        }}>
          <img 
            src={previewUrl} 
            alt="Preview" 
            style={{ 
              width: '100%', 
              maxHeight: 300, 
              objectFit: 'contain'
            }} 
          />
          <div style={{ marginTop: 5 }}>
            <button 
              type="button"
              onClick={clearPreview}
              style={{ cursor: 'pointer', padding: '4px 8px', marginTop: 5 }}
            >
              Remove
            </button>
          </div>
        </div>
      )}

      {/* Upload Progress */}
      {uploading && (
        <div style={{ marginBottom: 15 }}>
          <div style={{ fontSize: '12px', marginBottom: 5 }}>{uploadProgress}%</div>
          <div style={{ 
            width: '100%', 
            height: 10, 
            backgroundColor: '#ccc', 
            borderRadius: 5,
            overflow: 'hidden'
          }}>
            <div style={{ 
              width: `${uploadProgress}%`, 
              height: '100%', 
              backgroundColor: '#2196F3'
            }}></div>
          </div>
        </div>
      )}

      {!userId && (
        <div style={{ color: 'crimson', marginBottom: 10 }}>
          You must be logged in to submit photos.
        </div>
      )}

      <button 
        type="submit" 
        disabled={!challengeId || !photoFile || !userId || uploading}
        style={{
          width: '100%',
          padding: 12,
          fontSize: '16px',
          cursor: uploading || !challengeId || !photoFile || !userId ? 'not-allowed' : 'pointer',
          opacity: uploading || !challengeId || !photoFile || !userId ? 0.6 : 1
        }}
      >
        {uploading ? `Uploading... ${uploadProgress}%` : 'Submit Photo'}
      </button>

      {message && (
        <div style={{ 
          marginTop: 10,
          padding: 10,
          borderRadius: 4,
          backgroundColor: message.includes('Error') || message.includes('must') ? '#ffebee' : '#e8f5e9',
          color: message.includes('Error') || message.includes('must') ? '#c62828' : '#2e7d32',
          border: `1px solid ${message.includes('Error') || message.includes('must') ? '#ef9a9a' : '#a5d6a7'}`
        }}>
          {message}
        </div>
      )}
    </form>
  )
}
