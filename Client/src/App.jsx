import React from 'react'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import MyTasks from './pages/MyTasks'
import SubmitPhoto from './pages/SubmitPhoto'
import Leaderboard from './pages/Leaderboard'
import TaskCreate from './pages/TaskCreate'
import Logout from './pages/Logout'

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
            <Link to="/create-task" style={{ marginRight: 12 }}>Create Task</Link>
            <Link to="/mytasks" style={{ marginRight: 12 }}>My Tasks</Link>
            <Link to="/submit" style={{ marginRight: 12 }}>Submit Photo</Link>
            <Link to="/leaderboard">Leaderboard</Link>
            <Link to="/logout" style={{ marginLeft: 12 }}>Logout</Link>
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
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}