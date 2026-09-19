import { useEffect, useState } from 'react'

export default function Weather({ city = 'Seoul' }) {
  const [state, setState] = useState({ status: 'loading' })

  useEffect(() => {
    const controller = new AbortController()
    fetch(`/api/weather?city=${encodeURIComponent(city)}`, { signal: controller.signal })
      .then((res) => {
        if (!res.ok) throw new Error(res.status)
        return res.json()
      })
      .then((data) => setState({ status: 'ok', data }))
      .catch((e) => {
        if (e.name !== 'AbortError') setState({ status: 'error' })
      })
    return () => controller.abort()
  }, [city])

  if (state.status === 'loading') return <span className="weather">날씨 불러오는 중…</span>
  if (state.status === 'error') return <span className="weather weather--error">날씨 정보 없음</span>

  const { data } = state
  return (
    <span className="weather" title={`체감 ${data.feelsLike.toFixed(1)}°C · 습도 ${data.humidity}%`}>
      {data.city} {data.description} {data.temp.toFixed(1)}°C
    </span>
  )
}
