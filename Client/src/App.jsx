import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import MyTasks from './pages/MyTasks'
import SubmitPhoto from './pages/SubmitPhoto'
import Leaderboard from './pages/Leaderboard'
import TaskCreate from './pages/TaskCreate'
import Logout from './pages/Logout'
import JoinHub from './pages/JoinHub'

export default function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(
    typeof window !== 'undefined' && !!localStorage.getItem('userId')
  )

  useEffect(() => {
    const updateAuth = () => setIsAuthenticated(!!localStorage.getItem('userId'))
    // Custom event for same-tab auth changes
    window.addEventListener('auth-changed', updateAuth)
    // Storage event for cross-tab updates
    window.addEventListener('storage', updateAuth)
    return () => {
      window.removeEventListener('auth-changed', updateAuth)
      window.removeEventListener('storage', updateAuth)
    }
  }, [])

  return (
    <BrowserRouter>
      <div style={{ padding: 24, fontFamily: 'system-ui, Arial, sans-serif' }}>
        <header>
          <h1>Photo Scavenger Hunt</h1>
          <nav style={{ marginTop: 12 }}>
            <Link to="/" style={{ marginRight: 12 }}>Home</Link>

            {!isAuthenticated && (
              <>
                <Link to="/register" style={{ marginRight: 12 }}>Register</Link>
                <Link to="/login" style={{ marginRight: 12 }}>Login</Link>
              </>
            )}

            {isAuthenticated && (
              <>
                <Link to="/create-task" style={{ marginRight: 12 }}>Create Task</Link>
                <Link to="/mytasks" style={{ marginRight: 12 }}>My Tasks</Link>
                <Link to="/submit" style={{ marginRight: 12 }}>Submit Photo</Link>
                <Link to="/leaderboard" style={{ marginRight: 12 }}>Leaderboard</Link>
                <Link to="/hubs/join" style={{ marginRight: 12 }}>Join Hub</Link>
                <Link to="/logout" style={{ marginLeft: 12 }}>Logout</Link>
              </>
            )}
          </nav>
        </header>

        <main style={{ marginTop: 20 }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/mytasks" element={<MyTasks />} />
            <Route path="/submit" element={<SubmitPhoto />} />
            <Route path="/leaderboard" element={<Leaderboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/create-task" element={<TaskCreate />} />
            <Route path="/logout" element={<Logout />} />
            <Route path="/hubs/join" element={<JoinHub />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}