import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";

const timeline = [
  {
    period: "2025.06 – 2025.11",
    title: "ProjectRAID — 네트워크 담당 (7인 팀)",
    detail:
      "서버 권위 구조를 다시 설계하고, 클라이언트 예측과 서버 보정을 구현하였으며, 직접 만든 행동트리로 동행 NPC를 다시 만들었습니다. GStar 2025에 출품하였습니다.",
  },
  {
    period: "2025.04 – 2025.05",
    title: "Sunset Survival — 플레이어 로직·네트워크 (3인 팀)",
    detail:
      "비대칭 멀티플레이 서바이벌입니다. FishNet 기반 동기화를 처음부터 끝까지 완주해 보았습니다.",
  },
  {
    period: "2025.01 – 2025.06",
    title: "내일배움캠프 Unity 게임개발 과정 수료",
    detail: "팀스파르타에서 1,080시간 과정을 수료하였습니다. 장르별 모작과 상태머신 기초를 다진 시기입니다.",
  },
  {
    period: "2026 취득 예정",
    title: "전문학사 — 컴퓨터네트워크 (학점은행제)",
    detail: "전공이 컴퓨터네트워크라 netcode 경험과 직무 관련성이 높습니다.",
  },
  {
    period: "2023",
    title: "육군 병장 만기전역",
    detail: "",
  },
];

export function Experience() {
  return (
    <section
      id="experience"
      className="scroll-mt-16 py-24 sm:py-28"
    >
      <Container>
        <div className="grid gap-14 lg:grid-cols-[1fr_2fr] lg:gap-20">
          <div>
            <SectionLabel index="05." label="EXPERIENCE" />
            <h2 className="font-heading mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
              경험 및 활동
            </h2>
            <p className="text-muted-foreground mt-5 leading-8">
              정규 실무 경력은 아직 없기 때문에, 팀 프로젝트와 부트캠프에서
              쌓은 시간을 적었습니다.
            </p>
          </div>

          <ol className="border-border relative border-l">
            {timeline.map((item) => (
              <li key={item.title} className="relative pb-10 pl-8 last:pb-0">
                <span
                  aria-hidden
                  className="bg-primary absolute top-1.5 -left-[4.5px] size-2 rounded-full"
                />
                <p className="text-primary font-mono text-xs tracking-wide">
                  {item.period}
                </p>
                <h3 className="font-heading mt-2 font-semibold tracking-tight">
                  {item.title}
                </h3>
                {item.detail ? (
                  <p className="text-muted-foreground mt-2 text-sm leading-7">
                    {item.detail}
                  </p>
                ) : null}
              </li>
            ))}
          </ol>
        </div>
      </Container>
    </section>
  );
}
