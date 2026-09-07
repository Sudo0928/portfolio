"use client";

import { useSyncExternalStore } from "react";

const subscribe = () => () => {};

/**
 * 컴포넌트가 클라이언트에서 마운트(=hydration 완료)되었는지를 반환합니다.
 * SSR/CSR 마크업 차이로 인한 hydration mismatch 회피용으로 사용하세요.
 *
 * useSyncExternalStore 의 server snapshot 이 false 이므로 SSR 시 false,
 * 클라이언트에서는 항상 true 가 보장됩니다.
 */
export function useMounted() {
  return useSyncExternalStore(
    subscribe,
    () => true,
    () => false
  );
}
