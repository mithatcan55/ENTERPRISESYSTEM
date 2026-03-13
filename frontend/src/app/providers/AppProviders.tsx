import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import { useState } from "react";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "../../core/auth/AuthProvider";
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
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <BrandProvider>
          <AuthProvider>{children}</AuthProvider>
        </BrandProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );
}
