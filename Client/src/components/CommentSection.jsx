import React, { useState, useEffect, useRef } from 'react'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { API_BASE, getComments, addComment, deleteComment } from '../services/api.js'

const MAX_COMMENT_LENGTH = 500

export default function CommentSection({ submissionId, currentUserId }) {
  const [comments, setComments] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [newComment, setNewComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [expanded, setExpanded] = useState(false)
  const connectionRef = useRef(null)

  useEffect(() => {
    if (submissionId) {
      loadComments()
    }
  }, [submissionId])

  useEffect(() => {
    if (expanded && submissionId) {
      loadComments()
    }
  }, [expanded, submissionId])

  useEffect(() => {
    let disposed = false

    async function start() {
      if (!submissionId) return

      if (connectionRef.current) {
        try { await connectionRef.current.stop() } catch {}
        connectionRef.current = null
      }

      const base = (API_BASE || '').replace(/\/$/, '')
      const url = `${base}/hubs/comments`

      const conn = new HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .configureLogging(
          import.meta.env.DEV ? LogLevel.Information : LogLevel.Error
        )
        .build()

      conn.on('CommentsUpdated', updatedSubmissionId => {
        if (Number(updatedSubmissionId) === Number(submissionId)) {
          loadComments()
        }
      })

      try {
        await conn.start()
        if (disposed) {
          await conn.stop().catch(() => {})
          return
        }
        connectionRef.current = conn
        await conn.invoke('JoinSubmission', Number(submissionId))
      } catch {
      }
    }

    start()

    return () => {
      disposed = true
      if (connectionRef.current) {
        const c = connectionRef.current
        connectionRef.current = null
        c.stop().catch(() => {})
      }
    }
  }, [submissionId])

  async function loadComments() {
    setLoading(true)
    setError('')
    try {
      const res = await getComments(submissionId)
      if (!res.ok) {
        setError('Failed to load comments')
        return
      }
      const data = Array.isArray(res.data) ? res.data : []
      setComments(data)
    } catch (e) {
      setError('Error loading comments')
    } finally {
      setLoading(false)
    }
  }

  async function handleAddComment(e) {
    e.preventDefault()
    if (!newComment.trim() || !currentUserId) return
    
    if (newComment.length > MAX_COMMENT_LENGTH) {
      setError(`Comment cannot exceed ${MAX_COMMENT_LENGTH} characters.`)
      return
    }

    setSubmitting(true)
    setError('')
    const commentText = newComment.trim()
    setNewComment('')
    
    try {
      const res = await addComment(submissionId, currentUserId, commentText)
      if (!res.ok) {
        const errMsg = res.data?.message || res.text || 'Failed to add comment'
        setError(errMsg)
        setNewComment(commentText)
        return
      }
      
      if (res.data && Array.isArray(res.data)) {
        setComments(res.data)
      } else {
        await loadComments()
      }
    } catch (e) {
      setError('Error adding comment')
      setNewComment(commentText)
      await loadComments()
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDeleteComment(commentId) {
    if (!confirm('Delete this comment?')) return

    try {
      const res = await deleteComment(submissionId, commentId)
      if (!res.ok) {
        setError('Failed to delete comment')
        return
      }
      await loadComments()
    } catch (e) {
      setError('Error deleting comment')
    }
  }

  function formatTimestamp(timestamp) {
    if (!timestamp) return ''
    
    const timestampStr = typeof timestamp === 'string' && !timestamp.endsWith('Z') && !timestamp.includes('+') && !timestamp.includes('-', 10)
      ? timestamp + 'Z'
      : timestamp
    
    const date = new Date(timestampStr)
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    const diffMins = Math.floor(diffMs / 60000)
    const diffHours = Math.floor(diffMs / 3600000)
    const diffDays = Math.floor(diffMs / 86400000)

    if (diffMs < 0 || diffMins < 1) return 'just now'
    if (diffMins < 60) return `${diffMins}m ago`
    if (diffHours < 24) return `${diffHours}h ago`
    if (diffDays < 7) return `${diffDays}d ago`
    return date.toLocaleDateString()
  }

  return (
    <div style={{ marginTop: 16 }}>
      <button
        onClick={() => setExpanded(!expanded)}
        style={{
          background: 'transparent',
          border: '1px solid rgba(255, 255, 255, 0.3)',
          color: 'rgba(255, 255, 255, 0.8)',
          padding: '6px 12px',
          borderRadius: 6,
          cursor: 'pointer',
          fontSize: 14
        }}
      >
        {expanded ? '▼' : '▶'} Comments ({comments.length})
      </button>

      {expanded && (
        <div style={{ marginTop: 12 }}>
          {error && (
            <div style={{ color: '#ff6b6b', fontSize: 14, marginBottom: 8 }}>
              {error}
            </div>
          )}

          {loading && <div style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: 14 }}>Loading comments...</div>}

          {!loading && comments.length === 0 && (
            <div style={{ color: 'rgba(255, 255, 255, 0.5)', fontSize: 14, fontStyle: 'italic' }}>
              No comments yet. Be the first to comment!
            </div>
          )}

          {!loading && comments.length > 0 && (
            <div style={{ marginBottom: 12, maxHeight: 300, overflowY: 'auto' }}>
              {comments.map(comment => {
                const commentId = comment.id ?? comment.Id
                const userId = comment.userId ?? comment.UserId
                const userName = comment.userName ?? comment.UserName ?? `User ${userId}`
                const text = comment.text ?? comment.Text ?? ''
                const timestamp = comment.timestamp ?? comment.Timestamp
                const isOwner = Number(userId) === Number(currentUserId)

                return (
                  <div
                    key={commentId}
                    style={{
                      background: 'rgba(100, 108, 255, 0.1)',
                      borderRadius: 8,
                      padding: 12,
                      marginBottom: 8
                    }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <div style={{ flex: 1 }}>
                        <div style={{ fontSize: 12, color: 'rgba(255, 255, 255, 0.6)', marginBottom: 4 }}>
                          {userName} • {formatTimestamp(timestamp)}
                        </div>
                        <div style={{ color: 'rgba(255, 255, 255, 0.9)', fontSize: 14, wordWrap: 'break-word', overflowWrap: 'break-word', wordBreak: 'break-word', whiteSpace: 'pre-wrap', maxWidth: '100%' }}>
                          {text}
                        </div>
                      </div>
                      {isOwner && (
                        <button
                          onClick={() => handleDeleteComment(commentId)}
                          style={{
                            background: 'transparent',
                            border: 'none',
                            color: '#ff6b6b',
                            cursor: 'pointer',
                            fontSize: 12,
                            padding: '4px 8px'
                          }}
                        >
                          ×
                        </button>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          )}

          {currentUserId && (
            <form onSubmit={handleAddComment} style={{ marginTop: 8 }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
                <div style={{ display: 'flex', gap: 8 }}>
                  <input
                    type="text"
                    value={newComment}
                    onChange={e => {
                      if (e.target.value.length <= MAX_COMMENT_LENGTH) {
                        setNewComment(e.target.value)
                      }
                    }}
                    placeholder="Add a comment..."
                    disabled={submitting}
                    maxLength={MAX_COMMENT_LENGTH}
                    style={{
                      flex: 1,
                      padding: '8px 12px',
                      background: 'rgba(255, 255, 255, 0.1)',
                      border: newComment.length > MAX_COMMENT_LENGTH 
                        ? '1px solid #ff6b6b' 
                        : '1px solid rgba(255, 255, 255, 0.2)',
                      borderRadius: 6,
                      color: 'white',
                      fontSize: 14
                    }}
                  />
                  <button
                    type="submit"
                    disabled={!newComment.trim() || submitting || newComment.length > MAX_COMMENT_LENGTH}
                    style={{
                      padding: '8px 16px',
                      background: submitting || !newComment.trim() || newComment.length > MAX_COMMENT_LENGTH
                        ? 'rgba(100, 108, 255, 0.5)' 
                        : '#646cff',
                      border: 'none',
                      borderRadius: 6,
                      color: 'white',
                      cursor: submitting || !newComment.trim() || newComment.length > MAX_COMMENT_LENGTH
                        ? 'not-allowed' 
                        : 'pointer',
                      fontSize: 14
                    }}
                  >
                    {submitting ? '...' : 'Post'}
                  </button>
                </div>
                <div style={{ 
                  fontSize: 12, 
                  color: newComment.length > MAX_COMMENT_LENGTH 
                    ? '#ff6b6b' 
                    : newComment.length > MAX_COMMENT_LENGTH * 0.9
                    ? 'rgba(255, 255, 255, 0.7)'
                    : 'rgba(255, 255, 255, 0.5)',
                  textAlign: 'right',
                  paddingRight: 4
                }}>
                  {newComment.length} / {MAX_COMMENT_LENGTH}
                </div>
              </div>
            </form>
          )}
        </div>
      )}
    </div>
  )
}

