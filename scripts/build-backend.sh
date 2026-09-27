#!/bin/sh
# backend 전체 프로젝트를 Release로 빌드한다. (Harness Run 스텝 / 로컬 공용)
set -eu
cd "$(dirname "$0")/.."

for proj in backend/*/*.csproj; do
  echo "==> dotnet build $proj"
  dotnet build "$proj" -c Release --nologo
done
