import React, { createContext, useContext, useEffect, useRef, useState } from 'react'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { API_BASE } from '../services/api.js'

const ActiveUsersContext = createContext({ activeUsers: [] })

export function ActiveUsersProvider({ children }) {
    const [activeUsers, setActiveUsers] = useState([])
    const connectionRef = useRef(null)

    useEffect(() => {
        let disposed = false

        async function start() {
            if (connectionRef.current) {
                try { await connectionRef.current.stop() } catch { }
                connectionRef.current = null
            }

            const userId = localStorage.getItem('userId')
            const username = localStorage.getItem('username') || 'Guest'
            if (!userId) {
                setActiveUsers([])
                return
            }

            const base = (API_BASE || '').replace(/\/$/, '')
            const qs = `?userId=${encodeURIComponent(userId)}&username=${encodeURIComponent(username)}`
            const url = `${base}/hubs/active-users${qs}`

            const conn = new HubConnectionBuilder()
                .withUrl(url)
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Error)
                .build()

            conn.on('ActiveUsersUpdated', users => {
                try {
                    const list = Array.isArray(users) ? users : Array.from(users ?? [])
                    setActiveUsers(list)
                } catch (err) {
                    console.error("ActiveUsersUpdated parse error:", err, users)
                    setActiveUsers([])
                }
            })

            try {
                await conn.start()
                console.log("SignalR connected:", conn.connectionId)   // debug log
                if (!disposed) connectionRef.current = conn
            } catch (e) {
                console.error("SignalR connection failed:", e)          // debug log
                setTimeout(() => { if (!disposed) start() }, 2000)
            }
        }


        start()

        function handleAuthChange() {
            start()
        }

        window.addEventListener('auth-changed', handleAuthChange)
        window.addEventListener('storage', handleAuthChange)

        return () => {
            disposed = true
            window.removeEventListener('auth-changed', handleAuthChange)
            window.removeEventListener('storage', handleAuthChange)

            if (connectionRef.current) {
                const c = connectionRef.current
                connectionRef.current = null
                c.stop().catch(() => { })
            }
        }
    }, [])

    return (
        <ActiveUsersContext.Provider value={{ activeUsers }}>
            {children}
        </ActiveUsersContext.Provider>
    )
}

export function useActiveUsers() {
    return useContext(ActiveUsersContext)
}