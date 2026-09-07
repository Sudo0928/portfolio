import type { LucideIcon } from "lucide-react";

export type NavItem = {
  title: string;
  href: string;
  description?: string;
  external?: boolean;
};

export type SocialLink = {
  label: string;
  href: string;
  icon: LucideIcon;
};

export type SiteConfig = {
  name: string;
  description: string;
  url: string;
  ogImage: string;
  nav: NavItem[];
  footerNav: { title: string; items: NavItem[] }[];
  social: SocialLink[];
};
