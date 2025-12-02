import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createChallenge, createUserTask, getRandomTaskForUser } from '../services/api.js'

export default function CreateChallenge() {
 const navigate = useNavigate()
 const userId = Number(localStorage.getItem('userId') ||0)

 const [challengeName, setChallengeName] = useState('')
 const [isPrivate, setIsPrivate] = useState(false)
 const [tasks, setTasks] = useState([])
 const [selectedTaskIds, setSelectedTaskIds] = useState([])
 const [deadline, setDeadline] = useState('')
 const [error, setError] = useState('')
 const [creating, setCreating] = useState(false)
 const [showCreateTask, setShowCreateTask] = useState(false)
 const [taskDescription, setTaskDescription] = useState('')
 const [taskDeadline, setTaskDeadline] = useState('')
 const [creatingTask, setCreatingTask] = useState(false)
 const [generating, setGenerating] = useState(false)

 function toIsoForServer(dtLocal) {
 if (!dtLocal) return null
 const d = new Date(dtLocal)
 return d.toISOString()
 }

 function addSelectedTask(id) {
 const idStr = String(id)
 setSelectedTaskIds(prev => (prev.includes(idStr) ? prev : [...prev, idStr]))
 }

 function removeSelectedTask(id) {
 const idStr = String(id)
 setSelectedTaskIds(prev => prev.filter(x => x !== idStr))
 }

 async function handleCreateTask(e) {
 e.preventDefault()
 if (!taskDescription.trim()) { setError('Task description is required'); return }
 setCreatingTask(true)
 setError('')
 try {
 const iso = toIsoForServer(taskDeadline)
 const res = await createUserTask(taskDescription, iso, userId)
 if (!res.ok) { setError((res.data && (res.data.message || res.data.error)) || res.text || 'Failed to create task'); return }
 const newTask = res.data
 setTasks(prev => [...prev, newTask])
 addSelectedTask(newTask.id ?? newTask.Id)
 setTaskDescription('')
 setTaskDeadline('')
 } catch { setError('Network error') } finally { setCreatingTask(false) }
 }

 async function handleGenerateTask() {
 setGenerating(true)
 setError('')
 try {
 const res = await getRandomTaskForUser(userId)
 if (!res.ok) { setError(res.text || 'No random task available for you'); return }
 const randomTask = res.data
 if (!randomTask) { setError('No random task available'); return }
 const idVal = String(randomTask.id ?? randomTask.Id)
 const exists = tasks.some(t => String(t.id ?? t.Id) === idVal)
 if (!exists) setTasks(prev => [...prev, randomTask])
 addSelectedTask(idVal)
 } catch { setError('Network error while generating task') } finally { setGenerating(false) }
 }

 async function handleSubmit(e) {
 e.preventDefault()
 setError('')
 if (!challengeName.trim()) { setError('Challenge name is required'); return }
 if (!selectedTaskIds.length) { setError('Please add at least one task to this challenge'); return }
 setCreating(true)
 try {
 const iso = toIsoForServer(deadline)
 const ids = selectedTaskIds.map(Number)
 const res = await createChallenge(challengeName.trim(), userId, ids, iso, isPrivate)
 if (!res.ok) { setError((res.data && (res.data.error || res.data.message)) || res.text || 'Failed to create challenge'); return }
 const created = res.data
 const createdId = created?.id ?? created?.Id
 if (createdId) { localStorage.setItem('challengeId', String(createdId)); localStorage.setItem('challengeName', challengeName.trim()) }
 navigate(`/challenge-room/${createdId}`)
 } catch { setError('Network error') } finally { setCreating(false) }
 }

 const selectedTasks = selectedTaskIds.map(id => tasks.find(t => String(t.id ?? t.Id) === id)).filter(Boolean)

 return (
 <div style={{ minHeight: 'calc(100vh -70px)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding:40 }}>
 <div className="card" style={{ maxWidth:700, width: '100%' }}>
 <h1 style={{ fontSize:32, marginBottom:24, textAlign: 'center', color: 'white' }}>Create a Challenge</h1>
 <form onSubmit={handleSubmit}>
 <div style={{ marginBottom:20 }}>
 <label>Challenge Name</label>
 <input value={challengeName} onChange={e => setChallengeName(e.target.value)} placeholder="Enter challenge name" disabled={creating} />
 </div>
 <div style={{ marginBottom:20 }}>
 <label>Challenge Tasks</label>
 {selectedTasks.length >0 && (
 <div style={{ marginBottom:12, display: 'flex', flexDirection: 'column', gap:8 }}>
 {selectedTasks.map(t => (
 <div key={t.id ?? t.Id} style={{ display: 'flex', gap:8, alignItems: 'center', background: 'rgba(100,108,255,0.08)', padding:10, borderRadius:6 }}>
 <div style={{ flex:1, color: 'white' }}>{t.description ?? t.Description}</div>
 <button type="button" onClick={() => removeSelectedTask(String(t.id ?? t.Id))} disabled={creating} style={{ background: 'transparent', border: '1px solid #ff6b6b', color: '#ff6b6b' }}>Remove</button>
 </div>
 ))}
 </div>
 )}
 <div style={{ display: 'flex', flexDirection: 'column', gap:8 }}>
 <button type="button" onClick={() => setShowCreateTask(true)} disabled={creating} style={{ width: '100%', background: 'rgba(100,108,255,0.1)', border: '1px solid #646cff', boxShadow: 'none' }}>+ Create Task for Challenge</button>
 <button type="button" onClick={handleGenerateTask} disabled={creating || generating} style={{ width: '100%', background: 'rgba(56,176,0,0.1)', border: '1px solid #38b000', boxShadow: 'none', color: '#38b000' }}>{generating ? 'Generating...' : 'Generate Random Task'}</button>
 </div>
 </div>
 {showCreateTask && (
 <div style={{ background: 'rgba(100,108,255,0.05)', padding:20, borderRadius:8, marginBottom:20 }}>
 <h3 style={{ color: 'white', marginBottom:16, fontSize:18 }}>Create Task</h3>
 <div style={{ marginBottom:16 }}>
 <label>Task Description</label>
 <textarea value={taskDescription} onChange={e => setTaskDescription(e.target.value)} placeholder="Describe what participants need to photograph" rows={3} disabled={creatingTask} />
 </div>
 <div style={{ marginBottom:16 }}>
 <label>Task Deadline (Optional)</label>
 <input type="datetime-local" value={taskDeadline} onChange={e => setTaskDeadline(e.target.value)} disabled={creatingTask} />
 <div style={{ fontSize:12, color: 'rgba(255,255,255,0.6)', marginTop:6 }}>Leave blank for 7 days from now</div>
 </div>
 <div style={{ display: 'flex', gap:12 }}>
 <button onClick={handleCreateTask} disabled={creatingTask || !taskDescription.trim()} style={{ flex:1 }}>{creatingTask ? 'Creating...' : 'Create Task'}</button>
 <button type="button" onClick={() => { setShowCreateTask(false); setTaskDescription(''); setTaskDeadline('') }} disabled={creatingTask} style={{ background: 'transparent', border: '1px solid rgba(255,107,107,0.5)', color: '#ff6b6b', boxShadow: 'none' }}>Done</button>
 </div>
 </div>
 )}
 <div style={{ marginBottom:20 }}>
 <label>Challenge Deadline (Optional)</label>
 <input type="datetime-local" value={deadline} onChange={e => setDeadline(e.target.value)} disabled={creating} />
 <div style={{ fontSize:12, color: 'rgba(255,255,255,0.6)', marginTop:6 }}>Leave blank for 7 days from now</div>
 </div>
 <div style={{ marginBottom:24 }}>
 <label style={{ display: 'flex', alignItems: 'center', gap:12, cursor: 'pointer', userSelect: 'none' }}>
 <input type="checkbox" checked={isPrivate} onChange={e => setIsPrivate(e.target.checked)} disabled={creating} style={{ width: 'auto', cursor: 'pointer' }} />
 <span>Private Challenge (requires join code)</span>
 </label>
 </div>
 {error && <div className="error-message">{error}</div>}
 <button type="submit" disabled={creating || !challengeName.trim() || !selectedTaskIds.length} style={{ width: '100%', padding: '14px', fontSize:16 }}>{creating ? 'Creating Challenge...' : 'Create Challenge'}</button>
 </form>
 <div style={{ marginTop:24, textAlign: 'center', color: 'rgba(255,255,255,0.6)' }}>
 <a onClick={() => navigate('/')} style={{ color: '#646cff', cursor: 'pointer', fontWeight:500 }}>← Back to Home</a>
 </div>
 </div>
 </div>
 )
}