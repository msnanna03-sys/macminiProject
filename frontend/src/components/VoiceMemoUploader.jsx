import { useState } from 'react'

// 9단계(로컬 LLM 서빙)에서 백엔드(/api/voice-memo)와 실제로 연결될 예정.
// 지금은 파일 선택 UI와 업로드 버튼만 제공한다 (백엔드 연동 없음).
const VOICE_MEMO_ENDPOINT = '/api/voice-memo'

export default function VoiceMemoUploader() {
  const [file, setFile] = useState(null)
  const [status, setStatus] = useState('idle') // idle | selected | (추후: uploading | transcribing | done)

  function handleFileChange(event) {
    const selected = event.target.files?.[0] ?? null
    setFile(selected)
    setStatus(selected ? 'selected' : 'idle')
  }

  function handleSubmit(event) {
    event.preventDefault()
    // TODO(9단계): VOICE_MEMO_ENDPOINT로 POST 요청 후 전사/정리 상태를 업데이트한다.
    console.log(`업로드 대상 엔드포인트(아직 미연동): ${VOICE_MEMO_ENDPOINT}`, file)
  }

  return (
    <section className="voice-memo">
      <h2>오늘의 음성 메모</h2>
      <p className="voice-memo__hint">
        하루 녹음 파일(.m4a/.mp3)을 올려두면, 이후 단계에서 자동으로 전사·정리되어 블로그에 게시됩니다.
        지금은 UI만 준비된 상태입니다.
      </p>
      <form className="voice-memo__form" onSubmit={handleSubmit}>
        <input
          type="file"
          accept=".m4a,.mp3,audio/mp4,audio/mpeg"
          onChange={handleFileChange}
        />
        <button type="submit" disabled={!file}>
          업로드
        </button>
      </form>
      {file && <p className="voice-memo__filename">선택된 파일: {file.name}</p>}
    </section>
  )
}
