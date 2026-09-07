import { Container } from "@/components/layout/container";
import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <Container className="py-20">
      <div className="mx-auto max-w-3xl space-y-6">
        <Skeleton className="mx-auto h-6 w-32" />
        <Skeleton className="mx-auto h-12 w-full max-w-xl" />
        <Skeleton className="mx-auto h-12 w-full max-w-md" />
        <div className="mx-auto mt-6 flex max-w-sm justify-center gap-3">
          <Skeleton className="h-11 w-32" />
          <Skeleton className="h-11 w-32" />
        </div>
      </div>
    </Container>
  );
}
