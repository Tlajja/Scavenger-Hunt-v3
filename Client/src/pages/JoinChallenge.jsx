import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { joinChallenge, getChallenges, getChallengeById, getMyChallenges } from '../services/api.js'

export default function JoinChallenge() {
  const navigate = useNavigate()
  const userId = Number(localStorage.getItem('userId') || 0)
  
  const [joinCode, setJoinCode] = useState('')
  const [publicChallenges, setPublicChallenges] = useState([])
  const [myChallengeIds, setMyChallengeIds] = useState(new Set())
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [joining, setJoining] = useState(false)

  useEffect(() => {
    loadPublicChallenges()
    loadMyChallenges()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function loadMyChallenges() {
    try {
      const res = await getMyChallenges(userId)
      if (res.ok && Array.isArray(res.data)) {
        const ids = new Set(res.data.map(c => c.id ?? c.Id))
        setMyChallengeIds(ids)
      }
    } catch { /* ignore */ }
  }

  async function loadPublicChallenges() {
    setLoading(true)
    try {
      const res = await getChallenges(true)
      if (res.ok && Array.isArray(res.data)) {
        const open = res.data.filter(c => Number(c.status ?? c.Status ?? 0) === 0)
        setPublicChallenges(open)
      }
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  async function performJoin(code) {
    setJoining(true)
    setError('')
    setMessage('')
    
    try {
      const res = await joinChallenge(code, userId)
      if (!res.ok) {
        const errMsg = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
        
        if (/already a participant/i.test(errMsg)) {
          const mine = await getMyChallenges(userId)
          if (mine.ok && Array.isArray(mine.data)) {
            const match = mine.data.find(c => (c.joinCode ?? c.JoinCode ?? '').toUpperCase() === (code || '').toUpperCase())
            if (match) {
              const cid = match.id ?? match.Id
              const cname = match.name ?? match.Name ?? ''
              localStorage.setItem('challengeId', String(cid))
              localStorage.setItem('challengeName', cname)
              setMessage('You are already in this challenge. Redirecting...')
              setTimeout(() => navigate(`/challenge-room/${cid}`), 1000)
              return
            }
          }
        }
        
        // If challenge is full or any limit error, refresh the challenge list to show accurate counts
        if (/full|limit|maximum/i.test(errMsg)) {
          await loadPublicChallenges()
        }
        
        setError(errMsg)
        return
      }

      // New API shape: { participant: {...}, joinCode: 'ABC123' }
      const participantWrapper = res.data
      const participant = participantWrapper?.participant ?? participantWrapper?.Participant ?? null
      if (!participant) {
        setError('Invalid response from server.')
        return
      }

      let challengeId = participant.challengeId ?? participant.ChallengeId ?? null
      let challenge = participant.challenge ?? participant.Challenge ?? null

      if (!challenge && challengeId != null) {
        const hres = await getChallengeById(challengeId)
        if (hres.ok) challenge = hres.data
      }

      const finalChallengeId = challengeId ?? (challenge?.id ?? challenge?.Id ?? null)
      if (finalChallengeId != null) {
        const challengeName = challenge?.name ?? challenge?.Name ?? ''
        localStorage.setItem('challengeId', String(finalChallengeId))
        localStorage.setItem('challengeName', challengeName)
        
        setMessage('Successfully joined challenge!')
        // Refresh challenge list to update participant counts
        await loadPublicChallenges()
        await loadMyChallenges()
        setTimeout(() => {
          navigate(`/challenge-room/${finalChallengeId}`)
        }, 800)
      }
    } catch (err) {
      setError('Network error')
    } finally {
      setJoining(false)
    }
  }

  async function handleSubmitCode(e) {
    e.preventDefault()
    const code = (joinCode || '').trim()
    if (!code) {
      setError('Please enter a join code')
      return
    }
    await performJoin(code)
  }

  async function handleQuickJoin(challenge) {
    const code = (challenge?.joinCode ?? challenge?.JoinCode ?? '').trim()
    if (!code) {
      setError('This challenge cannot be joined automatically')
      return
    }
    await performJoin(code)
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
        Join a Challenge
      </h1>

      <div style={{
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: 32,
        marginBottom: 32
      }}>
        <div className="card">
          <h2 style={{ color: 'white', marginBottom: 24, fontSize: 24 }}>
            Join with Code
          </h2>
          
          <form onSubmit={handleSubmitCode}>
            <div style={{ marginBottom: 20 }}>
              <label>Enter Join Code</label>
              <input
                value={joinCode}
                onChange={e => setJoinCode(e.target.value)}
                placeholder="e.g. ABC123"
                disabled={joining}
                style={{ textTransform: 'uppercase' }}
              />
            </div>

            <button
              type="submit"
              disabled={!joinCode.trim() || joining}
              style={{ width: '100%', padding: '14px' }}
            >
              {joining ? 'Joining...' : 'Join Challenge'}
            </button>
          </form>
        </div>

        <div className="card">
          <h2 style={{ color: 'white', marginBottom: 16, fontSize: 24 }}>
            Quick Info
          </h2>
          <ul style={{
            color: 'rgba(255, 255, 255, 0.7)',
            lineHeight: 1.8,
            paddingLeft: 20
          }}>
            <li>Private challenges require a join code from the creator</li>
            <li>Public challenges can be joined instantly below</li>
            <li>You can be part of multiple challenges at once</li>
          </ul>
        </div>
      </div>

      {(error || message) && (
        <div className={error ? 'error-message' : 'success-message'} style={{ maxWidth: 800, margin: '0 auto 32px' }}>
          {error || message}
        </div>
      )}

      <div className="card">
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 24
        }}>
          <h2 style={{ color: 'white', fontSize: 24, margin: 0 }}>
            Public Challenges
          </h2>
          <button
            onClick={() => { loadPublicChallenges(); loadMyChallenges(); }}
            disabled={loading || joining}
            style={{
              background: 'transparent',
              border: '1px solid #646cff',
              padding: '8px 16px',
              boxShadow: 'none'
            }}
          >
            ↻ Refresh
          </button>
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.6)', padding: 40 }}>
            Loading challenges...
          </div>
        ) : publicChallenges.length === 0 ? (
          <div style={{ textAlign: 'center', color: 'rgba(255, 255, 255, 0.6)', padding: 40 }}>
            No public challenges available right now.
            <div style={{ marginTop: 16 }}>
              <a
                onClick={() => navigate('/create-challenge')}
                style={{ color: '#646cff', cursor: 'pointer', fontWeight: 500 }}
              >
                Create the first one!
              </a>
            </div>
          </div>
        ) : (
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
            gap: 20
          }}>
            {publicChallenges.map(c => {
              const challengeId = c.id ?? c.Id
              const name = c.name ?? c.Name ?? 'Unnamed Challenge'
              const alreadyJoined = myChallengeIds.has(challengeId)
              const participants = c.participants ?? c.Participants ?? []
              const participantCount = Array.isArray(participants) ? participants.length : 0
              const maxParticipants = c.maxParticipants ?? c.MaxParticipants ?? 10
              
              return (
                <div
                  key={challengeId}
                  style={{
                    background: 'rgba(100, 108, 255, 0.05)',
                    borderRadius: 12,
                    padding: 20,
                    border: '1px solid rgba(100, 108, 255, 0.2)',
                    transition: 'all 0.2s'
                  }}
                  onMouseEnter={e => {
                    e.currentTarget.style.background = 'rgba(100, 108, 255, 0.1)'
                    e.currentTarget.style.borderColor = 'rgba(100, 108, 255, 0.4)'
                  }}
                  onMouseLeave={e => {
                    e.currentTarget.style.background = 'rgba(100, 108, 255, 0.05)'
                    e.currentTarget.style.borderColor = 'rgba(100, 108, 255, 0.2)'
                  }}
                >
                  <h3 style={{
                    color: 'white',
                    fontSize: 18,
                    marginBottom: 12
                  }}>
                    {name}
                  </h3>
                  
                  <div style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    color: 'rgba(255, 255, 255, 0.6)',
                    fontSize: 14,
                    marginBottom: 8
                  }}>
                    <span>🌍</span>
                    <span>Public Challenge</span>
                  </div>

                  <div style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    color: participantCount >= maxParticipants ? '#ff6b6b' : 'rgba(255, 255, 255, 0.6)',
                    fontSize: 14,
                    marginBottom: 16,
                    fontWeight: participantCount >= maxParticipants ? 600 : 400
                  }}>
                    <span>👥</span>
                    <span>{participantCount} / {maxParticipants} participants</span>
                    {participantCount >= maxParticipants && <span style={{ color: '#ff6b6b' }}>• Full</span>}
                  </div>

                  <button
                    onClick={() => alreadyJoined ? navigate(`/challenge-room/${challengeId}`) : handleQuickJoin(c)}
                    disabled={joining || (!alreadyJoined && participantCount >= maxParticipants)}
                    style={{ 
                      width: '100%', 
                      padding: '10px',
                      opacity: (!alreadyJoined && participantCount >= maxParticipants) ? 0.5 : 1,
                      cursor: (!alreadyJoined && participantCount >= maxParticipants) ? 'not-allowed' : 'pointer'
                    }}
                  >
                    {joining ? 'Joining...' : alreadyJoined ? 'Enter' : participantCount >= maxParticipants ? 'Full' : 'Join Now'}
                  </button>
                </div>
              )
            })}
          </div>
        )}
      </div>

      <div style={{
        marginTop: 32,
        textAlign: 'center',
        color: 'rgba(255, 255, 255, 0.6)'
      }}>
        <a
          onClick={() => navigate('/')}
          style={{ color: '#646cff', cursor: 'pointer', fontWeight: 500 }}
        >
          ← Back to Home
        </a>
      </div>
    </div>
  )
}