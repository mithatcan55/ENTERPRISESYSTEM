import { httpClient } from "../../../core/api/httpClient";

export type PasswordPolicySnapshot = {
  minLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireDigit: boolean;
  requireSpecialCharacter: boolean;
  historyCount: number;
  minimumPasswordAgeMinutes: number;
};

export type PasswordPolicyPreviewSample = {
  password: string;
  username?: string;
  email?: string;
};

export type PasswordPolicyPreviewPayload = PasswordPolicySnapshot & {
  samples: PasswordPolicyPreviewSample[];
};

export type PasswordPolicyPreviewResult = {
  isValidConfiguration: boolean;
  validationErrors: string[];
  warnings: string[];
  sampleEvaluations: Array<{
    passwordMasked: string;
    isCompliant: boolean;
    errors: string[];
  }>;
};

export async function getPasswordPolicy(signal?: AbortSignal) {
  return httpClient.get<PasswordPolicySnapshot>("/api/ops/security/password-policy", signal);
}

export async function previewPasswordPolicy(payload: PasswordPolicyPreviewPayload, signal?: AbortSignal) {
  return httpClient.put<PasswordPolicyPreviewResult>("/api/ops/security/password-policy", payload, signal);
}
