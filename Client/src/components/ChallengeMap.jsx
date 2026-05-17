import React, { useEffect, useMemo, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'
import L from 'leaflet'

// Fix for default marker icon issue with webpack/vite
delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
})

// Create all marker icon instances once outside component to prevent recreation
const blueIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const redIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const greenIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const yellowIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

const greyIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-grey.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
})

// Calculate distance between two coordinates using Haversine formula
const calculateDistance = (lat1, lon1, lat2, lon2) => {
  const R = 6371 // Earth's radius in km
  const dLat = (lat2 - lat1) * Math.PI / 180
  const dLon = (lon2 - lon1) * Math.PI / 180
  const a =
    Math.sin(dLat/2) * Math.sin(dLat/2) +
    Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
    Math.sin(dLon/2) * Math.sin(dLon/2)
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a))
  return R * c
}

// Format distance for display
const formatDistance = (km) => {
  if (km < 1) return `${Math.round(km * 1000)}m away`
  return `${km.toFixed(1)}km away`
}

// Determine marker icon based on challenge status
const getMarkerIcon = (challenge, isPreview, alreadyJoined, isFull) => {
  if (isPreview) return redIcon
  if (alreadyJoined) return greenIcon
  if (isFull) return greyIcon
  
  // Check if deadline is within 24 hours
  if (challenge.deadline) {
    const deadline = new Date(challenge.deadline)
    const now = new Date()
    const hoursUntilDeadline = (deadline - now) / (1000 * 60 * 60)
    if (hoursUntilDeadline < 24 && hoursUntilDeadline > 0) return yellowIcon
  }
  
  return blueIcon
}

// Component to auto-fit map bounds to show all markers
function FitBounds({ challenges }) {
  const map = useMap()
  
  useEffect(() => {
    if (challenges.length > 1) {
      const bounds = challenges.map(c => [c.location.latitude, c.location.longitude])
      map.fitBounds(bounds, { padding: [50, 50], maxZoom: 15 })
    } else if (challenges.length === 1) {
      const c = challenges[0]
      map.setView([c.location.latitude, c.location.longitude], 13)
    }
  }, [challenges, map])
  
  return null
}

// Component to pan map to a specific location
function PanToLocation({ lat, lng }) {
  const map = useMap()
  useEffect(() => {
    if (lat && lng) {
      map.setView([lat, lng], 13, { animate: true })
    }
  }, [lat, lng, map])
  return null
}

// Custom checkbox component for filters
function FilterCheckbox({ checked, onChange, label, color }) {
  return (
    <label
      style={{
        display: 'flex',
        alignItems: 'center',
        gap: '10px',
        padding: '10px 12px',
        background: checked ? `${color}15` : 'rgba(40, 40, 50, 0.6)',
        border: `1px solid ${checked ? color + '40' : 'rgba(100, 108, 255, 0.2)'}`,
        borderRadius: '6px',
        cursor: 'pointer',
        transition: 'all 0.2s ease',
        userSelect: 'none'
      }}
      onMouseEnter={(e) => {
        if (!checked) {
          e.currentTarget.style.background = 'rgba(50, 50, 60, 0.7)'
          e.currentTarget.style.borderColor = 'rgba(100, 108, 255, 0.3)'
        }
      }}
      onMouseLeave={(e) => {
        if (!checked) {
          e.currentTarget.style.background = 'rgba(40, 40, 50, 0.6)'
          e.currentTarget.style.borderColor = 'rgba(100, 108, 255, 0.2)'
        }
      }}
    >
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        style={{
          width: '16px',
          height: '16px',
          cursor: 'pointer',
          accentColor: color
        }}
      />
      <span style={{
        fontSize: '14px',
        color: checked ? 'rgba(255, 255, 255, 0.95)' : 'rgba(255, 255, 255, 0.7)',
        fontWeight: checked ? 500 : 400,
        flex: 1
      }}>
        {label}
      </span>
      <div style={{
        width: '12px',
        height: '12px',
        borderRadius: '50%',
        background: color,
        opacity: checked ? 1 : 0.5,
        transition: 'opacity 0.2s ease'
      }} />
    </label>
  )
}

export default function ChallengeMap({ challenges, onJoin, myChallengeIds, loading = false, previewChallenge = null }) {
  // User location state for distance calculation
  const [userLocation, setUserLocation] = useState(null)
  
  // Address search state
  const [searchQuery, setSearchQuery] = useState('')
  const [searchResults, setSearchResults] = useState([])
  const [searching, setSearching] = useState(false)
  const [selectedLocation, setSelectedLocation] = useState(null)
  const [showResults, setShowResults] = useState(false)
  
  // Filter state
  const [filters, setFilters] = useState({
    showOpen: true,
    showJoined: true,
    showPrivate: true,
    showEndingSoon: true,
    showFull: true,
    maxDistance: null, // null = no limit, or number in km
  })
  const [showFilters, setShowFilters] = useState(false)
  
  // Get user location on mount
  useEffect(() => {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setUserLocation({
          lat: position.coords.latitude,
          lng: position.coords.longitude
        })
      },
      (error) => {
        console.log('Geolocation not available:', error.message)
      }
    )
  }, [])
  
  // Debounced address search
  useEffect(() => {
    if (searchQuery.length < 3) {
      setSearchResults([])
      setShowResults(false)
      return
    }
    
    const timer = setTimeout(async () => {
      setSearching(true)
      try {
        const response = await fetch(
          `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(searchQuery)}&limit=5`,
          { headers: { 'User-Agent': 'PhotoScavengerHunt/1.0' } }
        )
        const results = await response.json()
        setSearchResults(results)
        setShowResults(true)
      } catch (error) {
        console.error('Search error:', error)
        setSearchResults([])
      } finally {
        setSearching(false)
      }
    }, 500)
    
    return () => clearTimeout(timer)
  }, [searchQuery])
  // Filter challenges with valid coordinates
  const validChallenges = useMemo(() =>
    (challenges || []).filter(c => c.location?.latitude != null && c.location?.longitude != null),
    [challenges]
  )
  
  // Apply filters to challenges
  const filteredChallenges = useMemo(() => {
    return validChallenges.filter(challenge => {
      const alreadyJoined = myChallengeIds?.has(challenge.id)
      const isFull = challenge.participantCount >= challenge.maxParticipants
      const isPrivate = challenge.isPrivate
      
      // Check deadline proximity
      let isEndingSoon = false
      if (challenge.deadline) {
        const deadline = new Date(challenge.deadline)
        const now = new Date()
        const hoursUntilDeadline = (deadline - now) / (1000 * 60 * 60)
        isEndingSoon = hoursUntilDeadline < 24 && hoursUntilDeadline > 0
      }
      
      // Apply status filters
      if (alreadyJoined && !filters.showJoined) return false
      if (isFull && !alreadyJoined && !filters.showFull) return false
      if (isPrivate && !filters.showPrivate) return false
      if (isEndingSoon && !filters.showEndingSoon) return false
      if (!alreadyJoined && !isFull && !isPrivate && !isEndingSoon && !filters.showOpen) return false
      
      // Apply distance filter
      if (filters.maxDistance && userLocation) {
        const distance = calculateDistance(
          userLocation.lat,
          userLocation.lng,
          challenge.location.latitude,
          challenge.location.longitude
        )
        if (distance > filters.maxDistance) return false
      }
      
      return true
    })
  }, [validChallenges, filters, myChallengeIds, userLocation])
  
  // Combine filtered challenges with preview challenge
  const allChallenges = useMemo(() =>
    previewChallenge ? [...filteredChallenges, previewChallenge] : filteredChallenges,
    [filteredChallenges, previewChallenge]
  )

  // Calculate map center (used for initial render)
  const mapCenter = useMemo(() => {
    if (allChallenges.length === 0) {
      // Default to Vilnius
      return [54.6872, 25.2797]
    }
    // Center on first challenge initially
    const first = allChallenges[0]
    return [first.location.latitude, first.location.longitude]
  }, [allChallenges])

  return (
    <div style={{ width: '100%' }}>
      {/* Address Search and Filter Bar */}
      <div style={{
        marginBottom: '12px',
        position: 'relative',
        background: 'rgba(30, 30, 30, 0.95)',
        padding: '12px',
        borderRadius: '8px',
        border: '1px solid rgba(100, 108, 255, 0.3)'
      }}>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center', marginBottom: showFilters ? '12px' : '0' }}>
          <input
            type="text"
            placeholder="Search location on map..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchResults.length > 0 && setShowResults(true)}
            style={{
              flex: 1,
              padding: '10px 12px',
              background: 'rgba(50, 50, 50, 0.8)',
              border: '1px solid rgba(100, 108, 255, 0.3)',
              borderRadius: '6px',
              color: 'white',
              fontSize: '14px',
              outline: 'none'
            }}
          />
          {searchQuery && (
            <button
              onClick={() => {
                setSearchQuery('')
                setSearchResults([])
                setShowResults(false)
                setSelectedLocation(null)
              }}
              style={{
                padding: '10px 16px',
                background: 'rgba(100, 108, 255, 0.2)',
                border: '1px solid rgba(100, 108, 255, 0.3)',
                borderRadius: '6px',
                color: '#646cff',
                cursor: 'pointer',
                fontSize: '14px',
                fontWeight: 500
              }}
            >
              Clear
            </button>
          )}
          
          {/* Filter Toggle Button */}
          <button
            onClick={() => setShowFilters(!showFilters)}
            style={{
              padding: '10px 16px',
              background: showFilters ? 'rgba(100, 108, 255, 0.3)' : 'rgba(100, 108, 255, 0.2)',
              border: '1px solid rgba(100, 108, 255, 0.3)',
              borderRadius: '6px',
              color: '#646cff',
              cursor: 'pointer',
              fontSize: '14px',
              fontWeight: 500,
              whiteSpace: 'nowrap'
            }}
          >
            ⚙ Filters
          </button>
        </div>
        
        {/* Filter Controls */}
        {showFilters && (
          <div style={{
            padding: '16px',
            background: 'linear-gradient(135deg, rgba(20, 20, 30, 0.95) 0%, rgba(30, 30, 40, 0.95) 100%)',
            borderRadius: '8px',
            border: '1px solid rgba(100, 108, 255, 0.25)',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.3)'
          }}>
            {/* Status Filters Section */}
            <div style={{ marginBottom: '16px' }}>
              <h4 style={{
                margin: '0 0 12px 0',
                fontSize: '13px',
                fontWeight: 600,
                color: 'rgba(255, 255, 255, 0.9)',
                textTransform: 'uppercase',
                letterSpacing: '0.5px'
              }}>
                Challenge Status
              </h4>
              <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
                gap: '10px'
              }}>
                <FilterCheckbox
                  checked={filters.showOpen}
                  onChange={(checked) => setFilters({...filters, showOpen: checked})}
                  label="Open"
                  color="#3b82f6"
                />
                <FilterCheckbox
                  checked={filters.showJoined}
                  onChange={(checked) => setFilters({...filters, showJoined: checked})}
                  label="Joined"
                  color="#10b981"
                />
                <FilterCheckbox
                  checked={filters.showPrivate}
                  onChange={(checked) => setFilters({...filters, showPrivate: checked})}
                  label="Private"
                  color="#ef4444"
                />
                <FilterCheckbox
                  checked={filters.showEndingSoon}
                  onChange={(checked) => setFilters({...filters, showEndingSoon: checked})}
                  label="Ending Soon"
                  color="#f59e0b"
                />
                <FilterCheckbox
                  checked={filters.showFull}
                  onChange={(checked) => setFilters({...filters, showFull: checked})}
                  label="Full"
                  color="#6b7280"
                />
              </div>
            </div>
            
            {/* Distance Filter Section */}
            {userLocation && (
              <div style={{
                paddingTop: '16px',
                borderTop: '1px solid rgba(100, 108, 255, 0.15)'
              }}>
                <h4 style={{
                  margin: '0 0 10px 0',
                  fontSize: '13px',
                  fontWeight: 600,
                  color: 'rgba(255, 255, 255, 0.9)',
                  textTransform: 'uppercase',
                  letterSpacing: '0.5px'
                }}>
                  Distance Filter
                </h4>
                <select
                  value={filters.maxDistance || ''}
                  onChange={(e) => setFilters({...filters, maxDistance: e.target.value ? parseFloat(e.target.value) : null})}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'rgba(40, 40, 50, 0.9)',
                    border: '1px solid rgba(100, 108, 255, 0.3)',
                    borderRadius: '6px',
                    color: 'white',
                    fontSize: '14px',
                    cursor: 'pointer',
                    outline: 'none',
                    transition: 'all 0.2s ease'
                  }}
                  onMouseEnter={(e) => e.target.style.borderColor = 'rgba(100, 108, 255, 0.5)'}
                  onMouseLeave={(e) => e.target.style.borderColor = 'rgba(100, 108, 255, 0.3)'}
                >
                  <option value="">Any distance</option>
                  <option value="1">Within 1km</option>
                  <option value="5">Within 5km</option>
                  <option value="10">Within 10km</option>
                  <option value="25">Within 25km</option>
                  <option value="50">Within 50km</option>
                </select>
              </div>
            )}
            
          </div>
        )}
        
        {/* Search Results Dropdown */}
        {showResults && searchResults.length > 0 && (
          <div style={{
            position: 'absolute',
            top: '100%',
            left: '12px',
            right: '12px',
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
                onClick={() => {
                  setSelectedLocation({ lat: parseFloat(result.lat), lng: parseFloat(result.lon) })
                  setSearchQuery(result.display_name)
                  setShowResults(false)
                }}
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
            marginTop: '8px',
            fontSize: '12px',
            color: 'rgba(255, 255, 255, 0.5)',
            textAlign: 'center'
          }}>
            Searching...
          </div>
        )}
      </div>
      
      {/* Map Legend */}
      <div style={{
        marginBottom: '12px',
        padding: '10px 12px',
        background: 'rgba(30, 30, 30, 0.95)',
        borderRadius: '8px',
        border: '1px solid rgba(100, 108, 255, 0.3)',
        display: 'flex',
        flexWrap: 'wrap',
        gap: '16px',
        fontSize: '13px',
        color: 'rgba(255, 255, 255, 0.85)'
      }}>
        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '12px', height: '12px', background: '#2A81CB', display: 'inline-block', borderRadius: '2px' }}></span>
          Open
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '12px', height: '12px', background: '#2AAD27', display: 'inline-block', borderRadius: '2px' }}></span>
          Joined
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '12px', height: '12px', background: '#CB2B3E', display: 'inline-block', borderRadius: '2px' }}></span>
          Private
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '12px', height: '12px', background: '#FFD326', display: 'inline-block', borderRadius: '2px' }}></span>
          Ending Soon
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '12px', height: '12px', background: '#7B7B7B', display: 'inline-block', borderRadius: '2px' }}></span>
          Full
        </span>
      </div>
      
      <div style={{ height: '450px', width: '100%', borderRadius: '8px', overflow: 'hidden' }}>
        {loading ? (
        <div style={{
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'rgba(100, 108, 255, 0.05)',
          color: 'rgba(255, 255, 255, 0.6)',
          gap: '12px'
        }}>
          <div style={{
            width: '40px',
            height: '40px',
            border: '4px solid rgba(100, 108, 255, 0.2)',
            borderTop: '4px solid #646cff',
            borderRadius: '50%',
            animation: 'spin 1s linear infinite'
          }} />
          <span>Loading map challenges...</span>
        </div>
      ) : allChallenges.length === 0 ? (
        <div style={{
          height: '100%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'rgba(100, 108, 255, 0.05)',
          color: 'rgba(255, 255, 255, 0.6)'
        }}>
          No challenges with locations available
        </div>
      ) : (
        <MapContainer
          key="challenge-map"
          center={mapCenter}
          zoom={10}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <FitBounds challenges={allChallenges} />
          {selectedLocation && <PanToLocation lat={selectedLocation.lat} lng={selectedLocation.lng} />}
          
          {/* Marker Cluster Group */}
          <MarkerClusterGroup
            chunkedLoading
            maxClusterRadius={50}
            spiderfyOnMaxZoom={true}
            showCoverageOnHover={false}
            zoomToBoundsOnClick={true}
            iconCreateFunction={(cluster) => {
              const count = cluster.getChildCount()
              let size = 'small'
              if (count >= 10) size = 'large'
              else if (count >= 5) size = 'medium'
              
              return L.divIcon({
                html: `<div style="
                  background: rgba(100, 108, 255, 0.8);
                  border: 3px solid rgba(100, 108, 255, 1);
                  border-radius: 50%;
                  color: white;
                  display: flex;
                  align-items: center;
                  justify-content: center;
                  font-weight: bold;
                  font-size: ${size === 'large' ? '16px' : size === 'medium' ? '14px' : '12px'};
                  width: ${size === 'large' ? '50px' : size === 'medium' ? '40px' : '30px'};
                  height: ${size === 'large' ? '50px' : size === 'medium' ? '40px' : '30px'};
                ">${count}</div>`,
                className: 'custom-cluster-icon',
                iconSize: L.point(
                  size === 'large' ? 50 : size === 'medium' ? 40 : 30,
                  size === 'large' ? 50 : size === 'medium' ? 40 : 30
                )
              })
            }}
          >
            {allChallenges.map(challenge => {
            const alreadyJoined = myChallengeIds?.has(challenge.id)
            const isFull = challenge.participantCount >= challenge.maxParticipants
            const isPreview = previewChallenge && challenge.id === previewChallenge.id
            
            // Calculate distance if user location is available
            const distance = userLocation
              ? calculateDistance(
                  userLocation.lat,
                  userLocation.lng,
                  challenge.location.latitude,
                  challenge.location.longitude
                )
              : null
            
            // Get appropriate marker icon
            const markerIcon = getMarkerIcon(challenge, isPreview, alreadyJoined, isFull)
            
            return (
              <Marker
                key={challenge.id}
                position={[challenge.location.latitude, challenge.location.longitude]}
                icon={markerIcon}
              >
                <Popup className="challenge-marker-popup">
                  <div>
                    <h3 style={{ margin: '0 0 8px 0', color: isPreview ? '#ff6b6b' : '#646cff', fontSize: '16px' }}>
                      {challenge.name}
                      {isPreview && <span style={{ marginLeft: '8px', fontSize: '12px', color: '#ff6b6b' }}>(Private)</span>}
                    </h3>
                    <p style={{ margin: '4px 0', fontSize: '14px', color: '#666' }}>
                      {challenge.location.locationName}
                    </p>
                    {distance !== null && (
                      <p style={{ margin: '4px 0', fontSize: '12px', color: '#888', fontStyle: 'italic' }}>
                        📍 {formatDistance(distance)}
                      </p>
                    )}
                    <p style={{
                      margin: '4px 0',
                      fontSize: '14px',
                      color: isFull ? '#ff6b6b' : '#666',
                      fontWeight: isFull ? 600 : 400
                    }}>
                      {challenge.participantCount} / {challenge.maxParticipants} participants
                      {isFull && <span style={{ color: '#ff6b6b' }}> • Full</span>}
                    </p>
                    <button
                      onClick={() => onJoin(challenge)}
                      disabled={isFull && !alreadyJoined}
                      style={{
                        width: '100%',
                        marginTop: '8px',
                        padding: '8px',
                        background: alreadyJoined ? '#38b000' : '#646cff',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: (isFull && !alreadyJoined) ? 'not-allowed' : 'pointer',
                        opacity: (isFull && !alreadyJoined) ? 0.5 : 1,
                        fontSize: '14px',
                        fontWeight: 500
                      }}
                    >
                      {alreadyJoined ? 'Enter Challenge' : isFull ? 'Full' : 'Join Now'}
                    </button>
                  </div>
                </Popup>
              </Marker>
            )
            })}
          </MarkerClusterGroup>
        </MapContainer>
        )}
      </div>
    </div>
  )
}
