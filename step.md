# 단계별 체크리스트

기준 문서: `맥미니_개발커리큘럼.md`

## 0단계 — 사전 준비
- [ ] .NET SDK 설치 (`brew install --cask dotnet-sdk`)
- [ ] Node.js 설치 (`brew install node`)
- [ ] Docker Desktop 설치 (`brew install --cask docker`)
- [ ] Claude Code 설치 (`npm install -g @anthropic-ai/claude-code`)
- [ ] 프로젝트 루트 생성 및 `git init`
- [ ] `CLAUDE.md` 작성 (스택, 모노레포 구조, 코딩 컨벤션)

## 1단계 (하) — 개인 블로그/포트폴리오 (React) · 3~5일
- [v] `npm create vite@latest frontend -- --template react`로 프론트엔드 생성
- [v] 블로그 레이아웃 구현 (헤더, 포스트 목록, 상세 페이지 react-router 라우팅)
- [ ] GitHub Pages 또는 Vercel로 배포
- [ ] `git add/commit/push` 및 PR 생성 연습 (commit/push 완료, PR 미진행)
- [v] "오늘의 음성 메모" 업로드 UI 추가 (.m4a/.mp3 선택 + 업로드 버튼, 대상 `/api/voice-memo`만 지정)
- [ ] Harness 계정 생성 및 프로젝트("homelab") 생성
- [ ] Harness Policy(OPA) 초안 작성 (시크릿 하드코딩 금지 / production 승인 필수 / 이미지 태그 = commit SHA)
- [v] `CLAUDE.md`에 "배포 규칙" 섹션 추가 (Harness Policy와 1:1 대응)
- [v] 단계 마무리 git 커밋 (`4c34da0`)

## 2단계 (하) — CLI 도구 (.NET Console App) · 2~3일
- [v] `dotnet new console -o backend/WeatherCli`
- [v] OpenWeatherMap API 호출 및 오늘 날씨 콘솔 출력 (구현 완료, 실제 키로 출력 확인은 미진행)
- [v] API 키를 환경변수(`WEATHER_API_KEY`)로 관리
- [v] 단계 마무리 git 커밋

## 3단계 (하) — REST API 서버 (ASP.NET Core Web API) · 4~5일
- [v] `dotnet new webapi -o backend/WeatherApi`
- [v] WeatherCli 로직을 `GET /api/weather` 엔드포인트로 이전
- [v] Swagger 적용
- [v] React 날씨 컴포넌트 작성 후 블로그 헤더에 배치
- [v] frontend → backend API 호출 구조 동작 확인 (Vite 프록시 경유 호출까지 확인, 실제 날씨 데이터는 키 활성화 후 확인 필요)
- [v] 단계 마무리 git 커밋

## 4단계 (중) — 홈 자동화 대시보드 · 1주
- [ ] Home Assistant 준비 (또는 mock 센서 엔드포인트로 대체)
- [ ] WeatherApi에 Home Assistant REST API 서비스 레이어 추가
- [ ] 조명 on/off, 온도 조회 엔드포인트 구현
- [ ] frontend 대시보드 페이지 (기기 상태 카드 + 토글 제어)
- [ ] 단계 마무리 git 커밋

## 5단계 (중) — Docker 컨테이너화 · 1주
- [ ] backend Dockerfile 작성 (멀티스테이지 빌드)
- [ ] frontend Dockerfile 작성 (nginx 서빙)
- [ ] `docker-compose.yml`에 backend, frontend, home-assistant 통합
- [ ] `docker compose up -d`로 전체 시스템 기동 확인
- [ ] 단계 마무리 git 커밋

## 6단계 (중) — 파일 동기화 · 3~4일
- [ ] Syncthing 설치 (`brew install syncthing`)
- [ ] `~/homelab` 폴더를 다른 기기와 동기화 설정
- [ ] 동기화 동작 확인
- [ ] 단계 마무리 git 커밋

## 7단계 (중) — 크롤링/데이터 파이프라인 · 1~1.5주
- [ ] `dotnet new console -o backend/Crawler`
- [ ] WeatherApi에 EF Core SQLite 패키지 추가
- [ ] 주기 수집 백그라운드 워커 구현 (Hangfire 또는 BackgroundService) 후 SQLite 저장
- [ ] `GET /api/history` 엔드포인트 추가
- [ ] 대시보드에 recharts 시각화 컴포넌트 추가
- [ ] 단계 마무리 git 커밋

## 8단계 (상) — CI/CD (Harness) · 1.5주
- [ ] GitHub에 리포지토리 push
- [ ] Harness ↔ GitHub 연동
- [ ] backend/frontend build/test 스크립트 작성 (Run Step에서 호출)
- [ ] `docker-compose.yml`을 `.env` 기반 환경변수 구성으로 리팩터링
- [ ] Harness 파이프라인 구성: Build → Push (Docker Hub/GHCR) → Deploy
- [ ] 맥미니에 Harness Delegate를 Docker로 실행 (compose 서비스로 포함)
- [ ] 파이프라인이 `CLAUDE.md` 배포 규칙을 따르는지 확인 (이미지 태그 = commit SHA, 시크릿은 Secret Manager 참조)
- [ ] 1단계 Policy Set을 파이프라인 Pipeline Evaluation에 연결
- [ ] 파이프라인 YAML에 Policy Set 참조 추가 (Deploy 스테이지 진입 전 평가)
- [ ] staging 통과 후 production으로 진행하는 순서 확인
- [ ] `skill-creator`로 "harness-pipeline" 스킬 생성
- [ ] push 시 자동 빌드·배포 동작 확인
- [ ] 단계 마무리 git 커밋

## 9단계 (상) — 로컬 LLM 서빙 · 1주
- [ ] Ollama 설치 및 모델 받기 (`ollama pull llama3.1`)
- [ ] WeatherApi에 ChatController 추가 (`POST /api/chat`, 스트리밍 응답)
- [ ] frontend 챗봇 UI를 대시보드에 추가 (스트리밍 실시간 렌더링)
- [ ] whisper.cpp 설치 (`brew install whisper-cpp`)
- [ ] VoiceMemoController 구현 (`POST /api/voice-memo`)
  - [ ] whisper.cpp로 음성 전사
  - [ ] Ollama로 블로그 포스트 형식(제목+본문) 정리
  - [ ] SQLite Posts 테이블에 저장
  - [ ] 저장된 post id 반환
- [ ] 1단계 음성 메모 폼을 실제 API에 연결 (전사 중 → 정리 중 → 완료 상태 표시)
- [ ] 블로그 포스트 목록을 `GET /api/posts` 기반 동적 목록으로 전환
- [ ] 단계 마무리 git 커밋

## 10단계 (상) — k3s 오케스트레이션 · 1.5~2주
- [ ] k3d 설치 및 클러스터 생성 (`k3d cluster create homelab`)
- [ ] docker-compose를 Kubernetes manifest(Deployment, Service, Ingress)로 변환
- [ ] frontend, backend, home-assistant, ollama를 각각 별도 Deployment로 분리
- [ ] Harness Deploy 스테이지를 `docker compose` 방식에서 `kubectl apply` 방식으로 변경
- [ ] push → 빌드·테스트 → 이미지 push → k3s 자동 배포 전 과정 확인
- [ ] 단계 마무리 git 커밋

---

## 공통 체크 (매 단계)
- [ ] 단계 시작 전 Claude Code로 프로젝트 구조 요약
- [ ] 새 폴더/컨벤션이 생기면 `CLAUDE.md` 갱신
- [ ] 최소 1개의 의미 있는 git 커밋으로 마무리
