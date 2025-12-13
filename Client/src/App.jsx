import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, Link, useNavigate } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import SubmitPhoto from './pages/SubmitPhoto'
import HallOfFame from './pages/HallOfFame'
import TaskCreate from './pages/TaskCreate'
import Logout from './pages/Logout'
import JoinChallenge from './pages/JoinChallenge'
import CreateChallenge from './pages/CreateChallenge'
import Vote from './pages/Vote'
import Leaderboards from './pages/Leaderboards'
import LeaveChallenge from './pages/LeaveChallenge'
import MyChallenges from './pages/MyChallenges'
import ChallengeRoom from './pages/ChallengeRoom'
import Guide from './pages/Guide'
import { useActiveUsers } from './context/ActiveUsersContext.jsx'
import { deleteAccount } from './services/api.js'

function Header() {
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('userId'))
  const [showProfileMenu, setShowProfileMenu] = useState(false)
  const [showGuide, setShowGuide] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const navigate = useNavigate()
  const { activeUsersCount } = useActiveUsers()

  useEffect(() => {
    const updateAuth = () => setIsAuthenticated(!!localStorage.getItem('userId'))
    window.addEventListener('auth-changed', updateAuth)
    window.addEventListener('storage', updateAuth)
    return () => {
      window.removeEventListener('auth-changed', updateAuth)
      window.removeEventListener('storage', updateAuth)
    }
  }, [])

  useEffect(() => {
    function handleClickOutside(e) {
      if (showProfileMenu && !e.target.closest('.profile-menu-container')) {
        setShowProfileMenu(false)
      }
      if (showGuide && !e.target.closest('.guide-menu-container')) {
        setShowGuide(false)
      }
    }
    document.addEventListener('click', handleClickOutside)
    return () => document.removeEventListener('click', handleClickOutside)
  }, [showProfileMenu, showGuide])

  async function handleDeleteAccount() {
    const userId = localStorage.getItem('userId')
    if (!userId) return

    const confirmed = window.confirm(
      'Are you sure you want to delete your account? This action cannot be undone and will permanently delete all your data.'
    )
    
    if (!confirmed) return

    setDeleting(true)
    setShowProfileMenu(false)

    try {
      const res = await deleteAccount(userId)
      if (res.ok) {
        // Clear all user data
        localStorage.removeItem('userId')
        localStorage.removeItem('username')
        localStorage.removeItem('challengeId')
        localStorage.removeItem('challengeName')
        window.dispatchEvent(new Event('auth-changed'))
        navigate('/login')
      } else {
        const errorMsg = res.data?.message || res.text || 'Failed to delete account'
        alert(`Error: ${errorMsg}`)
      }
    } catch (error) {
      alert('An error occurred while deleting your account. Please try again.')
    } finally {
      setDeleting(false)
    }
  }

  return (
    <header style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      height: 70,
      background: 'rgba(26, 26, 46, 0.95)',
      backdropFilter: 'blur(10px)',
      borderBottom: '1px solid rgba(100, 108, 255, 0.2)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 40px',
      zIndex: 1000,
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.1)'
    }}>
      <Link to="/" style={{
        fontSize: 24,
        fontWeight: 700,
        color: '#646cff',
        textDecoration: 'none',
        letterSpacing: '-0.5px'
      }}>
        Photo Scavenger Hunt
      </Link>

      <div style={{ display: 'flex', gap: 24, alignItems: 'center' }}>
        {isAuthenticated && (
          <>
            <div className="guide-menu-container" style={{ position: 'relative' }}>
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  setShowGuide(!showGuide)
                  setShowProfileMenu(false)
                }}
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: 'rgba(255, 255, 255, 0.8)',
                  fontSize: 24,
                  cursor: 'pointer',
                  padding: 8,
                  display: 'flex',
                  alignItems: 'center',
                  boxShadow: 'none'
                }}
                title="How to play"
              >
                ?
              </button>
              {showGuide && (
                <div style={{
                  position: 'absolute',
                  top: '100%',
                  right: 0,
                  marginTop: 8,
                  background: 'rgba(42, 42, 62, 0.98)',
                  borderRadius: 8,
                  boxShadow: '0 8px 24px rgba(0, 0, 0, 0.3)',
                  minWidth: 200,
                  overflow: 'hidden',
                  border: '1px solid rgba(100, 108, 255, 0.2)'
                }}>
                  <button
                    onClick={() => {
                      navigate('/guide')
                      setShowGuide(false)
                    }}
                    style={{
                      width: '100%',
                      background: 'transparent',
                      border: 'none',
                      color: 'white',
                      padding: '12px 20px',
                      textAlign: 'left',
                      cursor: 'pointer',
                      fontSize: 15,
                      transition: 'background 0.2s',
                      boxShadow: 'none'
                    }}
                    onMouseEnter={(e) => e.target.style.background = 'rgba(100, 108, 255, 0.2)'}
                    onMouseLeave={(e) => e.target.style.background = 'transparent'}
                  >
                    How to Play
                  </button>
                </div>
              )}
            </div>

            <div className="profile-menu-container" style={{ position: 'relative' }}>
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  setShowProfileMenu(!showProfileMenu)
                  setShowGuide(false)
                }}
                style={{
                  background: 'rgba(100, 108, 255, 0.1)',
                  border: '1px solid rgba(100, 108, 255, 0.3)',
                  color: 'white',
                  padding: '10px 20px',
                  borderRadius: 8,
                  cursor: 'pointer',
                  fontSize: 15,
                  fontWeight: 500,
                  boxShadow: 'none'
                }}
              >
                Profile
              </button>
              {showProfileMenu && (
                <div style={{
                  position: 'absolute',
                  top: '100%',
                  right: 0,
                  marginTop: 8,
                  background: 'rgba(42, 42, 62, 0.98)',
                  borderRadius: 8,
                  boxShadow: '0 8px 24px rgba(0, 0, 0, 0.3)',
                  minWidth: 200,
                  overflow: 'hidden',
                  border: '1px solid rgba(100, 108, 255, 0.2)'
                }}>
                  <button
                    onClick={() => {
                      navigate('/my-challenges')
                      setShowProfileMenu(false)
                    }}
                    style={{
                      width: '100%',
                      background: 'transparent',
                      border: 'none',
                      color: 'white',
                      padding: '12px 20px',
                      textAlign: 'left',
                      cursor: 'pointer',
                      fontSize: 15,
                      transition: 'background 0.2s',
                      boxShadow: 'none'
                    }}
                    onMouseEnter={(e) => e.target.style.background = 'rgba(100, 108, 255, 0.2)'}
                    onMouseLeave={(e) => e.target.style.background = 'transparent'}
                  >
                    My Challenges
                  </button>
                  <button
                    onClick={() => {
                      navigate('/logout')
                      setShowProfileMenu(false)
                    }}
                    style={{
                      width: '100%',
                      background: 'transparent',
                      border: 'none',
                      color: '#ff6b6b',
                      padding: '12px 20px',
                      textAlign: 'left',
                      cursor: 'pointer',
                      fontSize: 15,
                      transition: 'background 0.2s',
                      boxShadow: 'none'
                    }}
                    onMouseEnter={(e) => e.target.style.background = 'rgba(255, 107, 107, 0.1)'}
                    onMouseLeave={(e) => e.target.style.background = 'transparent'}
                  >
                    Log Out
                  </button>
                  <div style={{
                    width: '100%',
                    height: 1,
                    background: 'rgba(255, 255, 255, 0.1)',
                    margin: '4px 0'
                  }} />
                  <button
                    onClick={handleDeleteAccount}
                    disabled={deleting}
                    style={{
                      width: '100%',
                      background: 'transparent',
                      border: 'none',
                      color: '#ff6b6b',
                      padding: '12px 20px',
                      textAlign: 'left',
                      cursor: deleting ? 'not-allowed' : 'pointer',
                      fontSize: 15,
                      opacity: deleting ? 0.6 : 1,
                      transition: 'background 0.2s',
                      boxShadow: 'none'
                    }}
                    onMouseEnter={(e) => {
                      if (!deleting) e.target.style.background = 'rgba(255, 107, 107, 0.1)'
                    }}
                    onMouseLeave={(e) => e.target.style.background = 'transparent'}
                  >
                    {deleting ? 'Deleting...' : 'Delete Account'}
                  </button>
                </div>
              )}
            </div>

            <div style={{ marginLeft: 16, padding: '4px 10px', background: '#2a2a3e', borderRadius: 6, color: '#fff', fontSize: 13 }}>
              🟢 {activeUsersCount} online
            </div>
          </>
        )}
      </div>
    </header>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <div style={{ minHeight: '100vh' }}>
        <Header />
        <main style={{ paddingTop: 70 }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/logout" element={<Logout />} />
            <Route path="/my-challenges" element={<MyChallenges />} />
            <Route path="/challenge-room/:challengeId" element={<ChallengeRoom />} />
            <Route path="/create-challenge" element={<CreateChallenge />} />
            <Route path="/join-challenge" element={<JoinChallenge />} />
            <Route path="/guide" element={<Guide />} />
            <Route path="/submit" element={<SubmitPhoto />} />
            <Route path="/vote" element={<Vote />} />
            <Route path="/leaderboards" element={<Leaderboards />} />
            <Route path="/halloffame" element={<HallOfFame />} />
            <Route path="/create-task" element={<TaskCreate />} />
            <Route path="/challenges/join" element={<JoinChallenge />} />
            <Route path="/challenges/create" element={<CreateChallenge />} />
            <Route path="/challenges/leave" element={<LeaveChallenge />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}
