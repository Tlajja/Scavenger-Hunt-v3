import React, { useState } from 'react'
import { register } from '../services/api.js'

export default function Register() {
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [age, setAge] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()

    if (!username || !username.trim()) {
      setMessage('Username is required.')
      return
    }

    if (!age || Number(age) <= 0) {
      setMessage('Age is required.')
      return
    }

    setMessage('Registering...')
    const res = await register(email, password, username, Number(age))
    if (!res.ok) {
      // prefer structured backend message, fallback to text/status
      const errMsg = (res.data && (res.data.message || res.data.error)) || res.text || `Error ${res.status}`
      setMessage(`Error: ${errMsg}`)
      return
    }

    const userId = res.data?.userId ?? ''

    if (!userId) {
      setMessage('Registration succeeded but server did not return a userId.')
      return
    }

    localStorage.setItem('userId', String(userId))
    localStorage.setItem('username', username)
    setMessage(res.data?.message || 'Registered and username set.')
    setEmail('')
    setUsername('')
    setPassword('')
    setAge('')
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 420 }}>
      <h2>Register</h2>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Email</label>
        <input 
            value={email} 
            onChange={e => setEmail(e.target.value)}
            style={{ width: '100%', padding: 8, boxSizing: 'border-box' }} 
        />
      </div>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Username</label>
        <input
          value={username}
          onChange={e => setUsername(e.target.value)}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        />
      </div>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Age</label>
        <input 
          type="number" 
          value={age} 
          onChange={e => setAge(e.target.value)} 
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }} />
      </div>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Password</label>
        <input 
            type="password" 
            value={password} 
            onChange={e => setPassword(e.target.value)} 
            style={{ width: '100%', padding: 8, boxSizing: 'border-box' }} 
        />
      </div>
      <button type="submit">Register</button>
      <div style={{ marginTop: 10 }}>{message}</div>
    </form>
  )
}