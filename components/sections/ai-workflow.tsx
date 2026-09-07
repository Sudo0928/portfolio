import Link from "next/link";
import { ArrowRight, ArrowUpRight } from "lucide-react";

import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";
import { aiProject } from "@/lib/projects";

const lectures = [
  { title: "설치와 첫 사용법", slides: 18 },
  { title: "컨텍스트 윈도우", slides: 18 },
  { title: "컨텍스트 엔지니어링", slides: 29 },
  { title: "훅", slides: 17 },
  { title: "스킬과 플러그인", slides: 21 },
  { title: "서브에이전트", slides: 16 },
  { title: "메타 프롬프트", slides: 18 },
  { title: "Agent Teams", slides: 27 },
  { title: "Dynamic Workflows", slides: 28 },
  { title: "하네스 엔지니어링", slides: 26 },
];

export function AiWorkflow() {
  return (
    <section id="ai" className="bg-band scroll-mt-16 border-y py-24 sm:py-28">
      <Container>
        <div className="grid gap-14 lg:grid-cols-[1fr_1.5fr] lg:gap-20">
          <div>
            <SectionLabel index="04." label="AI WORKFLOW" />
            <h2 className="font-heading mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
              AI를 쓰는 방식
            </h2>
            <div className="text-muted-foreground mt-5 space-y-4 leading-8">
              <p>
                설계와 구조, 무엇을 만들지에 대한 판단은 제가 하고 있습니다.
                AI에게는 반복되는 작업과 초안을 맡기고, 나온 결과는 직접
                검토하여 고쳐 쓰고 있습니다.
              </p>
              <p>
                도구를 사용할 줄 아는 것만으로는 부족하고 그 도구가 왜 그렇게
                동작하는지까지 알아야 한다고 생각하였기 때문에, 알게 된 것을 다른
                사람에게 설명할 수 있을 만큼 정리하다 보니 강의가 되었습니다.
              </p>
            </div>
            {aiProject ? (
              <div className="mt-8 flex flex-wrap items-center gap-5">
                <Link
                  href={`/projects/${aiProject.slug}`}
                  className="border-primary/60 hover:bg-primary/10 hover:border-primary group inline-flex items-center gap-2.5 rounded-full border px-6 py-3 text-sm transition-colors"
                >
                  자세히 보기
                  <ArrowRight className="size-4 transition-transform group-hover:translate-x-1" />
                </Link>
                <a
                  href="https://sudo0928.github.io/claude-code-master/"
                  target="_blank"
                  rel="noreferrer noopener"
                  className="text-muted-foreground hover:text-foreground inline-flex items-center gap-2 text-sm transition-colors"
                >
                  강의 사이트 열기
                  <ArrowUpRight className="size-4" />
                </a>
              </div>
            ) : null}
          </div>

          <div>
            <div className="grid grid-cols-3 gap-px overflow-hidden rounded-xl border">
              {[
                { value: "10", label: "강의" },
                { value: "218", label: "슬라이드" },
                { value: "자동", label: "목록·배포" },
              ].map((s) => (
                <div key={s.label} className="bg-card p-6 text-center">
                  <p className="font-heading text-primary text-2xl font-semibold">
                    {s.value}
                  </p>
                  <p className="text-muted-foreground mt-1.5 text-xs">
                    {s.label}
                  </p>
                </div>
              ))}
            </div>

            <ul className="mt-6 grid gap-x-8 gap-y-2.5 sm:grid-cols-2">
              {lectures.map((l) => (
                <li
                  key={l.title}
                  className="text-muted-foreground flex items-baseline justify-between gap-4 border-b border-dashed pb-2.5 text-sm"
                >
                  <span>{l.title}</span>
                  <span className="font-mono text-xs">{l.slides}장</span>
                </li>
              ))}
            </ul>

            <p className="text-muted-foreground mt-6 text-sm leading-7">
              강의 HTML을 폴더에 넣고 push하면 GitHub Actions가 제목과 태그,
              슬라이드 수를 읽어서 목록을 다시 만들고 사이트에 배포합니다. 그래서
              목록을 손으로 고칠 일이 없습니다.
            </p>
          </div>
        </div>
      </Container>
    </section>
  );
}
