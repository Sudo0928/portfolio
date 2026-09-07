"use client";

import Link from "next/link";

import { Logo } from "@/components/common/logo";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { siteConfig } from "@/lib/site";
import { useUIStore } from "@/stores/use-ui-store";

export function MobileNav() {
  const open = useUIStore((s) => s.mobileNavOpen);
  const setOpen = useUIStore((s) => s.setMobileNavOpen);

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetContent side="left" className="w-72">
        <SheetHeader>
          <SheetTitle asChild>
            <Logo />
          </SheetTitle>
          <SheetDescription className="sr-only">모바일 메뉴</SheetDescription>
        </SheetHeader>
        <nav className="flex flex-col gap-1 px-3 pb-4">
          {siteConfig.nav.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              onClick={() => setOpen(false)}
              className="hover:bg-muted rounded-md px-3 py-2 text-sm font-medium transition-colors"
            >
              {item.title}
            </Link>
          ))}
        </nav>
      </SheetContent>
    </Sheet>
  );
}
