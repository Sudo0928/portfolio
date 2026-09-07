import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArrowLeft, ExternalLink, Info } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ProjectThumb } from "@/components/common/project-thumb";
import { Container } from "@/components/layout/container";
import { getProject, projects } from "@/lib/projects";

type Props = {
  params: Promise<{ slug: string }>;
};

export function generateStaticParams() {
  return projects.map((p) => ({ slug: p.slug }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  const project = getProject(slug);
  if (!project) return {};
  return {
    title: project.title,
    description: project.summary,
  };
}

export default async function ProjectPage({ params }: Props) {
  const { slug } = await params;
  const project = getProject(slug);
  if (!project) notFound();

  const hasArtCover = Boolean(project.cover) && project.coverKind !== "diagram";
  const diagram = project.coverKind === "diagram" ? project.cover : undefined;

  return (
    <>
      {/* 헤더 */}
      <section className="bg-band border-b py-14">
        <Container>
          <Button asChild variant="ghost" size="sm" className="mb-8 -ml-2">
            <Link href="/#projects">
              <ArrowLeft /> 프로젝트 목록
            </Link>
          </Button>

          <div
            className={
              hasArtCover
                ? "grid gap-10 lg:grid-cols-[1.3fr_1fr] lg:items-end"
                : "max-w-3xl"
            }
          >
            <div>
              <p className="text-primary text-xs font-medium tracking-[0.2em]">
                {project.kind.toUpperCase()}
              </p>
              <h1 className="font-heading mt-5 text-3xl font-semibold tracking-tight sm:text-5xl">
                {project.title}
              </h1>
              <p className="text-muted-foreground mt-4 text-lg">
                {project.tagline}
              </p>
              <div className="mt-7 flex flex-wrap gap-1.5">
                {project.tech.map((t) => (
                  <Badge key={t} variant="outline" className="font-normal">
                    {t}
                  </Badge>
                ))}
              </div>
            </div>
            {hasArtCover ? (
              <ProjectThumb
                slug={project.slug}
                title={project.title}
                cover={project.cover}
                coverKind={project.coverKind}
                priority
                sizes="(min-width: 1024px) 40vw, 100vw"
                className="rounded-xl border"
              />
            ) : null}
          </div>
        </Container>
      </section>

      <Container className="py-14">
        <div className="mx-auto max-w-3xl">
          {/* 게임 자체 설명 */}
          <section>
            <h2 className="font-heading text-xl font-semibold tracking-tight sm:text-2xl">
              {project.overviewHeading ?? "어떤 게임인가"}
            </h2>
            <dl className="mt-6 grid gap-px overflow-hidden rounded-xl border sm:grid-cols-3">
              <div className="bg-card p-5">
                <dt className="text-muted-foreground text-xs">장르</dt>
                <dd className="mt-1.5 text-sm">{project.game.genre}</dd>
              </div>
              <div className="bg-card p-5">
                <dt className="text-muted-foreground text-xs">플레이 인원</dt>
                <dd className="mt-1.5 text-sm">{project.game.players}</dd>
              </div>
              <div className="bg-card p-5">
                <dt className="text-muted-foreground text-xs">플랫폼</dt>
                <dd className="mt-1.5 text-sm">{project.game.platform}</dd>
              </div>
            </dl>
            <div className="mt-6 space-y-4">
              {project.game.description.map((paragraph) => (
                <p key={paragraph} className="leading-8 text-pretty">
                  {paragraph}
                </p>
              ))}
            </div>

            {diagram ? (
              <figure className="mt-8">
                <div className="bg-deep overflow-hidden rounded-xl border">
                  <Image
                    src={diagram}
                    alt={`${project.title} 구조도`}
                    width={1600}
                    height={900}
                    priority
                    sizes="(min-width: 768px) 768px, 100vw"
                    className="w-full"
                  />
                </div>
                <figcaption className="text-muted-foreground mt-3 text-xs">
                  직접 작성한 구조도입니다. 수치는 코드에서 확인한 값입니다.
                </figcaption>
              </figure>
            ) : null}
          </section>

          {/* 참여 정보 */}
          <dl className="text-muted-foreground mt-12 grid gap-x-8 gap-y-2 border-y py-6 text-sm sm:grid-cols-[auto_1fr]">
            <dt className="font-medium">기간</dt>
            <dd>{project.period}</dd>
            <dt className="font-medium">규모</dt>
            <dd>{project.team}</dd>
            <dt className="font-medium">역할</dt>
            <dd>{project.role}</dd>
          </dl>

          {project.status ? (
            <Alert className="mt-8">
              <Info />
              <AlertTitle>저장소 안내</AlertTitle>
              <AlertDescription>{project.status}</AlertDescription>
            </Alert>
          ) : null}

          {project.links.length > 0 ? (
            <div className="mt-6 flex flex-wrap gap-2">
              {project.links.map((link) => (
                <Button asChild key={link.href} variant="outline" size="sm">
                  <a href={link.href} target="_blank" rel="noreferrer noopener">
                    {link.label} <ExternalLink />
                  </a>
                </Button>
              ))}
            </div>
          ) : null}

          {/* 본문 */}
          <div className="mt-14 space-y-12">
            {project.sections.map((section) => (
              <section key={section.heading} className="space-y-4">
                <h2 className="font-heading text-xl font-semibold tracking-tight sm:text-2xl">
                  {section.heading}
                </h2>
                {section.paragraphs?.map((paragraph) => (
                  <p key={paragraph} className="leading-8 text-pretty">
                    {paragraph}
                  </p>
                ))}
                {section.bullets ? (
                  <ul className="space-y-2.5 pl-1">
                    {section.bullets.map((bullet) => (
                      <li key={bullet} className="flex gap-3 leading-8">
                        <span
                          aria-hidden
                          className="bg-primary/60 mt-3.5 size-1.5 shrink-0 rounded-full"
                        />
                        <span>{bullet}</span>
                      </li>
                    ))}
                  </ul>
                ) : null}
              </section>
            ))}
          </div>
        </div>
      </Container>
    </>
  );
}
