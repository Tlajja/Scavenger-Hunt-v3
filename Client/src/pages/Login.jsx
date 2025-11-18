import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login } from '../services/api.js'

export default function Login() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const navigate = useNavigate()

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (!username || !username.trim()) {
      setError('Username is required')
      return
    }

    if (!password) {
      setError('Password is required')
      return
    }

    const res = await login(username, password)

    if (res instanceof Response) {
      const text = await res.text()
      let parsed = null
      try { parsed = text ? JSON.parse(text) : null } catch {}
      if (!res.ok) {
        setError(`Invalid username or password`)
        return
      }
      const userId = parsed?.userId
      const name = parsed?.username ?? username
      if (!userId) {
        setError('Unexpected response from server')
        return
      }
      // Clear any old challenge data from previous user
      localStorage.removeItem('challengeId')
      localStorage.removeItem('challengeName')
      
      localStorage.setItem('userId', String(userId))
      localStorage.setItem('username', name)
      window.dispatchEvent(new Event('auth-changed'))
      navigate('/')
      return
    }

    if (!res.ok) {
      setError('Invalid username or password')
      return
    }

    const payload = res.data ?? (res.text ? (() => { try { return JSON.parse(res.text) } catch { return res.text } })() : null)
    const userId = payload?.userId
    const name = payload?.username ?? username

    if (!userId) {
      setError('Unexpected response from server')
      return
    }

    // Clear any old challenge data from previous user
    localStorage.removeItem('challengeId')
    localStorage.removeItem('challengeName')
    
    localStorage.setItem('userId', String(userId))
    localStorage.setItem('username', name)
    window.dispatchEvent(new Event('auth-changed'))
    navigate('/')
  }

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: 40
    }}>
      <div className="card" style={{ maxWidth: 500, width: '100%' }}>
        <h1 style={{ 
          textAlign: 'center', 
          marginBottom: 16,
          fontSize: 32,
          color: 'white'
        }}>
          Welcome Back!
        </h1>
        <p style={{
          textAlign: 'center',
          color: 'rgba(255, 255, 255, 0.6)',
          marginBottom: 32
        }}>
          Log in to continue your adventure
        </p>

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: 20 }}>
            <label>Username</label>
            <input
              value={username}
              onChange={e => setUsername(e.target.value)}
              placeholder="Enter your username"
              autoComplete="username"
            />
          </div>

          <div style={{ marginBottom: 24 }}>
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Enter your password"
              autoComplete="current-password"
            />
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <button type="submit" style={{ width: '100%', padding: '14px 24px', fontSize: 16 }}>
            Log In
          </button>
        </form>

        <div style={{
          marginTop: 24,
          textAlign: 'center',
          color: 'rgba(255, 255, 255, 0.6)'
        }}>
          Don't have an account?{' '}
          <a
            onClick={() => navigate('/register')}
            style={{ color: '#646cff', cursor: 'pointer', fontWeight: 500 }}
          >
            Register
          </a>
        </div>
      </div>
    </div>
  )
}