import {
  flexRender,
  type Table as TanTable,
} from "@tanstack/react-table";

interface DataTableProps<T> {
  table: TanTable<T>;
  /** Column count for empty/loading states */
  columnCount: number;
  isLoading?: boolean;
  emptyText?: string;
}

export default function DataTable<T>({
  table,
  columnCount,
  isLoading,
  emptyText = "Kayıt bulunamadı.",
}: DataTableProps<T>) {
  return (
    <div
      className="overflow-hidden rounded-[10px]"
      style={{
        background: "#111318",
        border: "1px solid rgba(255,255,255,0.06)",
      }}
    >
      <table className="w-full">
        {/* Header */}
        <thead>
          {table.getHeaderGroups().map((hg) => (
            <tr
              key={hg.id}
              style={{
                background: "rgba(255,255,255,0.03)",
                borderBottom: "1px solid rgba(255,255,255,0.06)",
              }}
            >
              {hg.headers.map((header) => (
                <th
                  key={header.id}
                  className="px-4 py-2.5 text-left text-[11px] font-medium uppercase tracking-[0.08em]"
                  style={{
                    color: "rgba(255,255,255,0.35)",
                    fontFamily: "'JetBrains Mono', monospace",
                  }}
                >
                  {header.isPlaceholder
                    ? null
                    : flexRender(
                        header.column.columnDef.header,
                        header.getContext(),
                      )}
                </th>
              ))}
            </tr>
          ))}
        </thead>

        {/* Body */}
        <tbody>
          {isLoading ? (
            <tr>
              <td
                colSpan={columnCount}
                className="px-4 py-10 text-center text-[13px]"
                style={{ color: "rgba(255,255,255,0.35)" }}
              >
                Yükleniyor...
              </td>
            </tr>
          ) : table.getRowModel().rows.length === 0 ? (
            <tr>
              <td
                colSpan={columnCount}
                className="px-4 py-10 text-center text-[13px]"
                style={{ color: "rgba(255,255,255,0.35)" }}
              >
                {emptyText}
              </td>
            </tr>
          ) : (
            table.getRowModel().rows.map((row) => (
              <tr
                key={row.id}
                className="transition-colors duration-100"
                style={{ borderBottom: "1px solid rgba(255,255,255,0.04)" }}
                onMouseEnter={(e) =>
                  (e.currentTarget.style.background = "rgba(255,255,255,0.02)")
                }
                onMouseLeave={(e) =>
                  (e.currentTarget.style.background = "transparent")
                }
              >
                {row.getVisibleCells().map((cell) => (
                  <td
                    key={cell.id}
                    className="px-4 py-2.5 text-[13px]"
                    style={{
                      color: "rgba(255,255,255,0.7)",
                      fontFamily: "'Plus Jakarta Sans', sans-serif",
                    }}
                  >
                    {flexRender(
                      cell.column.columnDef.cell,
                      cell.getContext(),
                    )}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

/* ─── Pagination ─── */

interface PaginationBarProps {
  totalCount: number;
  pageIndex: number;
  pageCount: number;
  canPrevious: boolean;
  canNext: boolean;
  onPrevious: () => void;
  onNext: () => void;
}

export function PaginationBar({
  totalCount,
  pageIndex,
  pageCount,
  canPrevious,
  canNext,
  onPrevious,
  onNext,
}: PaginationBarProps) {
  const pillStyle = (disabled: boolean): React.CSSProperties => ({
    border: "1px solid rgba(255,255,255,0.08)",
    color: disabled ? "rgba(255,255,255,0.15)" : "rgba(255,255,255,0.35)",
    fontFamily: "'JetBrains Mono', monospace",
    cursor: disabled ? "not-allowed" : "pointer",
  });

  return (
    <div className="flex items-center justify-between pt-3">
      <span
        className="text-[12px]"
        style={{
          color: "rgba(255,255,255,0.25)",
          fontFamily: "'JetBrains Mono', monospace",
        }}
      >
        {totalCount} kayıt
      </span>
      <div className="flex items-center gap-1.5">
        <button
          onClick={onPrevious}
          disabled={!canPrevious}
          className="rounded-md px-2.5 py-1 text-[12px] transition-all disabled:opacity-50"
          style={pillStyle(!canPrevious)}
        >
          ‹ Önceki
        </button>
        <span
          className="px-2 text-[12px]"
          style={{
            color: "rgba(255,255,255,0.5)",
            fontFamily: "'JetBrains Mono', monospace",
          }}
        >
          {pageIndex + 1}/{pageCount || 1}
        </span>
        <button
          onClick={onNext}
          disabled={!canNext}
          className="rounded-md px-2.5 py-1 text-[12px] transition-all disabled:opacity-50"
          style={pillStyle(!canNext)}
        >
          Sonraki ›
        </button>
      </div>
    </div>
  );
}

/* ─── Search Input ─── */

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
}

export function SearchInput({
  value,
  onChange,
  placeholder = "Ara...",
  className,
}: SearchInputProps) {
  return (
    <input
      type="text"
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      className={`rounded-md px-3 py-1.5 text-[13px] outline-none transition-colors ${className ?? ""}`}
      style={{
        background: "#0C0E14",
        border: "1px solid rgba(255,255,255,0.08)",
        color: "#fff",
        fontFamily: "'Plus Jakarta Sans', sans-serif",
      }}
      onFocus={(e) =>
        (e.currentTarget.style.borderColor = "rgba(99,122,255,0.4)")
      }
      onBlur={(e) =>
        (e.currentTarget.style.borderColor = "rgba(255,255,255,0.08)")
      }
    />
  );
}
