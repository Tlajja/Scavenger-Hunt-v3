import React, { useEffect, useState } from 'react'
import { joinHub, getHubs, getHubById } from '../services/api.js'

export default function JoinHub() {
 const [joinCode, setJoinCode] = useState('')
 const [message, setMessage] = useState('')
 const [joining, setJoining] = useState(false)
 const [joinedHub, setJoinedHub] = useState(null)
 const [publicHubs, setPublicHubs] = useState([])

 const userId = Number(localStorage.getItem('userId') ||0)
 const userName = localStorage.getItem('username') || 'Guest'

 useEffect(() => {
 loadPublicHubs()
 }, [])

 async function loadPublicHubs() {
 const res = await getHubs(true)
 if (res.ok && Array.isArray(res.data)) setPublicHubs(res.data)
 }

 async function performJoin(code) {
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
 // try to get hub from payload; otherwise fetch by id
 let hub = member?.hub || member?.Hub || null
 let resolvedHubId = member?.hubId ?? member?.HubId ?? null
 if (!hub && resolvedHubId) {
 const hres = await getHubById(resolvedHubId)
 if (hres.ok) hub = hres.data
 }
 const finalHubId = (hub?.id ?? hub?.Id ?? resolvedHubId) ?? null
 if (finalHubId != null) {
 localStorage.setItem('hubId', String(finalHubId))
 const hubName = hub?.name ?? hub?.Name
 if (hubName) localStorage.setItem('hubName', hubName)
 }
 setJoinedHub(hub)
 setMessage(`Joined hub${hub?.name ? `: ${hub.name}` : ''}`)
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
 if (!userId) { setMessage('You must be logged in to join a hub.'); return }
 const code = (h?.joinCode ?? h?.JoinCode ?? '').trim()
 if (!code) { setMessage('This hub does not expose a join code.'); return }
 await performJoin(code)
 }

 return (
 <div style={{ maxWidth:520 }}>
 <h2>Join a Hub</h2>

 <form onSubmit={handleSubmit}>
 <div style={{ marginBottom:12 }}>
 <label style={{ display: 'block', marginBottom:6 }}>Join Code</label>
 <input
 value={joinCode}
 onChange={e => setJoinCode(e.target.value)}
 placeholder="e.g. ABC123"
 style={{ width: '100%', padding:8, boxSizing: 'border-box' }}
 disabled={joining}
 />
 </div>

 {!userId && (
 <div style={{ color: 'crimson', marginBottom:10 }}>
 You must be logged in. Current: {userName}
 </div>
 )}

 <button type="submit" disabled={!userId || joining}>
 {joining ? 'Joining…' : 'Join Hub'}
 </button>
 </form>

 {message && (
 <div style={{ marginTop:10 }}>
 {message}
 </div>
 )}

 <hr style={{ margin: '20px 0' }} />

 <div>
 <div style={{ display: 'flex', alignItems: 'center', gap:8 }}>
 <h3 style={{ margin:0 }}>Public Hubs</h3>
 <button onClick={loadPublicHubs} disabled={joining}>Refresh</button>
 </div>
 {publicHubs.length ===0 ? (
 <div style={{ marginTop:8, color: '#666' }}>No public hubs available.</div>
 ) : (
 <ul style={{ listStyle: 'none', padding:0, marginTop:12 }}>
 {publicHubs.map(h => (
 <li key={h.id ?? h.Id} style={{ border: '1px solid #eee', padding:10, marginBottom:8 }}>
 <div style={{ fontWeight:600 }}>{h.name ?? h.Name}</div>
 <div style={{ fontSize:12, color: '#666' }}>Id: {h.id ?? h.Id}</div>
 {h.joinCode || h.JoinCode ? (
 <div style={{ fontSize:12, color: '#666' }}>Join Code: {h.joinCode ?? h.JoinCode}</div>
 ) : null}
 <div style={{ marginTop:8 }}>
 <button onClick={() => handleQuickJoin(h)} disabled={!userId || joining}>Join</button>
 </div>
 </li>
 ))}
 </ul>
 )}
 </div>

 {joinedHub && (
 <div style={{ marginTop:20, padding:12, border: '1px solid #e0e0e0' }}>
 <div style={{ fontWeight:600 }}>Joined Hub</div>
 <div>Name: {joinedHub.name ?? joinedHub.Name}</div>
 <div>Id: {joinedHub.id ?? joinedHub.Id}</div>
 <div>Join Code: {joinedHub.joinCode ?? joinedHub.JoinCode}</div>
 </div>
 )}
 </div>
 )
}