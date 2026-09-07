import { cn } from "@/lib/utils";

type SectionLabelProps = {
  index: string;
  label: string;
  className?: string;
};

/** 참조 디자인의 "01. ABOUT ME" 형태 라벨 */
export function SectionLabel({ index, label, className }: SectionLabelProps) {
  return (
    <p
      className={cn(
        "flex items-center gap-3 text-xs font-medium tracking-[0.25em]",
        className
      )}
    >
      <span className="text-primary">{index}</span>
      <span className="text-muted-foreground">{label}</span>
    </p>
  );
}
