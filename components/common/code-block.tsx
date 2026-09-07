"use client";

import { Check, Copy } from "lucide-react";
import { useCopyToClipboard } from "usehooks-ts";
import { useEffect, useState } from "react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type CodeBlockProps = {
  code: string;
  language?: string;
  className?: string;
};

export function CodeBlock({ code, language, className }: CodeBlockProps) {
  const [, copy] = useCopyToClipboard();
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!copied) return;
    const t = setTimeout(() => setCopied(false), 1500);
    return () => clearTimeout(t);
  }, [copied]);

  const handleCopy = async () => {
    const ok = await copy(code);
    if (ok) setCopied(true);
  };

  return (
    <div className={cn("group relative", className)}>
      <pre className="bg-muted text-foreground overflow-x-auto rounded-lg border px-4 pt-7 pb-4 font-mono text-xs leading-6">
        <code>{code}</code>
      </pre>
      {language ? (
        <span className="text-muted-foreground absolute top-2 left-3 text-[11px] font-medium tracking-wider uppercase">
          {language}
        </span>
      ) : null}
      <Button
        type="button"
        variant="ghost"
        size="icon-sm"
        onClick={handleCopy}
        aria-label="코드 복사"
        className="absolute top-2 right-2 opacity-0 transition-opacity group-hover:opacity-100 focus-visible:opacity-100"
      >
        {copied ? <Check /> : <Copy />}
      </Button>
    </div>
  );
}
