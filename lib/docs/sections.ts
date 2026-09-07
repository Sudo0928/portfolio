import {
  BookOpen,
  Code,
  FileText,
  FolderTree,
  Layers,
  Palette,
  Rocket,
  Route,
  Settings,
  type LucideIcon,
} from "lucide-react";

export type DocSection = {
  id: string;
  title: string;
  description: string;
  icon: LucideIcon;
  bullets: string[];
};

export const docSections: DocSection[] = [
  {
    id: "intro",
    title: "시작하기",
    description: "스타터킷 소개와 사용 시 알아두면 좋은 핵심 컨셉을 정리했습니다.",
    icon: BookOpen,
    bullets: ["스타터킷 소개", "기술 스택", "디자인 철학", "한국어 친화 환경"],
  },
  {
    id: "install",
    title: "설치 및 실행",
    description: "프로젝트 설정과 개발/빌드 명령어를 한 자리에 모았습니다.",
    icon: Rocket,
    bullets: ["의존성 설치", "개발 서버 실행", "프로덕션 빌드", "정적 검사"],
  },
  {
    id: "structure",
    title: "폴더 구조",
    description: "라우팅과 컴포넌트의 5계층 분리 원칙을 설명합니다.",
    icon: FolderTree,
    bullets: ["app 라우팅", "ui · common · layout", "sections · forms", "단방향 의존"],
  },
  {
    id: "theme",
    title: "테마와 디자인 토큰",
    description: "CSS 변수 기반 테마와 다크 모드 구성 방식입니다.",
    icon: Palette,
    bullets: ["디자인 토큰", "다크 모드 구현", "next-themes 연동", "커스텀 스타일링"],
  },
  {
    id: "components",
    title: "컴포넌트 추가",
    description: "shadcn CLI로 새 컴포넌트를 추가하는 방법을 안내합니다.",
    icon: Layers,
    bullets: ["shadcn CLI", "components.json", "변형 추가", "스타일 커스터마이즈"],
  },
  {
    id: "state",
    title: "상태 관리",
    description: "Zustand 기반 전역 상태와 서버 상태 운영 가이드입니다.",
    icon: Code,
    bullets: ["Zustand 스토어", "전역 UI 상태", "스토어 분리", "서버 상태 권장 라이브러리"],
  },
  {
    id: "forms",
    title: "폼 작성",
    description: "react-hook-form과 zod로 안전한 폼을 작성하는 방법입니다.",
    icon: FileText,
    bullets: ["스키마 분리", "타입 추론", "에러 처리", "ContactForm 예시"],
  },
  {
    id: "routing",
    title: "라우팅과 페이지 셸",
    description: "App Router 특수 파일과 페이지 셸 구성을 설명합니다.",
    icon: Route,
    bullets: ["App Router 구조", "특수 파일", "라우트 그룹", "서버/클라이언트 분리"],
  },
  {
    id: "deploy",
    title: "배포",
    description: "Vercel 및 다양한 호스팅으로 배포하는 방법입니다.",
    icon: Settings,
    bullets: ["Vercel 배포", "프로덕션 빌드", "런타임 호환성", "환경 변수"],
  },
];

export function getDocSection(id: string): DocSection | undefined {
  return docSections.find((s) => s.id === id);
}
