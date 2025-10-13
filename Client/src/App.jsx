import React from 'react'

export default function App() {
  return (
    <div style={{ padding: 24, fontFamily: 'system-ui, Arial, sans-serif' }}>
      <header>
        <h1>Photo Scavenger Hunt</h1>
      </header>

      <main>
        <p>
          Welcome — frontend is running. Use the API via <a href="/swagger">Swagger</a> or call /api endpoints.
        </p>
        <nav>
          <ul>
            <li>Home</li>
            <li>Tasks</li>
            <li>Submit Photo</li>
            <li>Leaderboard</li>
            <li>Profile / Auth</li>
          </ul>
        </nav>
      </main>
    </div>
  )
}