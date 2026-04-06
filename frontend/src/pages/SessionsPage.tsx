import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  useReactTable,
  getCoreRowModel,
  type ColumnDef,
  type PaginationState,
} from "@tanstack/react-table";
import apiClient from "@/api/client";
import type { Session, SessionsResponse } from "@/types/session";
import TopBar from "@/components/TopBar";
import DataTable, { PaginationBar, SearchInput } from "@/components/DataTable";
import StatusBadge from "@/components/StatusBadge";
import { useDebounce } from "@/hooks/use-debounce";

const columns: ColumnDef<Session>[] = [
  {
    accessorKey: "userName",
    header: "Kullanıcı",
  },
  {
    accessorKey: "ipAddress",
    header: "IP Adresi",
    cell: ({ row }) => (
      <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
        {row.original.ipAddress}
      </span>
    ),
  },
  {
    accessorKey: "userAgent",
    header: "Tarayıcı",
    cell: ({ row }) => (
      <span className="block max-w-[200px] truncate">
        {row.original.userAgent}
      </span>
    ),
  },
  {
    accessorKey: "isActive",
    header: "Durum",
    cell: ({ row }) =>
      row.original.isActive ? (
        <StatusBadge variant="success">Aktif</StatusBadge>
      ) : (
        <StatusBadge variant="muted">Süresi Dolmuş</StatusBadge>
      ),
  },
  {
    accessorKey: "createdAt",
    header: "Başlangıç",
    cell: ({ row }) =>
      new Date(row.original.createdAt).toLocaleString("tr-TR"),
  },
  {
    accessorKey: "expiresAt",
    header: "Bitiş",
    cell: ({ row }) =>
      new Date(row.original.expiresAt).toLocaleString("tr-TR"),
  },
];

export default function SessionsPage() {
  const [search, setSearch] = useState("");
  const [activeOnly, setActiveOnly] = useState(false);
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 20,
  });

  const debouncedSearch = useDebounce(search, 300);

  const { data, isLoading, isRefetching, refetch } = useQuery({
    queryKey: [
      "sessions",
      pagination.pageIndex,
      pagination.pageSize,
      debouncedSearch,
      activeOnly,
    ],
    queryFn: async () => {
      const params: Record<string, string | number | boolean> = {
        page: pagination.pageIndex + 1,
        pageSize: pagination.pageSize,
      };
      if (debouncedSearch) params.search = debouncedSearch;
      if (activeOnly) params.isActive = true;
      const { data } = await apiClient.get<SessionsResponse>(
        "/api/sessions",
        { params },
      );
      return data;
    },
  });

  const table = useReactTable({
    data: data?.items ?? [],
    columns,
    rowCount: data?.totalCount ?? 0,
    state: { pagination },
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
  });

  const pageCount = Math.ceil((data?.totalCount ?? 0) / pagination.pageSize);

  const filterOptions = [
    { value: "all", label: "Tümü" },
    { value: "active", label: "Aktif" },
  ];

  return (
    <div className="space-y-4">
      <TopBar
        title="Oturumlar"
        subtitle="Aktif ve geçmiş kullanıcı oturumları"
        windowOptions={filterOptions}
        activeWindow={activeOnly ? "active" : "all"}
        onWindowChange={(v) => setActiveOnly(v === "active")}
        onRefresh={() => refetch()}
        isRefreshing={isRefetching}
        showLive
      />

      <SearchInput
        value={search}
        onChange={setSearch}
        placeholder="Kullanıcı veya IP ara..."
        className="w-full max-w-xs"
      />

      <DataTable
        table={table}
        columnCount={columns.length}
        isLoading={isLoading}
      />

      <PaginationBar
        totalCount={data?.totalCount ?? 0}
        pageIndex={pagination.pageIndex}
        pageCount={pageCount}
        canPrevious={table.getCanPreviousPage()}
        canNext={table.getCanNextPage()}
        onPrevious={() => table.previousPage()}
        onNext={() => table.nextPage()}
      />
    </div>
  );
}
