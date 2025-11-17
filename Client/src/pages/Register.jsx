import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../services/api.js'

export default function Register() {
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [age, setAge] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)
  const navigate = useNavigate()

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (!username || !username.trim()) {
      setError('Username is required')
      return
    }

    if (!email || !email.trim()) {
      setError('Email is required')
      return
    }

    if (!age || Number(age) <= 0) {
      setError('Age is required')
      return
    }

    if (!password || password.length < 6) {
      setError('Password must be at least 6 characters')
      return
    }

    if (password !== confirmPassword) {
      setError('Passwords do not match')
      return
    }

    const res = await register(email, password, username, Number(age))
    if (!res.ok) {
      const errMsg = (res.data && (res.data.message || res.data.error)) || res.text || `Error ${res.status}`
      setError(errMsg)
      return
    }

    setSuccess(true)
  }

  if (success) {
    return (
      <div style={{
        minHeight: 'calc(100vh - 70px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 40
      }}>
        <div className="card" style={{ maxWidth: 500, width: '100%', textAlign: 'center' }}>
          <div style={{
            width: 80,
            height: 80,
            borderRadius: '50%',
            background: 'rgba(81, 207, 102, 0.2)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            fontSize: 40
          }}>
            ✓
          </div>
          <h2 style={{ color: '#51cf66', marginBottom: 16 }}>Registration Successful!</h2>
          <p style={{ color: 'rgba(255, 255, 255, 0.7)', marginBottom: 32 }}>
            Your account has been created. You can now log in.
          </p>
          <button
            onClick={() => navigate('/login')}
            style={{ width: '100%', padding: '14px 24px', fontSize: 16 }}
          >
            Go to Login
          </button>
        </div>
      </div>
    )
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
          Welcome to Photo Scavenger Hunt!
        </h1>
        <p style={{
          textAlign: 'center',
          color: 'rgba(255, 255, 255, 0.6)',
          marginBottom: 32
        }}>
          Create your account to get started
        </p>

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: 20 }}>
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="your.email@example.com"
            />
          </div>

          <div style={{ marginBottom: 20 }}>
            <label>Username</label>
            <input
              value={username}
              onChange={e => setUsername(e.target.value)}
              placeholder="Choose a username"
            />
          </div>

          <div style={{ marginBottom: 20 }}>
            <label>Age</label>
            <input
              type="number"
              value={age}
              onChange={e => setAge(e.target.value)}
              placeholder="Enter your age"
              min="1"
              max="120"
            />
          </div>

          <div style={{ marginBottom: 20 }}>
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Create a password"
            />
          </div>

          <div style={{ marginBottom: 24 }}>
            <label>Confirm Password</label>
            <input
              type="password"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              placeholder="Re-enter your password"
            />
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <button type="submit" style={{ width: '100%', padding: '14px 24px', fontSize: 16 }}>
            Register
          </button>
        </form>

        <div style={{
          marginTop: 24,
          textAlign: 'center',
          color: 'rgba(255, 255, 255, 0.6)'
        }}>
          Already have an account?{' '}
          <a
            onClick={() => navigate('/login')}
            style={{ color: '#646cff', cursor: 'pointer', fontWeight: 500 }}
          >
            Log in
          </a>
        </div>
      </div>
    </div>
  )
}