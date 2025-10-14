import React from 'react'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import Tasks from './pages/Tasks'
import SubmitPhoto from './pages/SubmitPhoto'
import Leaderboard from './pages/Leaderboard'

export default function App() {
  return (
    <BrowserRouter>
      <div style={{ padding: 24, fontFamily: 'system-ui, Arial, sans-serif' }}>
        <header>
          <h1>Photo Scavenger Hunt</h1>
          <nav style={{ marginTop: 12 }}>
            <Link to="/" style={{ marginRight: 12 }}>Home</Link>
            <Link to="/register" style={{ marginRight: 12 }}>Register</Link>
            <Link to="/login" style={{ marginRight: 12 }}>Login</Link>
            <Link to="/tasks" style={{ marginRight: 12 }}>Tasks</Link>
            <Link to="/submit" style={{ marginRight: 12 }}>Submit Photo</Link>
            <Link to="/leaderboard">Leaderboard</Link>
          </nav>
        </header>

        <main style={{ marginTop: 20 }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/tasks" element={<Tasks />} />
            <Route path="/submit" element={<SubmitPhoto />} />
            <Route path="/leaderboard" element={<Leaderboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}