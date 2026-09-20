import { useEffect, useState } from 'react'

const formatDate = (iso) =>
  new Date(iso).toLocaleString('ko-KR', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' })

export default function NewsList({ limit = 10 }) {
  const [result, setResult] = useState({ items: null, error: false })

  useEffect(() => {
    const controller = new AbortController()
    fetch(`/api/news?limit=${limit}`, { signal: controller.signal })
      .then((res) => (res.ok ? res.json() : Promise.reject(res.status)))
      .then((items) => setResult({ items, error: false }))
      .catch((e) => {
        if (e?.name !== 'AbortError') setResult({ items: null, error: true })
      })
    return () => controller.abort()
  }, [limit])

  const { items, error } = result

  return (
    <section className="news-list">
      <h2>뉴스 헤드라인</h2>
      {error && <p className="dashboard__error">뉴스를 불러오지 못했습니다.</p>}
      {!error && !items && <p>불러오는 중…</p>}
      {items?.length === 0 && <p>아직 수집된 뉴스가 없습니다.</p>}
      {items?.length > 0 && (
        <ul>
          {items.map((item) => (
            <li key={item.url}>
              <a href={item.url} target="_blank" rel="noreferrer">
                {item.title}
              </a>
              <span className="news-list__meta">
                {item.source}
                {item.publishedAt && ` · ${formatDate(item.publishedAt)}`}
              </span>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
