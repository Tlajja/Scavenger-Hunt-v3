import React, { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'

export default function Logout() {
  const [done, setDone] = useState(false)
  const [isLoggedIn, setIsLoggedIn] = useState(false)

  useEffect(() => {
    const token = localStorage.getItem('authToken') || localStorage.getItem('access_token')
    const userId = localStorage.getItem('userId')
    setIsLoggedIn(!!(token || userId))
  }, [])

  function handleLogout() {
    try {
      localStorage.removeItem('userId')
      localStorage.removeItem('username')
      localStorage.removeItem('authToken')
      localStorage.removeItem('access_token')
      // Notify app about auth change
      window.dispatchEvent(new Event('auth-changed'))
    } catch {}
    setIsLoggedIn(false)
    setDone(true)
  }

  return (
    <div>
      {!done ? (
        <>
          <h2>Would you like to log out?</h2>
          <p>Click the button below to end your session.</p>
          {!isLoggedIn && (
            <p style={{ color: '#666' }}>
              You are not logged in. <Link to="/login">Go to login</Link>
            </p>
          )}
          <button onClick={handleLogout} style={{ padding: '8px 12px' }} disabled={!isLoggedIn}>Log out</button>
        </>
      ) : (
        <>
          <h2>Logout successful</h2>
          <p>Your session has been cleared.</p>
          <p>
            <Link to="/login">Go to login</Link> or <Link to="/">Return home</Link>
          </p>
        </>
      )}
    </div>
  )
}