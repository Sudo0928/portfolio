"use client";

import { useEffect } from "react";
import { AlertTriangle, RefreshCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/layout/container";

type ErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function GlobalError({ error, reset }: ErrorProps) {
  useEffect(() => {
    // 외부 에러 트래커(Sentry 등) 연동 위치
    console.error(error);
  }, [error]);

  return (
    <Container className="flex min-h-[60vh] flex-col items-center justify-center py-20 text-center">
      <div className="bg-destructive/10 text-destructive mb-6 inline-flex size-16 items-center justify-center rounded-full">
        <AlertTriangle className="size-8" />
      </div>
      <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
        문제가 발생했습니다
      </h1>
      <p className="text-muted-foreground mt-3 max-w-md text-base leading-7">
        잠시 후 다시 시도해 주세요. 문제가 반복되면 관리자에게 문의해 주세요.
      </p>
      {error.digest ? (
        <p className="text-muted-foreground mt-2 text-xs">에러 ID: {error.digest}</p>
      ) : null}
      <Button onClick={reset} size="lg" className="mt-8">
        <RefreshCcw /> 다시 시도
      </Button>
    </Container>
  );
}
