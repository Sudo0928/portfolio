import { cn } from "@/lib/utils";

import { Container } from "./container";

type SectionProps = React.ComponentProps<"section"> & {
  containerClassName?: string;
  bleed?: boolean;
};

export function Section({
  className,
  containerClassName,
  bleed = false,
  children,
  ...props
}: SectionProps) {
  return (
    <section className={cn("py-16 sm:py-24", className)} {...props}>
      {bleed ? children : <Container className={containerClassName}>{children}</Container>}
    </section>
  );
}
