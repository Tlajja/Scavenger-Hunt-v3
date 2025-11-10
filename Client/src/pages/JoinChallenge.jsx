import React, { useEffect, useState } from 'react'
import { joinChallenge, getChallenges, getChallengeById, leaveChallenge } from '../services/api.js'

export default function JoinChallenge() {
    const [joinCode, setJoinCode] = useState('')
    const [message, setMessage] = useState('')
    const [joining, setJoining] = useState(false)
    const [joinedChallenge, setJoinedChallenge] = useState(null)
    const [publicChallenges, setPublicChallenges] = useState([])

    const userId = Number(localStorage.getItem('userId') || 0)
    const userName = localStorage.getItem('username') || 'Guest'
    const [userChallengeId, setUserChallengeId] = useState(Number(localStorage.getItem('challengeId') || 0))
    const [userChallengeName, setUserChallengeName] = useState(localStorage.getItem('challengeName') || '')

    useEffect(() => {
        loadPublicChallenges()
    }, [])

    useEffect(() => {
        let mounted = true
        async function resolveName() {
            if (userChallengeId && !userChallengeName) {
                try {
                    const res = await getChallengeById(userChallengeId)
                    if (!mounted) return
                    if (res.ok) {
                        const h = res.data
                        const name = (h?.name ?? h?.Name) || ''
                        if (name) {
                            setUserChallengeName(name)
                            localStorage.setItem('challengeName', name)
                        }
                    }
                } catch {}
            }
        }
        resolveName()
        return () => { mounted = false }
    }, [userChallengeId, userChallengeName])

    // validate that stored challengeId actually belongs to the current user;
    // if not clear membership info
    useEffect(() => {
        let mounted = true
        async function validateLocalMembership() {
            if (!userChallengeId) return
            if (!userId) {
                localStorage.removeItem('challengeId')
                localStorage.removeItem('challengeName')
                if (!mounted) return
                setUserChallengeId(0)
                setUserChallengeName('')
                return
            }

            try {
                const res = await getChallengeById(userChallengeId)
                if (!mounted) return
                if (!res.ok) {
                    // challenge missing on server — clear local state
                    localStorage.removeItem('challengeId')
                    localStorage.removeItem('challengeName')
                    setUserChallengeId(0)
                    setUserChallengeName('')
                    return
                }
                const challenge = res.data
                const members = challenge?.members ?? challenge?.Members ?? []
                const isMember = Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId)

                if (!isMember) {
                    // current user is not a member -> clear stale local data
                    localStorage.removeItem('challengeId')
                    localStorage.removeItem('challengeName')
                    setUserChallengeId(0)
                    setUserChallengeName('')
                    setMessage('Cleared stale challenge membership (not a member on server).')
                    return
                }

                // ensure name is present
                const name = challenge?.name ?? challenge?.Name ?? ''
                if (name) {
                    setUserChallengeName(name)
                    localStorage.setItem('challengeName', name)
                }
            } catch {
            }
        }

        validateLocalMembership()
        return () => { mounted = false }
    }, [userChallengeId, userId])

    // attempt to recover membership from server after login / auth changes
    useEffect(() => {
        let mounted = true
        async function recoverMembership() {
            // if user is logged in but client has no challenge info, try to find which challenge the user belongs to
            if (!userId || userChallengeId > 0) return

            try {
                // ask server for challenges (not only public) and then fetch each challenge's details (members)
                const res = await getChallenges(false)
                if (!mounted || !res.ok || !Array.isArray(res.data)) return

                for (const h of res.data) {
                    const id = h?.id ?? h?.Id
                    if (!id) continue
                    // fetch full challenge (includes members)
                    const detail = await getChallengeById(id)
                    if (!mounted || !detail.ok) continue
                    const challenge = detail.data
                    const members = challenge?.members ?? challenge?.Members ?? []
                    if (Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId)) {
                        const name = challenge.name ?? challenge.Name ?? ''
                        localStorage.setItem('challengeId', String(id))
                        localStorage.setItem('challengeName', name)
                        setUserChallengeId(Number(id))
                        setUserChallengeName(name)
                        setMessage('Restored challenge membership from server.')
                        break
                    }
                }
            } catch {
            }
        }
        // run once on mount / when userId changes
        recoverMembership()

        // also respond to cross-tab or explicit auth change events
        const onAuth = () => setTimeout(recoverMembership, 50)
        window.addEventListener('auth-changed', onAuth)
        window.addEventListener('storage', onAuth)
        return () => {
            mounted = false
            window.removeEventListener('auth-changed', onAuth)
            window.removeEventListener('storage', onAuth)
        }
    }, [userId, userChallengeId])

    async function loadPublicChallenges() {
        const res = await getChallenges(true)
        if (res.ok && Array.isArray(res.data)) setPublicChallenges(res.data)
    }

    async function performJoin(code) {
        if (userChallengeId) { setMessage('Leave the current challenge before joining another.'); return }
        setJoining(true)
        try {
            const res = await joinChallenge(code, userId)
            if (!res.ok) {
                const errMsg = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
                setMessage(`Error: ${errMsg}`)
                setJoinedChallenge(null)
                return
            }
            const member = res.data
            let challenge = member?.challenge || member?.Challenge || null
            let resolvedChallengeId = member?.challengeId ?? member?.ChallengeId ?? null

            if (!challenge && resolvedChallengeId) {
                const hres = await getChallengeById(resolvedChallengeId)
                if (hres.ok) challenge = hres.data
            }

            const finalChallengeId = (challenge?.id ?? challenge?.Id ?? resolvedChallengeId) ?? null
            if (finalChallengeId != null) {
                localStorage.setItem('challengeId', String(finalChallengeId))
                const challengeName = challenge?.name ?? challenge?.Name ?? ''
                localStorage.setItem('challengeName', challengeName)
                setUserChallengeId(Number(finalChallengeId))
                setUserChallengeName(challengeName)
            }

            setJoinedChallenge(challenge)
            setMessage(`Joined challenge${(challenge?.name ?? userChallengeName) ? `: ${challenge?.name ?? userChallengeName}` : ''}`)
        } catch {
            setMessage('Network error')
        } finally {
            setJoining(false)
        }
    }

    async function handleSubmit(e) {
        e.preventDefault()
        setMessage('')
        if (!userId) { setMessage('You must be logged in to join a challenge.'); return }
        const code = (joinCode || '').trim()
        if (!code) { setMessage('Enter a join code.'); return }
        await performJoin(code)
    }

    async function handleQuickJoin(h) {
        if (userChallengeId) { setMessage('Leave current challenge before joining another.'); return }
        if (!userId) { setMessage('You must be logged in to join a challenge.'); return }
        const code = (h?.joinCode ?? h?.JoinCode ?? '').trim()
        if (!code) { setMessage('This challenge cannot be joined automatically.'); return }
        await performJoin(code)
    }

    async function handleLeave() {
        if (!userId || !userChallengeId) return
        if (!confirm('Leave current challenge?')) return
        setMessage('Leaving challenge...')
        const res = await leaveChallenge(userChallengeId, userId)
        if (!res.ok) {
            const errMsg = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            // If server reports user is not a member, clear local membership to avoid stale UI
            if ((errMsg || '').toString().toLowerCase().includes('not a member')) {
                localStorage.removeItem('challengeId')
                localStorage.removeItem('challengeName')
                setUserChallengeId(0)
                setUserChallengeName('')
                setJoinedChallenge(null)
                setMessage('You were not a member on the server; cleared local membership.')
                return
            }

            setMessage(`Error: ${errMsg}`)
            return
        }
        localStorage.removeItem('challengeId')
        localStorage.removeItem('challengeName')
        setUserChallengeId(0)
        setUserChallengeName('')
        setJoinedChallenge(null)
        setMessage('Left challenge.')
    }

    return (
        <div style={{ maxWidth: 520 }}>
            <h2>Join a Challenge</h2>

            {userChallengeId > 0 ? (
                <div style={{ marginBottom: 12 }}>
                    You are currently in challenge: <strong>{userChallengeName || `#${userChallengeId}`}</strong>
                </div>
            ) : null}

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'block', marginBottom: 6 }}>Join Code</label>
                    <input
                        value={joinCode}
                        onChange={e => setJoinCode(e.target.value)}
                        placeholder="e.g. ABC123"
                        style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                        disabled={joining}
                    />
                </div>

                {!userId && (
                    <div style={{ color: 'crimson', marginBottom: 10 }}>
                        You must be logged in. Current: {userName}
                    </div>
                )}

                <button type="submit" disabled={!userId || joining}>
                    {joining ? 'Joining�' : 'Join Challenge by Code'}
                </button>
            </form>

            {message && (
                <div style={{ marginTop: 10 }}>
                    {message}
                </div>
            )}

            <hr style={{ margin: '20px 0' }} />

            <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <h3 style={{ margin: 0 }}>Public Challenges</h3>
                    <button onClick={loadPublicChallenges} disabled={joining}>Refresh</button>
                </div>

                {publicChallenges.length === 0 ? (
                    <div style={{ marginTop: 8, color: '#666' }}>No public challenges available.</div>
                ) : (
                    <ul style={{ listStyle: 'none', padding: 0, marginTop: 12 }}>
                        {publicChallenges.map(h => (
                            <li key={h.id ?? h.Id} style={{ border: '1px solid #eee', padding: 10, marginBottom: 8 }}>
                                <div style={{ fontWeight: 600 }}>{h.name ?? h.Name}</div>
                                <div style={{ marginTop: 8 }}>
                                    <button onClick={() => handleQuickJoin(h)} disabled={!userId || joining}>
                                        Join
                                    </button>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            {joinedChallenge && (
                <div style={{ marginTop: 20, padding: 12, border: '1px solid #e0e0e0' }}>
                    <div style={{ fontWeight: 600 }}>Joined Challenge</div>
                    <div>Name: {joinedChallenge.name ?? joinedChallenge.Name}</div>
                </div>
            )}
        </div>
    )
}
