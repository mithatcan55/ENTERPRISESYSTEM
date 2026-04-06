export interface AuditDashboardSummary {
  totalEvents: number;
  uniqueUsers: number;
  failedLogins: number;
  criticalEvents: number;
  topTransactions: { name: string; count: number }[];
  eventBreakdown: { label: string; count: number }[];
}

export interface OutboxMessage {
  id: string;
  type: string;
  status: "Pending" | "Sent" | "Failed";
  createdAt: string;
  retryCount: number;
}

export interface OutboxResponse {
  items: OutboxMessage[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PasswordPolicy {
  minLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireDigit: boolean;
  requireSpecialCharacter: boolean;
  historyCount: number;
  minimumPasswordAgeMinutes: number;
}

export interface PasswordPolicyValidation {
  isValidConfiguration: boolean;
  errors: string[];
}
