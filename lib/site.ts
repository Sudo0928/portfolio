import { Code, Mail } from "lucide-react";

import type { SiteConfig } from "@/types";

export const siteConfig: SiteConfig = {
  name: "Yeom Giyong",
  description:
    "시스템 구조 설계에 강한 Unity 클라이언트 개발자 염기용의 포트폴리오입니다. 서버 권위 구조 재설계, 클라이언트 예측/서버 보정, 자작 HFSM·행동트리 프로젝트를 정리했습니다.",
  url: "https://sudo0928-portfolio-sudo-s-projects3.vercel.app",
  ogImage: "https://sudo0928-portfolio-sudo-s-projects3.vercel.app/og.png",
  nav: [
    { title: "Home", href: "/#home" },
    { title: "About", href: "/#about" },
    { title: "Projects", href: "/#projects" },
    { title: "Tech Stack", href: "/#tech" },
    { title: "AI", href: "/#ai" },
    { title: "Contact", href: "/#contact" },
  ],
  footerNav: [],
  social: [
    { label: "GitHub", href: "https://github.com/Sudo0928", icon: Code },
    { label: "Email", href: "mailto:duarldyd0928@gmail.com", icon: Mail },
  ],
};
