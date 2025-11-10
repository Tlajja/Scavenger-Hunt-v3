import React, { useMemo, useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { createChallenge, deleteChallenge, getChallenges, getChallengeById, leaveChallenge } from '../services/api.js'

export default function CreateChallenge() {
    const userId = Number(localStorage.getItem('userId') || 0)
    const [name, setName] = useState('')
    const [isPrivate, setIsPrivate] = useState(false)
    const [message, setMessage] = useState('')
    const [creating, setCreating] = useState(false)

    const [myChallenges, setMyChallenges] = useState([])
    const [loadingList, setLoadingList] = useState(true)
    const [listError, setListError] = useState('')

    const [userChallengeId, setUserChallengeId] = useState(Number(localStorage.getItem('challengeId') || 0))

    const canSubmit = useMemo(() => !!userId && name.trim().length > 0 && !creating && !userChallengeId, [userId, name, creating, userChallengeId])

    useEffect(() => {
        let mounted = true
        async function load() {
            setLoadingList(true); setListError('')
            try {
                const res = await getChallenges(false) // get all challenges
                if (!mounted) return
                if (!res.ok) {
                    setListError((res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`)
                    return
                }

                const challenges = Array.isArray(res.data) ? res.data : []
                // filter challenges that the user created
                const mine = challenges.filter(h => (h.creatorId ?? h.CreatorId) === userId)

                // For each of "mine", fetch full challenge details to see membership & roles
                const enriched = await Promise.all(mine.map(async h => {
                    const id = h.id ?? h.Id
                    if (!id) return { ...h, canDelete: false }
                    try {
                        const detail = await getChallengeById(id)
                        if (!detail.ok || !detail.data) return { ...h, canDelete: false }
                        const members = detail.data.members ?? detail.data.Members ?? []
                        const isAdmin = Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId && (m.role ?? m.Role) === 1)
                        return { ...h, canDelete: !!isAdmin }
                    } catch {
                        return { ...h, canDelete: false }
                    }
                }))

                if (mounted) setMyChallenges(enriched)
            } catch (err) {
                if (mounted) setListError(String(err))
            } finally {
                if (mounted) setLoadingList(false)
            }
        }
        load()
        return () => { mounted = false }
    }, [userId])

    async function handleCreate(e) {
        e.preventDefault()
        if (!userId) { setMessage('You must be logged in.'); return }
        if (userChallengeId) { setMessage('Leave your current challenge before creating a new one.'); return }
        if (!name.trim()) { setMessage('Challenge name is required.'); return }
        setCreating(true); setMessage('Creating challenge...')

        const res = await createChallenge(name.trim(), userId, isPrivate)
        if (!res.ok) {
            const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            setMessage(`Error: ${err}`)
            setCreating(false)
            return
        }
        setMessage('Challenge created.')
        setName('')
        setIsPrivate(false)
        const created = res.data
        if (created?.id ?? created?.Id) {
            const createdId = created.id ?? created.Id
            localStorage.setItem('challengeId', String(createdId))
            localStorage.setItem('challengeName', (created.name ?? created.Name) || '')
            setUserChallengeId(Number(createdId))
        }
        if (created) {
            setMyChallenges(prev => [{ ...created }, ...prev])
        }
        setCreating(false)
    }

    async function handleDelete(challenge) {
        if (!challenge) return
        const challengeId = challenge.id ?? challenge.Id
        if (!challengeId) return
        if (!userId) { setMessage('You must be logged in.'); return }

        // guard in UI: require canDelete true
        if (!challenge.canDelete) {
            setMessage('You are not an admin of this challenge and cannot delete it.')
            return
        }

        const confirmed = window.confirm(`Delete challenge "${challenge.name ?? challenge.Name}"? This cannot be undone.`)
        if (!confirmed) return
        setMessage('Deleting challenge...')
        const res = await deleteChallenge(challengeId, userId)
        if (!res.ok) {
            const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            setMessage(`Error: ${err}`)
            return
        }
        setMessage('Challenge deleted.')
        setMyChallenges(prev => prev.filter(h => (h.id ?? h.Id) !== challengeId))

        // if deleted challenge was current membership, clear it
        if (Number(localStorage.getItem('challengeId') || 0) === challengeId) {
            localStorage.removeItem('challengeId'); localStorage.removeItem('challengeName'); setUserChallengeId(0)
        }
    }

    return (
        <div style={{ maxWidth: 560 }}>
            <h2>Create Challenge</h2>
            {userChallengeId ? (
                <div style={{ marginBottom: 12 }}>
                    You are currently in challenge: <strong>{localStorage.getItem('challengeName') || `#${userChallengeId}`}</strong>.
                    {' '}Please leave it first on the <Link to="/challenges/join"> Join Challenge</Link> page if you want to create a new challenge.
                </div>
            ) : null}

            <form onSubmit={handleCreate}>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'block', marginBottom: 6 }}>Challenge Name</label>
                    <input
                        value={name}
                        onChange={e => setName(e.target.value)}
                        placeholder="My Awesome Challenge"
                        style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                        disabled={!userId || creating || !!userChallengeId}
                    />
                </div>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
                        <input
                            type="checkbox"
                            checked={isPrivate}
                            onChange={e => setIsPrivate(e.target.checked)}
                            disabled={!userId || creating || !!userChallengeId}
                        />
                        Private challenge
                    </label>
                </div>

                <button type="submit" disabled={!canSubmit}>
                    {creating ? 'Creating�' : 'Create Challenge'}
                </button>
            </form>

            {message && <div style={{ marginTop: 10 }}>{message}</div>}

            <hr style={{ margin: '20px 0' }} />

            <h3>Your Challenges</h3>
            {loadingList ? (
                <div>Loading�</div>
            ) : listError ? (
                <div style={{ color: 'crimson' }}>{listError}</div>
            ) : myChallenges.length === 0 ? (
                <div>You have not created any challenges yet.</div>
            ) : (
                <ul style={{ listStyle: 'none', padding: 0 }}>
                    {myChallenges.map(h => {
                        const isPrivateChallenge = h.isPrivate ?? h.private ?? h.IsPrivate ?? false
                        return (
                            <li key={h.id ?? h.Id} style={{ border: '1px solid #eee', padding: 10, marginBottom: 8 }}>
                                <div style={{ fontWeight: 600 }}>{h.name ?? h.Name}</div>
                                {isPrivateChallenge && (
                                    <div style={{ fontSize: 12, color: '#666' }}>
                                        Join Code: {h.joinCode ?? h.JoinCode}
                                    </div>
                                )}
                                <div style={{ marginTop: 8 }}>
                                    {h.canDelete ? (
                                        <button onClick={() => handleDelete(h)}>Delete</button>
                                    ) : (
                                        <button disabled title="Only challenge admins can delete this challenge">Delete (admin only)</button>
                                    )}
                                </div>
                            </li>
                        )
                    })}
                </ul>
            )}
        </div>
    )
}