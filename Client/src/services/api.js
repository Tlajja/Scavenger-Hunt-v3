export const API_BASE = ''

async function safeFetch(url, opts) {
  try {
    const res = await fetch(`${API_BASE}${url}`, opts)
    const text = await res.text()
    let data = null
    try { data = text ? JSON.parse(text) : null } catch { data = text }
    return { ok: res.ok, status: res.status, data, text }
  } catch (err) {
    return { ok: false, status: 0, data: null, text: 'Network error' }
  }
}

export async function register(email, password) {
  return await safeFetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Email: email, Password: password })
  })
}

export async function login(username, password) {
  return await safeFetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Username: username, Password: password })
  })
}

export async function createUsername(userId, username, age) {
  return await safeFetch(`/api/auth/create-username?userId=${userId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Username: username, Age: age })
  })
}

export async function getTasks() {
  return await safeFetch('/api/tasks', { method: 'GET' })
}

export async function submitPhoto(taskId, userId, photoUrl) {
  return await safeFetch('/api/submissions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ TaskId: taskId, UserId: userId, PhotoUrl: photoUrl })
  })
}

export async function getLeaderboard() {
  return await safeFetch('/api/leaderboard', { method: 'GET' })
}