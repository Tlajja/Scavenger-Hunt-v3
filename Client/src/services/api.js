export const API_BASE = 'http://localhost:5248'

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

export async function getTaskById(id) {
  return await safeFetch(`/api/tasks/${Number(id)}`, { method: 'GET' })
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

export async function getHallOfFame(top = 10) {
  return await safeFetch(`/api/leaderboard/halloffame?top=${Number(top)}`, { method: 'GET' })
}

// Challenges endpoints
export async function joinChallenge(joinCode, userId) {
 // backend generates uppercase alphanumeric join codes, normalize input
 const code = (joinCode || '').trim().toUpperCase()
 return await safeFetch('/api/challenge/join', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ JoinCode: code, UserId: Number(userId) })
 })
}

export async function getChallenges(publicOnly = true) {
 const q = publicOnly ? 'true' : 'false'
 return await safeFetch(`/api/challenge?publicOnly=${q}`, { method: 'GET' })
}

export async function getChallengeById(id) {
 return await safeFetch(`/api/challenge/${id}`, { method: 'GET' })
}

export async function createChallenge(name, creatorId, taskId, deadlineIso = null, isPrivate = false) {
  const payload = {
    Name: name,
    CreatorId: Number(creatorId),
    TaskId: Number(taskId),
    IsPrivate: !!isPrivate,
  }
  if (deadlineIso) payload.Deadline = deadlineIso
  return await safeFetch('/api/challenge', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })
}

export async function leaveChallenge(challengeId, userId) {
  return await safeFetch(`/api/challenge/${challengeId}/leave?userId=${Number(userId)}`, {
    method: 'DELETE'
  })
}

export async function deleteChallenge(challengeId, userId) {
 return await safeFetch(`/api/challenge/${challengeId}?userId=${Number(userId)}`, {
 method: 'DELETE'
 })
}

export async function advanceChallenge(challengeId, userId) {
  return await safeFetch(`/api/challenge/${Number(challengeId)}/advance?userId=${Number(userId)}`, {
    method: 'POST'
  })
}

// NEW: get challenges the user participates in (private + public)
export async function getMyChallenges(userId) {
 return await safeFetch(`/api/challenge/mine?userId=${Number(userId)}`, { method: 'GET' })
}

// Comments endpoints
export async function getComments(submissionId) {
  return await safeFetch(`/api/comments/${Number(submissionId)}`, { method: 'GET' })
}

export async function addComment(submissionId, userId, text) {
  return await safeFetch(`/api/comments/${Number(submissionId)}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ UserId: Number(userId), Text: text })
  })
}

export async function deleteComment(submissionId, commentId) {
  return await safeFetch(`/api/comments/${Number(submissionId)}/${Number(commentId)}`, {
    method: 'DELETE'
  })
}