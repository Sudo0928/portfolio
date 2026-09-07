"use client";

import Link from "next/link";
import { Menu } from "lucide-react";

import { Logo } from "@/components/common/logo";
import { MobileNav } from "@/components/layout/mobile-nav";
import { Button } from "@/components/ui/button";
import { Container } from "@/components/layout/container";
import { siteConfig } from "@/lib/site";
import { useUIStore } from "@/stores/use-ui-store";

export function Header() {
  const setMobileNavOpen = useUIStore((s) => s.setMobileNavOpen);

  return (
    <header className="bg-background/70 supports-backdrop-filter:bg-background/40 sticky top-0 z-40 w-full border-b backdrop-blur-md">
      <Container className="flex h-16 items-center justify-between gap-4">
        <Logo />
        <nav className="hidden items-center gap-9 md:flex">
          {siteConfig.nav.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="text-muted-foreground hover:text-foreground text-sm transition-colors"
            >
              {item.title}
            </Link>
          ))}
        </nav>
        <Button
          variant="ghost"
          size="icon"
          className="md:hidden"
          onClick={() => setMobileNavOpen(true)}
          aria-label="메뉴 열기"
        >
          <Menu />
        </Button>
      </Container>
      <MobileNav />
    </header>
  );
}
