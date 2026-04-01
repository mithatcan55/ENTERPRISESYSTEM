import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import { useState } from "react";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "../../core/auth/AuthProvider";
import { ErrorBoundary } from "../../design-system/feedback/ErrorBoundary";
import { ToastProvider } from "../../design-system/feedback/Toast";
import { BrandProvider } from "./BrandProvider";

export function AppProviders({ children }: PropsWithChildren) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            retry: 1,
            refetchOnWindowFocus: false
          }
        }
      })
  );

  return (
    <ErrorBoundary>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <BrandProvider>
            <ToastProvider>
              <AuthProvider>{children}</AuthProvider>
            </ToastProvider>
          </BrandProvider>
        </QueryClientProvider>
      </BrowserRouter>
    </ErrorBoundary>
  );
}
