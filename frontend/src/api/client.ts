import axios from "axios";
import { useAuthStore } from "@/store/auth-store";
import type { RefreshResponse, UserRole } from "@/types/auth";

const apiClient = axios.create({
  baseURL: "/",
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const state = useAuthStore.getState();
  const token = state.accessToken;
  const language = localStorage.getItem("ui-language") ?? "tr-TR";
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  config.headers["X-Culture"] = language;
  return config;
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach((p) => {
    if (error) p.reject(error);
    else p.resolve(token!);
  });
  failedQueue = [];
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({
          resolve: (token: string) => {
            original.headers.Authorization = `Bearer ${token}`;
            resolve(apiClient(original));
          },
          reject,
        });
      });
    }

    original._retry = true;
    isRefreshing = true;

    const { refreshToken, syncAuth, clear } = useAuthStore.getState();

    if (!refreshToken) {
      clear();
      window.location.href = "/login";
      return Promise.reject(error);
    }

    try {
      const { data } = await axios.post<RefreshResponse>(
        "/api/auth/refresh",
        { refreshToken },
      );
      syncAuth(data.accessToken, data.refreshToken, {
        id: String(data.userId),
        userName: data.userCode,
        displayName: data.userCode,
        roles: (data.effectiveAuthorization?.roles ?? []) as UserRole[],
        mustChangePassword: data.mustChangePassword,
        permissions: data.effectiveAuthorization?.permissions ?? [],
      });
      processQueue(null, data.accessToken);
      original.headers.Authorization = `Bearer ${data.accessToken}`;
      return apiClient(original);
    } catch (refreshError) {
      processQueue(refreshError, null);
      clear();
      window.location.href = "/login";
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

export default apiClient;
