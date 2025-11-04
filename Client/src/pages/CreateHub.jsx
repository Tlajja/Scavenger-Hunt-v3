import React, { useMemo, useState, useEffect } from 'react'
import { createHub, deleteHub, getHubs } from '../services/api.js'

export default function CreateHub() {
    const userId = Number(localStorage.getItem('userId') || 0)
    const [name, setName] = useState('')
    const [isPrivate, setIsPrivate] = useState(false)
    const [message, setMessage] = useState('')
    const [creating, setCreating] = useState(false)

    const [myHubs, setMyHubs] = useState([])
    const [loadingList, setLoadingList] = useState(true)
    const [listError, setListError] = useState('')

    const canSubmit = useMemo(() => !!userId && name.trim().length > 0 && !creating, [userId, name, creating])

    useEffect(() => {
        let mounted = true
        async function load() {
            setLoadingList(true); setListError('')
            try {
                const res = await getHubs(false) // get all hubs
                if (res.ok && Array.isArray(res.data)) {
                    const hubs = res.data
                    const mine = hubs.filter(h => (h.creatorId ?? h.CreatorId) === userId)
                    if (mounted) setMyHubs(mine)
                } else if (!res.ok) {
                    if (mounted) setListError((res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`)
                }
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
    }

    return (
        <div style={{ maxWidth: 560 }}>
            <h2>Create Hub</h2>

            {!userId && (
                <div style={{ color: 'crimson', marginBottom: 10 }}>
                    You must be logged in to create a hub.
                </div>
            )}

            <form onSubmit={handleCreate}>
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'block', marginBottom: 6 }}>Hub Name</label>
                    <input
                        value={name}
                        onChange={e => setName(e.target.value)}
                        placeholder="My Awesome Hub"
                        style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                        disabled={!userId || creating}
                    />
                </div>

                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
                        <input
                            type="checkbox"
                            checked={isPrivate}
                            onChange={e => setIsPrivate(e.target.checked)}
                            disabled={!userId || creating}
                        />
                        Private hub
                    </label>
                </div>

                <button type="submit" disabled={!canSubmit}>
                    {creating ? 'Creating…' : 'Create Hub'}
                </button>
            </form>

            {message && <div style={{ marginTop: 10 }}>{message}</div>}

            <hr style={{ margin: '20px 0' }} />

            <h3>Your Hubs</h3>
            {loadingList ? (
                <div>Loading…</div>
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
                                {}
                                {isPrivateHub && (
                                    <div style={{ fontSize: 12, color: '#666' }}>
                                        Join Code: {h.joinCode ?? h.JoinCode}
                                    </div>
                                )}
                                <div style={{ marginTop: 8 }}>
                                    <button onClick={() => handleDelete(h)}>Delete</button>
                                </div>
                            </li>
                        )
                    })}
                </ul>
            )}
        </div>
    )
}