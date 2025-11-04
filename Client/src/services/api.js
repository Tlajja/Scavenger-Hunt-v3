export const API_BASE = ''

async function safeFetch(url, opts) {
 try {
 const res = await fetch(`${API_BASE}${url}`, opts)
 const text = await res.text()
 let data = null
 try { data = text ? JSON.parse(text) : null } catch { data = text }
 return { ok: res.ok, status: res.status, data, text }
 } catch {
 return { ok: false, status:0, data: null, text: 'Network error' }
 }
}

// Authentication endpoints
export async function register(email, password, username, age) {
 return await safeFetch('/api/authentication/register', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ Email: email, Password: password, Username: username, Age: age })
 })
}

export async function login(username, password) {
 return await safeFetch('/api/authentication/login', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ Username: username, Password: password })
 })
}

// Task endpoints
export async function getTasks() {
 return await safeFetch('/api/tasks', { method: 'GET' })
}

export async function createTask(description, deadline) {
 return await safeFetch('/api/tasks', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ Description: description, Deadline: deadline, AuthorId:0 })
 })
}

export async function createUserTask(description, deadline, authorId) {
 return await safeFetch('/api/tasks/user', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ Description: description, Deadline: deadline, AuthorId: authorId })
 })
}

export async function submitPhoto(taskId, userId, photoFile) {
 const formData = new FormData()
 formData.append('taskId', taskId)
 formData.append('userId', userId)
 formData.append('file', photoFile)

 const response = await fetch('http://localhost:5248/api/photosubmissions/upload', {
 method: 'POST',
 body: formData
 })

 const text = await response.text()
 let data = null
 try {
 data = text ? JSON.parse(text) : null
 } catch {
 data = text
 }

 return {
 ok: response.ok,
 status: response.status,
 data: data,
 text: text
 }
}

// NEW: Upload photo file with streaming
export async function uploadPhotoFile(taskId, userId, file) {
 const formData = new FormData()
 formData.append('file', file)
 formData.append('taskId', taskId)
 formData.append('userId', userId)

 try {
 const res = await fetch(`${API_BASE}/api/photoupload/upload`, { 
 method: 'POST',
 body: formData
 })
 const text = await res.text()
 let data = null
 try { data = text ? JSON.parse(text) : null } catch { data = text }
 return { ok: res.ok, status: res.status, data, text }
 } catch {
 return { ok: false, status:0, data: null, text: 'Network error' }
 }
}

// Leaderboard endpoint
export async function getLeaderboard() {
 return await safeFetch('/api/leaderboard', { method: 'GET' })
}

// Hubs endpoints
export async function joinHub(joinCode, userId) {
 // backend generates uppercase alphanumeric join codes, normalize input
 const code = (joinCode || '').trim().toUpperCase()
 return await safeFetch('/api/hub/join', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ JoinCode: code, UserId: Number(userId) })
 })
}

export async function getHubs(publicOnly = true) {
 const q = publicOnly ? 'true' : 'false'
 return await safeFetch(`/api/hub?publicOnly=${q}`, { method: 'GET' })
}

export async function getHubById(id) {
 return await safeFetch(`/api/hub/${id}`, { method: 'GET' })
}

// NEW: Create a hub
export async function createHub(name, creatorId, isPrivate = false) {
 return await safeFetch('/api/hub', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ Name: name, CreatorId: Number(creatorId), IsPrivate: !!isPrivate })
 })
}

// NEW: Delete a hub (must be creator)
export async function deleteHub(hubId, userId) {
 return await safeFetch(`/api/hub/${hubId}?userId=${Number(userId)}`, {
 method: 'DELETE'
 })
}
