import Link from "next/link";
import { ArrowLeft, FileQuestion } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/layout/container";

export default function NotFound() {
  return (
    <Container className="flex min-h-[60vh] flex-col items-center justify-center py-20 text-center">
      <div className="bg-muted text-muted-foreground mb-6 inline-flex size-16 items-center justify-center rounded-full">
        <FileQuestion className="size-8" />
      </div>
      <p className="text-muted-foreground mb-2 text-sm font-medium">404</p>
      <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
        페이지를 찾을 수 없습니다
      </h1>
      <p className="text-muted-foreground mt-3 max-w-md text-base leading-7">
        요청하신 페이지가 이동되었거나 더 이상 존재하지 않습니다. 주소를 다시
        확인해 주세요.
      </p>
      <Button asChild size="lg" className="mt-8">
        <Link href="/">
          <ArrowLeft /> 홈으로 돌아가기
        </Link>
      </Button>
    </Container>
  );
}
