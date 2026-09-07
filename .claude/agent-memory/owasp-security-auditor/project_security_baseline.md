---
name: 적용된 보안 베이스라인 위치
description: 이 프로젝트에 이미 적용된 보안 헤더·의존성 패치·MCP 격리 설정의 위치. 동일 항목을 재차 지적하지 않기 위함.
type: project
---

이미 적용되어 재지적 불필요한 보안 베이스라인:

- **`next.config.ts`** — 모든 라우트에 부착된 헤더:
  - HSTS (Strict-Transport-Security)
  - X-Content-Type-Options: nosniff
  - X-Frame-Options
  - Referrer-Policy
  - Permissions-Policy
  - `poweredByHeader: false`

- **`package.json` overrides** — `postcss: ^8.5.10` 으로 트랜지티브 CVE 패치.

- **`.mcp/playwright.config.json`** — `chromiumSandbox: true`, capability `core` 만 허용.

- **CSP 미적용은 의도적**: next-themes 의 inline noFlash 스크립트 때문. CLAUDE.md 에 명시. CSP 도입 시 nonce 처리 필요 — 별도 작업.

- **`as unknown as` 패턴**: 2026-05-02 기준 코드베이스 전체 0건 (`forms-showcase.tsx` 의 `false as unknown as true` 가 마지막이었고 이번 PR 에서 제거됨).

- **DOM-XSS 직접 표면**: `dangerouslySetInnerHTML` / `eval(` / `new Function` 모두 코드베이스에 0건 (2026-05-02 grep 검증).

Why: 이 베이스라인을 모르면 "보안 헤더 누락", "of postcss CVE", "타입 우회 패턴" 등을 새 발견으로 잘못 보고하게 된다.

How to apply: 새 PR 감사 시 위 항목들이 회귀하지 않는지(헤더 제거, dangerouslySetInnerHTML 도입, as unknown as 부활)만 확인하고 다시 발견 보고하지 말 것. 회귀가 있으면 그때만 High/Medium 으로 보고.
