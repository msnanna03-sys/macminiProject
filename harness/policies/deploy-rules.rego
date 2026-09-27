# Harness Policy 초안 — CLAUDE.md "배포 규칙"과 1:1 대응
# 적용: Project Settings > Policies > New Policy 에 붙여넣고, Policy Set(On Run, Pipeline)에 연결한다.
# 주의: 파이프라인 YAML 구조(환경 ref 이름, 스텝 type 등)는 실제 파이프라인에 맞춰 조정하고
#       Harness Policy 편집기의 Testing 탭에서 샘플 입력으로 검증한 뒤 사용한다.
package pipeline

# 규칙 1) 시크릿 하드코딩 금지
# 이름에 key/token/secret/password가 들어간 환경변수는 Secret Manager 참조만 허용한다.
deny[msg] {
	stage := input.pipeline.stages[_].stage
	step := stage.spec.execution.steps[_].step
	value := step.spec.envVariables[name]
	regex.match("(?i)(key|token|secret|password)", name)
	not startswith(value, "<+secrets.getValue(")
	msg := sprintf("시크릿 하드코딩 금지: 스테이지 '%s' 스텝 '%s'의 환경변수 '%s'는 Secret Manager 참조여야 합니다", [stage.name, step.name, name])
}

# 규칙 2) 이미지 태그는 commit SHA
deny[msg] {
	stage := input.pipeline.stages[_].stage
	step := stage.spec.execution.steps[_].step
	step.type == "BuildAndPushDockerRegistry"
	tag := step.spec.tags[_]
	not contains(tag, "<+codebase.commitSha>")
	msg := sprintf("이미지 태그는 commit SHA여야 합니다: 스테이지 '%s' 스텝 '%s'의 태그 '%s'", [stage.name, step.name, tag])
}

# 규칙 3) production 배포 전에 승인 스테이지 필수
# 환경 identifier가 "production"인 배포 스테이지보다 앞에 Approval 스테이지가 있어야 한다.
deny[msg] {
	stage := input.pipeline.stages[i].stage
	stage.type == "Deployment"
	stage.spec.environment.environmentRef == "production"
	not approval_before(i)
	msg := sprintf("production 배포 스테이지 '%s' 앞에 Approval 스테이지가 필요합니다", [stage.name])
}

approval_before(i) {
	input.pipeline.stages[j].stage.type == "Approval"
	j < i
}
