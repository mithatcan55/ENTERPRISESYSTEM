export type ApiError = {
  status: number;
  title: string;
  detail?: string;
};

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

type RequestOptions = {
  method?: HttpMethod;
  body?: unknown;
  signal?: AbortSignal;
  useAuth?: boolean;
  retryOnUnauthorized?: boolean;
};

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, "") ?? "";
import { getHttpClientRuntime } from "./httpClientRuntime";

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  // Tek noktadan gecen client sayesinde auth header, correlation ve hata standardi
  // ileride burada merkezi olarak genisletilebilir.
  const headers: Record<string, string> = {
    "Content-Type": "application/json"
  };
  const runtime = getHttpClientRuntime();
  const useAuth = options.useAuth ?? true;
  const accessToken = useAuth ? runtime.getAccessToken() : null;

  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined,
    signal: options.signal
  });

  if (response.status === 401 && useAuth && (options.retryOnUnauthorized ?? true)) {
    const refreshed = await runtime.refreshSession();

    if (refreshed) {
      return request<T>(path, {
        ...options,
        retryOnUnauthorized: false
      });
    }
  }

  if (!response.ok) {
    const errorBody = (await response.json().catch(() => null)) as Partial<ApiError> | null;

    throw {
      status: response.status,
      title: errorBody?.title ?? "API request failed.",
      detail: errorBody?.detail
    } satisfies ApiError;
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const httpClient = {
  get<T>(path: string, signal?: AbortSignal, useAuth?: boolean) {
    return request<T>(path, { method: "GET", signal, useAuth });
  },
  post<T>(path: string, body?: unknown, signal?: AbortSignal, useAuth?: boolean) {
    return request<T>(path, { method: "POST", body, signal, useAuth });
  },
  put<T>(path: string, body?: unknown, signal?: AbortSignal, useAuth?: boolean) {
    return request<T>(path, { method: "PUT", body, signal, useAuth });
  },
  delete<T>(path: string, signal?: AbortSignal, useAuth?: boolean) {
    return request<T>(path, { method: "DELETE", signal, useAuth });
  }
};
