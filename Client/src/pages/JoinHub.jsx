import React, { useEffect, useState } from 'react'
import { joinHub, getHubs, getHubById, leaveHub } from '../services/api.js'

export default function JoinHub() {
    const [joinCode, setJoinCode] = useState('')
    const [message, setMessage] = useState('')
    const [joining, setJoining] = useState(false)
    const [joinedHub, setJoinedHub] = useState(null)
    const [publicHubs, setPublicHubs] = useState([])

    const userId = Number(localStorage.getItem('userId') || 0)
    const userName = localStorage.getItem('username') || 'Guest'
    const [userHubId, setUserHubId] = useState(Number(localStorage.getItem('hubId') || 0))
    const [userHubName, setUserHubName] = useState(localStorage.getItem('hubName') || '')

    useEffect(() => {
        loadPublicHubs()
    }, [])

    useEffect(() => {
        let mounted = true
        async function resolveName() {
            if (userHubId && !userHubName) {
                try {
                    const res = await getHubById(userHubId)
                    if (!mounted) return
                    if (res.ok) {
                        const h = res.data
                        const name = (h?.name ?? h?.Name) || ''
                        if (name) {
                            setUserHubName(name)
                            localStorage.setItem('hubName', name)
                        }
                    }
                } catch {}
            }
        }
        resolveName()
        return () => { mounted = false }
    }, [userHubId, userHubName])

    // validate that stored hubId actually belongs to the current user;
    // if not clear membership info
    useEffect(() => {
        let mounted = true
        async function validateLocalMembership() {
            if (!userHubId) return
            if (!userId) {
                localStorage.removeItem('hubId')
                localStorage.removeItem('hubName')
                if (!mounted) return
                setUserHubId(0)
                setUserHubName('')
                return
            }

            try {
                const res = await getHubById(userHubId)
                if (!mounted) return
                if (!res.ok) {
                    // hub missing on server — clear local state
                    localStorage.removeItem('hubId')
                    localStorage.removeItem('hubName')
                    setUserHubId(0)
                    setUserHubName('')
                    return
                }
                const hub = res.data
                const members = hub?.members ?? hub?.Members ?? []
                const isMember = Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId)

                if (!isMember) {
                    // current user is not a member -> clear stale local data
                    localStorage.removeItem('hubId')
                    localStorage.removeItem('hubName')
                    setUserHubId(0)
                    setUserHubName('')
                    setMessage('Cleared stale hub membership (not a member on server).')
                    return
                }

                // ensure name is present
                const name = hub?.name ?? hub?.Name ?? ''
                if (name) {
                    setUserHubName(name)
                    localStorage.setItem('hubName', name)
                }
            } catch {
            }
        }

        validateLocalMembership()
        return () => { mounted = false }
    }, [userHubId, userId])

    // attempt to recover membership from server after login / auth changes
    useEffect(() => {
        let mounted = true
        async function recoverMembership() {
            // if user is logged in but client has no hub info, try to find which hub the user belongs to
            if (!userId || userHubId > 0) return

            try {
                // ask server for hubs (not only public) and then fetch each hub's details (members)
                const res = await getHubs(false)
                if (!mounted || !res.ok || !Array.isArray(res.data)) return

                for (const h of res.data) {
                    const id = h?.id ?? h?.Id
                    if (!id) continue
                    // fetch full hub (includes members)
                    const detail = await getHubById(id)
                    if (!mounted || !detail.ok) continue
                    const hub = detail.data
                    const members = hub?.members ?? hub?.Members ?? []
                    if (Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId)) {
                        const name = hub.name ?? hub.Name ?? ''
                        localStorage.setItem('hubId', String(id))
                        localStorage.setItem('hubName', name)
                        setUserHubId(Number(id))
                        setUserHubName(name)
                        setMessage('Restored hub membership from server.')
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
    }, [userId, userHubId])

    async function loadPublicHubs() {
        const res = await getHubs(true)
        if (res.ok && Array.isArray(res.data)) setPublicHubs(res.data)
    }

    async function performJoin(code) {
        if (userHubId) { setMessage('Leave the current hub before joining another.'); return }
        setJoining(true)
        try {
            const res = await joinHub(code, userId)
            if (!res.ok) {
                const errMsg = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
                setMessage(`Error: ${errMsg}`)
                setJoinedHub(null)
                return
            }
            const member = res.data
            let hub = member?.hub || member?.Hub || null
            let resolvedHubId = member?.hubId ?? member?.HubId ?? null

            if (!hub && resolvedHubId) {
                const hres = await getHubById(resolvedHubId)
                if (hres.ok) hub = hres.data
            }

            const finalHubId = (hub?.id ?? hub?.Id ?? resolvedHubId) ?? null
            if (finalHubId != null) {
                localStorage.setItem('hubId', String(finalHubId))
                const hubName = hub?.name ?? hub?.Name ?? ''
                localStorage.setItem('hubName', hubName)
                setUserHubId(Number(finalHubId))
                setUserHubName(hubName)
            }

            setJoinedHub(hub)
            setMessage(`Joined hub${(hub?.name ?? userHubName) ? `: ${hub?.name ?? userHubName}` : ''}`)
        } catch {
            setMessage('Network error')
        } finally {
            setJoining(false)
        }
    }

    async function handleSubmit(e) {
        e.preventDefault()
        setMessage('')
        if (!userId) { setMessage('You must be logged in to join a hub.'); return }
        const code = (joinCode || '').trim()
        if (!code) { setMessage('Enter a join code.'); return }
        await performJoin(code)
    }

    async function handleQuickJoin(h) {
        if (userHubId) { setMessage('Leave current hub before joining another.'); return }
        if (!userId) { setMessage('You must be logged in to join a hub.'); return }
        const code = (h?.joinCode ?? h?.JoinCode ?? '').trim()
        if (!code) { setMessage('This hub cannot be joined automatically.'); return }
        await performJoin(code)
    }

    async function handleLeave() {
        if (!userId || !userHubId) return
        if (!confirm('Leave current hub?')) return
        setMessage('Leaving hub...')
        const res = await leaveHub(userHubId, userId)
        if (!res.ok) {
            const errMsg = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            // If server reports user is not a member, clear local membership to avoid stale UI
            if ((errMsg || '').toString().toLowerCase().includes('not a member')) {
                localStorage.removeItem('hubId')
                localStorage.removeItem('hubName')
                setUserHubId(0)
                setUserHubName('')
                setJoinedHub(null)
                setMessage('You were not a member on the server; cleared local membership.')
                return
            }

            setMessage(`Error: ${errMsg}`)
            return
        }
        localStorage.removeItem('hubId')
        localStorage.removeItem('hubName')
        setUserHubId(0)
        setUserHubName('')
        setJoinedHub(null)
        setMessage('Left hub.')
    }

    return (
        <div style={{ maxWidth: 520 }}>
            <h2>Join a Hub</h2>

            {userHubId > 0 ? (
                <div style={{ marginBottom: 12 }}>
                    You are currently in hub: <strong>{userHubName || `#${userHubId}`}</strong>
                    <button onClick={handleLeave}>Leave Hub</button>
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
                    {joining ? 'Joining�' : 'Join Hub by Code'}
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
                    <h3 style={{ margin: 0 }}>Public Hubs</h3>
                    <button onClick={loadPublicHubs} disabled={joining}>Refresh</button>
                </div>

                {publicHubs.length === 0 ? (
                    <div style={{ marginTop: 8, color: '#666' }}>No public hubs available.</div>
                ) : (
                    <ul style={{ listStyle: 'none', padding: 0, marginTop: 12 }}>
                        {publicHubs.map(h => (
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

            {joinedHub && (
                <div style={{ marginTop: 20, padding: 12, border: '1px solid #e0e0e0' }}>
                    <div style={{ fontWeight: 600 }}>Joined Hub</div>
                    <div>Name: {joinedHub.name ?? joinedHub.Name}</div>
                </div>
            )}
        </div>
    )
}
