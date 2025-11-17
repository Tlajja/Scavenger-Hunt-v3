import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { createChallenge, getTasks, createUserTask } from '../services/api.js'

export default function CreateChallenge() {
  const navigate = useNavigate()
  const userId = Number(localStorage.getItem('userId') || 0)
  
  const [challengeName, setChallengeName] = useState('')
  const [isPrivate, setIsPrivate] = useState(false)
  const [tasks, setTasks] = useState([])
  const [selectedTaskId, setSelectedTaskId] = useState('')
  const [deadline, setDeadline] = useState('')
  const [error, setError] = useState('')
  const [creating, setCreating] = useState(false)
  
  const [showCreateTask, setShowCreateTask] = useState(false)
  const [taskDescription, setTaskDescription] = useState('')
  const [taskDeadline, setTaskDeadline] = useState('')
  const [creatingTask, setCreatingTask] = useState(false)

  useEffect(() => {
    loadTasks()
  }, [])

  async function loadTasks() {
    try {
      const res = await getTasks()
      if (res.ok) {
        const data = Array.isArray(res.data) ? res.data : []
        setTasks(data)
        if (data.length && !selectedTaskId) {
          setSelectedTaskId(String(data[0].id ?? data[0].Id))
        }
      }
    } catch {}
  }

  function toIsoForServer(dtLocal) {
    if (!dtLocal) return null
    const d = new Date(dtLocal)
    return d.toISOString()
  }

  async function handleCreateTask(e) {
    e.preventDefault()
    if (!taskDescription.trim()) {
      setError('Task description is required')
      return
    }

    setCreatingTask(true)
    setError('')

    try {
      const iso = toIsoForServer(taskDeadline)
      const res = await createUserTask(taskDescription, iso, userId)

      if (!res.ok) {
        setError((res.data && (res.data.message || res.data.error)) || res.text || 'Failed to create task')
        return
      }

      const newTask = res.data
      setTasks(prev => [...prev, newTask])
      setSelectedTaskId(String(newTask.id ?? newTask.Id))
      setTaskDescription('')
      setTaskDeadline('')
      setShowCreateTask(false)
    } catch (err) {
      setError('Network error')
    } finally {
      setCreatingTask(false)
    }
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (!challengeName.trim()) {
      setError('Challenge name is required')
      return
    }

    if (!selectedTaskId) {
      setError('Please select or create a task')
      return
    }

    setCreating(true)

    try {
      const iso = toIsoForServer(deadline)
      const res = await createChallenge(challengeName.trim(), userId, Number(selectedTaskId), iso, isPrivate)

      if (!res.ok) {
        setError((res.data && (res.data.error || res.data.message)) || res.text || 'Failed to create challenge')
        return
      }

      const created = res.data
      const createdId = created?.id ?? created?.Id

      if (createdId) {
        localStorage.setItem('challengeId', String(createdId))
        localStorage.setItem('challengeName', challengeName.trim())
      }

      navigate(`/challenge-room/${createdId}`)
    } catch (err) {
      setError('Network error')
    } finally {
      setCreating(false)
    }
  }

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: 40
    }}>
      <div className="card" style={{ maxWidth: 600, width: '100%' }}>
        <h1 style={{
          fontSize: 32,
          marginBottom: 32,
          textAlign: 'center',
          color: 'white'
        }}>
          Create a Challenge
        </h1>

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: 20 }}>
            <label>Challenge Name</label>
            <input
              value={challengeName}
              onChange={e => setChallengeName(e.target.value)}
              placeholder="Enter challenge name"
              disabled={creating}
            />
          </div>

          <div style={{ marginBottom: 20 }}>
            <label>Select Task</label>
            <select
              value={selectedTaskId}
              onChange={e => setSelectedTaskId(e.target.value)}
              disabled={creating}
            >
              <option value="">-- Choose a task --</option>
              {tasks.map(t => (
                <option key={t.id ?? t.Id} value={t.id ?? t.Id}>
                  {t.description ?? t.Description ?? '(no description)'}
                </option>
              ))}
            </select>
            <button
              type="button"
              onClick={() => setShowCreateTask(!showCreateTask)}
              disabled={creating}
              style={{
                marginTop: 12,
                width: '100%',
                background: 'transparent',
                border: '1px solid #646cff',
                boxShadow: 'none'
              }}
            >
              {showCreateTask ? 'Cancel' : '+ Create New Task'}
            </button>
          </div>

          {showCreateTask && (
            <div style={{
              background: 'rgba(100, 108, 255, 0.05)',
              padding: 20,
              borderRadius: 8,
              marginBottom: 20
            }}>
              <h3 style={{ color: 'white', marginBottom: 16, fontSize: 18 }}>Create New Task</h3>
              
              <div style={{ marginBottom: 16 }}>
                <label>Task Description</label>
                <textarea
                  value={taskDescription}
                  onChange={e => setTaskDescription(e.target.value)}
                  placeholder="Describe what participants need to photograph"
                  rows={3}
                  disabled={creatingTask}
                />
              </div>

              <div style={{ marginBottom: 16 }}>
                <label>Task Deadline (Optional)</label>
                <input
                  type="datetime-local"
                  value={taskDeadline}
                  onChange={e => setTaskDeadline(e.target.value)}
                  disabled={creatingTask}
                />
                <div style={{ fontSize: 12, color: 'rgba(255, 255, 255, 0.6)', marginTop: 6 }}>
                  Leave blank for 7 days from now
                </div>
              </div>

              <button
                onClick={handleCreateTask}
                disabled={creatingTask || !taskDescription.trim()}
                style={{ width: '100%' }}
              >
                {creatingTask ? 'Creating Task...' : 'Create Task'}
              </button>
            </div>
          )}

          <div style={{ marginBottom: 20 }}>
            <label>Challenge Deadline (Optional)</label>
            <input
              type="datetime-local"
              value={deadline}
              onChange={e => setDeadline(e.target.value)}
              disabled={creating}
            />
            <div style={{ fontSize: 12, color: 'rgba(255, 255, 255, 0.6)', marginTop: 6 }}>
              Leave blank for 7 days from now
            </div>
          </div>

          <div style={{ marginBottom: 24 }}>
            <label style={{
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              cursor: 'pointer',
              userSelect: 'none'
            }}>
              <input
                type="checkbox"
                checked={isPrivate}
                onChange={e => setIsPrivate(e.target.checked)}
                disabled={creating}
                style={{ width: 'auto', cursor: 'pointer' }}
              />
              <span>Private Challenge (requires join code)</span>
            </label>
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={creating || !challengeName.trim() || !selectedTaskId}
            style={{ width: '100%', padding: '14px', fontSize: 16 }}
          >
            {creating ? 'Creating Challenge...' : 'Create Challenge'}
          </button>
        </form>

        <div style={{
          marginTop: 24,
          textAlign: 'center',
          color: 'rgba(255, 255, 255, 0.6)'
        }}>
          <a
            onClick={() => navigate('/')}
            style={{ color: '#646cff', cursor: 'pointer', fontWeight: 500 }}
          >
            ← Back to Home
          </a>
        </div>
      </div>
    </div>
  )
}