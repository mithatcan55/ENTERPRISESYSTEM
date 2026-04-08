import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { usersApi } from "./api";
import type { UserListItem } from "./api";
import { getUserColumns } from "./columns";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { FilterBar } from "@/components/ui/FilterBar";
import { DataGrid } from "@/components/ui/DataGrid";
import { CrudModal } from "@/components/ui/CrudModal";
import { useDebounce } from "@/hooks/use-debounce";
import { Users, UserCheck, UserX, ShieldAlert, UserPlus } from "lucide-react";

function extractError(err: unknown) {
  return (err as { response?: { data?: { detail?: string; message?: string } } }).response?.data?.detail
    ?? (err as { response?: { data?: { message?: string } } }).response?.data?.message
    ?? "İşlem başarısız";
}

export default function UsersListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState<string | undefined>();
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
  const [isActiveFilter, setIsActiveFilter] = useState<string>("all");
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const debouncedSearch = useDebounce(search, 300);

  const [modalMode, setModalMode] = useState<"delete" | "deactivate" | "reactivate" | null>(null);
  const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["users", page, pageSize, debouncedSearch, sortBy, sortDir, isActiveFilter, includeDeleted],
    queryFn: () => usersApi.list({ page, pageSize, search: debouncedSearch || undefined, sortBy, sortDirection: sortDir, isActive: isActiveFilter === "all" ? undefined : isActiveFilter === "active", includeDeleted }),
    placeholderData: (prev) => prev,
  });

  const items = data?.items ?? [];
  const stats = useMemo(() => ({
    total: data?.totalCount ?? 0,
    active: items.filter((u) => u.isActive).length,
    inactive: items.filter((u) => !u.isActive).length,
    mustChange: items.filter((u) => u.mustChangePassword).length,
  }), [data, items]);

  function handleSort(col: string) {
    if (sortBy === col) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else { setSortBy(col); setSortDir("asc"); }
    setPage(1);
  }

  const openDelete = (u: UserListItem) => { setSelectedUser(u); setModalMode("delete"); };
  const openDeactivate = (u: UserListItem) => { setSelectedUser(u); setModalMode("deactivate"); };
  const openReactivate = (u: UserListItem) => { setSelectedUser(u); setModalMode("reactivate"); };
  const closeModal = () => { setModalMode(null); setSelectedUser(null); };
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["users"] });

  const deleteMut = useMutation({ mutationFn: (id: number) => usersApi.delete(id), onSuccess: () => { toast.success("Kullanıcı silindi"); invalidate(); closeModal(); }, onError: (e) => toast.error(extractError(e)) });
  const deactivateMut = useMutation({ mutationFn: (id: number) => usersApi.deactivate(id), onSuccess: () => { toast.success("Kullanıcı pasife alındı"); invalidate(); closeModal(); }, onError: (e) => toast.error(extractError(e)) });
  const reactivateMut = useMutation({ mutationFn: (id: number) => usersApi.reactivate(id), onSuccess: () => { toast.success("Kullanıcı aktive edildi"); invalidate(); closeModal(); }, onError: (e) => toast.error(extractError(e)) });

  const columns = useMemo(() => getUserColumns({
    onEdit: (u) => navigate(`/users/${u.id}/edit`),
    onDetail: (u) => navigate(`/users/${u.id}`),
    onDeactivate: openDeactivate, onReactivate: openReactivate, onDelete: openDelete,
  }), [navigate]);

  const statCards = [
    { label: "Toplam", value: stats.total, Icon: Users, accent: "#2E6DA4" },
    { label: "Aktif", value: stats.active, Icon: UserCheck, accent: "#1E8A6E" },
    { label: "Pasif", value: stats.inactive, Icon: UserX, accent: "#E05252" },
    { label: "Şifre değişmeli", value: stats.mustChange, Icon: ShieldAlert, accent: "#D4891A" },
  ];

  return (
    <div className="flex flex-col gap-4">
      <PageHeader title="Kullanıcı Yönetimi" subtitle="Sistem kullanıcıları ve erişim hakları"
        actions={<PageAction onClick={() => navigate("/users/new")}><UserPlus size={16} /> Yeni Kullanıcı</PageAction>} />

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        {statCards.map((s) => (
          <div key={s.label} className="rounded-[10px] px-4 py-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderTop: `3px solid ${s.accent}` }}>
            <div className="flex items-center justify-between">
              <span className="text-[22px] font-bold" style={{ color: "#1B3A5C" }}>{s.value}</span>
              <s.Icon size={18} style={{ color: s.accent, opacity: 0.6 }} />
            </div>
            <span className="text-[11px]" style={{ color: "#7A96B0" }}>{s.label}</span>
          </div>
        ))}
      </div>

      <FilterBar search={{ value: search, onChange: setSearch, placeholder: "Kod veya e-posta ara..." }}
        filters={[{ key: "isActive", label: "Durum", type: "boolean",
          value: isActiveFilter === "active" ? true : isActiveFilter === "inactive" ? false : "",
          onChange: (v) => { setIsActiveFilter(v === true ? "active" : v === false ? "inactive" : "all"); setPage(1); } }]}
        onReset={() => { setSearch(""); setIsActiveFilter("all"); setIncludeDeleted(false); setPage(1); }}
        activeCount={(search ? 1 : 0) + (isActiveFilter !== "all" ? 1 : 0) + (includeDeleted ? 1 : 0)} />

      <DataGrid columns={columns} data={items} isLoading={isLoading} totalCount={data?.totalCount}
        pagination={{ page, pageSize, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1); } }}
        sorting={{ sortBy: sortBy ?? "", sortDir, onSort: handleSort }}
        onRowClick={(row) => navigate(`/users/${row.id}`)}
        emptyMessage="Kullanıcı bulunamadı" />

      <CrudModal mode="delete" title="Kullanıcı Sil" isOpen={modalMode === "delete"} onClose={closeModal}
        onSubmit={() => selectedUser && deleteMut.mutate(selectedUser.id)} isLoading={deleteMut.isPending}>
        <p style={{ color: "#2C4A6B", fontSize: 14 }}><strong>{selectedUser?.userCode}</strong> ({selectedUser?.userCode}) kalıcı olarak silinecek.</p>
        <p className="mt-2" style={{ color: "#7A96B0", fontSize: 12 }}>Bu işlem geri alınamaz.</p>
      </CrudModal>

      <CrudModal mode="delete" title="Kullanıcıyı Pasife Al" isOpen={modalMode === "deactivate"} onClose={closeModal}
        onSubmit={() => selectedUser && deactivateMut.mutate(selectedUser.id)} isLoading={deactivateMut.isPending}>
        <p style={{ color: "#2C4A6B", fontSize: 14 }}><strong>{selectedUser?.userCode}</strong> pasife alınacak. Giriş yapamayacak.</p>
      </CrudModal>

      <CrudModal mode="create" title="Kullanıcıyı Aktive Et" isOpen={modalMode === "reactivate"} onClose={closeModal}
        onSubmit={() => selectedUser && reactivateMut.mutate(selectedUser.id)} isLoading={reactivateMut.isPending}>
        <p style={{ color: "#2C4A6B", fontSize: 14 }}><strong>{selectedUser?.userCode}</strong> tekrar aktive edilecek.</p>
      </CrudModal>
    </div>
  );
}
