# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ⚠️ Next.js 16 — 트레이닝 데이터와 다를 수 있음

Next.js 16 는 App Router·캐싱·헤더·프리렌더 동작에 다수의 breaking change 가 들어있다. API 시그니처를 추측하지 말고 반드시 `node_modules/next/dist/docs/` 의 해당 가이드를 먼저 확인하고, deprecation notice 를 무시하지 말 것. (`AGENTS.md` 참고)

## 자주 쓰는 명령

```bash
npm run dev          # next dev (Turbopack, http://localhost:3000)
npm run build        # next build (정적 prerender 22 라우트)
npm run start        # next start (prod)
npm run lint         # eslint (eslint-config-next)
npm audit            # 의존성 취약점 점검
```

테스트 러너는 없음. UI/페이지 검증은 Playwright MCP (`mcp__playwright__*`) 로 dev 또는 prod 서버에 접속해 스크린샷·DOM 으로 수행한다.

## 빅 픽처 아키텍처

이 프로젝트는 **한국어 친화 Next.js 16 스타터킷**이다. 정적 콘텐츠 위주(랜딩·문서·예제)로 API 라우트·DB·환경변수가 없다. 기억해 둘 구조:

### 컴포넌트 5계층 (의존 방향: 위 → 아래만 허용)

`components/section-content.tsx`/스타터킷 자체 문서에서도 강조하는 원칙이다.

```
ui/        shadcn primitives (Radix 기반) — components/ui/*.tsx
common/    앱 전반 재사용 (Logo, ThemeToggle, CodeBlock 등)
layout/    페이지 셸 (Header, Footer, Container, Section)
sections/  랜딩 단위 블록 (Hero, Features, Pricing, FAQ, CTA)
forms/     도메인 폼 (ContactForm 등) — Zod + react-hook-form
```

새 컴포넌트는 위 분류 중 하나에 들어가야 하며, 하위 계층이 상위를 import 하지 않는다.

### 단일 출처 (Single Source of Truth)

- **사이트 메타·내비·푸터·소셜**: `lib/site.ts` 의 `siteConfig`. 헤더/푸터/메타데이터는 모두 여기를 읽는다. 새 nav 항목·SNS 링크 추가 시 이 파일만 수정.
- **문서 섹션 메타**: `lib/docs/sections.ts` 의 `docSections` 배열. id·title·icon·bullets 만 정의.
- **문서 본문(JSX)**: `components/docs/section-content.tsx` 의 함수 컴포넌트. 둘은 `app/docs/[slug]/page.tsx` 에서 `generateStaticParams` 로 결합되어 정적 prerender 된다. 새 섹션 추가는 sections.ts 와 section-content.tsx 양쪽을 갱신해야 한다.
- **Zod 스키마**: `lib/schemas/`. 폼 컴포넌트는 `@hookform/resolvers/zod` 로 연결.

### 라우트 그룹

- `app/(demo)/` — 데모용·랜딩 외 페이지(예: `/contact`). URL 에 `(demo)` 가 붙지 않음.
- `app/docs/[slug]/page.tsx` — 동적 세그먼트지만 `generateStaticParams` 로 SSG 처리.

### 스타일·테마

- **Tailwind CSS v4** — config 파일 없음. 모든 토큰은 `app/globals.css` 의 `@theme inline {}` 블록과 `:root`/`.dark` CSS 변수에 정의됨. shadcn 토큰은 `@import "shadcn/tailwind.css"` 로 합쳐짐.
- **다크 모드** — next-themes 의 class strategy. `<html>` 에 `light`/`dark` 클래스. `app/providers.tsx` 의 `ThemeProvider` 에서 `attribute="class"` 로 설정.
- **폰트** — Pretendard (CDN, sans/heading) + Geist Mono (next/font, mono).
- **유틸 헬퍼** — `lib/utils.ts` 의 `cn()` (clsx + tailwind-merge). 모든 컴포넌트에서 className 머지에 사용.

### 클라이언트 상태

- **전역 UI 상태**: Zustand. 현재는 `stores/use-ui-store.ts` 의 `mobileNavOpen` 만. 새 전역 UI 상태(모달 등)도 여기 추가.
- **토스트**: Sonner. 전역 `<Toaster richColors position="top-right" />` 가 `Providers` 에서 마운트됨. `import { toast } from "sonner"` 만 하면 어디서든 호출 가능.

### 보안 설정 (적용됨)

- `next.config.ts` — HSTS · X-Content-Type-Options · X-Frame-Options · Referrer-Policy · Permissions-Policy 헤더가 모든 라우트에 부착. `poweredByHeader: false`.
- `package.json` overrides — `postcss: ^8.5.10` 으로 트랜지티브 CVE 패치.
- `.mcp/playwright.config.json` — Playwright MCP 가 `chromiumSandbox: true` 로 실행, capability 는 `core` 만.

CSP 는 next-themes 의 inline noFlash 스크립트 때문에 미적용 — 필요해지면 nonce 처리와 함께 별도 작업.

## 프로젝트 규약

- **경로 alias**: `@/*` → 프로젝트 루트 (`tsconfig.json`).
- **TypeScript strict** — `any` 금지(전역 룰).
- **콘텐츠 언어** — 한국어 우선. UI 텍스트·문서·주석·커밋 메시지 한국어. 변수/함수명·식별자는 영어 camelCase / PascalCase.
- 커밋 컨벤션은 사용자 글로벌 룰의 이모지 prefix 사용(`✨ Feat:`, `🐛 Fix:`, `⚡️ Add:` 등).

## Playwright MCP 사용 시

`.mcp.json` 이 변경되면 **Claude Code 재시작 후** 적용된다. 검증용 스크린샷은 prod 서버(`npx next start -p <port>`) 를 권장 — dev 서버는 Turbopack HMR 가 종종 첫 요청에 500 을 낸다(이 코드베이스에서 확인됨).
