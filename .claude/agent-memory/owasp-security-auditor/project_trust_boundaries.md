---
name: 프로젝트 신뢰 경계 및 데모 폼 비-네트워크 특성
description: claude-nextjs-starterkit 의 폼·데이터 페치 컴포넌트는 모두 클라이언트 사이드 데모이며 API/DB/세션이 없다는 사실. FP 판정의 핵심 컨텍스트.
type: project
---

이 프로젝트(claude-nextjs-starterkit)는 한국어 친화 Next.js 16 스타터킷이며 다음 특성이 보안 감사의 신뢰 경계를 결정한다:

- **API 라우트 없음**: `app/api/*` 부재. 모든 onSubmit 은 `await new Promise(r => setTimeout(r, 600))` + Sonner toast 만 호출.
- **DB·세션·환경변수·인증 없음**: 서버 측 상태가 존재하지 않음.
- **모든 폼은 Client Component (`"use client"`)** 에서 동작하며 RHF + Zod 클라이언트 검증만 수행. `noValidate` 로 브라우저 기본 검증은 끔.
- **Sonner toast** 가 `app/providers.tsx` 에 전역 마운트. `toast.success(title, { description })` 는 React children 으로 렌더되어 자동 escape.
- **데이터 페치 데모 (`data-fetching-showcase.tsx`)** 는 실제 fetch 가 아니라 setTimeout + 하드코딩 배열 — 외부 입력 없음.

Why: 이 컨텍스트를 모르면 "PII 가 toast 에 표시됨", "서버 검증 없음" 같은 항목을 High/Medium 으로 잘못 분류한다. 실제 위험은 학습자가 패턴을 카피했을 때의 2차 영향에 한정된다.

How to apply: 이 프로젝트의 `components/forms/*` 와 `components/examples/*` 를 감사할 때, 사용자 입력의 도달 종점이 toast/DOM-text 라면 XSS 는 자동 차단됨. 진짜 위험은 (a) DOM-XSS 도입 (dangerouslySetInnerHTML, eval, inline script 추가), (b) API 라우트 신규 추가 시 서버 검증 누락, (c) 외부 fetch 도입 시 SSRF 검토. 그 외에는 "학습자료 Info" 카테고리로 기록.
