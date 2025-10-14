import React, { useState } from 'react'
import { createTask, createUserTask } from '../services/api.js'

export default function TaskCreate() {
  const [description, setDescription] = useState('')
  const [deadline, setDeadline] = useState('')
  const [message, setMessage] = useState('')
  const userId = localStorage.getItem('userId')

  function toIsoForServer(dtLocal) {
    if (!dtLocal) return null
    const d = new Date(dtLocal) // dtLocal is "yyyy-MM-ddTHH:mm"
    return d.toISOString()
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('Creating...')
    const iso = toIsoForServer(deadline)
    if (!description.trim()) { setMessage('Description is required.'); return }
    if (!iso) { setMessage('Deadline is required.'); return }
    if (!userId) { setMessage('You must be logged in to create a task.'); return }

    try {
      const res = await createUserTask(description, iso, Number(userId))

      if (!res.ok) {
        const text = res.data?.message || res.text || `Error ${res.status}`
        setMessage(`Error: ${text}`)
        return
      }

      setMessage('Task created.')
      setDescription('')
      setDeadline('')
    } catch (err) {
      setMessage('Network error')
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 640 }}>
      <h2>Create Task</h2>
      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Description</label>
        <textarea value={description} onChange={e => setDescription(e.target.value)} rows={3} style={{ width: '100%' }} />
      </div>
      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Deadline</label>
        <input type="datetime-local" value={deadline} onChange={e => setDeadline(e.target.value)} style={{ width: '100%' }} />
      </div>
      <div style={{ marginBottom: 12 }}>
        Creating as user: {localStorage.getItem('username') || userId || 'not logged in'}
      </div>
      <button type="submit" disabled={!userId}>Create</button>
      <div style={{ marginTop: 10 }}>{message}</div>
    </form>
  )
}