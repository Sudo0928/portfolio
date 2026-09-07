import { Boxes, Network, Users } from "lucide-react";

import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";

const strengths = [
  {
    icon: Boxes,
    title: "시스템 구조 설계",
    caption: "Architecture",
  },
  {
    icon: Network,
    title: "네트워크 동기화",
    caption: "Netcode",
  },
  {
    icon: Users,
    title: "팀을 위한 도구",
    caption: "Teamwork",
  },
];

/** 코드로 확인된 숫자만 적는다 */
const facts = [
  { value: "7인", label: "팀 프로젝트 네트워크 담당" },
  { value: "약 140", label: "ProjectRAID 커밋" },
  { value: "73개", label: "자작 행동트리 스크립트" },
  { value: "1,080h", label: "부트캠프 수료 시간" },
];

export function About() {
  return (
    <section id="about" className="bg-band scroll-mt-16 border-y py-24 sm:py-28">
      <Container>
        <SectionLabel index="01." label="ABOUT ME" />

        <div className="mt-8 grid gap-14 lg:grid-cols-[1.1fr_1fr] lg:gap-20">
          <div>
            <h2 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
              안녕하세요, 염기용입니다.
            </h2>
            <div className="text-muted-foreground mt-7 space-y-5 leading-8">
              <p>
                게임의 시스템들이 서로 어긋나지 않도록 잡아 두는 일에 관심을
                가지고 있습니다. 인벤토리와 퀘스트, 장비와 제작처럼 서로 얽혀
                있는 기능들을 어디에서 나누고 어떻게 연결할지 고민하다 보니
                자연스럽게 구조 설계 쪽으로 오게 되었습니다.
              </p>
              <p>
                상태머신과 행동트리는 상용 라이브러리를 사용하기 전에 직접
                만들어 보면서 익혔습니다. 7인 팀 프로젝트에서는 한 명이 혼자
                플레이하는 기준으로 짜여 있던 매니저와 유저 데이터 구조를 다시
                설계하는 일을 맡았습니다.
              </p>
              <p>
                아직 실무 경력이 없는 신입이기 때문에, 잘한 것뿐만 아니라 아직
                풀지 못한 문제도 이 사이트에 그대로 적어 두었습니다. 제가 쓴
                코드를 다시 읽고 무엇이 아쉬웠는지도 프로젝트마다 함께
                적었습니다.
              </p>
            </div>

            <div className="mt-10 flex flex-wrap gap-x-10 gap-y-5">
              {strengths.map(({ icon: Icon, title, caption }) => (
                <div key={title} className="flex items-center gap-3">
                  <Icon className="text-primary size-5" />
                  <div className="leading-tight">
                    <p className="text-sm font-medium">{title}</p>
                    <p className="text-muted-foreground text-xs">{caption}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="space-y-8">
            <div className="grid grid-cols-2 gap-px overflow-hidden rounded-xl border">
              {facts.map((fact) => (
                <div key={fact.label} className="bg-card p-6">
                  <p className="font-heading text-primary text-2xl font-semibold">
                    {fact.value}
                  </p>
                  <p className="text-muted-foreground mt-1.5 text-xs leading-5">
                    {fact.label}
                  </p>
                </div>
              ))}
            </div>

            <blockquote className="border-primary/50 border-l-2 pl-6">
              <p className="leading-8 text-pretty">
                &ldquo;기능 하나를 만드는 것보다 시스템 전체가 어긋나지 않도록
                잡아 두는 일이 훨씬 어렵고 중요하다는 것을 체감하였습니다.&rdquo;
              </p>
              <footer className="text-muted-foreground mt-4 text-sm">
                — 염기용
              </footer>
            </blockquote>
          </div>
        </div>
      </Container>
    </section>
  );
}
