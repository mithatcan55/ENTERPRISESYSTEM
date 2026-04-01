import { X } from "lucide-react";
import { useTranslation } from "react-i18next";
import { formatDateTime } from "../../core/utils/formatDateTime";
import type {
  EntityChangeLogListItem,
  HttpRequestLogListItem,
  SecurityEventListItem,
  SystemLogListItem,
} from "./operations.api";

type LogItem = SystemLogListItem | SecurityEventListItem | HttpRequestLogListItem | EntityChangeLogListItem;

interface LogDetailPanelProps {
  item: LogItem;
  logType: string;
  onClose: () => void;
}

export function LogDetailPanel({ item, logType, onClose }: LogDetailPanelProps) {
  const { t, i18n } = useTranslation(["operations"]);

  const fields = getFieldsForType(item, logType, i18n.language);

  return (
    <div className="log-detail-panel">
      <div className="log-detail-panel__title">
        <span>{t("logDetail")}</span>
        <button type="button" className="toast__close" onClick={onClose} aria-label="Close">
          <X size={16} />
        </button>
      </div>
      <div className="log-detail-panel__grid">
        {fields.map((f) => (
          <div key={f.label} className="log-detail-panel__field">
            <span className="log-detail-panel__label">{f.label}</span>
            <span className={`log-detail-panel__value ${f.mono ? "log-detail-panel__value--mono" : ""}`}>
              {f.value || "—"}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

function getFieldsForType(item: LogItem, logType: string, lang: string) {
  const ts = formatDateTime(item.timestamp, lang);

  switch (logType) {
    case "system": {
      const s = item as SystemLogListItem;
      return [
        { label: "Timestamp", value: ts },
        { label: "Level", value: s.level },
        { label: "Category", value: s.category },
        { label: "Source", value: s.source },
        { label: "Message", value: s.message, mono: false },
        { label: "Correlation ID", value: s.correlationId, mono: true },
        { label: "HTTP Status", value: s.httpStatusCode != null ? String(s.httpStatusCode) : null },
        { label: "User ID", value: s.userId },
        { label: "Username", value: s.username },
      ];
    }
    case "security": {
      const s = item as SecurityEventListItem;
      return [
        { label: "Timestamp", value: ts },
        { label: "Event Type", value: s.eventType },
        { label: "Severity", value: s.severity },
        { label: "User ID", value: s.userId },
        { label: "Username", value: s.username },
        { label: "Resource", value: s.resource },
        { label: "Action", value: s.action },
        { label: "Success", value: s.isSuccessful ? "Yes" : "No" },
        { label: "Failure Reason", value: s.failureReason },
        { label: "IP Address", value: s.ipAddress, mono: true },
      ];
    }
    case "http": {
      const s = item as HttpRequestLogListItem;
      return [
        { label: "Timestamp", value: ts },
        { label: "Method", value: s.method },
        { label: "Path", value: s.path, mono: true },
        { label: "Status Code", value: String(s.statusCode) },
        { label: "Duration (ms)", value: String(s.durationMs) },
        { label: "Is Error", value: s.isError ? "Yes" : "No" },
        { label: "Correlation ID", value: s.correlationId, mono: true },
        { label: "User ID", value: s.userId },
        { label: "Username", value: s.username },
        { label: "IP Address", value: s.ipAddress, mono: true },
      ];
    }
    case "entity-changes": {
      const s = item as EntityChangeLogListItem;
      return [
        { label: "Timestamp", value: ts },
        { label: "Entity Type", value: s.entityType },
        { label: "Entity ID", value: s.entityId, mono: true },
        { label: "Action", value: s.action },
        { label: "Table", value: s.tableName, mono: true },
        { label: "Schema", value: s.schemaName, mono: true },
        { label: "User ID", value: s.userId },
        { label: "Username", value: s.username },
        { label: "Correlation ID", value: s.correlationId, mono: true },
        { label: "Changed Properties", value: s.changedProperties, mono: true },
      ];
    }
    default:
      return [{ label: "ID", value: String(item.id) }];
  }
}
