import React, { useState, useRef, useLayoutEffect, useEffect, useCallback } from 'react'
import 'emoji-picker-element'

const QUICK_EMOJIS = ['👍', '❤️', '😂', '🔥', '💯']
const MARGIN = 8

export default function EmojiPicker({ onSelect, onClose, position }) {
  const [showFullPicker, setShowFullPicker] = useState(false)
  const [adjustedPos, setAdjustedPos] = useState(position)
  const containerRef = useRef(null)
  const pickerRef = useRef(null)

  const clampToViewport = useCallback(() => {
    if (!containerRef.current) return
    const rect = containerRef.current.getBoundingClientRect()
    let top = adjustedPos.top
    let left = adjustedPos.left

    if (rect.bottom > window.innerHeight - MARGIN) {
      top = window.innerHeight - rect.height - MARGIN
    }
    if (top < MARGIN) top = MARGIN
    if (rect.right > window.innerWidth - MARGIN) {
      left = window.innerWidth - rect.width - MARGIN
    }
    if (left < MARGIN) left = MARGIN

    if (top !== adjustedPos.top || left !== adjustedPos.left) {
      setAdjustedPos({ top, left })
    }
  }, [adjustedPos])

  useLayoutEffect(() => {
    clampToViewport()
  }, [showFullPicker, adjustedPos])

  useLayoutEffect(() => {
    setAdjustedPos(position)
  }, [position?.top, position?.left])

  useEffect(() => {
    if (!containerRef.current) return
    const observer = new ResizeObserver(() => clampToViewport())
    observer.observe(containerRef.current)
    return () => observer.disconnect()
  }, [showFullPicker, clampToViewport])

  useEffect(() => {
    if (!showFullPicker || !pickerRef.current) return

    const picker = pickerRef.current
    picker.classList.add('dark')
    picker.style.setProperty('--num-columns', '7')
    picker.style.setProperty('--emoji-size', '1.2rem')
    picker.style.setProperty('--emoji-padding', '0.3rem')
    picker.style.setProperty('--category-emoji-size', '1rem')
    picker.style.setProperty('--indicator-height', '2px')
    picker.style.setProperty('--input-font-size', '0.85rem')
    picker.style.setProperty('--input-padding', '0.3rem 0.5rem')

    const handler = (event) => {
      const emoji = event.detail.unicode
      // DEBUG: log what the picker gives us
      console.log('=== EMOJI PICKER DEBUG ===')
      console.log('emoji string:', emoji)
      console.log('emoji length:', emoji.length)
      console.log('codepoints:', [...emoji].map(c => 'U+' + c.codePointAt(0).toString(16).toUpperCase()))
      console.log('full event detail:', event.detail)
      console.log('==========================')

      onSelect(emoji)
      onClose()
    }

    picker.addEventListener('emoji-click', handler)
    return () => picker.removeEventListener('emoji-click', handler)
  }, [showFullPicker, onSelect, onClose])

  return (
    <>
      <div
        onClick={onClose}
        style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, zIndex: 999 }}
      />

      {!showFullPicker ? (
        <div
          ref={containerRef}
          style={{
            position: 'fixed',
            top: adjustedPos?.top ?? 0,
            left: adjustedPos?.left ?? 0,
            background: 'rgba(30, 30, 40, 0.98)',
            border: '1px solid rgba(255, 255, 255, 0.15)',
            borderRadius: 8,
            padding: 6,
            display: 'flex',
            alignItems: 'center',
            gap: 2,
            boxShadow: '0 4px 16px rgba(0, 0, 0, 0.4)',
            zIndex: 1000
          }}
        >
          {QUICK_EMOJIS.map(emoji => (
            <button
              key={emoji}
              onClick={() => {
                // DEBUG: log quick picker too
                console.log('=== QUICK EMOJI DEBUG ===')
                console.log('emoji string:', emoji)
                console.log('codepoints:', [...emoji].map(c => 'U+' + c.codePointAt(0).toString(16).toUpperCase()))
                console.log('=========================')
                onSelect(emoji)
                onClose()
              }}
              style={{
                background: 'transparent',
                border: 'none',
                fontSize: 18,
                cursor: 'pointer',
                padding: 5,
                borderRadius: 6,
                transition: 'all 0.15s',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                lineHeight: 1
              }}
              onMouseEnter={e => {
                e.currentTarget.style.background = 'rgba(100, 108, 255, 0.2)'
                e.currentTarget.style.transform = 'scale(1.15)'
              }}
              onMouseLeave={e => {
                e.currentTarget.style.background = 'transparent'
                e.currentTarget.style.transform = 'scale(1)'
              }}
            >
              {emoji}
            </button>
          ))}
          <button
            onClick={(e) => { e.stopPropagation(); setShowFullPicker(true) }}
            title="More emojis"
            style={{
              background: 'rgba(100, 108, 255, 0.15)',
              border: '1px solid rgba(100, 108, 255, 0.3)',
              fontSize: 14,
              cursor: 'pointer',
              padding: 5,
              borderRadius: 6,
              transition: 'all 0.15s',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'rgba(255, 255, 255, 0.7)',
              lineHeight: 1,
              width: 28,
              height: 28
            }}
            onMouseEnter={e => {
              e.currentTarget.style.background = 'rgba(100, 108, 255, 0.3)'
              e.currentTarget.style.transform = 'scale(1.1)'
            }}
            onMouseLeave={e => {
              e.currentTarget.style.background = 'rgba(100, 108, 255, 0.15)'
              e.currentTarget.style.transform = 'scale(1)'
            }}
          >
            +
          </button>
        </div>
      ) : (
        <div
          ref={containerRef}
          onClick={(e) => e.stopPropagation()}
          style={{
            position: 'fixed',
            top: adjustedPos?.top ?? 0,
            left: adjustedPos?.left ?? 0,
            zIndex: 1000,
            borderRadius: 10,
            overflow: 'hidden',
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.5)',
            width: 272,
            height: 300
          }}
        >
          <emoji-picker
            ref={pickerRef}
            style={{ width: '100%', height: '100%' }}
          />
        </div>
      )}
    </>
  )
}
// Made with Bob