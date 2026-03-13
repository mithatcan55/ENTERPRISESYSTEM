import { useMutation, useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import {
  getPasswordPolicy,
  previewPasswordPolicy,
  type PasswordPolicyPreviewPayload,
  type PasswordPolicySnapshot
} from "./passwordPolicy.api";

const sampleDefaults = [
  { password: "Weakpass", username: "demo.user", email: "demo@example.com" },
  { password: "StrongP@ssw0rd", username: "core.admin", email: "core@example.com" }
];

export function PasswordPolicyPage() {
  const { t } = useTranslation(["identity"]);
  const [form, setForm] = useState<PasswordPolicyPreviewPayload | null>(null);

  const snapshotQuery = useQuery({
    queryKey: ["identity", "password-policy"],
    queryFn: ({ signal }) => getPasswordPolicy(signal)
  });

  useEffect(() => {
    if (snapshotQuery.data) {
      setForm({
        ...snapshotQuery.data,
        samples: sampleDefaults
      });
    }
  }, [snapshotQuery.data]);

  const previewMutation = useMutation<
    Awaited<ReturnType<typeof previewPasswordPolicy>>,
    ApiError,
    PasswordPolicyPreviewPayload
  >({
    mutationFn: (payload: PasswordPolicyPreviewPayload) => previewPasswordPolicy(payload),
  });

  const snapshotFields = buildPasswordPolicyFields(t, form);

  return (
    <div className="page-grid">
      <PageHeader title={t("passwordPolicyPageTitle")} description={t("passwordPolicyPageDescription")} />
      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("passwordPolicySnapshotTitle")} subtitle={t("passwordPolicySnapshotSubtitle")}>
          {form ? (
            <StandardForm
              fields={snapshotFields}
              onChange={(key, value) =>
                setForm((current) =>
                  current
                    ? {
                        ...current,
                        [key]: typeof current[key as keyof PasswordPolicySnapshot] === "number" ? Number(value) : Boolean(value)
                      }
                    : current
                )
              }
              onSubmit={() => form && previewMutation.mutate(form)}
              submitLabel={previewMutation.isPending ? t("passwordPolicyPreviewing") : t("passwordPolicyPreviewAction")}
            />
          ) : null}
        </PanelCard>

        <PanelCard title={t("passwordPolicyPreviewTitle")} subtitle={t("passwordPolicyPreviewSubtitle")}>
          {previewMutation.data ? (
            <div className="event-list">
              <div className={`event-list__item ${previewMutation.data.isValidConfiguration ? "" : "event-list__item--danger"}`}>
                {previewMutation.data.isValidConfiguration ? t("passwordPolicyValidConfig") : t("passwordPolicyInvalidConfig")}
              </div>
              {previewMutation.data.validationErrors.map((item) => (
                <div key={item} className="event-list__item event-list__item--danger">
                  {item}
                </div>
              ))}
              {previewMutation.data.warnings.map((item) => (
                <div key={item} className="event-list__item event-list__item--accent">
                  {item}
                </div>
              ))}
              {previewMutation.data.sampleEvaluations.map((item) => (
                <div key={item.passwordMasked} className="event-list__item">
                  <strong>{item.passwordMasked}</strong>
                  <span>{item.isCompliant ? t("passwordPolicyCompliant") : item.errors.join(" | ")}</span>
                </div>
              ))}
            </div>
          ) : (
            <div className="standard-table__empty">
              <strong>{t("passwordPolicyPreviewEmptyTitle")}</strong>
              <span>{t("passwordPolicyPreviewEmptyDescription")}</span>
            </div>
          )}

          {previewMutation.isError ? (
            <p className="form-feedback form-feedback--error">
              {previewMutation.error.detail ?? previewMutation.error.title}
            </p>
          ) : null}
        </PanelCard>
      </div>
    </div>
  );
}

function buildPasswordPolicyFields(
  t: (key: string) => string,
  form: PasswordPolicyPreviewPayload | null
): FormField[] {
  if (!form) {
    return [];
  }

  return [
    { key: "minLength", label: t("minLength"), type: "number", value: form.minLength },
    { key: "historyCount", label: t("historyCount"), type: "number", value: form.historyCount },
    {
      key: "minimumPasswordAgeMinutes",
      label: t("minimumPasswordAgeMinutes"),
      type: "number",
      value: form.minimumPasswordAgeMinutes
    },
    { key: "requireUppercase", label: t("requireUppercase"), type: "switch", value: form.requireUppercase },
    { key: "requireLowercase", label: t("requireLowercase"), type: "switch", value: form.requireLowercase },
    { key: "requireDigit", label: t("requireDigit"), type: "switch", value: form.requireDigit },
    { key: "requireSpecialCharacter", label: t("requireSpecialCharacter"), type: "switch", value: form.requireSpecialCharacter }
  ];
}
