import React from 'react'
import { useNavigate } from 'react-router-dom'

export default function Guide() {
  const navigate = useNavigate()

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      padding: '60px 40px',
      maxWidth: 900,
      margin: '0 auto'
    }}>
      <h1 style={{
        fontSize: 40,
        marginBottom: 16,
        textAlign: 'center',
        color: 'white'
      }}>
        How to Play
      </h1>
      
      <p style={{
        textAlign: 'center',
        color: 'rgba(255, 255, 255, 0.6)',
        marginBottom: 48,
        fontSize: 18
      }}>
        Follow these steps to participate in photo scavenger hunt challenges
      </p>

      <div className="card" style={{ marginBottom: 24 }}>
        <div style={{
          display: 'flex',
          gap: 20,
          alignItems: 'flex-start',
          marginBottom: 24,
          paddingBottom: 24,
          borderBottom: '1px solid rgba(100, 108, 255, 0.2)'
        }}>
          <div style={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 24,
            fontWeight: 700,
            flexShrink: 0
          }}>
            1
          </div>
          <div>
            <h3 style={{ color: 'white', marginBottom: 8, fontSize: 20 }}>
              Create or Join a Challenge
            </h3>
            <p style={{ color: 'rgba(255, 255, 255, 0.7)', lineHeight: 1.6, margin: 0 }}>
              Start by creating a new challenge with a task, or join an existing public challenge. 
              Private challenges require a join code from the creator.
            </p>
          </div>
        </div>

        <div style={{
          display: 'flex',
          gap: 20,
          alignItems: 'flex-start',
          marginBottom: 24,
          paddingBottom: 24,
          borderBottom: '1px solid rgba(100, 108, 255, 0.2)'
        }}>
          <div style={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 24,
            fontWeight: 700,
            flexShrink: 0
          }}>
            2
          </div>
          <div>
            <h3 style={{ color: 'white', marginBottom: 8, fontSize: 20 }}>
              Submission Phase
            </h3>
            <p style={{ color: 'rgba(255, 255, 255, 0.7)', lineHeight: 1.6, margin: 0 }}>
              During this phase, participants upload photos that complete the challenge task. 
              Be creative and have fun! Read the task description carefully before submitting.
            </p>
          </div>
        </div>

        <div style={{
          display: 'flex',
          gap: 20,
          alignItems: 'flex-start',
          marginBottom: 24,
          paddingBottom: 24,
          borderBottom: '1px solid rgba(100, 108, 255, 0.2)'
        }}>
          <div style={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #ffd43b 0%, #ff922b 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 24,
            fontWeight: 700,
            flexShrink: 0
          }}>
            3
          </div>
          <div>
            <h3 style={{ color: 'white', marginBottom: 8, fontSize: 20 }}>
              Voting Phase
            </h3>
            <p style={{ color: 'rgba(255, 255, 255, 0.7)', lineHeight: 1.6, margin: 0 }}>
              The challenge leader advances the challenge to voting. All participants can now vote 
              for their favorite submissions. Choose the most creative, funny, or impressive photos!
            </p>
          </div>
        </div>

        <div style={{
          display: 'flex',
          gap: 20,
          alignItems: 'flex-start',
          marginBottom: 24,
          paddingBottom: 24,
          borderBottom: '1px solid rgba(100, 108, 255, 0.2)'
        }}>
          <div style={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #51cf66 0%, #37b24d 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 24,
            fontWeight: 700,
            flexShrink: 0
          }}>
            4
          </div>
          <div>
            <h3 style={{ color: 'white', marginBottom: 8, fontSize: 20 }}>
              Results & Winners
            </h3>
            <p style={{ color: 'rgba(255, 255, 255, 0.7)', lineHeight: 1.6, margin: 0 }}>
              The challenge leader finalizes the challenge. Winners are determined by vote count. 
              Check the leaderboard to see final results, and view the Hall of Fame for overall top players!
            </p>
          </div>
        </div>

        <div style={{
          display: 'flex',
          gap: 20,
          alignItems: 'flex-start'
        }}>
          <div style={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #748ffc 0%, #5c7cfa 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 24,
            fontWeight: 700,
            flexShrink: 0
          }}>
            5
          </div>
          <div>
            <h3 style={{ color: 'white', marginBottom: 8, fontSize: 20 }}>
              Join More Challenges
            </h3>
            <p style={{ color: 'rgba(255, 255, 255, 0.7)', lineHeight: 1.6, margin: 0 }}>
              You can participate in multiple challenges simultaneously. View all your active 
              challenges in the Profile menu under "My Challenges".
            </p>
          </div>
        </div>
      </div>

      <div className="card" style={{
        background: 'rgba(100, 108, 255, 0.1)',
        border: '1px solid rgba(100, 108, 255, 0.3)'
      }}>
        <button
          onClick={() => navigate('/')}
          style={{
            padding: '14px 32px',
            fontSize: 16
          }}
        >
          Got it! Let's Go
        </button>
      </div>
    </div>
  )
}