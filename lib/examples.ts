import {
  Code,
  Database,
  FileText,
  LayoutGrid,
  Palette,
  Settings,
  type LucideIcon,
} from "lucide-react";

export type ExampleMeta = {
  slug: string;
  title: string;
  description: string;
  icon: LucideIcon;
  tags: string[];
};

export const examples: ExampleMeta[] = [
  {
    slug: "components",
    title: "컴포넌트 쇼케이스",
    description: "모든 UI 컴포넌트의 실제 동작을 확인하고 코드 예제를 살펴보세요.",
    icon: Palette,
    tags: ["UI/UX", "인터랙티브"],
  },
  {
    slug: "forms",
    title: "폼 예제",
    description: "react-hook-form과 zod를 활용한 다양한 폼 구현 예제입니다.",
    icon: FileText,
    tags: ["검증", "상태관리"],
  },
  {
    slug: "layouts",
    title: "레이아웃 예제",
    description: "다양한 레이아웃 패턴과 반응형 디자인 구현 방법을 확인하세요.",
    icon: LayoutGrid,
    tags: ["반응형", "레이아웃"],
  },
  {
    slug: "hooks",
    title: "usehooks-ts 예제",
    description: "usehooks-ts 라이브러리의 다양한 훅 사용법과 실용적인 예제들입니다.",
    icon: Code,
    tags: ["훅", "유틸리티"],
  },
  {
    slug: "data-fetching",
    title: "데이터 페칭",
    description: "API 호출, 로딩 상태, 에러 처리 등 데이터 관리 예제입니다.",
    icon: Database,
    tags: ["API", "비동기"],
  },
  {
    slug: "optimization",
    title: "설정 및 최적화",
    description: "성능 최적화, SEO 설정, PWA 구현 등 프로덕션 환경을 위한 설정들입니다.",
    icon: Settings,
    tags: ["최적화", "SEO"],
  },
];

export function getExample(slug: string): ExampleMeta | undefined {
  return examples.find((e) => e.slug === slug);
}
