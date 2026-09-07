import Link from "next/link";
import { ArrowRight } from "lucide-react";

import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";
import { ProjectThumb } from "@/components/common/project-thumb";
import { featuredProjects, otherProjects } from "@/lib/projects";

export function Projects() {
  return (
    <section id="projects" className="scroll-mt-16 py-24 sm:py-28">
      <Container>
        <div className="flex flex-wrap items-end justify-between gap-6">
          <div>
            <SectionLabel index="02." label="PROJECTS" />
            <h2 className="font-heading mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
              주요 프로젝트
            </h2>
            <p className="text-muted-foreground mt-5 max-w-xl leading-8">
              직접 개발한 프로젝트입니다. 원본 저장소가 비공개인 경우에는 그
              이유를 적었고, 제가 작성한 코드는 따로 발췌해 두었습니다. 각
              프로젝트에는 지금 다시 만든다면 어디를 고치고 싶은지도 함께
              적었습니다.
            </p>
          </div>
        </div>

        <div className="mt-12 grid gap-7 md:grid-cols-3">
          {featuredProjects.map((project) => (
            <Link
              key={project.slug}
              href={`/projects/${project.slug}`}
              className="group bg-card hover:border-primary/40 flex flex-col overflow-hidden rounded-xl border transition-colors"
            >
              <ProjectThumb
                slug={project.slug}
                title={project.title}
                cover={project.cover}
                coverKind={project.coverKind}
              />
              <div className="flex flex-1 flex-col p-6">
                <h3 className="font-heading text-lg font-semibold tracking-tight">
                  {project.title}
                </h3>
                <p className="text-muted-foreground border-border mt-3 w-fit rounded border px-2 py-0.5 text-[11px]">
                  {project.kind}
                </p>
                <p className="text-muted-foreground mt-4 flex-1 text-sm leading-6">
                  {project.summary}
                </p>
                <span className="text-primary mt-6 inline-flex items-center gap-2 text-sm">
                  자세히 보기
                  <ArrowRight className="size-3.5 transition-transform group-hover:translate-x-1" />
                </span>
              </div>
            </Link>
          ))}
        </div>

        <div className="mt-16">
          <h3 className="font-heading text-xl font-semibold tracking-tight">
            학습 프로젝트
          </h3>
          <p className="text-muted-foreground mt-3 max-w-xl leading-7">
            상태머신을 직접 짜 보는 것에서 시작하여 netcode로 이어진
            과정입니다. 모작이나 재현에 해당하는 것은 그 사실을 함께
            밝혔습니다.
          </p>
          <div className="mt-8 grid gap-4 sm:grid-cols-2">
            {otherProjects.map((project) => (
              <Link
                key={project.slug}
                href={`/projects/${project.slug}`}
                className="group bg-card hover:border-primary/40 flex items-start justify-between gap-6 rounded-xl border p-6 transition-colors"
              >
                <div>
                  <h4 className="font-heading font-semibold tracking-tight">
                    {project.title}
                  </h4>
                  <p className="text-muted-foreground mt-2 text-sm leading-6">
                    {project.tagline}
                  </p>
                </div>
                <ArrowRight className="text-muted-foreground group-hover:text-primary mt-1 size-4 shrink-0 transition-all group-hover:translate-x-1" />
              </Link>
            ))}
          </div>
        </div>
      </Container>
    </section>
  );
}
