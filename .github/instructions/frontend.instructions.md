---
applyTo: "frontend/**"
---
- React 함수형 컴포넌트와 hooks만 사용한다. 클래스 컴포넌트 금지.
- 백엔드 호출은 `/api/...` 상대 경로를 쓴다 (개발 시 Vite가 5224로, Docker에서는 nginx가 backend:8080으로 프록시).
- 로컬 실행: `cd frontend && npm run dev`.
