import { ArrowUpRight, Code, Mail } from "lucide-react";

import { Container } from "@/components/layout/container";
import { SectionLabel } from "@/components/common/section-label";

const channels = [
  {
    icon: Mail,
    label: "Email",
    value: "duarldyd0928@gmail.com",
    href: "mailto:duarldyd0928@gmail.com",
  },
  {
    icon: Code,
    label: "GitHub",
    value: "github.com/Sudo0928",
    href: "https://github.com/Sudo0928",
  },
];

export function Contact() {
  return (
    <section
      id="contact"
      className="bg-band scroll-mt-16 border-t py-24 sm:py-28"
    >
      <Container>
        <div className="grid gap-12 lg:grid-cols-[1fr_1.4fr] lg:items-end lg:gap-20">
          <div>
            <SectionLabel index="06." label="CONTACT" />
            <h2 className="font-heading mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
              연락하기
            </h2>
            <p className="text-muted-foreground mt-5 leading-8">
              궁금한 점이 있으시거나 함께하고 싶으시다면 언제든지 연락 주시기
              바랍니다.
            </p>
          </div>

          <div className="grid gap-3 sm:grid-cols-2">
            {channels.map(({ icon: Icon, label, value, href }) => (
              <a
                key={label}
                href={href}
                target={href.startsWith("http") ? "_blank" : undefined}
                rel={href.startsWith("http") ? "noreferrer noopener" : undefined}
                className="group bg-card hover:border-primary/40 flex items-center gap-4 rounded-xl border p-5 transition-colors"
              >
                <Icon className="text-primary size-5 shrink-0" />
                <div className="min-w-0 flex-1">
                  <p className="text-muted-foreground text-xs">{label}</p>
                  <p className="truncate text-sm">{value}</p>
                </div>
                <ArrowUpRight className="text-muted-foreground group-hover:text-primary size-4 shrink-0 transition-colors" />
              </a>
            ))}
          </div>
        </div>
      </Container>
    </section>
  );
}
