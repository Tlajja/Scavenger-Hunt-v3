import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getHallOfFame } from '../services/api.js'

export default function Home() {
  const [hallOfFame, setHallOfFame] = useState([])
  const [loading, setLoading] = useState(true)
  const navigate = useNavigate()
  const isAuthenticated = !!localStorage.getItem('userId')

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/register')
      return
    }

    let mounted = true
    async function load() {
      try {
        const res = await getHallOfFame(10)
        let data = null

        if (res instanceof Response) {
          if (res.ok) data = await res.json()
        } else {
          if (res.ok) data = res.data
        }

        if (mounted) setHallOfFame(Array.isArray(data) ? data : [])
      } catch {}
      finally {
        if (mounted) setLoading(false)
      }
    }

    load()
    return () => { mounted = false }
  }, [isAuthenticated, navigate])

  if (!isAuthenticated) return null

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      padding: '60px 40px'
    }}>
      <h1 style={{
        fontSize: 48,
        marginBottom: 48,
        textAlign: 'center',
        color: 'white'
      }}>
        What would you like to do?
      </h1>

      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(2, 1fr)',
        gap: 32,
        maxWidth: 800,
        width: '100%',
        marginBottom: 80
      }}>
        <button
          onClick={() => navigate('/create-challenge')}
          style={{
            height: 200,
            fontSize: 24,
            fontWeight: 600,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            border: 'none',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 16,
            boxShadow: '0 8px 32px rgba(102, 126, 234, 0.4)'
          }}
        >
          <div style={{ fontSize: 48 }}>+</div>
          <div>Create Challenge</div>
        </button>

        <button
          onClick={() => navigate('/join-challenge')}
          style={{
            height: 200,
            fontSize: 24,
            fontWeight: 600,
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            border: 'none',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 16,
            boxShadow: '0 8px 32px rgba(240, 147, 251, 0.4)'
          }}
        >
          <div style={{ fontSize: 48 }}>🔍</div>
          <div>Join Challenge</div>
        </button>
      </div>

      <div className="card" style={{
        maxWidth: 900,
        width: '100%'
      }}>
        <h2 style={{
          fontSize: 28,
          marginBottom: 24,
          textAlign: 'center',
          color: 'white'
        }}>
          🏆 Hall of Fame
        </h2>

        {loading ? (
          <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.6)' }}>
            Loading...
          </div>
        ) : hallOfFame.length === 0 ? (
          <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.6)' }}>
            No winners yet. Be the first!
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{
              width: '100%',
              borderCollapse: 'separate',
              borderSpacing: '0 8px'
            }}>
              <thead>
                <tr>
                  <th style={{
                    textAlign: 'left',
                    padding: '12px 16px',
                    color: 'rgba(255, 255, 255, 0.7)',
                    fontWeight: 500,
                    fontSize: 14
                  }}>
                    Rank
                  </th>
                  <th style={{
                    textAlign: 'left',
                    padding: '12px 16px',
                    color: 'rgba(255, 255, 255, 0.7)',
                    fontWeight: 500,
                    fontSize: 14
                  }}>
                    Player
                  </th>
                  <th style={{
                    textAlign: 'right',
                    padding: '12px 16px',
                    color: 'rgba(255, 255, 255, 0.7)',
                    fontWeight: 500,
                    fontSize: 14
                  }}>
                    Wins
                  </th>
                </tr>
              </thead>
              <tbody>
                {hallOfFame.map((entry, index) => {
                  const medal = index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : ''
                  return (
                    <tr key={entry.userId ?? index} style={{
                      background: 'rgba(100, 108, 255, 0.05)',
                      transition: 'background 0.2s'
                    }}
                    onMouseEnter={(e) => e.currentTarget.style.background = 'rgba(100, 108, 255, 0.1)'}
                    onMouseLeave={(e) => e.currentTarget.style.background = 'rgba(100, 108, 255, 0.05)'}
                    >
                      <td style={{
                        padding: '16px',
                        borderRadius: '8px 0 0 8px',
                        fontSize: 18,
                        fontWeight: 600
                      }}>
                        {medal || `#${index + 1}`}
                      </td>
                      <td style={{
                        padding: '16px',
                        color: 'white',
                        fontWeight: 500
                      }}>
                        {entry.userName ?? 'Unknown'}
                      </td>
                      <td style={{
                        padding: '16px',
                        borderRadius: '0 8px 8px 0',
                        textAlign: 'right',
                        color: '#51cf66',
                        fontWeight: 600,
                        fontSize: 18
                      }}>
                        {entry.totalVotes ?? 0}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}