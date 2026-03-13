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
};

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, "") ?? "";

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  // Tek noktadan gecen client sayesinde auth header, correlation ve hata standardi
  // ileride burada merkezi olarak genisletilebilir.
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json"
    },
    body: options.body ? JSON.stringify(options.body) : undefined,
    signal: options.signal
  });

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
  get<T>(path: string, signal?: AbortSignal) {
    return request<T>(path, { method: "GET", signal });
  },
  post<T>(path: string, body?: unknown, signal?: AbortSignal) {
    return request<T>(path, { method: "POST", body, signal });
  },
  put<T>(path: string, body?: unknown, signal?: AbortSignal) {
    return request<T>(path, { method: "PUT", body, signal });
  },
  delete<T>(path: string, signal?: AbortSignal) {
    return request<T>(path, { method: "DELETE", signal });
  }
};
