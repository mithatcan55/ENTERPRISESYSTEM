import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  useReactTable,
  getCoreRowModel,
  type ColumnDef,
  type PaginationState,
} from "@tanstack/react-table";
import apiClient from "@/api/client";
import { useDebounce } from "@/hooks/use-debounce";
import type { User, UsersResponse } from "@/types/user";
import { Dialog, DialogTrigger } from "@/components/ui/dialog";
import { Plus } from "lucide-react";
import UserForm from "@/components/UserForm";
import TopBar from "@/components/TopBar";
import DataTable, { PaginationBar, SearchInput } from "@/components/DataTable";
import StatusBadge from "@/components/StatusBadge";

const columns: ColumnDef<User>[] = [
  {
    accessorKey: "userCode",
    header: "Kod",
    cell: ({ row }) => (
      <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
        {row.original.userCode}
      </span>
    ),
  },
  { accessorKey: "userName", header: "Kullanıcı Adı" },
  {
    accessorKey: "email",
    header: "E-posta",
    cell: ({ row }) => (
      <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
        {row.original.email}
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
        <StatusBadge variant="muted">Pasif</StatusBadge>
      ),
  },
  {
    accessorKey: "createdAt",
    header: "Oluşturulma",
    cell: ({ row }) =>
      new Date(row.original.createdAt).toLocaleDateString("tr-TR"),
  },
];

export default function UsersPage() {
  const [search, setSearch] = useState("");
  const [activeOnly, setActiveOnly] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 20,
  });

  const debouncedSearch = useDebounce(search, 300);

  const { data, isLoading } = useQuery({
    queryKey: [
      "users",
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
      const { data } = await apiClient.get<UsersResponse>("/api/users", {
        params,
      });
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

  const activeFilterOptions = [
    { value: "all", label: "Tümü" },
    { value: "active", label: "Aktif" },
  ];

  return (
    <div className="space-y-4">
      <TopBar
        title="Kullanıcılar"
        subtitle="Sistem kullanıcı yönetimi"
        windowOptions={activeFilterOptions}
        activeWindow={activeOnly ? "active" : "all"}
        onWindowChange={(v) => setActiveOnly(v === "active")}
      >
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <button
              className="flex items-center gap-1.5 rounded-md px-3 py-1 text-[12px] font-medium transition-all"
              style={{
                background: "rgba(99,122,255,0.15)",
                border: "1px solid rgba(99,122,255,0.35)",
                color: "#8B9FFF",
                fontFamily: "'JetBrains Mono', monospace",
              }}
            >
              <Plus className="h-3 w-3" />
              Yeni
            </button>
          </DialogTrigger>
          <UserForm onSuccess={() => setDialogOpen(false)} />
        </Dialog>
      </TopBar>

      {/* Search */}
      <SearchInput
        value={search}
        onChange={setSearch}
        placeholder="Kullanıcı ara..."
        className="w-full max-w-xs"
      />

      {/* Table */}
      <DataTable
        table={table}
        columnCount={columns.length}
        isLoading={isLoading}
      />

      {/* Pagination */}
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
