import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import MyTasks from './pages/MyTasks'
import SubmitPhoto from './pages/SubmitPhoto'
import HallOfFame from './pages/HallOfFame'
import TaskCreate from './pages/TaskCreate'
import Logout from './pages/Logout'
import JoinChallenge from './pages/JoinChallenge'
import CreateChallenge from './pages/CreateChallenge'
import Vote from './pages/Vote'
import Leaderboards from './pages/Leaderboards'
import LeaveChallenge from './pages/LeaveChallenge'
import { useActiveUsers } from './context/ActiveUsersContext.jsx'

export default function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(
        typeof window !== 'undefined' && !!localStorage.getItem('userId')
    )
    const { activeUsers } = useActiveUsers()

    useEffect(() => {
        const updateAuth = () => setIsAuthenticated(!!localStorage.getItem('userId'))
        window.addEventListener('auth-changed', updateAuth)
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
                    <nav style={{ marginTop: 12, display: 'flex', alignItems: 'center', gap: 12, flexWrap: 'wrap' }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
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
                                    <Link to="/challenges/create" style={{ marginRight: 12 }}>Create Challenge</Link>
                                    <Link to="/challenges/join" style={{ marginRight: 12 }}>Join Challenge</Link>
                                    <Link to="/challenges/leave" style={{ marginRight: 12 }}>Leave Challenge</Link>
                                    <Link to="/submit" style={{ marginRight: 12 }}>Submit Photo</Link>
                                    <Link to="/vote" style={{ marginRight: 12 }}>Vote</Link>
                                    <Link to="/leaderboards" style={{ marginRight: 12 }}>Leaderboards</Link>
                                    <Link to="/halloffame" style={{ marginLeft: 12 }}>Hall of Fame</Link>
                                    <Link to="/logout" style={{ marginLeft: 12 }}>Logout</Link>
                                </>
                            )}
                        </div>

                        {/* Always show active users count */}
                        <div style={{ marginLeft: 'auto', background: '#d1d1d1', border: '1px solid #b0b0b0', padding: '6px 10px', borderRadius: 6, fontSize: 13, color: '#222' }} aria-label={`Active users: ${activeUsers.length}`}>
                            Active users: <strong style={{ color: '#000' }}>{activeUsers.length}</strong>
                            <span style={{ marginLeft: 8, color: '#222' }}>
                                {activeUsers.length > 0 ? `[${activeUsers.slice(0, 6).join(', ')}${activeUsers.length > 6 ? ', …' : ''}]` : '(none)'}
                            </span>
                        </div>
                    </nav>
                </header>

                <main style={{ marginTop: 20 }}>
                    <Routes>
                        <Route path="/" element={<Home />} />
                        <Route path="/mytasks" element={<MyTasks />} />
                        <Route path="/submit" element={<SubmitPhoto />} />
                        <Route path="/halloffame" element={<HallOfFame />} />
                        <Route path="/login" element={<Login />} />
                        <Route path="/register" element={<Register />} />
                        <Route path="/create-task" element={<TaskCreate />} />
                        <Route path="/logout" element={<Logout />} />
                        <Route path="/challenges/join" element={<JoinChallenge />} />
                        <Route path="/challenges/create" element={<CreateChallenge />} />
                        <Route path="/challenges/leave" element={<LeaveChallenge />} />
                        <Route path="/vote" element={<Vote />} />
                        <Route path="/leaderboards" element={<Leaderboards />} />
                    </Routes>
                </main>
            </div>
        </BrowserRouter>
    )
}