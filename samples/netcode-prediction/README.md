# netcode 프로토타입 — 예측/보정 실험 (발췌)

## 무엇인가

게임이 아니라 기술 실험입니다. 제가 만든 계층형 상태머신(HFSM) 캐릭터(구체 상태 13개)를 그대로 FishNet의 클라이언트 예측 파이프라인에 올리면 어떻게 되는지 확인하려고 만들었습니다.

원본 저장소에는 FishNet Pro 소스가 포함되어 라이선스상 공개할 수 없어, 제가 작성한 스크립트만 발췌했습니다. *(전체 단독 작성)*

## 결과 — 절반만 성공했습니다

혼자 플레이할 때는 의도대로 움직입니다. 하지만 **여러 명이 접속하면 캐릭터가 떨리고 상태가 어긋납니다.** 동작한다고 말할 수 있는 상태가 아니므로, 원인으로 확인한 것을 코드 위치와 함께 적습니다.

### 1. 상태머신이 `Time.time`을 쓴다

```
Characters/Player/StateMachines/Movement/State/Grounded/Moving/PlayerRunningState.cs:31, 42
Characters/Player/StateMachines/Movement/State/Grounded/Moving/PlayerSprintingState.cs:37, 60
Characters/Player/StateMachines/Movement/State/Grounded/PlayerDashingState.cs:43, 112
```

달리기→걷기, 질주→달리기 전이와 대시 연속 판정이 벽시계 시간에 걸려 있습니다. 서버 보정이 과거 틱을 다시 재생할 때 같은 틱인데도 `Time.time` 값이 달라지므로, 재생할 때마다 결과가 바뀝니다. 비결정성의 직접적인 원인입니다.

### 2. 틱 시간을 계산해 놓고 쓰지 않는다

```
Characters/Player/Player.cs:125
```

`Replicate` 안에서 `TimeManager.TickDelta`를 꺼내지만 실제 이동·물리 계산으로 넘기지 않습니다. 상태머신은 여전히 프레임 시간 기준으로 움직입니다.

### 3. 입력을 전역 싱글톤에 쓴다

```
Characters/Player/Player.cs:129-134 (Replicate 내부)
Manager/Managers.cs:9  →  public static InputManager Input
```

`Replicate` 안에서 전역 `InputManager`에 입력을 대입합니다. 서버가 접속자 여러 명을 차례로 시뮬레이션하면 서로의 입력을 덮어쓰고, 소유자가 아닌 캐릭터는 `Replicate(default)`로 전역 값을 빈 입력으로 밀어 버립니다. 혼자일 때는 충돌할 상대가 없어 드러나지 않던 문제입니다.

## 고치는 방향 (아직 적용하지 않음)

상태별 경과 시간을 벽시계 대신 **틱 카운터**로 바꾸고, 입력을 전역이 아니라 **캐릭터 인스턴스가 들고 있게** 하는 것입니다. 적용하지 않았으므로 고쳤다고 적지 않았습니다.

## 읽는 순서

1. **`Characters/Player/Player.cs`** — 예측/보정 루프의 전부입니다. `ReplicateData`(6개 필드), `ReconcileData`(위치·회전·속도), `OnTick`/`OnPostTick` 연결까지 이 한 파일에 있습니다.
2. **`Characters/Player/StateMachines/`** — 구체 상태 13개·4단계 상속 HFSM. 위 1번 문제가 있는 곳이기도 합니다.
3. **`StateMachine/`** — 상태 인터페이스와 추상 기반 클래스.

## 고지

- FishNet 소스는 포함하지 않았습니다(참조만). 이 폴더 단독으로는 컴파일되지 않는 읽기용 발췌입니다.
- Steamworks.NET의 공식 배포 파일(SteamManager)은 제 저작이 아니라 제외했습니다.
