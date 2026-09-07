import Image from "next/image";

import { cn } from "@/lib/utils";

/**
 * 프로젝트 카드·상세 헤더의 비주얼.
 * cover가 있으면 이미지를, 없으면 CSS 그라디언트를 쓴다.
 * (있지도 않은 게임 화면을 지어내지 않기 위한 폴백)
 */
const accents: Record<string, string> = {
  "netcode-prototype":
    "bg-[radial-gradient(120%_120%_at_50%_0%,oklch(0.44_0.14_200/0.8),transparent_60%)]",
};

type ProjectThumbProps = {
  slug: string;
  title: string;
  cover?: string;
  coverKind?: "art" | "diagram";
  className?: string;
  priority?: boolean;
  sizes?: string;
};

export function ProjectThumb({
  slug,
  title,
  cover,
  coverKind = "art",
  className,
  priority,
  sizes = "(min-width: 768px) 33vw, 100vw",
}: ProjectThumbProps) {
  if (cover) {
    return (
      <div
        className={cn(
          "bg-deep relative aspect-[16/9] overflow-hidden",
          className
        )}
      >
        <Image
          src={cover}
          alt={`${title} 이미지`}
          fill
          sizes={sizes}
          priority={priority}
          className={cn(
            coverKind === "diagram" ? "object-contain" : "object-cover"
          )}
        />
      </div>
    );
  }

  return (
    <div
      className={cn(
        "bg-deep relative aspect-[16/9] overflow-hidden border-b",
        className
      )}
    >
      <div
        aria-hidden
        className={cn(
          "absolute inset-0",
          accents[slug] ??
            "bg-[radial-gradient(120%_120%_at_50%_0%,oklch(0.40_0.10_265/0.8),transparent_60%)]"
        )}
      />
      <div aria-hidden className="bg-grid absolute inset-0 opacity-50" />
      <div className="absolute inset-0 flex items-end p-5">
        <p className="font-heading text-foreground/85 text-sm font-medium tracking-tight">
          {title}
        </p>
      </div>
    </div>
  );
}
