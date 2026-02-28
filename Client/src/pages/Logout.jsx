import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'

export default function Logout() {
  const [done, setDone] = useState(false)
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    const userId = localStorage.getItem('userId')
    setIsLoggedIn(!!userId)
  }, [])

  function handleLogout() {
    try {
      localStorage.removeItem('userId')
      localStorage.removeItem('username')
      localStorage.removeItem('challengeId')
      localStorage.removeItem('challengeName')
      window.dispatchEvent(new Event('auth-changed'))
      localStorage.setItem('__logout_ts', String(Date.now()))
    } catch {}
    setIsLoggedIn(false)
    setDone(true)
  }

  if (done) {
    return (
      <div style={{
        minHeight: 'calc(100vh - 70px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 40
      }}>
        <div className="card" style={{
          maxWidth: 500,
          width: '100%',
          textAlign: 'center'
        }}>
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
            👋
          </div>
          <h2 style={{ color: 'white', marginBottom: 16 }}>
            Logged Out Successfully
          </h2>
          <p style={{
            color: 'rgba(255, 255, 255, 0.7)',
            marginBottom: 32
          }}>
            Your session has been cleared. See you next time!
          </p>
          <div style={{ display: 'flex', gap: 12, justifyContent: 'center' }}>
            <button
              onClick={() => navigate('/login')}
              style={{ padding: '12px 24px' }}
            >
              Log In Again
            </button>
            <button
              onClick={() => navigate('/register')}
              style={{
                background: 'transparent',
                border: '1px solid #646cff',
                padding: '12px 24px'
              }}
            >
              Create Account
            </button>
          </div>
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
      <div className="card" style={{
        maxWidth: 500,
        width: '100%',
        textAlign: 'center'
      }}>
        <h2 style={{ color: 'white', marginBottom: 16 }}>
          Log Out?
        </h2>
        <p style={{
          color: 'rgba(255, 255, 255, 0.7)',
          marginBottom: 32
        }}>
          Are you sure you want to end your session?
        </p>
        {!isLoggedIn && (
          <div className="error-message" style={{ marginBottom: 24 }}>
            You are not logged in
          </div>
        )}
        <div style={{ display: 'flex', gap: 12, justifyContent: 'center' }}>
          <button
            onClick={handleLogout}
            disabled={!isLoggedIn}
            style={{
              background: '#ff6b6b',
              padding: '12px 24px'
            }}
          >
            Yes, Log Out
          </button>
          <button
            onClick={() => navigate('/')}
            style={{
              background: 'transparent',
              border: '1px solid #646cff',
              padding: '12px 24px'
            }}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  )
}