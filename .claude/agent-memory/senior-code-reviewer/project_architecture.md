---
name: 프로젝트 아키텍처 규약
description: claude-nextjs-starterkit의 5계층 컴포넌트 구조, 단일 출처 원칙, 기술 스택 특이사항 요약
type: project
---

## 컴포넌트 5계층 (의존 방향: 위→아래만 허용)

1. ui/ — shadcn primitives (Radix UI 기반, radix-ui 패키지 직접 사용)
2. common/ — Logo, ThemeToggle, CodeBlock, PageHeader
3. layout/ — Header, Footer, Container, Section, MobileNav
4. sections/ — Hero, Features, Pricing, FAQ, CTA (랜딩 블록)
5. forms/ — ContactForm (Zod + react-hook-form)
6. examples/ — 별도 공간, 계층 오염 없이 격리됨

**Why:** 단방향 의존으로 순환 참조 방지. 첫 전체 리뷰 기준 위반 사례 없음.

## 단일 출처 (Single Source of Truth)

- 사이트 메타·nav·소셜: `lib/site.ts` → siteConfig
- 문서 섹션 메타: `lib/docs/sections.ts` → docSections
- 예제 메타: `lib/examples.ts` → examples
- Zod 스키마: `lib/schemas/`
- 전역 UI 상태: `stores/use-ui-store.ts` (Zustand, mobileNavOpen만)

## 기술 스택 특이사항

- **Next.js 16 App Router** — params는 `Promise<{ slug: string }>` 형태 (async params)
- **Tailwind CSS v4** — config 파일 없음, `app/globals.css`의 `@theme inline {}` 블록 사용
- **shadcn** — `radix-ui` 패키지 직접 사용 (기존 `@radix-ui/*` 방식과 다름)
- **Pretendard** — CDN 로드 (`globals.css` @import), next/font/local 미사용
- **Geist Mono** — next/font/google으로 자체 호스팅, `--font-geist-mono` 변수
- **다크 모드** — next-themes class strategy, `<html>` 에 light/dark 클래스
- **useMounted** — `useSyncExternalStore` 기반, SSR-safe hydration mismatch 방지

## 보안 설정

- `next.config.ts`: HSTS(2년), X-Content-Type-Options, X-Frame-Options(DENY), Referrer-Policy, Permissions-Policy 전 라우트 적용
- `poweredByHeader: false`
- CSP 미적용 (next-themes inline script 때문, nonce 처리 필요)
- `package.json` overrides: postcss ^8.5.10 (CVE 패치)

## 라우트 구조

- `app/page.tsx` — 랜딩 (/)
- `app/(demo)/contact/page.tsx` — 문의 (URL: /contact)
- `app/docs/[slug]/page.tsx` — 동적이지만 generateStaticParams로 SSG
- `app/examples/` — 예제 하위 라우트 6개

**How to apply:** 새 파일 제안 시 위 계층 분류 기준으로 위치 결정. siteConfig/docSections 이외 경로에 nav나 메타 추가 제안 금지.
