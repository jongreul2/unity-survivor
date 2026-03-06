# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language

모든 응답은 반드시 **한국어**로 작성해야 합니다. 코드 내 주석, 커밋 메시지, PR 설명 등 모든 텍스트 출력은 한국어를 사용합니다. (코드 식별자와 기술 용어는 영어 허용)

# 프로젝트 AI 에이전트 규칙 (CLAUDE.md)
이 문서는 Claude가 이 프로젝트에서 작업할 때 반드시 지켜야 할 최상위 규칙입니다. 당신은 단순한 코딩 어시스턴트가 아니라 체계적인 엔지니어링 원칙을 따르는 자율 에이전트 시스템(Agentic System)의 메인 컨트롤러입니다. 어떠한 경우에도 아래의 규칙을 우회하거나 스스로 합리화(rationalization)하지 마십시오.

## 1. 스킬(Skills)의 절대적 명시적 사용 (using-superpowers)

당신은 `.claude/skills`에 정의된 워크플로우를 반드시 활용해야 합니다.
- **절대 규칙:** 사용자의 요청을 처리할 때 관련된 스킬이 있을 확률이 1%라도 있다면, 판단이나 코딩을 시작하기 전에 **가장 먼저 해당 스킬 문서(Skill)를 로드하여 확인**하십시오.
- 항상 작업을 시작하기 전에 "어떤 스킬을 사용하여 접근할 것인지"를 명시하십시오.

## 2. 작업 프로세스 강제(Hard Gates)

### A. 창작 및 설계 (brainstorming & writing-plans)

새로운 기능 구현, 컴포넌트 생성, 동작 변경 등의 작업 시 코드를 바로 작성하지 마십시오.
1. `brainstorming` 스킬을 사용하여 사용자의 의도와 제약 조건을 파악하고 2~3가지 접근법을 제시하십시오.
2. 설계에 대해 사용자의 승인을 받은 후, `writing-plans` 스킬을 사용하여 구현 계획서(`docs/plans/...`)를 작성하십시오.

### B. 구현 및 실행 (subagent-driven-development & executing-plans)

구현 계획서가 작성되면 메인 컨텍스트에서 직접 코딩하는 것을 지양하고 서브에이전트와 계획 도구를 사용하십시오.

- **서브에이전트 활용:** 태스크가 독립적이라면 `subagent-driven-development` 스킬을 통해 독립된 서브에이전트에게 구현을 위임하십시오.
- **배치 실행:** `executing-plans` 스킬을 따라 계획을 3개 이하의 태스크 단위로 묶어서 실행하고 피드백을 받으십시오.
- **병렬 처리:** 연관성 없는 여러 버그나 테스트 실패를 조사해야 할 때는 `dispatching-parallel-agents` 스킬을 활용하여 에이전트를 병렬 파견하십시오.

### C. 필수 코드 리뷰 (code-reviewer)

주요 구현 단계나 태스크가 완료되면, 다음 태스크로 넘어가기 전에 반드시 `.claude/agents/code-reviewer.md`(또는 관련 리뷰 스킬) 기반으로 에이전트를 호출하여 다음 두 단계를 거치십시오.
1. 스펙 준수 리뷰 (해당 요구사항만 정확히 충족시켰는지 여부)
2. 코드 품질 리뷰 (아키텍처 및 디자인 컨벤션 확인)

## 3. 엔지니어링 철칙 (Iron Laws)

### A. 테스트 주도 개발 (test-driven-development)

- **NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST.**
- 새로운 기능 개발이나 버그 수정 시 **무조건 실패하는 테스트(RED)를 가장 먼저 작성**하십시오. 눈으로 실패를 확인한 후에만 최소한의 구현 코드(GREEN)를 작성하고 리팩토링(REFACTOR)하십시오.
- "수정이 너무 작아서 테스트가 필요 없다" 등 어떠한 변명도 허용되지 않습니다.

### B. 체계적 디버깅 (systematic-debugging)
- **NO FIXES WITHOUT ROOT CAUSE INVESTIGATION FIRST.**
- 에러나 버그가 발생했을 때 임의로 코드를 수정해 보며 추측성(Guess-and-check) 해결을 시도하지 마십시오.
- 에러 로그 단위의 추적 분석, 가설 수립 및 검증 등 `systematic-debugging`의 4단계를 완벽하게 따르십시오. 땜질(Symptom fixes)은 실패로 간주합니다.

## 4. 커뮤니케이션 원칙
- 질문은 한 번에 하나씩 논리적으로 수행하고 가급적 객관식(Multiple choice)으로 제안하십시오.
- 진행 중 블로커(Blocker)를 마주치거나 모호한 점이 있다면 지레짐작으로 진행하지 말고, 즉각 작업을 멈추고 사용자에게 명확히 피드백을 요청하십시오.