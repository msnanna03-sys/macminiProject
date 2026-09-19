# 맥미니 홈서버 프로젝트

React(Vite) + .NET Core Web API 기반의 홈서버/자동화 시스템. Docker Compose로 컨테이너화하고,
Harness로 CI/CD, k3s로 오케스트레이션하는 것을 최종 목표로 단계적으로 확장한다.

## 스택

| 계층 | 기술 |
| --- | --- |
| 프론트엔드 | React (Vite) |
| 백엔드 | .NET Core (ASP.NET Core Web API) |
| 컨테이너 | Docker / Docker Compose |
| CI/CD | Harness |
| 오케스트레이션 | k3s (경량 Kubernetes) |

## 폴더 구조 (모노레포)

```
/
├── frontend/   # React (Vite) 앱
├── backend/    # .NET Core 프로젝트들 (WeatherApi, WeatherCli, Crawler 등)
└── CLAUDE.md
```

새 서비스는 `backend/<ServiceName>` 또는 `frontend/` 하위에 추가하고, 기존 구조를 벗어나지 않는다.

## 코딩 컨벤션

- **C#**: 최신 문법 사용 (top-level statements, record, pattern matching, nullable reference types 등).
- **React**: 함수형 컴포넌트 + hooks만 사용한다. 클래스 컴포넌트 금지.
- 각 단계는 이전 단계의 결과물을 이어받아 확장한다. 새로 갈아엎지 않는다.

## 배포 규칙 (Harness Policy와 1:1로 대응)

- `main` 브랜치는 테스트 통과 후에만 머지한다.
- 시크릿(API 키 등)은 반드시 Harness Secret Manager 참조로만 사용하고, 코드에 하드코딩하지 않는다.
- backend/frontend 이미지 태그는 git commit SHA 기준으로 생성한다.
- Deploy 스테이지는 staging 통과 후에만 production으로 진행한다.

## 진행 방식

- 각 단계 시작 전 프로젝트 구조를 먼저 요약하고 시작한다.
- 단계가 늘어날 때마다 새 폴더/컨벤션을 이 파일에 추가한다.
- 각 단계는 최소 1개의 의미 있는 git 커밋으로 마무리한다.
