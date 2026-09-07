---
name: 반복적으로 가짜 양성으로 판정되는 패턴
description: 이 코드베이스에서 첫눈에 의심스러워 보이지만 컨텍스트상 안전한 패턴들. 매 감사마다 재검증 시간 절감.
type: feedback
---

다음 패턴들은 이 프로젝트에서 반복적으로 "가짜 양성"으로 판정된다.

**1) Zod 에러 메시지 reflection XSS**
- 패턴: `<p>{errors.X.message}</p>` 형태로 폼 에러 표시.
- Why: `lib/schemas/*` 와 `components/examples/forms-showcase.tsx` 의 모든 Zod 메시지는 하드코딩된 한국어 정적 문자열이며 사용자 입력을 보간하지 않음. React children 자동 escape.
- How to apply: 새 스키마가 `${userInput}` 같은 템플릿 리터럴로 메시지를 만들면 그때만 의심. 정적 메시지면 즉시 FP 판정.

**2) Sonner toast description 의 사용자 입력**
- 패턴: `toast.success("...", { description: data.email })`.
- Why: Sonner 의 `description` 은 React 자식으로 렌더되어 escape 됨. 데모 폼은 외부 송출이 없음. XSS 불가.
- How to apply: XSS 측면에선 FP. 단 PII 가시성(세션 리플레이 캡처 위험)은 학습자료 Info 로 기록 가치 있음.

**3) `aria-describedby` 의 동적 ID 문자열**
- 패턴: `aria-describedby={errors.X ? "X-error" : undefined}`, `id={\`${id}-error\`}`.
- Why: 모든 ID 는 컴포넌트 내 리터럴 또는 호출자가 넘긴 상수 prop. 사용자 입력 영향 없음 → DOM clobbering 표면 미형성.
- How to apply: `id` prop 에 사용자 입력이 흘러들어가는 경로가 있으면 그때만 의심. 현재까지 그런 경로 없음.

**4) setTimeout + cancelled flag 패턴 (data-fetching-showcase)**
- 패턴: `useEffect` 내부에서 `let cancelled = false` + cleanup 의 `cancelled = true; clearTimeout(timer)`.
- Why: state-after-unmount, race condition, 메모리 누수 모두 차단. 적절한 패턴.
- How to apply: cancelled 체크가 setState 직전에 있는지, cleanup 에 clearTimeout 이 있는지 둘 다 확인. 둘 다 있으면 FP.

**5) 데모 폼의 약한 비밀번호 정책 / `noValidate`**
- 패턴: `password: z.string().min(8)`, `<form noValidate>`.
- Why: 서버가 없는 학습용 폼. 직접 익스플로잇 경로 없음.
- How to apply: 학습자료 Info 로만 기록. High/Medium 금지.
