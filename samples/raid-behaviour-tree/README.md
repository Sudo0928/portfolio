# ProjectRAID — 자작 Behavior Tree (발췌, 73개 스크립트)

## 어떤 게임의, 어떤 NPC인가

**멀티플레이 헌팅 액션 RPG** ProjectRAID(최대 4인 협동, GStar 2025 출품)에서, 플레이어를 따라다니며 함께 싸우는 **동행 NPC**의 두뇌입니다. 보스가 아니라 아군입니다 — 플레이어 곁을 지키다가 적을 발견하면 접근해 공격하고, 멀어지면 돌아오고, 체력이 떨어지면 회복합니다.

팀원이 먼저 만들었으나 진척이 막혀 있던 것을, 외부 에셋 없이 직접 설계한 ScriptableObject 기반 행동트리로 약 2주(2025.10.28 – 11.12) 만에 다시 만들었습니다. *(단독 설계·작성)*

## 구조

- 최상위 `BTNode` 아래 **Action / Composite / Decorator / Service** 네 계층, Success / Failure / Running 세 상태
- `Composite/` — Selector · Sequence · Parallel
- `Decorator/` — Cooldown · Timeout · Repeat · Guard 등
- `Blackboard/` — 값 변경 시에만 이벤트를 발행하는 블랙보드. 이벤트를 구독한 노드가 실행 중이던 행동을 즉시 중단하고 재판단합니다(reactive abort). 플레이어 상태 변화나 적 등장에 곧바로 반응하는 핵심 장치입니다.
- `Action/` · `Service/` — NavMesh 추적·복귀·적 탐지·접근·3단계 공격·체력 회복 등 실제 행동. 길찾기 부하를 줄이려 NavMesh 목적지는 0.5초 간격으로만 갱신합니다.

## 네트워크 결합

이 BT는 NetworkBehaviour로 감싸 **서버에서만 틱**합니다. AI 판단을 서버로 모으고, NPC 체력은 SyncVar로 서버 기준 동기화했습니다.

## 고지

- Odin Inspector(커스텀 에디터·직렬화), ECM2, FishNet은 서드파티라 소스를 포함하지 않았습니다 — `using` 참조만 있습니다. 이 폴더 단독으로는 컴파일되지 않는 읽기용 발췌입니다.
- 실행 상태를 색으로 보여 주는 Odin 기반 커스텀 에디터도 직접 만들었습니다(에디터 코드 일부 포함).

## 지금 다시 만든다면

**블랙보드의 문자열 키** — `BTBlackBoard`가 `Dictionary<string, BlackboardHolder>`로 값을 들고 있고, 값 변경 이벤트(`OnKeyChanged`)도 문자열로 구독합니다. 키를 잘못 적어도 컴파일 시점에 걸리지 않고, 나중에 키 이름을 바꾸면 조용히 동작하지 않게 됩니다. 키를 ScriptableObject 에셋이나 타입이 붙은 구조체로 만들어 컴파일러가 잡아 주도록 바꾸고 싶습니다.

**`Decorator/Guard.cs` (546줄)** — Guard 본체와 `GuardCondition`, 값 타입 판별(`ValueTypeTag`)이 한 파일에 함께 있습니다. 타입 판별이 `typeof` 비교라 새 타입을 추가할 때마다 이 파일을 고쳐야 합니다. 파일을 나누고 타입 판별은 제네릭으로 처리하는 것이 맞습니다.

**`BTNode.Clone()` 기반 런타임 복제** — 트리를 ScriptableObject로 두고 NPC마다 통째로 복제합니다. 노드가 73개이므로 NPC 수만큼 인스턴스가 늘어납니다. 트리 구조는 공유하고 실행 중 바뀌는 상태만 인스턴스별로 분리하는 방식을 시도해 보고 싶습니다.

**`BTNode`의 public 필드** — 시각화를 위해 `status`와 `frame`을 public 필드로 노출했습니다. 에디터 전용이라면 조건부 컴파일로 감싸는 편이 안전합니다.

**이벤트를 중복 등록하고 해제하지 않습니다 (`Composite/SelectorWithObserver.cs:22-23`, `SequenceWithObserver.cs:22-23`).**

```csharp
foreach (var key in _observeKeys)
    _bb.OnKeyChanged += __OnBBChanged;
```

`key`를 루프 안에서 쓰지 않기 때문에, 감시 키가 세 개면 **같은 핸들러가 세 번 등록**되어 값이 한 번 바뀔 때 콜백이 세 번 호출됩니다. 게다가 이 저장소 전체에 `OnKeyChanged -=` 가 한 줄도 없습니다. NPC가 사라져도 블랙보드가 죽은 노드를 계속 붙잡고 있게 되고, `OnInit`이 다시 불리면 구독이 계속 쌓입니다.

두 파일에 같은 코드가 복사되어 있다는 점도 문제입니다. 한쪽을 고치면서 다른 쪽을 놓치기 쉬운 구조로 만들어 두었습니다. 감시 키별로 다른 처리가 필요한 것이 아니라면 등록은 한 번이면 충분하고, `OnExit`이나 `Reset`에서 반드시 해제해야 합니다. 자랑스럽게 적어 온 reactive abort의 구현에 이런 기본적인 실수가 있었다는 것을 발췌를 정리하면서 알게 되었습니다.
