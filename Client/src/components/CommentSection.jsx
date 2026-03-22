import React, { useState, useEffect, useRef } from 'react'
import ReactDOM from 'react-dom'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { API_BASE, getComments, addComment, deleteComment, addReaction, removeReaction, getReactions } from '../services/api.js'
import EmojiPicker from './EmojiPicker.jsx'

const MAX_COMMENT_LENGTH = 500

export default function CommentSection({ submissionId, currentUserId }) {
  const [comments, setComments] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [newComment, setNewComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [expanded, setExpanded] = useState(false)
  const [showEmojiPicker, setShowEmojiPicker] = useState(null) // commentId or null
  const [commentReactions, setCommentReactions] = useState({}) // { commentId: [reactions] }
  const [pickerPosition, setPickerPosition] = useState({ top: 0, left: 0 })
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

      conn.on('ReactionsUpdated', updatedCommentId => {
        loadReactionsForComment(updatedCommentId)
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
      
      // Load reactions for all comments
      for (const comment of data) {
        const commentId = comment.id ?? comment.Id
        await loadReactionsForComment(commentId)
      }
    } catch (e) {
      setError('Error loading comments')
    } finally {
      setLoading(false)
    }
  }

  async function loadReactionsForComment(commentId) {
    try {
      const res = await getReactions(commentId)
      if (res.ok && Array.isArray(res.data)) {
        setCommentReactions(prev => ({
          ...prev,
          [commentId]: res.data
        }))
      }
    } catch (e) {
      console.error('Error loading reactions:', e)
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

  async function handleAddReaction(commentId, emoji) {
    try {
      const res = await addReaction(commentId, currentUserId, emoji)
      if (res.ok) {
        await loadReactionsForComment(commentId)
      } else {
        console.error('Failed to add reaction:', res.text)
      }
    } catch (e) {
      console.error('Error adding reaction:', e)
    }
  }

  async function handleRemoveReaction(commentId, emoji) {
    try {
      const res = await removeReaction(commentId, currentUserId, emoji)
      if (res.ok) {
        await loadReactionsForComment(commentId)
      } else {
        console.error('Failed to remove reaction:', res.text)
      }
    } catch (e) {
      console.error('Error removing reaction:', e)
    }
  }

  function handleReactionButtonClick(commentId, event) {
    // Toggle off if already showing for this comment
    if (showEmojiPicker === commentId) {
      setShowEmojiPicker(null)
      return
    }

    const rect = event.currentTarget.getBoundingClientRect()
    const PICKER_QUICK_H = 80   // quick picker is ~1 row, roughly 80px
    const PICKER_W = 360        // max width (full picker is 350)
    const MARGIN = 8

    // Default: position below the button, aligned to its left edge
    let top = rect.bottom + MARGIN
    let left = rect.left

    // If not enough space below for even the quick picker, position above
    const spaceBelow = window.innerHeight - rect.bottom
    if (spaceBelow < PICKER_QUICK_H + MARGIN) {
      top = rect.top - PICKER_QUICK_H - MARGIN
    }

    // Clamp horizontally so it doesn't go off the right edge
    if (left + PICKER_W > window.innerWidth) {
      left = window.innerWidth - PICKER_W - MARGIN
    }
    // Don't go off the left edge either
    if (left < MARGIN) {
      left = MARGIN
    }

    // Clamp vertically — never go above the viewport
    if (top < MARGIN) {
      top = MARGIN
    }

    setPickerPosition({ top, left })
    setShowEmojiPicker(commentId)
  }

  function getReactionSummary(commentId) {
    const reactions = commentReactions[commentId] || []
    const summary = {}
    
    reactions.forEach(r => {
      const emoji = r.emoji ?? r.Emoji
      if (!summary[emoji]) {
        summary[emoji] = { count: 0, users: [] }
      }
      summary[emoji].count++
      summary[emoji].users.push(r.userName ?? r.UserName ?? `User ${r.userId ?? r.UserId}`)
    })
    
    return summary
  }

  function hasUserReacted(commentId, emoji) {
    const reactions = commentReactions[commentId] || []
    return reactions.some(r => 
      (r.userId ?? r.UserId) === currentUserId && 
      (r.emoji ?? r.Emoji) === emoji
    )
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
                const reactionSummary = getReactionSummary(commentId)

                return (
                  <div
                    key={commentId}
                    style={{
                      background: 'rgba(100, 108, 255, 0.1)',
                      borderRadius: 8,
                      padding: 12,
                      marginBottom: 8,
                      position: 'relative'
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

                    {/* Reactions section */}
                    <div style={{ marginTop: 8, display: 'flex', alignItems: 'center', gap: 6, flexWrap: 'wrap' }}>
                      {/* Add reaction button */}
                      <button
                        onClick={(e) => handleReactionButtonClick(commentId, e)}
                        title="Add reaction"
                        style={{
                          background: 'rgba(255, 255, 255, 0.05)',
                          border: '1px solid rgba(255, 255, 255, 0.2)',
                          borderRadius: 4,
                          padding: 2,
                          cursor: 'pointer',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          transition: 'all 0.2s',
                          width: 24,
                          height: 24
                        }}
                        onMouseEnter={e => {
                          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.1)'
                        }}
                        onMouseLeave={e => {
                          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.05)'
                        }}
                      >
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 160 160" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                          <circle cx="72" cy="72" r="68"/>
                          <circle cx="52" cy="62" r="7" fill="currentColor" stroke="none"/>
                          <circle cx="92" cy="62" r="7" fill="currentColor" stroke="none"/>
                          <path d="M50 92 Q72 116 94 92"/>
                          <line x1="128" y1="140" x2="148" y2="140" strokeWidth="4"/>
                          <line x1="138" y1="130" x2="138" y2="150" strokeWidth="4"/>
                        </svg>
                      </button>

                      {/* Display reactions */}
                      {Object.entries(reactionSummary).map(([emoji, data]) => {
                        const userReacted = hasUserReacted(commentId, emoji)
                        return (
                          <button
                            key={emoji}
                            onClick={() => userReacted ? handleRemoveReaction(commentId, emoji) : handleAddReaction(commentId, emoji)}
                            title={data.users.join(', ')}
                            style={{
                              background: userReacted ? 'rgba(100, 108, 255, 0.3)' : 'rgba(255, 255, 255, 0.05)',
                              border: userReacted ? '1px solid rgba(100, 108, 255, 0.5)' : '1px solid rgba(255, 255, 255, 0.2)',
                              borderRadius: 6,
                              padding: '4px 8px',
                              cursor: 'pointer',
                              display: 'flex',
                              alignItems: 'center',
                              gap: 4,
                              fontSize: 14,
                              color: 'rgba(255, 255, 255, 0.9)',
                              transition: 'all 0.2s'
                            }}
                            onMouseEnter={e => {
                              e.currentTarget.style.transform = 'scale(1.05)'
                            }}
                            onMouseLeave={e => {
                              e.currentTarget.style.transform = 'scale(1)'
                            }}
                          >
                            <span>{emoji}</span>
                            <span style={{ fontSize: 12 }}>{data.count}</span>
                          </button>
                        )
                      })}
                    </div>
                  </div>
                )
              })}
            </div>
          )}

          {/* Portal the emoji picker to document.body so it escapes all overflow clipping */}
          {showEmojiPicker != null && ReactDOM.createPortal(
            <EmojiPicker
              position={pickerPosition}
              onSelect={(emoji) => handleAddReaction(showEmojiPicker, emoji)}
              onClose={() => setShowEmojiPicker(null)}
            />,
            document.body
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

// Made with Bob