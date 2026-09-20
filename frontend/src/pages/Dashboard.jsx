import { useCallback, useEffect, useState } from 'react'

const POLL_MS = 5000

export default function Dashboard() {
  const [light, setLight] = useState(null)
  const [temperature, setTemperature] = useState(null)
  const [error, setError] = useState(false)
  const [busy, setBusy] = useState(false)

  const refresh = useCallback(async (signal) => {
    try {
      const [l, t] = await Promise.all([
        fetch('/api/home/light', { signal }).then((r) => (r.ok ? r.json() : Promise.reject(r.status))),
        fetch('/api/home/temperature', { signal }).then((r) => (r.ok ? r.json() : Promise.reject(r.status))),
      ])
      setLight(l)
      setTemperature(t)
      setError(false)
    } catch (e) {
      if (e?.name !== 'AbortError') setError(true)
    }
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    refresh(controller.signal)
    const timer = setInterval(() => refresh(controller.signal), POLL_MS)
    return () => {
      controller.abort()
      clearInterval(timer)
    }
  }, [refresh])

  async function toggleLight() {
    setBusy(true)
    try {
      const res = await fetch('/api/home/light', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isOn: !light.isOn }),
      })
      if (!res.ok) throw new Error(res.status)
      setLight(await res.json())
      setError(false)
    } catch {
      setError(true)
    } finally {
      setBusy(false)
    }
  }

  return (
    <section>
      <h1>홈 대시보드</h1>
      {error && <p className="dashboard__error">기기 상태를 불러오지 못했습니다.</p>}
      <div className="device-grid">
        <article className="device-card">
          <h2>조명</h2>
          <p className={`device-card__value ${light?.isOn ? 'is-on' : ''}`}>
            {light ? (light.isOn ? '켜짐' : '꺼짐') : '-'}
          </p>
          <button type="button" onClick={toggleLight} disabled={!light || busy}>
            {light?.isOn ? '끄기' : '켜기'}
          </button>
        </article>
        <article className="device-card">
          <h2>온도</h2>
          <p className="device-card__value">
            {temperature ? `${temperature.value.toFixed(1)}${temperature.unit}` : '-'}
          </p>
        </article>
      </div>
    </section>
  )
}
