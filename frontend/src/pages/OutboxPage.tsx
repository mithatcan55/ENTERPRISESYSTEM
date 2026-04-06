import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  useReactTable,
  getCoreRowModel,
  type ColumnDef,
  type PaginationState,
} from "@tanstack/react-table";
import apiClient from "@/api/client";
import type { OutboxMessage, OutboxResponse } from "@/types/ops";
import TopBar from "@/components/TopBar";
import DataTable, { PaginationBar } from "@/components/DataTable";
import StatusBadge from "@/components/StatusBadge";

const statusMap: Record<string, "success" | "danger" | "muted"> = {
  Sent: "success",
  Failed: "danger",
  Pending: "muted",
};

const columns: ColumnDef<OutboxMessage>[] = [
  {
    accessorKey: "id",
    header: "ID",
    cell: ({ row }) => (
      <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
        {row.original.id.slice(0, 8)}...
      </span>
    ),
  },
  { accessorKey: "type", header: "Tip" },
  {
    accessorKey: "status",
    header: "Durum",
    cell: ({ row }) => (
      <StatusBadge variant={statusMap[row.original.status] ?? "muted"}>
        {row.original.status}
      </StatusBadge>
    ),
  },
  {
    accessorKey: "createdAt",
    header: "Oluşturulma",
    cell: ({ row }) =>
      new Date(row.original.createdAt).toLocaleString("tr-TR"),
  },
  {
    accessorKey: "retryCount",
    header: "Deneme",
    cell: ({ row }) => (
      <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
        {row.original.retryCount}
      </span>
    ),
  },
];

export default function OutboxPage() {
  const [status, setStatus] = useState<string>("all");
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 20,
  });

  const { data, isLoading, isRefetching, refetch } = useQuery({
    queryKey: ["outbox", pagination.pageIndex, pagination.pageSize, status],
    queryFn: async () => {
      const params: Record<string, string | number> = {
        page: pagination.pageIndex + 1,
        pageSize: pagination.pageSize,
      };
      if (status !== "all") params.status = status;
      const { data } = await apiClient.get<OutboxResponse>(
        "/api/ops/outbox/messages",
        { params },
      );
      return data;
    },
    refetchInterval: 30_000,
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

  const statusOptions = [
    { value: "all", label: "Tümü" },
    { value: "Pending", label: "Pending" },
    { value: "Sent", label: "Sent" },
    { value: "Failed", label: "Failed" },
  ];

  return (
    <div className="space-y-4">
      <TopBar
        title="Outbox Mesajları"
        subtitle="30 saniyede bir otomatik yenilenir"
        windowOptions={statusOptions}
        activeWindow={status}
        onWindowChange={setStatus}
        onRefresh={() => refetch()}
        isRefreshing={isRefetching}
        showLive
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
