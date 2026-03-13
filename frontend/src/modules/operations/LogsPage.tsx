import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../design-system/forms/StandardForm";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import {
  getEntityChangeLogs,
  getHttpLogs,
  getSecurityLogs,
  getSystemLogs,
  type EntityChangeLogListItem,
  type HttpRequestLogListItem,
  type LogQuery,
  type SecurityEventListItem,
  type SystemLogListItem
} from "./operations.api";

type LogType = "system" | "security" | "http" | "entity-changes";
type LogItem = SystemLogListItem | SecurityEventListItem | HttpRequestLogListItem | EntityChangeLogListItem;
type LogQueryResult = {
  items: LogItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export function LogsPage() {
  const { t } = useTranslation(["operations"]);
  const location = useLocation();
  const navigate = useNavigate();
  const logType = resolveLogTypeFromPath(location.pathname);
  const [query, setQuery] = useState<LogQuery>({
    page: 1,
    pageSize: 20,
    search: "",
    correlationId: ""
  });

  const logsQuery = useQuery<LogQueryResult>({
    queryKey: ["operations", "logs", logType, query],
    queryFn: async ({ signal }) => {
      switch (logType) {
        case "security":
          return (await getSecurityLogs(query, signal)) as LogQueryResult;
        case "http":
          return (await getHttpLogs(query, signal)) as LogQueryResult;
        case "entity-changes":
          return (await getEntityChangeLogs(query, signal)) as LogQueryResult;
        default:
          return (await getSystemLogs(query, signal)) as LogQueryResult;
      }
    }
  });

  const filterFields: FormField[] = [
    {
      key: "logType",
      label: t("logType"),
      type: "select",
      value: logType,
      options: [
        { value: "system", label: t("systemLogs") },
        { value: "security", label: t("securityLogs") },
        { value: "http", label: t("httpLogs") },
        { value: "entity-changes", label: t("entityChangeLogs") }
      ]
    },
    {
      key: "search",
      label: t("search"),
      type: "text",
      value: query.search ?? ""
    },
    {
      key: "correlationId",
      label: t("correlationId"),
      type: "text",
      value: query.correlationId ?? ""
    }
  ];

  const columns: Array<TableColumn<LogItem>> =
    logType === "security"
      ? [
          { key: "timestamp", header: t("timestamp"), cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date((item as SecurityEventListItem).timestamp)) },
          { key: "eventType", header: t("eventType"), cell: (item) => (item as SecurityEventListItem).eventType ?? "-" },
          { key: "severity", header: t("severity"), cell: (item) => (item as SecurityEventListItem).severity ?? "-" },
          { key: "username", header: t("username"), cell: (item) => (item as SecurityEventListItem).username ?? "-" }
        ]
      : logType === "http"
        ? [
            { key: "timestamp", header: t("timestamp"), cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date((item as HttpRequestLogListItem).timestamp)) },
            { key: "method", header: t("method"), cell: (item) => (item as HttpRequestLogListItem).method ?? "-" },
            { key: "path", header: t("path"), cell: (item) => (item as HttpRequestLogListItem).path ?? "-" },
            { key: "statusCode", header: t("statusCode"), cell: (item) => String((item as HttpRequestLogListItem).statusCode) }
          ]
        : logType === "entity-changes"
          ? [
              { key: "timestamp", header: t("timestamp"), cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date((item as EntityChangeLogListItem).timestamp)) },
              { key: "entityType", header: t("entityType"), cell: (item) => (item as EntityChangeLogListItem).entityType ?? "-" },
              { key: "action", header: t("action"), cell: (item) => (item as EntityChangeLogListItem).action ?? "-" },
              { key: "username", header: t("username"), cell: (item) => (item as EntityChangeLogListItem).username ?? "-" }
            ]
          : [
              { key: "timestamp", header: t("timestamp"), cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date((item as SystemLogListItem).timestamp)) },
              { key: "level", header: t("level"), cell: (item) => (item as SystemLogListItem).level ?? "-" },
              { key: "category", header: t("category"), cell: (item) => (item as SystemLogListItem).category ?? "-" },
              { key: "message", header: t("message"), cell: (item) => (item as SystemLogListItem).message ?? "-" }
            ];

  return (
    <div className="page-grid">
      <PageHeader title={t("logsPageTitle")} description={t("logsPageDescription")} />
      <PanelCard title={t("logExplorerTitle")} subtitle={t("logExplorerSubtitle")}>
        <StandardForm
          fields={filterFields}
          onChange={(key, value) => {
            if (key === "logType") {
              const nextType = value as LogType;
              navigate(`/operations/logs/${nextType}`);
              return;
            }

            setQuery((current) => ({
              ...current,
              [key]: value
            }));
          }}
        />

        <div className="spacer-block" />

        <StandardDataTable
          columns={columns}
          items={logsQuery.data?.items ?? []}
          rowKey={(item) => item.id}
          loading={logsQuery.isLoading}
          emptyTitle={t("noLogsTitle")}
          emptyDescription={t("noLogsDescription")}
          totalCount={logsQuery.data?.totalCount ?? 0}
          page={logsQuery.data?.page ?? 1}
          pageSize={logsQuery.data?.pageSize ?? 20}
          onPageChange={(nextPage) =>
            setQuery((current) => ({
              ...current,
              page: nextPage
            }))
          }
        />
      </PanelCard>
    </div>
  );
}

function resolveLogTypeFromPath(pathname: string): LogType {
  if (pathname.includes("/logs/security")) {
    return "security";
  }

  if (pathname.includes("/logs/http")) {
    return "http";
  }

  if (pathname.includes("/logs/entity-changes")) {
    return "entity-changes";
  }

  return "system";
}
