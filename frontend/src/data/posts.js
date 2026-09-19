// 임시 정적 데이터. 9단계에서 GET /api/posts 기반 동적 목록으로 교체 예정.
export const posts = [
  {
    id: '1',
    title: '홈서버 프로젝트를 시작하며',
    date: '2026-09-01',
    summary: 'React + .NET Core로 홈서버를 만드는 여정을 기록합니다.',
    content:
      '이 블로그는 맥미니 홈서버 프로젝트의 기록입니다. 프론트엔드는 React(Vite), 백엔드는 ASP.NET Core Web API를 사용하고, Docker와 Harness, k3s까지 단계별로 확장해 나갈 예정입니다.',
  },
  {
    id: '2',
    title: '1단계: 블로그 레이아웃 잡기',
    date: '2026-09-05',
    summary: 'react-router로 헤더, 목록, 상세 페이지 라우팅을 구성했습니다.',
    content:
      'Git, 터미널, 배포 흐름을 익히는 것이 이번 단계의 목표입니다. 다음 단계에서는 .NET Core CLI 도구로 날씨 데이터를 가져와 이 블로그 헤더에 붙일 예정입니다.',
  },
]
