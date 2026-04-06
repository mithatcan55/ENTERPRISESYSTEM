export interface User {
  id: string;
  userCode: string;
  userName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  companyId?: string;
}

export interface UsersResponse {
  items: User[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateUserRequest {
  userCode: string;
  userName: string;
  email: string;
  password: string;
  companyId: string;
  notifyAdminByMail: boolean;
}
