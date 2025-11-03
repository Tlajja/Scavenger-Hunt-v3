import React, { useState } from 'react'
import { login } from '../services/api.js'

export default function Login() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('Logging in...')
    const res = await login(username, password)

    // handle case api.js returned a raw Response (old version) or the safeFetch object
    if (res instanceof Response) {
      // fallback for older api shape
      const text = await res.text()
      let parsed = null
      try { parsed = text ? JSON.parse(text) : null } catch {}
      if (!res.ok) {
        setMessage(`Error: ${(parsed && (parsed.message || parsed.error)) || text || res.statusText}`)
        return
      }
      const userId = parsed?.userId
      const name = parsed?.username ?? username
      if (!userId) {
        setMessage('Unexpected response from server.')
        return
      }
      localStorage.setItem('userId', String(userId))
      localStorage.setItem('username', name)
      window.dispatchEvent(new Event('auth-changed'))
      setMessage(parsed?.message || 'Login successful.')
      return
    }

    // expected shape from safeFetch: { ok, status, data, text }
    if (!res.ok) {
      const errMsg = (res.data && (res.data.message || res.data.error)) || res.text || `Error ${res.status}`
      setMessage(`Error: ${errMsg}`)
      return
    }

    const payload = res.data ?? (res.text ? (() => { try { return JSON.parse(res.text) } catch { return res.text } })() : null)
    const userId = payload?.userId
    const name = payload?.username ?? username

    if (!userId) {
      setMessage(payload ?? 'Unexpected response from server.')
      return
    }

    localStorage.setItem('userId', String(userId))
    localStorage.setItem('username', name)
    window.dispatchEvent(new Event('auth-changed'))
    setMessage(payload?.message || 'Login successful.')
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 420 }}>
      <h2>Login</h2>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Username</label>
        <input
          value={username}
          onChange={e => setUsername(e.target.value)}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        />
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
      <button type="submit">Login</button>
      <div style={{ marginTop: 10 }}>{message}</div>
    </form>
  )
}