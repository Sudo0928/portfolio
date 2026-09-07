import { Logo } from "@/components/common/logo";
import { Container } from "@/components/layout/container";
import { siteConfig } from "@/lib/site";

export function Footer() {
  const year = new Date().getFullYear();

  return (
    <footer className="bg-deep border-t">
      <Container className="flex flex-col gap-5 py-8 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-4">
          <Logo />
          <span className="text-muted-foreground hidden text-xs sm:inline">
            Game Programmer Portfolio
          </span>
        </div>
        <div className="text-muted-foreground flex items-center gap-6 text-xs">
          <span>© {year} Yeom Giyong</span>
          {siteConfig.social.map(({ label, href, icon: Icon }) => (
            <a
              key={label}
              href={href}
              target="_blank"
              rel="noreferrer noopener"
              aria-label={label}
              className="hover:text-foreground transition-colors"
            >
              <Icon className="size-4" />
            </a>
          ))}
        </div>
      </Container>
    </footer>
  );
}
