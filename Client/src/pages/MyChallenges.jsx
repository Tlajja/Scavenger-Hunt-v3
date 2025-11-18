import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getChallenges, getChallengeById } from '../services/api.js'

export default function MyChallenges() {
  const [challenges, setChallenges] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const navigate = useNavigate()
  const userId = Number(localStorage.getItem('userId') || 0)

  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await getChallenges(false)
        if (!mounted) return
        if (!res.ok) {
          setError((res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`)
          return
        }

        const allChallenges = Array.isArray(res.data) ? res.data : []
        
        const enriched = await Promise.all(allChallenges.map(async c => {
          try {
            const detail = await getChallengeById(c.id ?? c.Id)
            if (!detail.ok || !detail.data) return null
            
            const participants = detail.data.participants ?? detail.data.Participants ?? detail.data.members ?? detail.data.Members ?? []
            const isMember = Array.isArray(participants) && participants.some(m => Number(m.userId ?? m.UserId ?? m.id ?? m.Id ?? 0) === userId)
            
            if (!isMember) return null
            
            return {
              ...c,
              ...detail.data,
              members: participants
            }
          } catch {
            return null
          }
        }))

        const myChallenges = enriched.filter(c => c !== null)
        if (mounted) setChallenges(myChallenges)
      } catch (err) {
        if (mounted) setError(String(err))
      } finally {
        if (mounted) setLoading(false)
      }
    }
    load()
    return () => { mounted = false }
  }, [userId])

  const getStatusInfo = (status) => {
    const statusNum = Number(status ?? 0)
    switch (statusNum) {
      case 0: return { text: 'Open', color: '#51cf66' }
      case 1: return { text: 'Voting', color: '#ffd43b' }
      case 2: return { text: 'Completed', color: '#868e96' }
      default: return { text: 'Unknown', color: '#868e96' }
    }
  }

  if (loading) {
    return (
      <div style={{
        minHeight: 'calc(100vh - 70px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        <div style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: 18 }}>
          Loading your challenges...
        </div>
      </div>
    )
  }

  return (
    <div style={{
      minHeight: 'calc(100vh - 70px)',
      padding: '60px 40px',
      maxWidth: 1200,
      margin: '0 auto'
    }}>
      <h1 style={{
        fontSize: 40,
        marginBottom: 40,
        textAlign: 'center',
        color: 'white'
      }}>
        My Challenges
      </h1>

      {error && (
        <div className="error-message" style={{ maxWidth: 600, margin: '0 auto 24px' }}>
          {error}
        </div>
      )}

      {challenges.length === 0 ? (
        <div className="card" style={{
          maxWidth: 600,
          margin: '0 auto',
          textAlign: 'center',
          padding: 60
        }}>
          <div style={{ fontSize: 64, marginBottom: 24 }}>🔍</div>
          <h2 style={{ color: 'white', marginBottom: 16 }}>No Challenges Yet</h2>
          <p style={{ color: 'rgba(255, 255, 255, 0.6)', marginBottom: 32 }}>
            You haven't joined any challenges yet. Create or join one to get started!
          </p>
          <div style={{ display: 'flex', gap: 16, justifyContent: 'center' }}>
            <button onClick={() => navigate('/create-challenge')}>
              Create Challenge
            </button>
            <button onClick={() => navigate('/join-challenge')} style={{
              background: 'transparent',
              border: '1px solid #646cff'
            }}>
              Join Challenge
            </button>
          </div>
        </div>
      ) : (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))',
          gap: 24
        }}>
          {challenges.map(c => {
            const statusInfo = getStatusInfo(c.status ?? c.Status)
            const challengeId = c.id ?? c.Id
            
            return (
              <div
                key={challengeId}
                className="card"
                style={{
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  position: 'relative'
                }}
                onClick={() => navigate(`/challenge-room/${challengeId}`)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-4px)'
                  e.currentTarget.style.boxShadow = '0 12px 40px rgba(0, 0, 0, 0.4)'
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)'
                  e.currentTarget.style.boxShadow = '0 8px 32px rgba(0, 0, 0, 0.3)'
                }}
              >
                <div style={{
                  position: 'absolute',
                  top: 16,
                  right: 16,
                  background: statusInfo.color,
                  color: statusInfo.color === '#ffd43b' ? '#1a1a2e' : 'white',
                  padding: '4px 12px',
                  borderRadius: 12,
                  fontSize: 12,
                  fontWeight: 600
                }}>
                  {statusInfo.text}
                </div>

                <h3 style={{
                  fontSize: 22,
                  marginBottom: 12,
                  color: 'white',
                  paddingRight: 80
                }}>
                  {c.name ?? c.Name ?? 'Unnamed Challenge'}
                </h3>

                <div style={{
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 8,
                  marginTop: 16,
                  color: 'rgba(255, 255, 255, 0.6)',
                  fontSize: 14
                }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <span>👥</span>
                    <span>{(c.members ?? []).length} members</span>
                  </div>
                  {(c.isPrivate ?? c.IsPrivate ?? c.private ?? false) ? (
                    <>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <span>🔒</span>
                        <span>Private Challenge</span>
                      </div>
                      {(c.joinCode ?? c.JoinCode) && (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                          <span>🔑</span>
                          <span>Code: {c.joinCode ?? c.JoinCode}</span>
                        </div>
                      )}
                    </>
                  ) : (
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <span>🌍</span>
                      <span>Public Challenge</span>
                    </div>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}