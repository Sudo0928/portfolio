---
name: 반복 이슈 패턴
description: 전체 리뷰에서 확인된 이 코드베이스의 상습적 이슈 및 개선 필요 패턴
type: project
---

## 1. aria-describedby 연결 누락 (접근성)

`contact-form.tsx`와 `forms-showcase.tsx`의 Field 컴포넌트가 에러 메시지에 id를 부여하지만 Input/Textarea에 `aria-describedby`를 연결하지 않음. 향후 폼 추가 시 반드시 체크.

## 2. false as unknown as true 타입 강제 우회

`forms-showcase.tsx` SignupForm의 defaultValues에서 `false as unknown as true` 사용. `DefaultValues<T>` 유틸리티로 대체해야 함. 새 폼 작성 시 동일 패턴 반복 가능성 높음.

## 3. Field 컴포넌트 중복 정의

`contact-form.tsx`(input prop 방식)와 `forms-showcase.tsx`(children 방식) 두 곳에 Field 컴포넌트가 각자 구현됨. 공통 추출 미완료 상태.

## 4. setTimeout 클린업 비일관성

`code-block.tsx`는 useEffect + clearTimeout 클린업을 올바르게 구현. 반면 `hooks-showcase.tsx`의 CopyToClipboardExample은 클린업 없이 setTimeout 사용. 언마운트 후 state 업데이트 가능성.

## 5. 예제 페이지 메타데이터 폴백 없음

components/forms/hooks/layouts/data-fetching/optimization 예제 페이지 모두 `getExample("slug")`의 undefined 반환 시 title/description이 undefined. `?? "기본값"` 폴백 패턴 미적용.

## 6. Footer의 런타임 new Date() — SSG 연도 고정

Footer Server Component에서 `new Date().getFullYear()` 사용. 정적 빌드 시 빌드 시간 연도로 고정됨. 의도적 트레이드오프지만 주석 없음.

**How to apply:** 폼 관련 코드 리뷰 시 위 1~3번 체크. 클라이언트 컴포넌트 리뷰 시 4번 체크. 새 예제 페이지 추가 시 5번 체크.
