// 포트폴리오 프로젝트 데이터.
// 모든 서술은 실제 코드·커밋으로 확인된 사실만 담는다. 측정하지 않은 수치는 쓰지 않는다.
// 한계·미완성도 확인된 근거와 함께 그대로 적는다.

export type ProjectLink = {
  label: string;
  href: string;
};

export type ProjectSection = {
  heading: string;
  paragraphs?: string[];
  bullets?: string[];
};

export type Project = {
  slug: string;
  title: string;
  tagline: string;
  /** 카드에 쓰는 짧은 분류 뱃지 */
  kind: string;
  period: string;
  team: string;
  role: string;
  tech: string[];
  featured?: boolean;
  /** AI 활용 섹션에서 단독으로 소개하는 항목 */
  aiShowcase?: boolean;
  /** 카드·상세 헤더 이미지 */
  cover?: string;
  /** art = 키비주얼, diagram = 구조도 */
  coverKind?: "art" | "diagram";
  /** 저장소 공개 상태에 대한 정직한 고지 */
  status?: string;
  links: ProjectLink[];
  summary: string;
  /** 개요 섹션 제목 (게임이 아닌 항목은 따로 지정) */
  overviewHeading?: string;
  /** 코드가 아니라 게임 자체에 대한 설명 */
  game: {
    genre: string;
    players: string;
    platform: string;
    description: string[];
  };
  sections: ProjectSection[];
};

export const projects: Project[] = [
  {
    slug: "project-raid",
    title: "ProjectRAID",
    tagline: "멀티플레이 헌팅 액션 RPG · GStar 2025 출품",
    kind: "Unity / Multiplayer",
    period: "2025.06 – 2025.11",
    team: "7인 팀",
    role: "네트워크 담당 — 서버 권위 구조 재설계, 클라이언트 예측/서버 보정, 전투 보조 NPC(자작 BT)",
    tech: ["Unity", "C#", "FishNet", "MessagePack", "UniTask", "Addressables"],
    featured: true,
    cover: "/images/project-raid.webp",
    coverKind: "art",
    status:
      "에셋 라이선스 문제로 원본 저장소는 비공개입니다. 대신 제가 작성한 핵심 코드를 팀의 동의를 받아 이 저장소의 samples 폴더에 발췌해 두었습니다.",
    links: [
      {
        label: "네트워크 코어 코드",
        href: "https://github.com/Sudo0928/portfolio/tree/main/samples/raid-network-core",
      },
      {
        label: "행동트리 코드 (73개)",
        href: "https://github.com/Sudo0928/portfolio/tree/main/samples/raid-behaviour-tree",
      },
    ],
    summary:
      "한 명이 플레이하는 기준으로 짜여 있던 프로젝트를 서버 권위 멀티플레이 구조로 다시 설계하였고, 클라이언트 예측과 서버 보정 루프를 직접 작성하였습니다.",
    game: {
      genre: "헌팅 액션 RPG",
      players: "최대 4인 협동",
      platform: "PC (Windows)",
      description: [
        "여럿이 함께 거대한 몬스터를 사냥하고, 사냥에서 얻은 재료로 장비를 만들어 다시 더 강한 사냥에 도전하는 협동 액션 RPG입니다. 파티를 맺어 같은 필드에 들어가고, 퀘스트를 받아 사냥을 나가고, 돌아와 제작과 보상을 정리하는 순환이 게임의 뼈대입니다.",
        "전투는 무기를 휘두르는 실시간 액션이고, 플레이어를 따라다니며 함께 싸우는 동행 NPC가 곁을 지킵니다. 제가 맡은 네트워크 파트는 파티와 퀘스트, 인벤토리, 제작, 화폐, 장비가 여러 사람 사이에서 어긋나지 않도록 만드는 일이었습니다.",
      ],
    },
    sections: [
      {
        heading: "어떤 프로젝트였나",
        paragraphs: [
          "7명이 함께 만든 멀티플레이 헌팅 액션 RPG이고, 내일배움캠프 선정을 통해 GStar 2025에 출품하였습니다. 저는 네트워크 담당으로 합류하여 feature 브랜치를 단독으로 운영하면서 약 140개의 커밋을 남겼습니다.",
          "다만 출품 빌드는 멀티플레이 이슈 때문에 단일 플레이 전투 버전으로 나가게 되었습니다. 그 경위와 배운 점은 아래 한계 항목에 정직하게 적어 두었습니다.",
        ],
      },
      {
        heading: "서버 권위 구조 재설계",
        paragraphs: [
          "합류 시점의 프로젝트는 한 명이 혼자 플레이하는 기준으로 짜여 있었기 때문에 그대로는 멀티플레이를 올릴 수 없었습니다. 그래서 매니저와 유저 데이터 구조를 서버 권위 모델로 다시 설계하는 것이 제 역할의 중심이 되었습니다.",
        ],
        bullets: [
          "중앙 서비스 로케이터가 서브매니저를 비동기(UniTask)로 순차 초기화하도록 정리",
          "기능을 도메인별 네트워크 시스템(상위 10개 클래스, 약 20개 파일)으로 분리해 스폰하도록 재편",
          "커넥션별 유저 매니저를 서버 권위로 두고 인벤토리·퀵슬롯·퀘스트·화폐·장비를 동기화",
          "런타임 상태를 MessagePack 영속 모델로 직렬화해 네트워크 계층과 세이브 계층을 일원화",
          "씬 로딩을 전역·커넥션·파티 단위로 구분하고, 완충 씬을 끼워 교체하며 진행률을 클라이언트 UI에 동기화",
          "종료 시 토큰 기반 핸드셰이크로 접속한 클라이언트의 데이터를 회수·저장한 뒤 종료(타임아웃 기본 10초, 최대 접속 4인)",
        ],
      },
      {
        heading: "클라이언트 예측과 서버 보정",
        paragraphs: [
          "FishNet 틱 루프 위에서 입력(이동·대시·점프 등)을 한 틱 단위 구조체(ReplicateData)로 모아 매 틱 재현하고, 서버가 보내 준 위치와 회전, 속도(ReconcileData)로 보정하는 루프를 직접 작성하였습니다.",
          "중력이나 점프 같은 물리는 프레임이 아니라 틱 단위로 적분해야 매 틱 똑같이 재현되기 때문에, 예측이 서버 보정과 어긋나지 않도록 그 부분까지 맞추었습니다. 메인 NetworkManager 프리팹의 틱레이트는 50입니다.",
        ],
      },
      {
        heading: "전투 보조 NPC — 자작 Behavior Tree (약 2주)",
        paragraphs: [
          "팀원이 먼저 만들었으나 진척이 막혀 있던 동행 NPC(보스가 아니라 플레이어를 따라다니며 전투를 돕는 아군)를, 외부 에셋 없이 직접 설계한 ScriptableObject 기반 행동트리로 다시 만들었습니다. 약 2주가 걸렸습니다.",
        ],
        bullets: [
          "최상위 BTNode 아래 Action·Composite·Decorator·Service 네 계층, Success/Failure/Running 세 상태",
          "Selector·Sequence·Parallel 복합 노드와 Cooldown·Timeout·Repeat·Guard 데코레이터를 직접 구현하였고, 관련 스크립트는 73개가 되었습니다",
          "블랙보드 값 변경 이벤트를 구독한 노드가 실행 중이던 행동을 즉시 중단하고 재판단(reactive abort)",
          "적 탐지·접근·3단계 공격·플레이어 복귀·체력 회복으로 트리 구성, NavMesh 목적지는 0.5초 간격 갱신",
          "실행 상태를 색으로 보여 주는 커스텀 에디터를 Odin Inspector로 제작",
          "NetworkBehaviour로 감싸서 서버에서만 틱하도록 하여, AI 판단을 서버로 모으고 NPC 체력도 서버 기준으로 동기화",
        ],
      },
      {
        heading: "팀을 위한 도구",
        paragraphs: [
          "팀원마다 작업하는 씬이 달라서 멀티플레이 테스트를 할 때마다 네트워크 매니저를 손으로 올려야 했습니다. 그래서 어느 씬에서 Play를 눌러도 필요한 매니저가 자동으로 준비되는 부트스트랩 도구를 단독으로 만들었고, 그 이후로는 누구나 곧바로 멀티플레이 테스트를 돌릴 수 있었습니다.",
          "GStar 개막 직전에는 이동에서 상호작용, 전투, 채집, 보상으로 이어지는 조작 튜토리얼을 짧은 기간 안에 마무리하였습니다. UniTask 비동기 플로우와 Unity Localization으로 구성하였습니다.",
        ],
      },
      {
        heading: "한계와 배운 점",
        paragraphs: [
          "출품 직전 멀티플레이 빌드에서 캐릭터가 미세하게 떨리는 문제가 발생하였습니다. 핑과 지연 요인을 하나씩 배제한 끝에 틱마다 결과가 같게 재현되지 않는 비결정적 코드를 원인으로 좁혔지만, 출품일까지 해결을 장담할 수 없었기 때문에 팀과 상의하여 단일 플레이 전투 버전으로 출품하였습니다. 잘 알려진 방법으로 풀리지 않는 문제일수록 가능성을 하나씩 지워 가며 좁혀야 한다는 것을 그때 체감하였습니다.",
          "네트워크 코어를 혼자 다 만든 것은 아닙니다. 네트워크 매니저와 유저 매니저는 제가 주도하였고 씬 부트스트래퍼는 단독으로 작성하였지만, 일부 네트워크 코어는 팀원과 함께 작업하였으며 보스와 몬스터 코드는 다른 팀원의 영역이었습니다.",
          "발췌한 코드를 다시 읽다가 미완성으로 남은 것도 확인하였습니다. 트랜스포트는 Multipass 위에 여러 종류를 고를 수 있도록 자리를 잡아 두었지만, 실제로 클라이언트를 연결하는 곳에는 로컬(Yak)만 들어가 있어 원격 전환이 완성되지 않았습니다. 로비를 여는 함수도 본문이 비어 있습니다. 당시에는 로컬 환경에서 여러 클라이언트를 띄워 테스트하는 데 집중했기 때문에 원격 경로를 끝까지 연결하지 못했습니다.",
        ],
      },
      {
        heading: "지금 다시 만든다면",
        paragraphs: [
          "발췌해 둔 코드를 다시 읽어 보니 그때는 보이지 않던 것들이 보였습니다. 고치지 않은 채로 두었기 때문에, 무엇이 아쉬웠고 어떻게 바꾸고 싶은지를 그대로 적어 두었습니다.",
        ],
        bullets: [
          "시스템을 타입으로 찾아 주는 함수에서, 딕셔너리에 없으면 씬 전체를 뒤져 캐시하도록 만들었습니다. 그런데 씬에서도 찾지 못하면 null을 그대로 캐시에 넣어 버리기 때문에 그 이후로는 계속 null이 돌아옵니다. 초기화 순서가 어긋나면 조용히 실패하는 구조이므로, 지금이라면 찾지 못했을 때는 캐시에 넣지 않고 초기화가 끝났는지를 먼저 확인하도록 만들겠습니다.",
          "인벤토리나 퀵슬롯 같은 하위 컴포넌트의 참조를 SyncVar로 동기화하고 있습니다. 하지만 이 컴포넌트들은 같은 프리팹 안에 있어서 클라이언트도 직접 찾을 수 있기 때문에 참조를 굳이 네트워크로 보낼 필요가 없었습니다. 지금이라면 참조는 로컬에서 얻고, SyncVar는 실제로 값이 바뀌는 데이터에만 쓰겠습니다.",
          "UserManager의 초기화 함수 안에 인벤토리와 퀵슬롯, 기록, 퀘스트, 화폐, 대화, 장비를 차례로 초기화하는 코드가 나열되어 있습니다. 유저 데이터 종류가 하나 늘어날 때마다 이 함수를 고쳐야 하기 때문에, 지금이라면 하위 시스템이 공통 인터페이스를 구현하도록 하고 목록을 순회하며 초기화하는 방식으로 바꾸겠습니다.",
          "시스템을 스폰하는 초기화 함수가 UniTask를 반환하지만 실제로는 반복문을 한 번에 다 돌고 완료된 결과를 그대로 돌려줍니다. 비동기처럼 보이지만 동기이기 때문에, 스폰할 시스템이 늘어나면 그 프레임이 통째로 멈추게 됩니다. 지금이라면 실제로 프레임을 나누어 스폰하고 진행률을 보여 주도록 만들겠습니다.",
          "NetworkManagerEx 한 클래스가 트랜스포트 전환과 서버 시작·종료, 종료 핸드셰이크, 브로드캐스트 등록, 강제 퇴장까지 맡고 있어서 354줄이 되었습니다. 지금이라면 종료 절차만 따로 떼어 별도 클래스로 옮기고, NetworkManagerEx는 연결 상태만 관리하도록 나누겠습니다.",
          "직접 만든 행동트리에서는 블랙보드가 문자열을 키로 쓰고 있습니다. 키 이름을 잘못 적어도 컴파일할 때 걸리지 않고, 값이 바뀌었을 때 알리는 이벤트도 문자열로 구독하기 때문에 나중에 키 이름을 바꾸면 조용히 동작하지 않게 됩니다. 지금이라면 키를 ScriptableObject 에셋이나 타입이 붙은 구조체로 만들어 컴파일러가 잡아 주도록 하겠습니다.",
          "조건 검사를 담당하는 Guard가 546줄까지 커졌고, 그 안에 조건을 표현하는 클래스와 값 타입을 판별하는 코드까지 함께 들어가 있습니다. 타입 판별이 typeof 비교로 되어 있어서 새로운 타입을 추가할 때마다 이 파일을 고쳐야 합니다. 지금이라면 조건과 값 타입을 별도 파일로 나누고 타입 판별은 제네릭으로 처리하겠습니다.",
          "행동트리를 ScriptableObject로 만들어 두고 NPC마다 통째로 복제하여 사용합니다. 노드가 73개이기 때문에 NPC가 늘어나면 복제 비용과 메모리가 함께 늘어납니다. 지금이라면 트리 구조는 공유하고 실행 중에 바뀌는 상태만 인스턴스별로 분리하는 방식을 시도해 보겠습니다.",
        ],
      },
    ],
  },
  {
    slug: "sunset-survival",
    title: "Sunset Survival",
    tagline: "비대칭 멀티플레이 서바이벌",
    kind: "Unity / Multiplayer",
    period: "2025.04 – 2025.05",
    team: "3인 팀",
    role: "플레이어 로직 · 네트워크",
    tech: ["Unity 6", "C#", "FishNet", "Steamworks.NET", "Unity Google Sheet"],
    featured: true,
    cover: "/images/sunset-survival.webp",
    coverKind: "art",
    status:
      "팀 저장소에는 외부 유료 에셋이 포함되어 있기 때문에 링크로 안내해 드리지 못하였습니다. 결과물은 시연 영상으로 확인하실 수 있습니다.",
    links: [
      { label: "시연 영상 (YouTube)", href: "https://youtu.be/7PB0w4Xyckg" },
    ],
    summary:
      "생존자와 살인마로 나뉘어 심리전을 벌이는 비대칭 멀티플레이 서바이벌입니다. 3인 팀에서 플레이어 로직과 네트워크를 맡았습니다.",
    game: {
      genre: "비대칭 멀티플레이 서바이벌",
      players: "생존자 vs 살인마",
      platform: "PC (Steam 연동)",
      description: [
        "플레이어는 생존자 또는 살인마 중 하나를 맡습니다. 생존자는 맵을 돌아다니며 아이템을 파밍하고 제작해 퀘스트를 클리어해야 하고, 살인마는 그들을 찾아 막아야 합니다. 양쪽이 서로 다른 목표와 정보를 갖는 비대칭 구조라, 소리와 시야를 둘러싼 심리전이 핵심입니다.",
        "팀에서 직접 제작한 사운드로 긴장감을 만들었고, 저는 양 진영의 플레이어 로직과 이러한 상호작용들의 네트워크 동기화를 맡았습니다.",
      ],
    },
    sections: [
      {
        heading: "맡은 부분",
        bullets: [
          "생존자와 살인마 양 진영의 플레이어 로직",
          "FishNet 기반 네트워크 동기화",
        ],
      },
      {
        heading: "왜 의미가 있었나",
        paragraphs: [
          "ProjectRAID에 합류하기 전에 FishNet 기반 멀티플레이를 처음부터 끝까지 팀으로 완주해 본 프로젝트입니다. 여기에서 겪은 동기화 문제들이 이후 RAID에서 구조를 다시 설계할 때의 판단 근거가 되었습니다.",
        ],
      },
    ],
  },
  {
    slug: "netcode-prototype",
    title: "netcode 프로토타입",
    tagline: "예측과 보정 실험 — 멀티에서 실패한 이유까지",
    kind: "Unity / 학습 실험",
    period: "2025",
    team: "개인",
    role: "전체 설계·구현",
    tech: ["Unity", "C#", "FishNet"],
    featured: true,
    cover: "/images/netcode-prototype.webp",
    coverKind: "diagram",
    status:
      "원본 저장소에는 FishNet Pro 소스가 포함되어 있어 라이선스상 공개할 수 없었습니다. 제가 작성한 코드만 이 저장소의 samples 폴더에 발췌해 두었습니다.",
    links: [
      {
        label: "예측/보정 코드",
        href: "https://github.com/Sudo0928/portfolio/tree/main/samples/netcode-prediction",
      },
    ],
    summary:
      "직접 만든 HFSM 캐릭터를 FishNet 예측 파이프라인에 올려 본 실험입니다. 혼자 할 때는 동작하지만 여럿이 접속하면 떨림이 남았고, 그 원인을 코드에서 찾아냈습니다.",
    overviewHeading: "무엇을 실험했나",
    game: {
      genre: "학습용 프로토타입 (게임 아님)",
      players: "테스트 목적 다중 접속",
      platform: "Unity 에디터 / PC 빌드",
      description: [
        "게임이 아니라 기술 실험입니다. 제가 만든 계층형 상태머신 캐릭터(구체 상태 13개)를 그대로 FishNet의 클라이언트 예측 파이프라인에 올리면 어떻게 되는지 확인해 보고 싶어서 만들었습니다.",
        "결과부터 말씀드리면 절반만 성공하였습니다. 무엇이 왜 되지 않는지는 아래에 코드 위치와 함께 적어 두었습니다.",
      ],
    },
    sections: [
      {
        heading: "무엇을 만들었나",
        paragraphs: [
          "FishNet Prediction API 위에서 입력을 ReplicateData(이동·걷기 토글·대시·스프린트·점프·시점의 6개 필드)로 묶어 틱 단위로 전송하고, ReconcileData(위치·회전·속도)로 상태를 복원하는 루프를 작성하였습니다. 소유 클라이언트만 실제 입력을 보내고, 예측 콜백 안에서 캐릭터 상태머신을 돌리는 구조입니다.",
          "혼자 플레이할 때는 의도대로 움직입니다. 입력이 틱에 실려 가고 상태 전이도 정상적으로 이루어집니다.",
        ],
      },
      {
        heading: "지금 이 코드의 한계 — 여럿이 접속하면 떨립니다",
        paragraphs: [
          "하지만 여러 명이 접속하면 캐릭터가 떨리고 상태가 어긋납니다. 동작한다고 적을 수 있는 상태가 아니기 때문에, 원인으로 확인한 세 가지를 그대로 적어 두었습니다.",
        ],
        bullets: [
          "상태머신이 Time.time을 쓰고 있습니다. 달리기와 질주, 대시 상태의 전이 판정이 벽시계 시간에 걸려 있습니다(PlayerRunningState, PlayerSprintingState, PlayerDashingState 6곳). 서버 보정으로 과거 틱을 다시 재생하면 같은 틱인데도 값이 달라지기 때문에, 재생할 때마다 결과가 바뀝니다. 비결정성의 직접적인 원인입니다.",
          "틱 시간을 계산해 놓고 사용하지 않습니다. Replicate 안에서 TickDelta를 꺼내지만(Player.cs:125) 실제 이동과 물리 계산에는 넘기지 않기 때문에, 결국 상태머신은 여전히 프레임 시간 기준으로 움직입니다.",
          "입력을 전역 싱글톤에 쓰고 있습니다. Replicate 안에서 전역 InputManager에 입력을 대입하기 때문에, 서버가 접속자 여러 명을 차례로 시뮬레이션하면 서로의 입력을 덮어쓰게 되고, 소유자가 아닌 캐릭터는 빈 입력으로 전역 값을 밀어 버립니다. 혼자일 때는 충돌할 상대가 없어서 드러나지 않던 문제였습니다.",
        ],
      },
      {
        heading: "그래서 무엇이 남았나",
        paragraphs: [
          "이 실험에서 제가 얻은 것은 동작하는 코드가 아니라 진단하는 눈이었습니다. 예측과 보정 구조에서는 한 틱을 몇 번 다시 실행해도 같은 결과가 나오는지가 전부인데, 벽시계 시간과 전역 상태, 프레임 델타가 그 조건을 조용히 깨뜨리고 있었습니다.",
          "이 감각은 이후 ProjectRAID에서 캐릭터 떨림의 원인을 네트워크 지연이 아닌 비결정적 코드로 좁힐 때 그대로 쓰였습니다. 고치는 방향도 알고 있습니다. 상태별 경과 시간을 틱 카운터로 바꾸고, 입력을 전역이 아니라 캐릭터 인스턴스가 들고 있도록 하는 것입니다. 다만 아직 적용하지는 않았기 때문에 고쳤다고 적지 않았습니다.",
        ],
      },
    ],
  },
  {
    slug: "genshin-hfsm",
    title: "자작 HFSM",
    tagline: "19개 상태 클래스의 계층형 캐릭터 이동 시스템",
    kind: "Unity / 시스템 설계",
    period: "2024",
    team: "개인",
    role: "전체 설계·구현",
    tech: ["Unity", "C#"],
    cover: "/images/genshin-hfsm.webp",
    coverKind: "diagram",
    status: "공개 저장소",
    links: [
      {
        label: "GitHub",
        href: "https://github.com/Sudo0928/GenshinImpactMovementSystem",
      },
    ],
    summary:
      "상태 인터페이스와 추상 상태머신을 직접 설계하고, 상용 게임의 캐릭터 이동을 학습 삼아 계층형 상태머신으로 재현하였습니다.",
    game: {
      genre: "학습 프로젝트 (재현)",
      players: "싱글",
      platform: "Unity 에디터",
      description: [
        "걷기에서 달리기와 질주로 이어지고 점프와 낙하, 착지, 대시가 자연스럽게 섞이는 원신의 캐릭터 이동 감각을 직접 만들어 보면서 계층형 상태머신을 익힌 학습 프로젝트입니다. 무에서 독창적으로 설계한 것이 아니라 재현입니다.",
      ],
    },
    sections: [
      {
        heading: "구조",
        paragraphs: [
          "상태 인터페이스와 추상 상태머신 클래스를 직접 설계하고, 이를 상속하여 Grounded/Airborne에서 Moving/Stopping/Landing으로, 다시 Walking/Running/Sprinting 등 19개 상태 클래스로 내려가는 계층 구조를 만들었습니다(추상·중간 계층 6개, 머신이 인스턴스화하는 구체 상태 13개).",
          "공통 이동과 회전, 경사 부양, 낙하 판정 로직은 상위 상태에 모으고 하위 상태에서는 차이만 재정의하였습니다. 상태 인스턴스는 생성자에서 미리 만들어 두고 전이할 때 재사용하였으며, 공유 런타임 값과 정적 튜닝 값은 각각 재사용 데이터 클래스와 ScriptableObject로 분리하였습니다.",
          "이 HFSM 패턴은 이후 ProjectRAID의 네트워크 예측 플레이어 컨트롤러에 응용하였습니다.",
        ],
      },
      {
        heading: "지금 다시 만든다면",
        bullets: [
          "상태들이 공유하는 값을 PlayerStateReusableData 한 곳에 모아 두었습니다. 덕분에 상태 사이에 값을 넘기기는 편했지만, 어떤 상태가 어떤 값을 바꾸는지 추적하기가 어려워졌습니다. 게다가 회전 목표값 같은 일부 필드는 ref로 반환하고 있어서 바깥에서 내부 값을 직접 바꿀 수 있습니다. 지금이라면 상태가 읽기만 하는 값과 바꾸는 값을 나누고 ref 반환은 없애겠습니다.",
          "달리기와 질주, 대시 상태의 전이 판정을 Time.time으로 재고 있습니다. 싱글 플레이에서는 문제가 없었지만 이 코드를 그대로 네트워크 예측에 올렸을 때 문제가 되었습니다. 그 경위는 netcode 프로토타입 쪽에 자세히 적어 두었습니다.",
          "상태를 상속으로만 확장하였기 때문에, 구르면서 공격하는 것처럼 두 가지 성질을 함께 가지는 동작이 필요해지면 표현하기가 어렵습니다. 지금이라면 이동과 전투를 별도의 상태머신으로 두고 병렬로 돌리는 구조를 먼저 고려하겠습니다.",
        ],
      },
    ],
  },
  {
    slug: "mini-isaac",
    title: "Mini Isaac",
    tagline: "던전 자동 생성과 보스 패턴 — 1주 모작",
    kind: "Unity / 게임플레이",
    period: "2025.02",
    team: "개인",
    role: "전체 구현",
    tech: ["Unity", "C#"],
    status: "공개 저장소 (학습 목적의 모작 과제입니다)",
    links: [{ label: "GitHub", href: "https://github.com/Sudo0928/ProjectI" }],
    summary:
      "게임 아이작을 학습 삼아 일주일 동안 모작한 과제입니다. 방 자동 생성 던전과 보스 패턴을 구현하였습니다.",
    game: {
      genre: "탑다운 로그라이크 (모작)",
      players: "싱글",
      platform: "PC",
      description: [
        "방에서 방으로 이동하며 몬스터를 처치하고 아이템을 모아 보스에 도전하는 탑다운 로그라이크입니다. 원작인 더 바인딩 오브 아이작의 구조를 학습 목적으로 일주일 동안 모작하였습니다.",
      ],
    },
    sections: [
      {
        heading: "핵심 구현",
        paragraphs: [
          "던전의 각 방을 문끼리 연결하는 방식으로 이동 로직을 만들었습니다. 방 프리팹의 모든 방향에 문을 설치해 두고, 현재 방에서 랜덤하게 방향과 문을 선택하여 다음 방의 반대편 문과 연결하는 것을 반복하여 방을 자동으로 생성합니다.",
          "짧은 과제 기간에 맞추어 몬스터와 던전, 아이템 시스템 등 필수 요소만 구현하였습니다.",
        ],
      },
    ],
  },
  {
    slug: "performance-optimization",
    title: "성능 최적화 두 가지",
    tagline: "먼저 재고, 원인을 찾고, 그다음에 고쳤습니다",
    kind: "Unity / 최적화",
    period: "2024 이전",
    team: "개인",
    role: "최적화 작업 전반",
    tech: ["Unity", "C#", "HDRP", "Photon PUN2"],
    status:
      "두 프로젝트 모두 원격 저장소 없이 로컬에서만 작업하였기 때문에 커밋 기록은 없습니다. 대신 제가 작성한 스크립트를 이 저장소의 samples 폴더에 발췌해 두어 코드를 직접 보실 수 있습니다.",
    links: [
      {
        label: "복셀 Chunk 코드",
        href: "https://github.com/Sudo0928/portfolio/tree/main/samples/voxel-chunk",
      },
      {
        label: "HDRP 최적화 코드",
        href: "https://github.com/Sudo0928/portfolio/tree/main/samples/hdrp-optimization",
      },
    ],
    summary:
      "HDRP 프로젝트에서는 그림자와 Occlusion Culling, DLSS로 프레임을 개선하였고, 복셀 프로젝트에서는 보이지 않는 면을 만들지 않고 메시를 결합하여 DrawCall을 줄였습니다.",
    overviewHeading: "무엇을 고쳤나",
    game: {
      genre: "최적화 작업 (두 프로젝트)",
      players: "-",
      platform: "PC",
      description: [
        "만든 것이 실제로 잘 돌아가게 만드는 일을 배운 두 번의 경험입니다. 두 프로젝트 모두 먼저 측정하고 원인을 찾은 다음에 고쳤다는 점에서 저에게는 같은 경험이었습니다.",
      ],
    },
    sections: [
      {
        heading: "Another_world — HDRP 실시간 조명",
        paragraphs: [
          "HDRP를 사용한 실시간 빛 처리가 무거웠는데, 프로파일링 도구로 확인해 보니 VRAM 사용량이 압도적으로 많다는 것을 알게 되었습니다. 그래서 퀄리티에 큰 영향을 주지 않는 작은 사물들의 그림자를 끄고, Occlusion Culling으로 보이지 않는 사물은 아예 그리지 않게 하였으며, 여기에 DLSS까지 적용하여 30FPS 수준이던 것을 200FPS까지 끌어올렸습니다.",
          "HDRP 품질 프로파일에 DLSS를 켜 두었고, 씬에는 Occlusion Culling 베이크 데이터가 함께 들어 있습니다. 근거가 되는 품질 프로파일 파일도 발췌에 함께 넣어 두었습니다. 다만 이 수치는 당시 제 작업 환경에서 잰 값이므로, 해상도와 하드웨어 기준을 함께 놓고 보아야 하는 값입니다.",
          "덧붙이자면 이 최적화의 상당 부분은 코드가 아니라 에디터 설정에 있었고, DLSS는 제가 무언가를 구현한 것이 아니라 품질 프로파일에서 켠 것에 가깝습니다. 그래서 성과라기보다 적용에 가깝다고 생각합니다.",
        ],
      },
      {
        heading: "Bed War — 복셀 메시 생성",
        paragraphs: [
          "처음에는 블록 하나하나를 개별 오브젝트로 처리하다가 성능이 크게 떨어지는 문제를 만났습니다. 이를 해결하기 위해 Chunk 개념을 도입하였고, 블록의 면을 코드로 직접 만들되 여섯 방향마다 이웃 블록이 있는지를 먼저 검사하여 가려서 보이지 않는 면은 아예 만들지 않도록 하였습니다.",
          "그렇게 만들어진 면들은 Chunk 단위로 하나의 메시로 결합하였고, TextureAtlas를 사용해 블록 종류마다 UV 좌표만 다르게 주어 Material은 하나만 쓰도록 하였습니다. 그 결과 DrawCall을 크게 줄일 수 있었습니다. Photon PUN2로 멀티플레이를 붙인 마인크래프트류 프로젝트이기도 합니다.",
        ],
      },
      {
        heading: "여기에서 배운 것",
        paragraphs: [
          "두 번 모두 눈으로 보고 짐작해서 고친 것이 아니라 프로파일러로 먼저 재고 나서 고쳤습니다. 무거워 보이는 것과 실제로 무거운 것이 다르다는 것을 그때 체감하였고, 이후 네트워크에서 캐릭터 떨림의 원인을 찾을 때도 같은 순서로 접근하였습니다.",
        ],
      },
    ],
  },
  {
    slug: "fsm-foundations",
    title: "FSM 기초",
    tagline: "순수 C# 상태머신부터 UI 흐름 제어까지",
    kind: "C# / 기초",
    period: "2025",
    team: "개인",
    role: "전체 구현",
    tech: ["C#", "Unity"],
    status: "공개 저장소",
    links: [
      {
        label: "StateMachineExample",
        href: "https://github.com/Sudo0928/StateMachineExample",
      },
      { label: "Inventory", href: "https://github.com/Sudo0928/Inventory" },
    ],
    summary:
      "엔진 없이도 상태머신을 짤 수 있다는 것을 보여 주는 기초 프로젝트 묶음입니다.",
    overviewHeading: "무엇인가",
    game: {
      genre: "학습 예제",
      players: "-",
      platform: "C# 콘솔 / Unity",
      description: [
        "게임이라기보다는 구조 연습입니다. 상태머신을 라이브러리로 쓰기 전에 직접 짜 보면서 원리를 익혔고, 여기에서 손에 익힌 패턴이 계층형 HFSM으로, 다시 ProjectRAID의 예측 컨트롤러로 이어졌습니다.",
      ],
    },
    sections: [
      {
        heading: "구성",
        bullets: [
          "StateMachineExample: 이전 상태를 Stack에 push/pop하여 BackState를 지원하는 순수 C# 콘솔 FSM",
          "Inventory: FSM을 게임플레이가 아닌 인벤토리 UI 흐름 제어에 적용하고, 데이터는 ScriptableObject/JSON으로 분리",
        ],
      },
    ],
  },
  {
    slug: "claude-code-lectures",
    title: "Claude Code 강의 자료",
    tagline: "AI 코딩 도구를 가르치려고 정리한 강의 10종",
    kind: "AI 활용 / 문서·자동화",
    period: "2026.07 – 2026.08",
    team: "개인",
    role: "강의 기획·집필·배포 자동화 전부 단독",
    tech: ["HTML/CSS", "JavaScript", "GitHub Actions", "GitHub Pages"],
    aiShowcase: true,
    status: "공개 저장소 · 웹에서 바로 열람 가능",
    links: [
      {
        label: "강의 사이트",
        href: "https://sudo0928.github.io/claude-code-master/",
      },
      {
        label: "GitHub",
        href: "https://github.com/Sudo0928/claude-code-master",
      },
    ],
    summary:
      "AI 코딩 도구를 어디까지 이해하고 사용하는지 보여 드리는 자료입니다. 강의 10종과 슬라이드 218장을 직접 썼고, 새 강의를 올리면 목록이 자동으로 갱신되는 배포 파이프라인까지 만들었습니다.",
    overviewHeading: "무엇인가",
    game: {
      genre: "강의 자료 (게임 아님)",
      players: "-",
      platform: "웹 (GitHub Pages)",
      description: [
        "게임이 아니라, 제가 AI 코딩 도구를 어떻게 사용하는지 보여 드리는 자료입니다. Claude Code를 사용하면서 알게 된 것을 다른 사람에게 설명할 수 있을 만큼 정리하다 보니 강의 열 편이 되었습니다.",
        "도구를 사용할 줄 아는 것만으로는 부족하고 그 도구가 왜 그렇게 동작하는지까지 알아야 한다고 생각하였습니다. 이 자료는 그것을 향해 정리해 온 기록입니다.",
      ],
    },
    sections: [
      {
        heading: "무엇을 정리했나",
        paragraphs: [
          "설치와 첫 사용법 같은 입문부터 시작하여, AI가 한 번에 볼 수 있는 정보량을 어떻게 관리할지(컨텍스트 윈도우·컨텍스트 엔지니어링)와 규칙을 코드로 강제하는 방법(훅), 일을 여러 에이전트에게 나누는 방법(서브에이전트·Agent Teams·Dynamic Workflows), 그리고 AI가 일하기 좋은 환경 자체를 설계하는 관점(하네스 엔지니어링)까지 다루었습니다.",
        ],
        bullets: [
          "설치와 첫 사용법: 터미널이 처음인 사람 기준으로 (18장)",
          "컨텍스트 윈도우: 성능을 좌우하는 단 하나의 자원 (18장)",
          "컨텍스트 엔지니어링: 대규모 프로젝트에서 규칙을 어디에 둘지 (29장)",
          "훅: AI의 행동을 내 코드로 강제하는 법 (17장)",
          "스킬과 플러그인: 반복 절차를 재사용 가능한 단위로 (21장)",
          "서브에이전트: 일을 나누어 병렬로 처리하기 (16장)",
          "메타 프롬프트: 학습을 위한 사용법 (18장)",
          "Agent Teams: 에이전트끼리 협업시키기 (27장)",
          "Dynamic Workflows: 다단계 작업을 스크립트로 지휘하기 (28장)",
          "하네스 엔지니어링: AI가 잘 일할 수 있는 환경 설계 (26장)",
        ],
      },
      {
        heading: "손이 가지 않게 만든 것",
        paragraphs: [
          "강의가 늘어날 때마다 목록을 손으로 고치는 일은 금방 번거로워집니다. 그래서 강의 HTML을 폴더에 넣고 push하기만 하면 나머지가 자동으로 도는 구조를 만들었습니다.",
          "GitHub Actions가 폴더를 훑어서 각 HTML의 제목과 태그, 한 줄 설명, 슬라이드 수를 읽어내고, 마지막 커밋 날짜까지 붙여서 목록 파일을 다시 만듭니다. 메인 페이지는 그 파일을 읽어 카드를 그리기 때문에 사람이 목록을 건드릴 일이 없습니다. 순서를 바꾸고 싶을 때만 별도 파일에 파일명을 적으면 되고, 거기에 없는 강의는 최근 수정 순으로 뒤에 자동으로 붙습니다.",
        ],
      },
      {
        heading: "AI를 쓰는 제 방식",
        paragraphs: [
          "설계와 구조, 무엇을 만들지에 대한 판단은 제가 하고 있습니다. AI에게는 반복되는 작업과 초안 작성을 맡기고, 나온 결과는 직접 검토하여 고쳐 쓰고 있습니다. 이 강의 자료들도 그렇게 만들었습니다. 목차와 설명 순서, 무엇을 빼고 무엇을 강조할지는 제가 정하였고, 각 강의는 공식 문서를 근거로 확인하면서 정리하였습니다.",
          "이 포트폴리오 사이트도 같은 방식으로 만들었습니다. 그래서 이 사이트에는 잘한 것뿐만 아니라 아직 풀지 못한 문제도 함께 적혀 있습니다. AI에게 물어서 답을 받는 것보다, 받은 답이 맞는지 확인하는 쪽이 실제로는 더 중요한 일이라고 생각합니다.",
        ],
      },
    ],
  },
];

export const featuredProjects = projects.filter((p) => p.featured);
export const otherProjects = projects.filter(
  (p) => !p.featured && !p.aiShowcase
);
export const aiProject = projects.find((p) => p.aiShowcase);

export function getProject(slug: string) {
  return projects.find((p) => p.slug === slug);
}
