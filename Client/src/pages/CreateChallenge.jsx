import React, { useMemo, useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { createChallenge, deleteChallenge, getChallenges, getChallengeById, leaveChallenge, advanceChallenge, getTasks } from '../services/api.js'

export default function CreateChallenge() {
    const userId = Number(localStorage.getItem('userId') || 0)
    const [name, setName] = useState('')
    const [isPrivate, setIsPrivate] = useState(false)
    const [message, setMessage] = useState('')
    const [creating, setCreating] = useState(false)

    const [myChallenges, setMyChallenges] = useState([])
    const [tasks, setTasks] = useState([])
    const [selectedTaskId, setSelectedTaskId] = useState('')
    const [deadline, setDeadline] = useState('') // local datetime-local value
    const [loadingList, setLoadingList] = useState(true)
    const [listError, setListError] = useState('')

    const [userChallengeId, setUserChallengeId] = useState(Number(localStorage.getItem('challengeId') || 0))

    const canSubmit = useMemo(() =>
        !!userId && name.trim().length > 0 && !creating && !userChallengeId && !!selectedTaskId,
        [userId, name, creating, userChallengeId, selectedTaskId])

    function toIsoForServer(dtLocal) {
        if (!dtLocal) return null
        const d = new Date(dtLocal)
        return d.toISOString()
    }

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

        // load task list for challenge creation
        let tmounted = true
        ;(async function loadTasks() {
            try {
                const tres = await getTasks()
                if (!tmounted || !tres.ok) return
                const data = Array.isArray(tres.data) ? tres.data : []
                if (mounted) {
                  setTasks(data)
                  if (!selectedTaskId && data.length) setSelectedTaskId(String(data[0].id ?? data[0].Id))
                }
            } catch {}
        })()

        return () => { mounted = false; tmounted = false }
    }, [userId])

    useEffect(() => { return () => {} }, [tasks])

    async function handleCreate(e) {
        e.preventDefault()
        if (!userId) { setMessage('You must be logged in.'); return }
        if (userChallengeId) { setMessage('Leave your current challenge before creating a new one.'); return }
        if (!name.trim()) { setMessage('Challenge name is required.'); return }
        if (!selectedTaskId) { setMessage('Please choose a task for the challenge.'); return }
        setCreating(true); setMessage('Creating challenge...')

        const iso = toIsoForServer(deadline)
        const res = await createChallenge(name.trim(), userId, Number(selectedTaskId), iso, isPrivate)
        if (!res.ok) {
            const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
            setMessage(`Error: ${err}`)
            setCreating(false)
            return
        }

        // Prefer to fetch full challenge details so we know membership & roles (so canDelete is accurate)
        const created = res.data
        const createdId = created?.id ?? created?.Id ?? null
        let detail = null
        if (createdId) {
            try {
                const dres = await getChallengeById(createdId)
                if (dres.ok) detail = dres.data
            } catch {}
        }

        // If we got full detail, compute canDelete/admin flag
        const finalEntry = detail ?? created
        // ensure canDelete true for creator if we don't yet have members info
        finalEntry.canDelete = !!(
            (detail && Array.isArray(detail.members) && detail.members.some(m => Number(m.userId ?? m.UserId ?? 0) === userId && (m.role ?? m.Role) === 1))
            || (created && (created.creatorId ?? created.CreatorId) === userId)
        )

        // persist membership for creator
        if (createdId) {
            localStorage.setItem('challengeId', String(createdId))
            localStorage.setItem('challengeName', (finalEntry.name ?? finalEntry.Name) || '')
            setUserChallengeId(Number(createdId))
        }

        setMyChallenges(prev => [{ ...finalEntry }, ...prev])
        setMessage('Challenge created.')
        setName('')
        setIsPrivate(false)
        setSelectedTaskId('')
        setDeadline('')
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

                {/* NEW: Task picker (required) */}
                <div style={{ marginBottom: 12 }}>
                    <label style={{ display: 'block', marginBottom: 6 }}>Choose Task</label>
                    <select
                        value={selectedTaskId}
                        onChange={e => setSelectedTaskId(e.target.value)}
                        disabled={!userId || creating || !!userChallengeId}
                        style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                    >
                        <option value="">-- choose a task --</option>
                        {tasks.map(t => (
                            <option key={t.id ?? t.Id} value={t.id ?? t.Id}>
                                {String(t.id ?? t.Id)} — {t.description ?? t.Description ?? '(no description)'}
                            </option>
                        ))}
                    </select>
                    <div style={{ fontSize: 12, color: '#666', marginTop: 6 }}>
                      Deadline optional — leave blank to default (7 days).
                    </div>
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
                                        <>
                                            <button onClick={() => handleDelete(h)} style={{ marginRight: 8 }}>Delete</button>
                                            <button onClick={async () => {
                                                setMessage('Advancing stage...')
                                                try {
                                                    const cid = h.id ?? h.Id
                                                    const res = await advanceChallenge(cid, userId)
                                                    if (!res.ok) {
                                                        const err = (res.data && (res.data.error || res.data.message)) || res.text || `Error ${res.status}`
                                                        setMessage(`Error: ${err}`)
                                                    } else {
                                                        setMessage('Stage advanced.')
                                                        // update this challenge in local list
                                                        const updated = res.data
                                                        setMyChallenges(prev => prev.map(x => (x.id ?? x.Id) === (updated.id ?? updated.Id) ? updated : x))
                                                    }
                                                } catch (e) {
                                                    setMessage(String(e))
                                                }
                                            }}>Advance Stage</button>
                                        </>
                                    ) : (
                                        <button disabled title="Only challenge admins can delete or advance this challenge">Admin only</button>
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