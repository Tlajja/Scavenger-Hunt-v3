import React, { useState, useEffect, useRef } from 'react'
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import L from 'leaflet'

// Fix for default marker icons in React-Leaflet
delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
})

// Component to handle map clicks and center updates
function LocationMarker({ position, setPosition, onPositionChange }) {
  const map = useMap()
  
  useMapEvents({
    click(e) {
      const newPos = [e.latlng.lat, e.latlng.lng]
      setPosition(newPos)
      // Notify parent to trigger reverse geocoding
      if (onPositionChange) {
        onPositionChange(newPos)
      }
    },
  })

  // Center map when position changes
  useEffect(() => {
    if (position) {
      map.setView(position, map.getZoom())
    }
  }, [position, map])

  return position ? <Marker position={position} /> : null
}

export default function LocationPicker({ latitude, longitude, locationName, onLocationChange }) {
  const [position, setPosition] = useState(
    latitude && longitude ? [latitude, longitude] : [54.6872, 25.2797] // Default to Vilnius
  )
  const [name, setName] = useState(locationName || '')
  const [gettingLocation, setGettingLocation] = useState(false)
  const [geocoding, setGeocoding] = useState(false)
  
  // Address search state
  const [searchQuery, setSearchQuery] = useState('')
  const [searchResults, setSearchResults] = useState([])
  const [searching, setSearching] = useState(false)
  const [showResults, setShowResults] = useState(false)

  // Reverse geocode coordinates to get location name
  const reverseGeocode = async (lat, lng) => {
    setGeocoding(true)
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`,
        {
          headers: {
            'User-Agent': 'PhotoScavengerHunt/1.0'
          }
        }
      )
      const data = await response.json()
      if (data.display_name) {
        // Extract a shorter, more readable name
        const parts = data.display_name.split(',')
        const shortName = parts.slice(0, 3).join(',').trim()
        setName(shortName)
      }
    } catch (error) {
      console.error('Reverse geocoding error:', error)
      // Keep existing name or leave empty
    } finally {
      setGeocoding(false)
    }
  }
  
  // Forward geocode address to coordinates
  const searchAddress = async (query) => {
    if (query.length < 3) {
      setSearchResults([])
      setShowResults(false)
      return
    }
    
    setSearching(true)
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=5`,
        {
          headers: {
            'User-Agent': 'PhotoScavengerHunt/1.0'
          }
        }
      )
      const results = await response.json()
      setSearchResults(results)
      setShowResults(true)
    } catch (error) {
      console.error('Address search error:', error)
      setSearchResults([])
    } finally {
      setSearching(false)
    }
  }
  
  // Debounced search effect
  useEffect(() => {
    if (searchQuery.length < 3) {
      setSearchResults([])
      setShowResults(false)
      return
    }
    
    const timer = setTimeout(() => {
      searchAddress(searchQuery)
    }, 500)
    
    return () => clearTimeout(timer)
  }, [searchQuery])

  // Update parent when position or name changes
  useEffect(() => {
    if (position && position.length === 2) {
      onLocationChange({
        latitude: position[0],
        longitude: position[1],
        locationName: name
      })
    }
  }, [position, name, onLocationChange])

  const handleGetCurrentLocation = () => {
    setGettingLocation(true)
    
    if ('geolocation' in navigator) {
      navigator.geolocation.getCurrentPosition(
        async (pos) => {
          const newPosition = [pos.coords.latitude, pos.coords.longitude]
          setPosition(newPosition)
          setGettingLocation(false)
          // Auto-fill location name
          await reverseGeocode(pos.coords.latitude, pos.coords.longitude)
        },
        (error) => {
          console.error('Geolocation error:', error)
          // Fallback to Vilnius
          setPosition([54.6872, 25.2797])
          setName('Vilnius, Lithuania (default)')
          setGettingLocation(false)
          alert('Could not get your location. Using Vilnius as default.')
        },
        {
          enableHighAccuracy: true,
          timeout: 5000,
          maximumAge: 0
        }
      )
    } else {
      // Geolocation not supported, use Vilnius
      setPosition([54.6872, 25.2797])
      setName('Vilnius, Lithuania (default)')
      setGettingLocation(false)
      alert('Geolocation is not supported by your browser. Using Vilnius as default.')
    }
  }
  
  const handleSelectSearchResult = (result) => {
    const newPosition = [parseFloat(result.lat), parseFloat(result.lon)]
    setPosition(newPosition)
    setName(result.display_name.split(',').slice(0, 3).join(',').trim())
    setSearchQuery('')
    setShowResults(false)
    setSearchResults([])
  }

  return (
    <div style={{ marginBottom: 20 }}>
      <label style={{ display: 'block', marginBottom: 8, color: 'white', fontWeight: 500 }}>
        Challenge Location *
      </label>
      
      <div style={{ marginBottom: 12 }}>
        {/* Address Search Input */}
        <div style={{ position: 'relative', marginBottom: 8 }}>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchResults.length > 0 && setShowResults(true)}
            placeholder="Search for an address or place..."
            style={{
              width: '100%',
              padding: '10px 12px',
              background: 'rgba(50, 50, 50, 0.8)',
              border: '1px solid rgba(100, 108, 255, 0.3)',
              borderRadius: '6px',
              color: 'white',
              fontSize: '14px',
              outline: 'none'
            }}
          />
          
          {/* Search Results Dropdown */}
          {showResults && searchResults.length > 0 && (
            <div style={{
              position: 'absolute',
              top: '100%',
              left: 0,
              right: 0,
              marginTop: '4px',
              background: 'rgba(30, 30, 30, 0.98)',
              border: '1px solid rgba(100, 108, 255, 0.3)',
              borderRadius: '6px',
              maxHeight: '200px',
              overflowY: 'auto',
              zIndex: 1000,
              boxShadow: '0 4px 12px rgba(0, 0, 0, 0.5)'
            }}>
              {searchResults.map((result, idx) => (
                <div
                  key={idx}
                  onClick={() => handleSelectSearchResult(result)}
                  style={{
                    padding: '10px 12px',
                    cursor: 'pointer',
                    borderBottom: idx < searchResults.length - 1 ? '1px solid rgba(100, 108, 255, 0.1)' : 'none',
                    color: 'rgba(255, 255, 255, 0.9)',
                    fontSize: '13px',
                    transition: 'background 0.2s'
                  }}
                  onMouseEnter={(e) => e.currentTarget.style.background = 'rgba(100, 108, 255, 0.2)'}
                  onMouseLeave={(e) => e.currentTarget.style.background = 'transparent'}
                >
                  {result.display_name}
                </div>
              ))}
            </div>
          )}
          
          {searching && (
            <div style={{
              marginTop: '4px',
              fontSize: '12px',
              color: 'rgba(255, 255, 255, 0.5)'
            }}>
              Searching...
            </div>
          )}
        </div>
        
        {/* Location Name Input */}
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Location name (auto-filled from search or click)"
          style={{
            width: '100%',
            marginBottom: 8,
            padding: '10px 12px',
            background: 'rgba(50, 50, 50, 0.8)',
            border: '1px solid rgba(100, 108, 255, 0.3)',
            borderRadius: '6px',
            color: 'white',
            fontSize: '14px',
            outline: 'none'
          }}
        />
        <button
          type="button"
          onClick={handleGetCurrentLocation}
          disabled={gettingLocation || geocoding}
          style={{
            width: '100%',
            padding: '10px',
            background: 'rgba(100, 108, 255, 0.2)',
            border: '1px solid #646cff',
            marginBottom: 8
          }}
        >
          {gettingLocation ? 'Getting location...' : geocoding ? 'Finding address...' : 'Use My Current Location'}
        </button>
        <div style={{
          fontSize: 13,
          color: 'rgba(255, 255, 255, 0.6)',
          marginBottom: 8
        }}>
          Search for an address, click on the map, or use your current location.
        </div>
      </div>

      <div style={{ 
        height: 400, 
        borderRadius: 8, 
        overflow: 'hidden',
        border: '1px solid rgba(100, 108, 255, 0.3)'
      }}>
        <MapContainer
          center={position}
          zoom={13}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <LocationMarker
            position={position}
            setPosition={setPosition}
            onPositionChange={(pos) => reverseGeocode(pos[0], pos[1])}
          />
        </MapContainer>
      </div>

      <div style={{
        marginTop: 8,
        fontSize: 13,
        color: 'rgba(255, 255, 255, 0.6)'
      }}>
        {geocoding ? (
          <span>Looking up address...</span>
        ) : (
          <span>Selected: {position[0].toFixed(6)}, {position[1].toFixed(6)}</span>
        )}
      </div>
    </div>
  )
}
