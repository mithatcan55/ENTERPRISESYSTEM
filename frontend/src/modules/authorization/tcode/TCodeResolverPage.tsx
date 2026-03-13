import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import { resolveTCode, type TCodeResolveQuery } from "./tcode.api";

const initialResolverQuery: TCodeResolveQuery = {
  transactionCode: "SYS01",
  userId: 1,
  companyId: 1,
  actionCode: "READ",
  amount: 1000,
  denyOnUnsatisfiedConditions: true
};

export function TCodeResolverPage() {
  const { t } = useTranslation(["authorization"]);
  const [query, setQuery] = useState<TCodeResolveQuery>(initialResolverQuery);
  const resolveMutation = useMutation<
    Awaited<ReturnType<typeof resolveTCode>>,
    ApiError,
    TCodeResolveQuery
  >({
    mutationFn: (payload: TCodeResolveQuery) => resolveTCode(payload)
  });

  const fields: FormField[] = [
    { key: "transactionCode", label: t("transactionCode"), type: "text", value: query.transactionCode },
    { key: "userId", label: t("userId"), type: "number", value: query.userId },
    { key: "companyId", label: t("companyId"), type: "number", value: query.companyId },
    { key: "actionCode", label: t("actionCode"), type: "text", value: query.actionCode ?? "" },
    { key: "amount", label: t("amount"), type: "number", value: query.amount ?? 0 },
    {
      key: "denyOnUnsatisfiedConditions",
      label: t("denyOnUnsatisfiedConditions"),
      type: "switch",
      value: query.denyOnUnsatisfiedConditions ?? true
    }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("tcodePageTitle")} description={t("tcodePageDescription")} />
      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("tcodeResolveTitle")} subtitle={t("tcodeResolveSubtitle")}>
          <StandardForm
            fields={fields}
            onChange={(key, value) =>
              setQuery((current) => ({
                ...current,
                [key]:
                  key === "userId" || key === "companyId" || key === "amount"
                    ? Number(value)
                    : key === "denyOnUnsatisfiedConditions"
                      ? Boolean(value)
                      : value
              }))
            }
            onSubmit={() => resolveMutation.mutate(query)}
            submitLabel={resolveMutation.isPending ? t("tcodeResolving") : t("tcodeResolveAction")}
          />
        </PanelCard>

        <PanelCard title={t("tcodeResultTitle")} subtitle={t("tcodeResultSubtitle")}>
          {resolveMutation.data ? (
            <div className="event-list">
              <div className={`event-list__item ${resolveMutation.data.isAllowed ? "" : "event-list__item--danger"}`}>
                <strong>{resolveMutation.data.transactionCode}</strong>
                <span>{resolveMutation.data.isAllowed ? t("allowed") : resolveMutation.data.deniedReason ?? t("denied")}</span>
              </div>
              <div className="event-list__item">
                {t("requiredActionCode")}: {resolveMutation.data.requiredActionCode ?? "-"}
              </div>
              {Object.entries(resolveMutation.data.actions).map(([key, value]) => (
                <div key={key} className="event-list__item">
                  {key}: {value ? t("allowed") : t("denied")}
                </div>
              ))}
              {resolveMutation.data.conditions.map((item) => (
                <div key={`${item.fieldName}-${item.operator}`} className={`event-list__item ${item.isSatisfied ? "" : "event-list__item--accent"}`}>
                  {item.fieldName} {item.operator} {item.expectedValue} / {item.actualValue ?? "-"}
                </div>
              ))}
            </div>
          ) : (
            <div className="standard-table__empty">
              <strong>{t("tcodeResultEmptyTitle")}</strong>
              <span>{t("tcodeResultEmptyDescription")}</span>
            </div>
          )}

          {resolveMutation.isError ? (
            <p className="form-feedback form-feedback--error">
              {resolveMutation.error.detail ?? resolveMutation.error.title}
            </p>
          ) : null}
        </PanelCard>
      </div>
    </div>
  );
}
