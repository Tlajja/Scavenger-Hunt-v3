import React, { useState } from 'react'
import { submitPhoto } from '../services/api.js'

export default function SubmitPhoto() {
  const [taskId, setTaskId] = useState('')
  const [userId, setUserId] = useState(localStorage.getItem('userId') || '')
  const [photoUrl, setPhotoUrl] = useState('')
  const [message, setMessage] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('Submitting...')
    try {
      const res = await submitPhoto(Number(taskId), Number(userId), photoUrl)
      if (!res.ok) {
        const text = await res.text()
        setMessage(`Error: ${text}`)
        return
      }
      setMessage('Submission created.')
      setPhotoUrl('')
    } catch (err) {
      setMessage('Network error')
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 480 }}>
      <h1>Not fixed yet</h1>
      <h2>Submit Photo</h2>
      <label>
        Task ID
        <input value={taskId} onChange={e => setTaskId(e.target.value)} />
      </label>
      <label>
        User ID
        <input value={userId} onChange={e => setUserId(e.target.value)} />
      </label>
      <label>
        Photo URL
        <input value={photoUrl} onChange={e => setPhotoUrl(e.target.value)} />
      </label>
      <button type="submit">Submit</button>
      <div>{message}</div>
    </form>
  )
}