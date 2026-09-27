#!/bin/sh
# frontend 의존성 설치, 린트, 프로덕션 빌드. (Harness Run 스텝 / 로컬 공용)
set -eu
cd "$(dirname "$0")/../frontend"

echo "==> npm ci"
npm ci
echo "==> npm run lint"
npm run lint
echo "==> npm run build"
npm run build
