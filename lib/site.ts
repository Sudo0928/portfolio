import { Code, Mail } from "lucide-react";

import type { SiteConfig } from "@/types";

export const siteConfig: SiteConfig = {
  name: "Yeom Giyong",
  description:
    "시스템 구조 설계에 강한 Unity 클라이언트 개발자 염기용의 포트폴리오입니다. 직접 만든 계층형 상태머신과 행동트리, 매니저·유저 데이터 구조 재설계, 성능 최적화 작업을 정리했습니다.",
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
