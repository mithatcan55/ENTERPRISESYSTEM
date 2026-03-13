import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import {
  listOutboxMessages,
  queueExcel,
  queueMail,
  type OutboxMessageListItem,
  type OutboxMessageQuery,
  type QueueExcelPayload,
  type QueueMailPayload
} from "./outbox.api";

const initialOutboxQuery: OutboxMessageQuery = {
  page: 1,
  pageSize: 20,
  status: "",
  eventType: "",
  search: ""
};

const initialMailForm: QueueMailPayload = {
  to: "",
  subject: "",
  body: ""
};

const initialExcelForm: QueueExcelPayload = {
  reportName: "",
  headers: ["Kod", "Ad", "Durum"],
  rows: [["USR-001", "core.admin", "Aktif"]],
  notifyEmail: ""
};

export function OutboxPage() {
  const { t } = useTranslation(["integrations"]);
  const queryClient = useQueryClient();
  const [query, setQuery] = useState(initialOutboxQuery);
  const [mailForm, setMailForm] = useState(initialMailForm);
  const [excelForm, setExcelForm] = useState(initialExcelForm);

  const outboxQuery = useQuery({
    queryKey: ["integrations", "outbox", query],
    queryFn: ({ signal }) => listOutboxMessages(query, signal)
  });

  const queueMailMutation = useMutation<{ id: number }, ApiError, QueueMailPayload>({
    mutationFn: (payload) => queueMail(payload),
    onSuccess: async () => {
      setMailForm(initialMailForm);
      await queryClient.invalidateQueries({ queryKey: ["integrations", "outbox"] });
    }
  });

  const queueExcelMutation = useMutation<{ id: number }, ApiError, QueueExcelPayload>({
    mutationFn: (payload) => queueExcel(payload),
    onSuccess: async () => {
      setExcelForm(initialExcelForm);
      await queryClient.invalidateQueries({ queryKey: ["integrations", "outbox"] });
    }
  });

  const columns: Array<TableColumn<OutboxMessageListItem>> = [
    { key: "createdAt", header: t("createdAt"), cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date(item.createdAt)) },
    { key: "eventType", header: t("eventType"), cell: (item) => item.eventType },
    { key: "status", header: t("status"), cell: (item) => item.status },
    { key: "attempts", header: t("attempts"), cell: (item) => `${item.attemptCount}/${item.maxAttempts}` }
  ];

  const queryFields: FormField[] = [
    { key: "status", label: t("status"), type: "text", value: query.status ?? "" },
    { key: "eventType", label: t("eventType"), type: "text", value: query.eventType ?? "" },
    { key: "search", label: t("search"), type: "text", value: query.search ?? "" }
  ];

  const mailFields: FormField[] = [
    { key: "to", label: t("mailTo"), type: "email", value: mailForm.to },
    { key: "subject", label: t("mailSubject"), type: "text", value: mailForm.subject },
    { key: "body", label: t("mailBody"), type: "textarea", value: mailForm.body }
  ];

  const excelFields: FormField[] = [
    { key: "reportName", label: t("reportName"), type: "text", value: excelForm.reportName },
    { key: "notifyEmail", label: t("notifyEmail"), type: "email", value: excelForm.notifyEmail ?? "" }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("outboxPageTitle")} description={t("outboxPageDescription")} />

      <PanelCard title={t("outboxListTitle")} subtitle={t("outboxListSubtitle")}>
        <StandardForm
          fields={queryFields}
          onChange={(key, value) =>
            setQuery((current) => ({
              ...current,
              [key]: value
            }))
          }
        />

        <div className="spacer-block" />

        <StandardDataTable
          columns={columns}
          items={outboxQuery.data?.items ?? []}
          rowKey={(item) => item.id}
          loading={outboxQuery.isLoading}
          emptyTitle={t("noOutboxTitle")}
          emptyDescription={t("noOutboxDescription")}
          totalCount={outboxQuery.data?.totalCount ?? 0}
          page={outboxQuery.data?.page ?? 1}
          pageSize={outboxQuery.data?.pageSize ?? 20}
          onPageChange={(nextPage) =>
            setQuery((current) => ({
              ...current,
              page: nextPage
            }))
          }
        />
      </PanelCard>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("queueMailTitle")} subtitle={t("queueMailSubtitle")}>
          <StandardForm
            fields={mailFields}
            onChange={(key, value) =>
              setMailForm((current) => ({
                ...current,
                [key]: value
              }))
            }
            onSubmit={() => queueMailMutation.mutate(mailForm)}
            submitLabel={queueMailMutation.isPending ? t("saving") : t("queueMail")}
          />
        </PanelCard>

        <PanelCard title={t("queueExcelTitle")} subtitle={t("queueExcelSubtitle")}>
          <StandardForm
            fields={excelFields}
            onChange={(key, value) =>
              setExcelForm((current) => ({
                ...current,
                [key]: value
              }))
            }
            onSubmit={() => queueExcelMutation.mutate(excelForm)}
            submitLabel={queueExcelMutation.isPending ? t("saving") : t("queueExcel")}
          />
        </PanelCard>
      </div>
    </div>
  );
}
