import React, { useMemo, useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { createHub, deleteHub, getHubs, getHubById, leaveHub } from '../services/api.js'

export default function CreateHub() {
    const userId = Number(localStorage.getItem('userId') || 0)
    const [name, setName] = useState('')
    const [isPrivate, setIsPrivate] = useState(false)
    const [message, setMessage] = useState('')
    const [creating, setCreating] = useState(false)

    const [myHubs, setMyHubs] = useState([])
    const [loadingList, setLoadingList] = useState(true)
    const [listError, setListError] = useState('')

    const [userHubId, setUserHubId] = useState(Number(localStorage.getItem('hubId') || 0))

    const canSubmit = useMemo(() => !!userId && name.trim().length > 0 && !creating && !userHubId, [userId, name, creating, userHubId])

    useEffect(() => {
        let mounted = true
        async function load() {
            setLoadingList(true); setListError('')
            try {
                const res = await getHubs(false) // get all hubs
                if (!mounted) return
                if (!res.ok) {
                    setListError((res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`)
                    return
                }

                const hubs = Array.isArray(res.data) ? res.data : []
                // filter hubs that the user created
                const mine = hubs.filter(h => (h.creatorId ?? h.CreatorId) === userId)

                // For each of "mine", fetch full hub details to see membership & roles
                const enriched = await Promise.all(mine.map(async h => {
                    const id = h.id ?? h.Id
                    if (!id) return { ...h, canDelete: false }
                    try {
                        const detail = await getHubById(id)
                        if (!detail.ok || !detail.data) return { ...h, canDelete: false }
                        const members = detail.data.members ?? detail.data.Members ?? []
                        const isAdmin = Array.isArray(members) && members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId && (m.role ?? m.Role) === 1)
                        return { ...h, canDelete: !!isAdmin }
                    } catch {
                        return { ...h, canDelete: false }
                    }
                }))

                if (mounted) setMyHubs(enriched)
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
        if (userHubId) { setMessage('Leave your current hub before creating a new one.'); return }
        if (!name.trim()) { setMessage('Hub name is required.'); return }
        setCreating(true); setMessage('Creating hub...')

        const res = await createHub(name.trim(), userId, isPrivate)
        if (!res.ok) {
            const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            setMessage(`Error: ${err}`)
            setCreating(false)
            return
        }
        setMessage('Hub created.')
        setName('')
        setIsPrivate(false)
        const created = res.data
        if (created?.id ?? created?.Id) {
            const createdId = created.id ?? created.Id
            localStorage.setItem('hubId', String(createdId))
            localStorage.setItem('hubName', (created.name ?? created.Name) || '')
            setUserHubId(Number(createdId))
        }
        if (created) {
            setMyHubs(prev => [{ ...created }, ...prev])
        }
        setCreating(false)
    }

    async function handleDelete(hub) {
        if (!hub) return
        const hubId = hub.id ?? hub.Id
        if (!hubId) return
        if (!userId) { setMessage('You must be logged in.'); return }

        // guard in UI: require canDelete true
        if (!hub.canDelete) {
            setMessage('You are not an admin of this hub and cannot delete it.')
            return
        }

        const confirmed = window.confirm(`Delete hub "${hub.name ?? hub.Name}"? This cannot be undone.`)
        if (!confirmed) return
        setMessage('Deleting hub...')
        const res = await deleteHub(hubId, userId)
        if (!res.ok) {
            const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            setMessage(`Error: ${err}`)
            return
        }
        setMessage('Hub deleted.')
        setMyHubs(prev => prev.filter(h => (h.id ?? h.Id) !== hubId))
        
        // if deleted hub was current membership, clear it
        if (Number(localStorage.getItem('hubId') || 0) === hubId) {
            localStorage.removeItem('hubId'); localStorage.removeItem('hubName'); setUserHubId(0)
        }
    }

    return (
        <div style={{ maxWidth: 560 }}>
            <h2>Create Hub</h2>
            {userHubId ? (
                <div style={{ marginBottom: 12 }}>
                    You are currently in hub: <strong>{localStorage.getItem('hubName') || `#${userHubId}`}</strong>.
                    {' '}Please leave it first on the <Link to="/hubs/join"> Join Hub</Link> page if you want to create a new hub.
                </div>
            ) : null}

            <form onSubmit={handleCreate}>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'block', marginBottom: 6 }}>Hub Name</label>
                    <input
                        value={name}
                        onChange={e => setName(e.target.value)}
                        placeholder="My Awesome Hub"
                        style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                        disabled={!userId || creating || !!userHubId}
                    />
                </div>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
                        <input
                            type="checkbox"
                            checked={isPrivate}
                            onChange={e => setIsPrivate(e.target.checked)}
                            disabled={!userId || creating || !!userHubId}
                        />
                        Private hub
                    </label>
                </div>

                <button type="submit" disabled={!canSubmit}>
                    {creating ? 'Creating�' : 'Create Hub'}
                </button>
            </form>

            {message && <div style={{ marginTop: 10 }}>{message}</div>}

            <hr style={{ margin: '20px 0' }} />

            <h3>Your Hubs</h3>
            {loadingList ? (
                <div>Loading�</div>
            ) : listError ? (
                <div style={{ color: 'crimson' }}>{listError}</div>
            ) : myHubs.length === 0 ? (
                <div>You have not created any hubs yet.</div>
            ) : (
                <ul style={{ listStyle: 'none', padding: 0 }}>
                    {myHubs.map(h => {
                        const isPrivateHub = h.isPrivate ?? h.private ?? h.IsPrivate ?? false
                        return (
                            <li key={h.id ?? h.Id} style={{ border: '1px solid #eee', padding: 10, marginBottom: 8 }}>
                                <div style={{ fontWeight: 600 }}>{h.name ?? h.Name}</div>
                                {isPrivateHub && (
                                    <div style={{ fontSize: 12, color: '#666' }}>
                                        Join Code: {h.joinCode ?? h.JoinCode}
                                    </div>
                                )}
                                <div style={{ marginTop: 8 }}>
                                    {h.canDelete ? (
                                        <button onClick={() => handleDelete(h)}>Delete</button>
                                    ) : (
                                        <button disabled title="Only hub admins can delete this hub">Delete (admin only)</button>
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