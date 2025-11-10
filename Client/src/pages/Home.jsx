import React, { useEffect } from 'react'
import { Link } from 'react-router-dom'

export default function Home() {
  useEffect(() => {
    console.log('Home mounted')
  }, [])
  return (
    <div>
      <h2>Home</h2>
      <p>Frontend running.</p>

      <h3>Quick guide — how a challenge progresses</h3>
      <ol>
        <li>
          Create a challenge: choose a task and create the challenge.
          <br />
          <Link to="/challenges/create">Create Challenge</Link>
        </li>
        <li>
          Share the Join Code so participants can join.
          <br />
          <Link to="/challenges/join">Join Challenge</Link>
        </li>
        <li>
          Participants upload photos during the submission stage.
          <br />
          <Link to="/submit">Submit Photo</Link>
        </li>
        <li>
          Leader advances the challenge into voting (use "Advance Stage" on Create Challenge page).
        </li>
        <li>
          Participants vote on submissions.
          <br />
          <Link to="/vote">Vote</Link>
        </li>
        <li>
          Leader finalizes the challenge (advance again) — see results on Leaderboards (challenge results) and Hall of Fame (total wins).
          <br />
          <Link to="/leaderboards">Leaderboards</Link> · <Link to="/halloffame">Hall of Fame</Link>
        </li>
        <li>
          To leave a challenge, use the dedicated page:
          <br />
          <Link to="/challenges/leave">Leave Challenge</Link>
        </li>
      </ol>

      <p style={{ fontSize: 13, color: '#666' }}>
        Note: stage transitions are currently manual (leader advances stages). Timestamps/automatic transitions are planned to be created later.
      </p>
    </div>
  )
}