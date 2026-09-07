import Link from "next/link";

import { cn } from "@/lib/utils";

type LogoProps = {
  className?: string;
  href?: string;
};

export function Logo({ className, href = "/" }: LogoProps) {
  return (
    <Link
      href={href}
      className={cn(
        "font-heading inline-flex items-center gap-2.5 text-sm font-semibold tracking-[0.15em]",
        className
      )}
      aria-label="홈으로 이동"
    >
      <span className="bg-primary text-primary-foreground inline-flex size-6 items-center justify-center rounded text-xs font-bold">
        K
      </span>
      <span>KIYONG</span>
    </Link>
  );
}
