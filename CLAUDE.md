# 맥미니 홈서버 프로젝트

React(Vite) + ASP.NET Core Web API 모노레포. 블로그, 날씨, 스마트홈 대시보드, 데이터 히스토리, 로컬 LLM 챗봇을 하나의 앱으로 통합한다.

## 스택
- 프론트엔드: React (Vite), `frontend/`
- 백엔드: .NET Core (ASP.NET Core Web API), `backend/`
- 컨테이너: Docker / Docker Compose
- CI/CD: Harness
- 오케스트레이션: k3s (10단계)

## 폴더 구조
- `frontend/` — React 앱
- `backend/` — .NET 프로젝트들 (WeatherCli, WeatherApi, Crawler 등)
- 새 서비스는 `backend/<서비스명>/` 아래에 추가한다.

## 코딩 컨벤션
- C#은 최신 문법을 사용한다 (file-scoped namespace, primary constructor, records, nullable enable 등).
- React는 함수형 컴포넌트와 hooks만 사용한다. 클래스 컴포넌트 금지.
- API 경로는 `/api/...` 접두사를 쓴다.

## 배포 규칙 (Harness Policy와 1:1 대응, 1단계에서 확정)
- main 브랜치는 테스트 통과 후에만 머지한다.
- 시크릿(API 키 등)은 Harness Secret Manager 참조로만 사용하고 코드에 하드코딩하지 않는다. 로컬은 환경변수 사용.
- backend/frontend 이미지 태그는 git commit SHA 기준으로 생성한다.
- Deploy 스테이지는 staging 통과 후에만 production으로 진행한다.

## 작업 방식
- 각 단계는 최소 1개의 의미 있는 git 커밋으로 마무리한다.
- 새 폴더/컨벤션이 생기면 이 파일을 갱신한다.
