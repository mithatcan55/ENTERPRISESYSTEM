export interface Session {
  id: string;
  userId: string;
  userName: string;
  ipAddress: string;
  userAgent: string;
  isActive: boolean;
  createdAt: string;
  expiresAt: string;
}

export interface SessionsResponse {
  items: Session[];
  totalCount: number;
  page: number;
  pageSize: number;
}
