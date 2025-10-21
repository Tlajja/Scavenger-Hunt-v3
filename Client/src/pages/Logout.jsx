import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'

export default function Logout() {
  const navigate = useNavigate()

  useEffect(() => {
    // Remove user session info
    localStorage.removeItem('userId')
    localStorage.removeItem('username')
    // Optionally remove other session keys
    // Redirect to home or login
    navigate('/login', { replace: true })
  }, [navigate])

  return (
    <div>
      <h2>Logging out...</h2>
    </div>
  )
}