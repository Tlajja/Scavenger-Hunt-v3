import React, { useEffect, useState } from 'react'
import { getTasks, API_BASE } from '../services/api.js'

export default function MyTasks() {
  const [tasks, setTasks] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const userId = Number(localStorage.getItem('userId') || 0)

  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await getTasks()
        let data = null
        if (res instanceof Response) {
          if (!res.ok) throw new Error(await res.text() || res.statusText)
          data = await res.json()
        } else {
          if (!res.ok) throw new Error(res.text || `Error ${res.status}`)
          data = res.data
        }
        if (mounted) setTasks(Array.isArray(data) ? data : [])
      } catch (err) {
        if (mounted) setError(String(err))
      } finally {
        if (mounted) setLoading(false)
      }
    }
    load()
    return () => { mounted = false }
  }, [])

  async function handleDelete(taskId) {
    if (!userId) { setError('Not logged in.'); return }
    if (!confirm('Delete this task?')) return
    try {
      const res = await fetch(`${API_BASE}/api/tasks/user/${userId}/${taskId}`, { method: 'DELETE' })
      if (!res.ok) {
        const text = await res.text()
        setError(`Delete failed: ${text || res.statusText}`)
        return
      }
      setTasks(prev => prev?.filter(t => t.id !== taskId) ?? [])
    } catch (err) {
      setError('Network error')
    }
  }

  if(!userId) return <div style={{ color: 'crimson' }}>You must be logged in to view your tasks.</div>
  if (loading) return <div>Loading your tasks…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error: {error}</div>
  if (!tasks || tasks.length === 0) return <div>No tasks found.</div>

  const my = tasks.filter(t => Number(t.authorId ?? t.AuthorId ?? 0) === userId)

  if (my.length === 0) return <div>You have not created any tasks.</div>

  return (
    <div>
      <h2>My Tasks</h2>
      <ul style={{ listStyle: 'none', padding: 0 }}>
        {my.map(task => (
          <li key={task.id} style={{ border: '1px solid #eee', padding: 12, marginBottom: 8 }}>
            <div style={{ fontWeight: 600 }}>{task.description ?? task.Description}</div>
            <div style={{ color: '#666', fontSize: 13 }}>
              Deadline: {new Date(task.deadline ?? task.Deadline).toLocaleString()}
              {' • '}
              Status: {task.status ?? task.Status}
            </div>
            <div style={{ marginTop: 8 }}>
              <button onClick={() => handleDelete(task.id ?? task.Id)} style={{ marginRight: 8 }}>Delete</button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}