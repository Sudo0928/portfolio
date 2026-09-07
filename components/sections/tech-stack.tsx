import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";

const stack = [
  { name: "Unity", caption: "게임 엔진" },
  { name: "C#", caption: "주 언어" },
  { name: "FishNet", caption: "네트워킹" },
  { name: "MessagePack", caption: "직렬화·세이브" },
  { name: "UniTask", caption: "비동기 흐름" },
  { name: "Odin Inspector", caption: "에디터 툴" },
];

const groups = [
  {
    title: "주력",
    items: [
      "서버 권위 구조 설계 (매니저·유저 데이터 재설계, 도메인별 시스템 분리)",
      "직접 만든 상태머신과 행동트리 (계층형 HFSM, ScriptableObject 기반 BT)",
    ],
  },
  {
    title: "실전 경험",
    items: [
      "FishNet 클라이언트 예측/서버 보정, SyncVar·RPC, Multipass 트랜스포트",
      "Addressables 부트스트랩, Unity Localization, Input System",
      "프로파일링 기반 최적화 (Occlusion Culling·DLSS·메시 결합·TextureAtlas)",
    ],
  },
  {
    title: "학습 중",
    items: [
      "Steamworks.NET P2P (골격 수준이고 아직 완성하지 못하였습니다)",
      "데이터 지향 스킬 시스템, DOTS(ECS)",
    ],
  },
];

export function TechStack() {
  return (
    <section
      id="tech"
      className="scroll-mt-16 py-24 sm:py-28"
    >
      <Container>
        <div className="grid gap-14 lg:grid-cols-[1fr_1.6fr] lg:gap-20">
          <div>
            <SectionLabel index="03." label="TECH STACK" />
            <h2 className="font-heading mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
              사용 기술
            </h2>
            <p className="text-muted-foreground mt-5 leading-8">
              도구를 늘리기보다는 사용하는 도구가 어떻게 동작하는지 아는 쪽을
              택하였습니다.
            </p>
          </div>

          <div>
            <div className="grid grid-cols-2 gap-px overflow-hidden rounded-xl border sm:grid-cols-3">
              {stack.map((item) => (
                <div key={item.name} className="bg-card px-5 py-6">
                  <p className="font-heading font-semibold tracking-tight">
                    {item.name}
                  </p>
                  <p className="text-muted-foreground mt-1 text-xs">
                    {item.caption}
                  </p>
                </div>
              ))}
            </div>

            <div className="mt-10 space-y-7">
              {groups.map((group) => (
                <div key={group.title}>
                  <p className="text-primary text-xs font-medium tracking-[0.2em]">
                    {group.title.toUpperCase()}
                  </p>
                  <ul className="mt-3 space-y-2">
                    {group.items.map((item) => (
                      <li
                        key={item}
                        className="text-muted-foreground flex gap-3 text-sm leading-7"
                      >
                        <span
                          aria-hidden
                          className="bg-primary/50 mt-3 size-1 shrink-0 rounded-full"
                        />
                        <span>{item}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}
