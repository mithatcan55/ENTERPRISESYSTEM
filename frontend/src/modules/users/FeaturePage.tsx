import { useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { usersApi } from "./api";
import type { UserListItem } from "./api";
import { getUserColumns } from "./columns";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { FilterBar } from "@/components/ui/FilterBar";
import { DataGrid } from "@/components/ui/DataGrid";
import { CrudModal } from "@/components/ui/CrudModal";
import { ProfileImageDisplay } from "@/components/ui/ProfileImage";
import { useDebounce } from "@/hooks/use-debounce";
import { useAuthStore } from "@/store/auth-store";
import { AppPermission, hasPermission } from "@/lib/permissions";
import { ShieldAlert, UserCheck, UserPlus, Users, UserX, X } from "lucide-react";
import UserFormPage from "./UserFormPage";

type PanelMode = "create" | "edit" | "detail" | null;

function extractError(err: unknown) {
  return (err as { response?: { data?: { detail?: string; message?: string } } }).response?.data?.detail
    ?? (err as { response?: { data?: { message?: string } } }).response?.data?.message
    ?? "Islem basarisiz";
}

function UserDetailModal({
  userId,
  onClose,
}: {
  userId: number | null;
  onClose: () => void;
}) {
  const { data: detail, isLoading } = useQuery({
    queryKey: ["users", "panel", userId],
    queryFn: () => usersApi.getById(userId as number),
    enabled: !!userId,
  });

  function fmtDate(value: string | null | undefined) {
    if (!value) return "-";
    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return "-";
    return parsed.toLocaleString("tr-TR", { dateStyle: "short", timeStyle: "short" });
  }

  const displayName = [detail?.firstName, detail?.lastName].filter(Boolean).join(" ").trim() || detail?.userCode || "-";

  return (
    <div className="fixed inset-0 z-[80] flex items-center justify-center p-4" style={{ background: "rgba(16,24,40,0.44)" }}>
      <div className="w-full max-w-[920px] rounded-xl p-5" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-[15px] font-semibold ui-text">Kullanici Detay</h3>
          <button type="button" onClick={onClose} className="ui-text-muted"><X size={16} /></button>
        </div>

        {isLoading && <p className="text-[12px] ui-text-muted">Yukleniyor...</p>}
        {!isLoading && !detail && <p className="text-[12px] ui-text-muted">Kullanici bulunamadi.</p>}

        {detail && (
          <div className="flex flex-col gap-4">
            <div className="rounded-xl p-4 flex items-center gap-4" style={{ border: "1px solid var(--ui-border)" }}>
              <ProfileImageDisplay src={detail.profileImageUrl} displayName={displayName} size={60} />
              <div className="min-w-0">
                <p className="text-[16px] font-semibold ui-text">{displayName}</p>
                <p className="text-[13px] ui-text-muted break-all">{detail.email}</p>
                <div className="mt-1 flex flex-wrap gap-2 text-[11px]">
                  <span className="rounded-full px-2 py-0.5" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
                    {detail.isActive ? "Aktif" : "Pasif"}
                  </span>
                  {detail.mustChangePassword && (
                    <span className="rounded-full px-2 py-0.5" style={{ background: "var(--ui-warning-bg)", color: "var(--ui-warning)" }}>
                      Sifre degisimi zorunlu
                    </span>
                  )}
                  {detail.isDeleted && (
                    <span className="rounded-full px-2 py-0.5" style={{ background: "var(--ui-danger-bg)", color: "var(--ui-danger)" }}>
                      Silinmis
                    </span>
                  )}
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-[12px]">
              <div className="rounded-xl p-4" style={{ border: "1px solid var(--ui-border)" }}>
                <p className="text-[13px] font-semibold ui-text mb-3">Kimlik Bilgileri</p>
                <div className="grid grid-cols-[130px_1fr] gap-y-2 gap-x-3">
                  <p className="ui-text-muted">Kod</p><p className="ui-text break-all">{detail.userCode}</p>
                  <p className="ui-text-muted">Ad Soyad</p><p className="ui-text break-all">{displayName}</p>
                  <p className="ui-text-muted">E-posta</p><p className="ui-text break-all">{detail.email}</p>
                  <p className="ui-text-muted">Rol Sayisi</p><p className="ui-text">{detail.roles.length}</p>
                  <p className="ui-text-muted">Direct Permission</p><p className="ui-text">{detail.directPermissions.length}</p>
                  <p className="ui-text-muted">Sifre Son</p><p className="ui-text">{fmtDate(detail.passwordExpiresAt)}</p>
                </div>
              </div>

              <div className="rounded-xl p-4" style={{ border: "1px solid var(--ui-border)" }}>
                <p className="text-[13px] font-semibold ui-text mb-3">Denetim Bilgileri</p>
                <div className="grid grid-cols-[130px_1fr] gap-y-2 gap-x-3">
                  <p className="ui-text-muted">Olusturulma</p><p className="ui-text">{fmtDate(detail.createdAt)}</p>
                  <p className="ui-text-muted">Olusturan</p><p className="ui-text break-all">{detail.createdBy ?? "-"}</p>
                  <p className="ui-text-muted">Son Guncelleme</p><p className="ui-text">{fmtDate(detail.modifiedAt)}</p>
                  <p className="ui-text-muted">Guncelleyen</p><p className="ui-text break-all">{detail.modifiedBy ?? "-"}</p>
                  <p className="ui-text-muted">Silinme Tarihi</p><p className="ui-text">{fmtDate(detail.deletedAt)}</p>
                  <p className="ui-text-muted">Silen</p><p className="ui-text break-all">{detail.deletedBy ?? "-"}</p>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default function UsersFeaturePage() {
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const [searchParams, setSearchParams] = useSearchParams();

  const canView = hasPermission(user, AppPermission.UsersListView);
  const canCreate = hasPermission(user, AppPermission.UsersCreate);
  const canUpdate = hasPermission(user, AppPermission.UsersUpdate);
  const canDeactivate = hasPermission(user, AppPermission.UsersUpdate);
  const canReactivate = hasPermission(user, AppPermission.UsersUpdate);
  const canDelete = hasPermission(user, AppPermission.UsersDelete);

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

  const panelMode = (searchParams.get("mode") as PanelMode) ?? null;
  const panelUserId = searchParams.get("id") ? Number(searchParams.get("id")) : null;

  const { data, isLoading } = useQuery({
    queryKey: ["users", page, pageSize, debouncedSearch, sortBy, sortDir, isActiveFilter, includeDeleted],
    queryFn: () => usersApi.list({
      page,
      pageSize,
      search: debouncedSearch || undefined,
      sortBy,
      sortDirection: sortDir,
      isActive: isActiveFilter === "all" ? undefined : isActiveFilter === "active",
      includeDeleted,
    }),
    enabled: canView,
    placeholderData: (prev) => prev,
  });

  const items = data?.items ?? [];
  const stats = useMemo(() => ({
    total: data?.totalCount ?? 0,
    active: items.filter((u) => u.isActive).length,
    inactive: items.filter((u) => !u.isActive).length,
    mustChange: items.filter((u) => u.mustChangePassword).length,
  }), [data, items]);

  function openPanel(mode: PanelMode, id?: number) {
    const next = new URLSearchParams(searchParams);
    if (!mode) {
      next.delete("mode");
      next.delete("id");
    } else {
      next.set("mode", mode);
      if (id) next.set("id", String(id));
      else next.delete("id");
    }
    setSearchParams(next);
  }

  function handleSort(col: string) {
    if (sortBy === col) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else { setSortBy(col); setSortDir("asc"); }
    setPage(1);
  }

  const deleteMut = useMutation({
    mutationFn: (id: number) => usersApi.delete(id),
    onSuccess: () => {
      toast.success("Kullanici silindi");
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setModalMode(null);
      setSelectedUser(null);
    },
    onError: (e) => toast.error(extractError(e)),
  });

  const deactivateMut = useMutation({
    mutationFn: (id: number) => usersApi.deactivate(id),
    onSuccess: () => {
      toast.success("Kullanici pasife alindi");
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setModalMode(null);
      setSelectedUser(null);
    },
    onError: (e) => toast.error(extractError(e)),
  });

  const reactivateMut = useMutation({
    mutationFn: (id: number) => usersApi.reactivate(id),
    onSuccess: () => {
      toast.success("Kullanici aktive edildi");
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setModalMode(null);
      setSelectedUser(null);
    },
    onError: (e) => toast.error(extractError(e)),
  });

  const columns = useMemo(() => getUserColumns({
    onEdit: (u) => canUpdate && openPanel("edit", u.id),
    onDetail: (u) => canView && openPanel("detail", u.id),
    onDeactivate: (u) => { setSelectedUser(u); setModalMode("deactivate"); },
    onReactivate: (u) => { setSelectedUser(u); setModalMode("reactivate"); },
    onDelete: (u) => { setSelectedUser(u); setModalMode("delete"); },
    permissions: {
      canReadDetail: canView,
      canUpdate,
      canDeactivate,
      canReactivate,
      canDelete,
    },
  }), [canView, canUpdate, canDeactivate, canReactivate, canDelete]);

  if (!canView) {
    return <p className="text-[13px] ui-text-muted">Users.View yetkisi gerekli.</p>;
  }

  const showFormOverlay = panelMode === "create" || panelMode === "edit";
  const statCards = [
    { label: "Toplam", value: stats.total, Icon: Users, accent: "#2E6DA4" },
    { label: "Aktif", value: stats.active, Icon: UserCheck, accent: "#1E8A6E" },
    { label: "Pasif", value: stats.inactive, Icon: UserX, accent: "#E05252" },
    { label: "Sifre degismeli", value: stats.mustChange, Icon: ShieldAlert, accent: "#D4891A" },
  ];

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title="Kullanici Yonetimi"
        subtitle="Feature + mode tek ekran (list, create, edit, detail)"
        actions={canCreate ? <PageAction onClick={() => openPanel("create")}><UserPlus size={16} /> Yeni Kullanici</PageAction> : undefined}
      />

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        {statCards.map((s) => (
          <div key={s.label} className="rounded-[10px] px-4 py-3 ui-card" style={{ borderTop: `3px solid ${s.accent}` }}>
            <div className="flex items-center justify-between">
              <span className="text-[22px] font-bold ui-text">{s.value}</span>
              <s.Icon size={18} style={{ color: s.accent, opacity: 0.7 }} />
            </div>
            <span className="text-[11px] ui-text-muted">{s.label}</span>
          </div>
        ))}
      </div>

      <FilterBar
        search={{ value: search, onChange: setSearch, placeholder: "Kod veya e-posta ara..." }}
        filters={[
          {
            key: "isActive",
            label: "Durum",
            type: "boolean",
            value: isActiveFilter === "active" ? true : isActiveFilter === "inactive" ? false : "",
            onChange: (v) => { setIsActiveFilter(v === true ? "active" : v === false ? "inactive" : "all"); setPage(1); },
          },
          {
            key: "includeDeleted",
            label: "Silinmis Kayitlar",
            type: "select",
            value: includeDeleted ? "1" : "0",
            options: [
              { value: "0", label: "Silinenler haric" },
              { value: "1", label: "Silinenler dahil" },
            ],
            onChange: (v) => { setIncludeDeleted(v === "1"); setPage(1); },
          },
        ]}
        onReset={() => { setSearch(""); setIsActiveFilter("all"); setIncludeDeleted(false); setPage(1); }}
        activeCount={(search ? 1 : 0) + (isActiveFilter !== "all" ? 1 : 0) + (includeDeleted ? 1 : 0)}
      />

      <DataGrid
        columns={columns}
        data={items}
        isLoading={isLoading}
        totalCount={data?.totalCount}
        pagination={{ page, pageSize, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1); } }}
        sorting={{ sortBy: sortBy ?? "", sortDir, onSort: handleSort }}
        onRowClick={(row) => openPanel("detail", row.id)}
        emptyMessage="Kullanici bulunamadi"
      />

      {panelMode === "detail" && (
        <UserDetailModal userId={panelUserId} onClose={() => openPanel(null)} />
      )}

      {showFormOverlay && (
        <div className="fixed inset-0 z-[70] overflow-y-auto" style={{ background: "rgba(16,24,40,0.44)" }}>
          <div className="min-h-full flex items-start justify-center p-4 md:p-6">
            <div className="w-full max-w-[980px]">
              <UserFormPage />
            </div>
          </div>
        </div>
      )}

      <CrudModal
        mode="delete"
        title="Kullanici Sil"
        isOpen={modalMode === "delete"}
        onClose={() => setModalMode(null)}
        onSubmit={() => selectedUser && deleteMut.mutate(selectedUser.id)}
        isLoading={deleteMut.isPending}
      >
        <p className="text-[13px] ui-text">Bu kullanici silinecek: <strong>{selectedUser?.userCode}</strong></p>
      </CrudModal>

      <CrudModal
        mode="delete"
        title="Kullaniciyi Pasife Al"
        isOpen={modalMode === "deactivate"}
        onClose={() => setModalMode(null)}
        onSubmit={() => selectedUser && deactivateMut.mutate(selectedUser.id)}
        isLoading={deactivateMut.isPending}
      >
        <p className="text-[13px] ui-text"><strong>{selectedUser?.userCode}</strong> pasife alinacak.</p>
      </CrudModal>

      <CrudModal
        mode="create"
        title="Kullaniciyi Aktive Et"
        isOpen={modalMode === "reactivate"}
        onClose={() => setModalMode(null)}
        onSubmit={() => selectedUser && reactivateMut.mutate(selectedUser.id)}
        isLoading={reactivateMut.isPending}
      >
        <p className="text-[13px] ui-text"><strong>{selectedUser?.userCode}</strong> aktive edilecek.</p>
      </CrudModal>
    </div>
  );
}
