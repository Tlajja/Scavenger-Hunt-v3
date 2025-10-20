import React, { useState, useEffect } from 'react'
import { submitPhoto, getTasks } from '../services/api.js'

export default function SubmitPhoto() {
  const [photoFile, setPhotoFile] = useState(null)
  const [message, setMessage] = useState('')
  const [taskId, setTaskId] = useState('')
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const userId = localStorage.getItem('userId') || ''

  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await getTasks()
        let data = res.data
        if (!mounted) return
        setTasks(Array.isArray(data) ? data : [])
      } catch (err) {
        if (!mounted) return
        setError(String(err))
      } finally {
        if (!mounted) return
        setLoading(false)
      }
    }
    load()
    return () => { mounted = false }
  }, [])

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('')
    if (!userId) { setMessage('You must be logged in to submit.'); return }
    if (!taskId) { setMessage('Please select a task.'); return }
    if (!photoFile) { setMessage('Please select a photo file.'); return }

    try {
      const res = await submitPhoto(Number(taskId), Number(userId), photoFile)
      if (!res.ok) {
        const err = (res.data && (res.data.message || res.data.error)) || res.text || `Error ${res.status}`
        setMessage(`Error: ${err}`)
        return
      }
      setMessage('Submission created.')
      setPhotoFile(null)
      e.target.reset()
    } catch (err) {
      setMessage('Network error')
    }
  }

  if (loading) return <div>Loading tasks…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error loading tasks: {error}</div>

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 480 }}>
      <h2>Submit Photo</h2>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Select Task</label>
        <select
          value={taskId}
          onChange={e => setTaskId(e.target.value)}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        >
          <option value="">-- choose a task --</option>
          {tasks.map(t => (
            <option key={t.id ?? t.Id ?? `${t.description}-${t.deadline}`} value={t.id ?? t.Id}>
              {`${t.id ?? t.Id ?? ''} — ${t.description ?? t.Description ?? '(no description)'}`}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Upload Photo</label>
        <input
          type="file"
          accept="image/jpeg,image/png,image/gif"
          onChange={e => setPhotoFile(e.target.files[0])}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        />
      </div>

      {!userId && <div style={{ color: 'crimson', marginBottom: 10 }}>You must be logged in to submit photos.</div>}

      <button type="submit" disabled={!taskId || !photoFile || !userId}>Submit</button>

      <div style={{ marginTop: 10 }}>{message}</div>
    </form>
  )
}
