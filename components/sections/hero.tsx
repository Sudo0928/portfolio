import Image from "next/image";
import Link from "next/link";
import { ArrowRight, ArrowUpRight } from "lucide-react";

import { Container } from "@/components/layout/container";

export function Hero() {
  return (
    <section
      id="home"
      className="relative flex min-h-[92vh] items-center overflow-hidden"
    >
      <div aria-hidden className="absolute inset-0 -z-10">
        <Image
          src="/images/hero.webp"
          alt=""
          fill
          priority
          sizes="100vw"
          className="object-cover object-right"
        />
        <div className="from-background via-background/85 absolute inset-0 bg-gradient-to-r to-transparent" />
        <div className="from-background absolute inset-x-0 bottom-0 h-40 bg-gradient-to-t to-transparent" />
      </div>

      <Container className="py-24">
        <div className="max-w-3xl">
          <p className="text-primary text-xs font-medium tracking-[0.3em]">
            GAME PROGRAMMER PORTFOLIO
          </p>
          <h1 className="font-heading mt-7 text-4xl leading-[1.12] font-semibold tracking-tight text-balance sm:text-6xl lg:text-7xl">
            Build Systems,
            <br />
            Not Just <span className="text-primary">Features.</span>
          </h1>
          <p className="text-muted-foreground mt-7 max-w-xl leading-8 text-pretty sm:text-lg">
            기능이 늘어나도 무너지지 않는 게임 시스템을 만드는 일에 관심을
            가지고 있습니다.
            <br />
            구조를 먼저 세우고 그 위에 재미를 올리는 방식으로 개발하고
            있습니다.
          </p>
          <div className="mt-10 flex flex-wrap items-center gap-6">
            <Link
              href="#projects"
              className="border-primary/60 hover:bg-primary/10 hover:border-primary group inline-flex items-center gap-3 rounded-full border px-7 py-3.5 text-sm font-medium transition-colors"
            >
              프로젝트 보기
              <ArrowRight className="size-4 transition-transform group-hover:translate-x-1" />
            </Link>
            <a
              href="https://github.com/Sudo0928"
              target="_blank"
              rel="noreferrer noopener"
              className="text-muted-foreground hover:text-foreground inline-flex items-center gap-2 text-sm transition-colors"
            >
              GitHub 둘러보기
              <ArrowUpRight className="size-4" />
            </a>
          </div>
        </div>
      </Container>
    </section>
  );
}
