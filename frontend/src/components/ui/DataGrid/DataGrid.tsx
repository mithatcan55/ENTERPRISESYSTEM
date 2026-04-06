import { useMemo } from "react";
import {
  useReactTable, getCoreRowModel, flexRender,
  type ColumnDef, createColumnHelper,
} from "@tanstack/react-table";
import { cn } from "@/lib/utils";
import { ArrowUp, ArrowDown, ChevronsUpDown, Inbox } from "lucide-react";

export { createColumnHelper };

export interface DataGridPagination {
  page: number; pageSize: number;
  onPageChange: (p: number) => void; onPageSizeChange: (s: number) => void;
}
export interface DataGridSorting {
  sortBy: string; sortDir: "asc" | "desc"; onSort: (col: string) => void;
}
export interface DataGridProps<T> {
  columns: ColumnDef<T, unknown>[]; data: T[]; isLoading?: boolean; totalCount?: number;
  pagination: DataGridPagination; sorting?: DataGridSorting;
  onRowClick?: (row: T) => void; emptyMessage?: string;
}

const PAGE_SIZES = [10, 20, 50, 100] as const;
const SKELETON_ROWS = 6;
const MAX_VISIBLE_PAGES = 5;
const sans = "'Plus Jakarta Sans', sans-serif";

function Shimmer({ width }: { width: string }) {
  return <div className="animate-pulse rounded" style={{ width, height: 14,
    background: "linear-gradient(90deg, #E2EBF3 25%, #D6E4F0 50%, #E2EBF3 75%)",
    backgroundSize: "200% 100%", animation: "shimmer 1.5s ease-in-out infinite" }} />;
}

function SortIcon({ column, sorting }: { column: string; sorting?: DataGridSorting }) {
  if (!sorting) return null;
  if (sorting.sortBy === column) return sorting.sortDir === "asc"
    ? <ArrowUp className="h-3 w-3" style={{ color: "#2E6DA4" }} />
    : <ArrowDown className="h-3 w-3" style={{ color: "#2E6DA4" }} />;
  return <ChevronsUpDown className="h-3 w-3 opacity-0 transition-opacity group-hover:opacity-100" style={{ color: "#B0BEC5" }} />;
}

function getVisiblePages(current: number, total: number): (number | "...")[] {
  if (total <= MAX_VISIBLE_PAGES) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "...")[] = [];
  const half = Math.floor(MAX_VISIBLE_PAGES / 2);
  let start = Math.max(1, current - half);
  let end = start + MAX_VISIBLE_PAGES - 1;
  if (end > total) { end = total; start = Math.max(1, end - MAX_VISIBLE_PAGES + 1); }
  if (start > 1) { pages.push(1); if (start > 2) pages.push("..."); }
  for (let i = start; i <= end; i++) { if (!pages.includes(i)) pages.push(i); }
  if (end < total) { if (end < total - 1) pages.push("..."); pages.push(total); }
  return pages;
}

export default function DataGrid<T>({
  columns, data, isLoading, totalCount, pagination, sorting, onRowClick,
  emptyMessage = "Kayıt bulunamadı.",
}: DataGridProps<T>) {
  const { page, pageSize, onPageChange, onPageSizeChange } = pagination;
  const total = totalCount ?? data.length;
  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  const table = useReactTable({ data, columns, rowCount: total,
    state: { pagination: { pageIndex: page - 1, pageSize } },
    getCoreRowModel: getCoreRowModel(), manualPagination: true, manualSorting: true });
  const visiblePages = useMemo(() => getVisiblePages(page, pageCount), [page, pageCount]);
  const showFrom = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const showTo = Math.min(page * pageSize, total);

  const pillBase = "rounded-md px-2 sm:px-2.5 py-1 text-[13px] font-medium transition-all outline-none min-h-[32px]";

  return (
    <div className="space-y-3">
      {/* ─── Desktop/Tablet: Table view ─── */}
      <div className="hidden sm:block overflow-hidden rounded-[10px]"
        style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              {table.getHeaderGroups().map((hg) => (
                <tr key={hg.id} style={{ background: "#F7FAFD", borderBottom: "1px solid #E2EBF3" }}>
                  {hg.headers.map((header) => {
                    const colId = header.column.id;
                    const isSorted = sorting?.sortBy === colId;
                    const meta = header.column.columnDef.meta as { hideOnMobile?: boolean; hideOnTablet?: boolean } | undefined;
                    return (
                      <th key={header.id}
                        className={cn(
                          "group px-3 lg:px-4 py-2.5 text-left text-[11px] font-medium tracking-[0.03em] select-none",
                          sorting && "cursor-pointer",
                          meta?.hideOnMobile && "hidden md:table-cell",
                          meta?.hideOnTablet && "hidden lg:table-cell",
                        )}
                        style={{ color: isSorted ? "#2E6DA4" : "#94A8BE", fontFamily: sans }}
                        onClick={() => sorting?.onSort(colId)}>
                        <span className="flex items-center gap-1.5">
                          {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                          <SortIcon column={colId} sorting={sorting} />
                        </span>
                      </th>
                    );
                  })}
                </tr>
              ))}
            </thead>
            <tbody>
              {isLoading ? Array.from({ length: SKELETON_ROWS }).map((_, ri) => (
                <tr key={`skel-${ri}`} style={{ borderBottom: "1px solid #F0F4F8" }}>
                  {columns.map((_, ci) => <td key={ci} className="px-3 lg:px-4 py-3"><Shimmer width={ci === 0 ? "60%" : "75%"} /></td>)}
                </tr>
              )) : table.getRowModel().rows.length === 0 ? (
                <tr><td colSpan={columns.length} className="px-4 py-16 text-center">
                  <div className="flex flex-col items-center gap-3">
                    <Inbox className="h-10 w-10" style={{ color: "#D6E4F0" }} />
                    <span className="text-[13px]" style={{ color: "#7A96B0" }}>{emptyMessage}</span>
                  </div>
                </td></tr>
              ) : table.getRowModel().rows.map((row) => (
                <tr key={row.id} className="transition-colors duration-100"
                  style={{ borderBottom: "1px solid #F0F4F8", cursor: onRowClick ? "pointer" : undefined }}
                  onClick={() => onRowClick?.(row.original)}
                  onMouseEnter={(e) => (e.currentTarget.style.background = "#F7FAFD")}
                  onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}>
                  {row.getVisibleCells().map((cell) => {
                    const meta = cell.column.columnDef.meta as { hideOnMobile?: boolean; hideOnTablet?: boolean } | undefined;
                    return (
                      <td key={cell.id}
                        className={cn("px-3 lg:px-4 py-2.5 text-[12px]",
                          meta?.hideOnMobile && "hidden md:table-cell",
                          meta?.hideOnTablet && "hidden lg:table-cell")}
                        style={{ color: "#2C4A6B", fontFamily: sans }}>
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ─── Mobile: Card view ─── */}
      <div className="sm:hidden flex flex-col gap-2">
        {isLoading ? Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="rounded-lg p-3 space-y-2" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}>
            <Shimmer width="60%" /><Shimmer width="80%" /><Shimmer width="40%" />
          </div>
        )) : table.getRowModel().rows.length === 0 ? (
          <div className="flex flex-col items-center gap-3 py-12">
            <Inbox className="h-10 w-10" style={{ color: "#D6E4F0" }} />
            <span className="text-[13px]" style={{ color: "#7A96B0" }}>{emptyMessage}</span>
          </div>
        ) : table.getRowModel().rows.map((row) => (
          <div key={row.id} className="rounded-lg p-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}
            onClick={() => onRowClick?.(row.original)}>
            <div className="flex flex-col gap-1.5">
              {row.getVisibleCells().map((cell) => {
                const header = cell.column.columnDef.header;
                const headerText = typeof header === "string" ? header : cell.column.id;
                if (cell.column.id === "actions") {
                  return (
                    <div key={cell.id} className="flex justify-end pt-1">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </div>
                  );
                }
                return (
                  <div key={cell.id} className="flex items-center justify-between gap-2">
                    <span className="text-[10px] uppercase tracking-wider shrink-0"
                      style={{ color: "#7A96B0", fontFamily: sans }}>{headerText}</span>
                    <span className="text-[12px] text-right" style={{ color: "#2C4A6B" }}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>
        ))}
      </div>

      {/* ─── Pagination ─── */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-2 rounded-lg px-3 py-2"
        style={{ background: "#F7FAFD", border: "1px solid #E2EBF3" }}>
        <span className="text-[12px]" style={{ color: "#94A8BE", fontFamily: sans }}>
          {total > 0 ? `${showFrom}–${showTo} / ${total} kayıt` : "0 kayıt"}
        </span>
        <div className="flex items-center gap-1">
          <button onClick={() => onPageChange(page - 1)} disabled={page <= 1}
            className={cn(pillBase, "disabled:opacity-40")}
            style={{ border: "1px solid #D6E4F0", color: page <= 1 ? "#B0BEC5" : "#7A96B0", fontFamily: sans }}>‹</button>
          {visiblePages.map((p, i) => p === "..." ? (
            <span key={`d${i}`} className="px-1 text-[11px]" style={{ color: "#B0BEC5", fontFamily: sans }}>...</span>
          ) : (
            <button key={p} onClick={() => onPageChange(p)} className={pillBase}
              style={{ fontFamily: sans, ...(p === page
                ? { background: "#1B3A5C", color: "#FFFFFF", border: "1px solid #1B3A5C" }
                : { background: "transparent", border: "1px solid #D6E4F0", color: "#7A96B0" }) }}>{p}</button>
          ))}
          <button onClick={() => onPageChange(page + 1)} disabled={page >= pageCount}
            className={cn(pillBase, "disabled:opacity-40")}
            style={{ border: "1px solid #D6E4F0", color: page >= pageCount ? "#B0BEC5" : "#7A96B0", fontFamily: sans }}>›</button>
        </div>
        {/* Page size — hidden on mobile */}
        <div className="hidden sm:flex items-center gap-1">
          {PAGE_SIZES.map((s) => (
            <button key={s} onClick={() => { onPageSizeChange(s); onPageChange(1); }} className={pillBase}
              style={{ fontFamily: sans, ...(s === pageSize
                ? { background: "#1B3A5C", color: "#FFFFFF", border: "1px solid #1B3A5C" }
                : { background: "transparent", border: "1px solid #D6E4F0", color: "#7A96B0" }) }}>{s}</button>
          ))}
        </div>
      </div>
    </div>
  );
}
