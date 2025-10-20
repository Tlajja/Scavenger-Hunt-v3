import React, { useState, useEffect } from 'react'
import { submitPhoto, getTasks } from '../services/api.js'

export default function SubmitPhoto() {
  const [photoFile, setPhotoFile] = useState(null)
  const [previewUrl, setPreviewUrl] = useState(null)
  const [message, setMessage] = useState('')
  const [taskId, setTaskId] = useState('')
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [uploading, setUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)
  const userId = localStorage.getItem('userId') || ''

  useEffect(() => {
    let mounted = true
    async function load() {
      setLoading(true)
      setError('')
      try {
        const res = await getTasks()
        let data = res.data
        if (!mounted) return
        setTasks(Array.isArray(data) ? data : [])
      } catch (err) {
        if (!mounted) return
        setError(String(err))
      } finally {
        if (!mounted) return
        setLoading(false)
      }
    }
    load()
    return () => { mounted = false }
  }, [])

  function handleFileChange(e) {
    const file = e.target.files[0]
    if (!file) {
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif']
    if (!allowedTypes.includes(file.type)) {
      setMessage('Only JPG, PNG, and GIF images are allowed.')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    if (file.size > 10_000_000) {
      setMessage('File size must be less than 10MB.')
      setPhotoFile(null)
      setPreviewUrl(null)
      return
    }

    setPhotoFile(file)
    setMessage('')

    const reader = new FileReader()
    reader.onloadend = () => {
      setPreviewUrl(reader.result)
    }
    reader.readAsDataURL(file)
  }

  function clearPreview() {
    setPhotoFile(null)
    setPreviewUrl(null)
    setUploadProgress(0)
    setMessage('')
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setMessage('')

    if (!userId) { 
      setMessage('You must be logged in to submit.')
      return 
    }
    if (!taskId) { 
      setMessage('Please select a task.')
      return 
    }
    if (!photoFile) { 
      setMessage('Please select a photo file.')
      return 
    }

    setUploading(true)
    setUploadProgress(0)

    try {
      const result = await uploadWithProgress(taskId, userId, photoFile)

      if (!result.ok) {
        const err = (result.data && (result.data.message || result.data.error)) || result.text || `Error ${result.status}`
        setMessage(`Error: ${err}`)
        return
      }

      setMessage('✅ Photo uploaded successfully!')

      setTimeout(() => {
        clearPreview()
        setTaskId('')
        e.target.reset()
      }, 2000)

    } catch (err) {
      console.error('Upload error:', err)
      setMessage(`Network error: ${err.message}`)
    } finally {
      setUploading(false)
    }
  }

  function uploadWithProgress(taskId, userId, file) {
    return new Promise((resolve, reject) => {
      const formData = new FormData()
      formData.append('taskId', taskId)
      formData.append('userId', userId)
      formData.append('file', file)

      const xhr = new XMLHttpRequest()
      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          const percentComplete = Math.round((e.loaded / e.total) * 100)
          setUploadProgress(percentComplete)
        }
      }

      xhr.onload = () => {
        const text = xhr.responseText
        let data = null
        try { data = text ? JSON.parse(text) : null } catch { data = text }

        resolve({
          ok: xhr.status >= 200 && xhr.status < 300,
          status: xhr.status,
          data: data,
          text: text
        })
      }

      xhr.onerror = () => reject(new Error('Network error occurred'))
      xhr.onabort = () => reject(new Error('Upload cancelled'))

      xhr.open('POST', 'http://localhost:5248/api/photosubmissions/upload')
      xhr.send(formData)
    })
  }

  if (loading) return <div>Loading tasks…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error loading tasks: {error}</div>

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 480, margin: '0 auto' }}>
      <h2>Submit Photo</h2>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Select Task</label>
        <select
          value={taskId}
          onChange={e => setTaskId(e.target.value)}
          disabled={uploading}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        >
          <option value="">-- choose a task --</option>
          {tasks.map(t => (
            <option key={t.id ?? t.Id ?? `${t.description}-${t.deadline}`} value={t.id ?? t.Id}>
              {`${t.id ?? t.Id ?? ''} — ${t.description ?? t.Description ?? '(no description)'}`}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: 10 }}>
        <label style={{ display: 'block', marginBottom: 6 }}>Upload Photo</label>
        <input
          type="file"
          accept="image/jpeg,image/png,image/gif"
          onChange={handleFileChange}
          disabled={uploading}
          style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
        />
      </div>

      {/* Image Preview */}
      {previewUrl && (
        <div style={{ 
          marginBottom: 15,
          borderRadius: 4,
          overflow: 'hidden',
          backgroundColor: 'inherit', // use default background
          textAlign: 'center'
        }}>
          <img 
            src={previewUrl} 
            alt="Preview" 
            style={{ 
              width: '100%', 
              maxHeight: 300, 
              objectFit: 'contain'
            }} 
          />
          <div style={{ marginTop: 5 }}>
            <button 
              type="button"
              onClick={clearPreview}
              style={{ cursor: 'pointer', padding: '4px 8px', marginTop: 5 }}
            >
              Remove
            </button>
          </div>
        </div>
      )}

      {/* Upload Progress */}
      {uploading && (
        <div style={{ marginBottom: 15 }}>
          <div style={{ fontSize: '12px', marginBottom: 5 }}>{uploadProgress}%</div>
          <div style={{ 
            width: '100%', 
            height: 10, 
            backgroundColor: '#ccc', 
            borderRadius: 5,
            overflow: 'hidden'
          }}>
            <div style={{ 
              width: `${uploadProgress}%`, 
              height: '100%', 
              backgroundColor: '#2196F3'
            }}></div>
          </div>
        </div>
      )}

      {!userId && (
        <div style={{ color: 'crimson', marginBottom: 10 }}>
          You must be logged in to submit photos.
        </div>
      )}

      <button 
        type="submit" 
        disabled={!taskId || !photoFile || !userId || uploading}
        style={{
          width: '100%',
          padding: 12,
          fontSize: '16px',
          cursor: uploading || !taskId || !photoFile || !userId ? 'not-allowed' : 'pointer',
          opacity: uploading || !taskId || !photoFile || !userId ? 0.6 : 1
        }}
      >
        {uploading ? `Uploading... ${uploadProgress}%` : 'Submit Photo'}
      </button>

      {message && (
        <div style={{ 
          marginTop: 10,
          padding: 10,
          borderRadius: 4,
          backgroundColor: message.includes('Error') || message.includes('must') ? '#ffebee' : '#e8f5e9',
          color: message.includes('Error') || message.includes('must') ? '#c62828' : '#2e7d32',
          border: `1px solid ${message.includes('Error') || message.includes('must') ? '#ef9a9a' : '#a5d6a7'}`
        }}>
          {message}
        </div>
      )}
    </form>
  )
}
