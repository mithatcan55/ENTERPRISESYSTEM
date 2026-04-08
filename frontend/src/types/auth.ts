export interface LoginRequest {
  identifier: string;
  password: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export type UserRole = "SYS_ADMIN" | "SYS_OPERATOR";

export interface AuthUser {
  id: string;
  userName: string;
  displayName: string;
  roles: UserRole[];
}

/** Matches backend LoginResponseDto */
export interface LoginResponse {
  userId: number;
  userCode: string;
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  tokenType: string;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
  isPasswordExpiringSoon: boolean;
  daysUntilPasswordExpiry: number | null;
  effectiveAuthorization: {
    roles: string[];
    transactionCodes: string[];
    permissions: string[];
  };
}
