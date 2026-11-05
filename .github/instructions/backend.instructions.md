---
applyTo: "backend/**"
---
- C#은 최신 문법을 사용한다: file-scoped namespace, primary constructor, records, `<Nullable>enable</Nullable>`.
- API 경로는 `/api/...` 접두사를 쓴다.
- 시크릿(API 키, 토큰)은 코드에 하드코딩하지 말고 환경변수(`WEATHER_API_KEY`, `HOME_ASSISTANT_TOKEN` 등)로 읽는다.
- 새 서비스는 `backend/<서비스명>/` 아래에 추가한다.
- 로컬 실행: `dotnet run --project backend/WeatherApi --launch-profile http` (포트 5224).
