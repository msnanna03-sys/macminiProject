import { useEffect, useState } from 'react'
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'

const METRICS = [
  { key: 'usd_krw', label: '환율 (USD/KRW)', hours: 24 * 30, range: '최근 30일' },
  { key: 'home_temperature', label: '실내 온도 (°C)', hours: 24, range: '최근 24시간' },
]

const formatTime = (iso, hours) =>
  hours > 48
    ? new Date(iso).toLocaleDateString('ko-KR', { month: 'numeric', day: 'numeric' })
    : new Date(iso).toLocaleTimeString('ko-KR', { hour: '2-digit', minute: '2-digit' })

export default function HistoryChart() {
  const [metric, setMetric] = useState(METRICS[0].key)
  const current = METRICS.find((m) => m.key === metric)
  const [result, setResult] = useState({ metric: null, data: [], error: false })

  useEffect(() => {
    const controller = new AbortController()
    fetch(`/api/history?metric=${metric}&hours=${current.hours}`, { signal: controller.signal })
      .then((res) => (res.ok ? res.json() : Promise.reject(res.status)))
      .then((data) => setResult({ metric, data, error: false }))
      .catch((e) => {
        if (e?.name !== 'AbortError') setResult({ metric, data: [], error: true })
      })
    return () => controller.abort()
  }, [metric, current.hours])

  const loading = result.metric !== metric
  const { data, error } = result

  return (
    <section className="history-chart">
      <h2>수집 히스토리 ({current.range})</h2>
      <div className="history-chart__tabs">
        {METRICS.map((m) => (
          <button
            key={m.key}
            type="button"
            className={m.key === metric ? 'is-active' : ''}
            onClick={() => setMetric(m.key)}
          >
            {m.label}
          </button>
        ))}
      </div>
      {loading && <p>불러오는 중…</p>}
      {!loading && error && <p className="dashboard__error">히스토리를 불러오지 못했습니다.</p>}
      {!loading && !error && data.length === 0 && <p>아직 수집된 데이터가 없습니다.</p>}
      {!loading && !error && data.length > 0 && (
        <ResponsiveContainer width="100%" height={260}>
          <LineChart data={data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="collectedAt" tickFormatter={(v) => formatTime(v, current.hours)} />
            <YAxis domain={['auto', 'auto']} />
            <Tooltip labelFormatter={(v) => formatTime(v, current.hours)} />
            <Line type="monotone" dataKey="value" name="값" stroke="#2563eb" dot={false} />
          </LineChart>
        </ResponsiveContainer>
      )}
    </section>
  )
}
