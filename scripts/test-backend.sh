#!/bin/sh
# backend 테스트 프로젝트(*Tests.csproj)를 실행한다.
# 테스트 프로젝트가 아직 없으면 그 사실을 남기고 성공 종료한다. (테스트 추가 시 자동으로 실행됨)
set -eu
cd "$(dirname "$0")/.."

found=0
for proj in backend/*/*Tests.csproj backend/*Tests/*.csproj; do
  [ -f "$proj" ] || continue
  found=1
  echo "==> dotnet test $proj"
  dotnet test "$proj" -c Release --nologo
done

if [ "$found" -eq 0 ]; then
  echo "WARN: backend 테스트 프로젝트가 없어 테스트를 건너뜁니다."
fi
