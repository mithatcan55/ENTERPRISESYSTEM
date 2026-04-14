import { useEffect, useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { usersApi } from "./api";
import type {
  UserListItem,
  LookupItem,
  PermissionLookupItem,
  CreateUserPayload,
  UpdateUserPayload,
} from "./api";
import { getUserColumns } from "./columns";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { FilterBar } from "@/components/ui/FilterBar";
import { DataGrid } from "@/components/ui/DataGrid";
import { CrudModal } from "@/components/ui/CrudModal";
import { useDebounce } from "@/hooks/use-debounce";
import { useAuthStore } from "@/store/auth-store";
import { AppPermission, hasPermission } from "@/lib/permissions";
import { UserPlus, X } from "lucide-react";

type PanelMode = "create" | "edit" | "detail" | null;

function extractError(err: unknown) {
  return (err as { response?: { data?: { detail?: string; message?: string } } }).response?.data?.detail
    ?? (err as { response?: { data?: { message?: string } } }).response?.data?.message
    ?? "Islem basarisiz";
}

function UserPanel({
  mode,
  userId,
  canCreate,
  canUpdate,
  onClose,
  onSaved,
}: {
  mode: PanelMode;
  userId: number | null;
  canCreate: boolean;
  canUpdate: boolean;
  onClose: () => void;
  onSaved: () => void;
}) {
  const queryClient = useQueryClient();

  const { data: detail, isLoading: detailLoading } = useQuery({
    queryKey: ["users", "panel", userId],
    queryFn: () => usersApi.getById(userId as number),
    enabled: (mode === "detail" || mode === "edit") && !!userId,
  });

  const { data: lookups } = useQuery({
    queryKey: ["users", "lookups", "panel"],
    queryFn: () => usersApi.lookups(),
    enabled: mode === "create" || mode === "edit",
    staleTime: 60_000,
  });

  const [createForm, setCreateForm] = useState<CreateUserPayload>({
    userCode: "",
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    companyId: 1,
    notifyAdminByMail: false,
    roleIds: [],
    permissionIds: [],
  });

  const [editForm, setEditForm] = useState<UpdateUserPayload>({
    firstName: "",
    lastName: "",
    email: "",
    isActive: true,
    mustChangePassword: false,
    profileImageUrl: "",
    roleIds: [],
    permissionIds: [],
  });

  const [formInitializedFor, setFormInitializedFor] = useState<number | null>(null);

  useEffect(() => {
    if (!(mode === "edit" || mode === "detail") || !detail || formInitializedFor === detail.id) {
      return;
    }

    setEditForm({
      firstName: detail.firstName ?? "",
      lastName: detail.lastName ?? "",
      email: detail.email,
      isActive: detail.isActive,
      mustChangePassword: detail.mustChangePassword,
      profileImageUrl: detail.profileImageUrl ?? "",
      roleIds: detail.roles.map((x) => x.roleId),
      permissionIds: detail.directPermissions.map((x) => x.subModulePageId),
    });
    setFormInitializedFor(detail.id);
  }, [detail, mode, formInitializedFor]);

  const createMut = useMutation({
    mutationFn: () => usersApi.create(createForm),
    onSuccess: () => {
      toast.success("Kullanici olusturuldu");
      queryClient.invalidateQueries({ queryKey: ["users"] });
      onSaved();
    },
    onError: (e) => toast.error(extractError(e)),
  });

  const updateMut = useMutation({
    mutationFn: () => usersApi.update(userId as number, editForm),
    onSuccess: () => {
      toast.success("Kullanici guncellendi");
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["users", "panel", userId] });
      onSaved();
    },
    onError: (e) => toast.error(extractError(e)),
  });

  if (!mode) {
    return null;
  }

  if (mode === "detail") {
    return (
      <aside className="w-full lg:w-[420px] rounded-xl p-4 h-fit" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-[14px] font-semibold ui-text">Kullanici Detay</h3>
          <button type="button" onClick={onClose} className="ui-text-muted"><X size={16} /></button>
        </div>
        {detailLoading && <p className="text-[12px] ui-text-muted">Yukleniyor...</p>}
        {!detailLoading && !detail && <p className="text-[12px] ui-text-muted">Kullanici bulunamadi.</p>}
        {detail && (
          <div className="text-[12px] space-y-2">
            <p><strong>Kod:</strong> {detail.userCode}</p>
            <p><strong>E-posta:</strong> {detail.email}</p>
            <p><strong>Durum:</strong> {detail.isActive ? "Aktif" : "Pasif"}</p>
            <p><strong>Sifre Degisimi:</strong> {detail.mustChangePassword ? "Zorunlu" : "Normal"}</p>
            <p><strong>Rol Sayisi:</strong> {detail.roles.length}</p>
            <p><strong>Direct Permission:</strong> {detail.directPermissions.length}</p>
          </div>
        )}
      </aside>
    );
  }

  if (mode === "create" && !canCreate) {
    return null;
  }

  if (mode === "edit" && !canUpdate) {
    return null;
  }

  const roleItems: LookupItem[] = lookups?.roles ?? [];
  const permissionItems: PermissionLookupItem[] = lookups?.permissions ?? [];

  const roleIds = mode === "create" ? (createForm.roleIds ?? []) : (editForm.roleIds ?? []);
  const permissionIds = mode === "create" ? (createForm.permissionIds ?? []) : (editForm.permissionIds ?? []);

  function toggleRole(roleId: number) {
    if (mode === "create") {
      setCreateForm((prev) => ({
        ...prev,
        roleIds: (prev.roleIds ?? []).includes(roleId)
          ? (prev.roleIds ?? []).filter((x) => x !== roleId)
          : [...(prev.roleIds ?? []), roleId],
      }));
      return;
    }

    setEditForm((prev) => ({
      ...prev,
      roleIds: (prev.roleIds ?? []).includes(roleId)
        ? (prev.roleIds ?? []).filter((x) => x !== roleId)
        : [...(prev.roleIds ?? []), roleId],
    }));
  }

  function togglePermission(permissionId: number) {
    if (mode === "create") {
      setCreateForm((prev) => ({
        ...prev,
        permissionIds: (prev.permissionIds ?? []).includes(permissionId)
          ? (prev.permissionIds ?? []).filter((x) => x !== permissionId)
          : [...(prev.permissionIds ?? []), permissionId],
      }));
      return;
    }

    setEditForm((prev) => ({
      ...prev,
      permissionIds: (prev.permissionIds ?? []).includes(permissionId)
        ? (prev.permissionIds ?? []).filter((x) => x !== permissionId)
        : [...(prev.permissionIds ?? []), permissionId],
    }));
  }

  return (
    <aside className="w-full lg:w-[420px] rounded-xl p-4 h-fit" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-[14px] font-semibold ui-text">{mode === "create" ? "Kullanici Olustur" : "Kullanici Duzenle"}</h3>
        <button type="button" onClick={onClose} className="ui-text-muted"><X size={16} /></button>
      </div>

      <div className="space-y-3">
        {mode === "create" && (
          <>
            <input className="w-full h-[38px] rounded border px-2 text-[12px]" style={{ borderColor: "var(--ui-border)" }} placeholder="UserCode"
              value={createForm.userCode}
              onChange={(e) => setCreateForm((p) => ({ ...p, userCode: e.target.value.toUpperCase() }))} />
            <input className="w-full h-[38px] rounded border px-2 text-[12px]" style={{ borderColor: "var(--ui-border)" }} placeholder="Password"
              type="password"
              value={createForm.password}
              onChange={(e) => setCreateForm((p) => ({ ...p, password: e.target.value }))} />
          </>
        )}

        <input className="w-full h-[38px] rounded border px-2 text-[12px]" style={{ borderColor: "var(--ui-border)" }} placeholder="FirstName"
          value={mode === "create" ? createForm.firstName ?? "" : editForm.firstName ?? ""}
          onChange={(e) => mode === "create"
            ? setCreateForm((p) => ({ ...p, firstName: e.target.value }))
            : setEditForm((p) => ({ ...p, firstName: e.target.value }))} />

        <input className="w-full h-[38px] rounded border px-2 text-[12px]" style={{ borderColor: "var(--ui-border)" }} placeholder="LastName"
          value={mode === "create" ? createForm.lastName ?? "" : editForm.lastName ?? ""}
          onChange={(e) => mode === "create"
            ? setCreateForm((p) => ({ ...p, lastName: e.target.value }))
            : setEditForm((p) => ({ ...p, lastName: e.target.value }))} />

        <input className="w-full h-[38px] rounded border px-2 text-[12px]" style={{ borderColor: "var(--ui-border)" }} placeholder="Email"
          value={mode === "create" ? createForm.email : editForm.email}
          onChange={(e) => mode === "create"
            ? setCreateForm((p) => ({ ...p, email: e.target.value }))
            : setEditForm((p) => ({ ...p, email: e.target.value }))} />

        {mode === "edit" && (
          <label className="flex items-center gap-2 text-[12px]">
            <input type="checkbox" checked={editForm.isActive}
              onChange={(e) => setEditForm((p) => ({ ...p, isActive: e.target.checked }))} />
            Aktif
          </label>
        )}

        {mode === "edit" && (
          <label className="flex items-center gap-2 text-[12px]">
            <input type="checkbox" checked={editForm.mustChangePassword}
              onChange={(e) => setEditForm((p) => ({ ...p, mustChangePassword: e.target.checked }))} />
            MustChangePassword
          </label>
        )}

        <div className="max-h-[160px] overflow-auto rounded border p-2" style={{ borderColor: "var(--ui-border)" }}>
          <p className="text-[11px] font-semibold ui-text mb-2">Roles</p>
          {roleItems.map((role) => (
            <label key={role.id} className="flex items-center gap-2 text-[12px] py-0.5">
              <input type="checkbox" checked={roleIds.includes(role.id)} onChange={() => toggleRole(role.id)} />
              {role.name}
            </label>
          ))}
        </div>

        <div className="max-h-[180px] overflow-auto rounded border p-2" style={{ borderColor: "var(--ui-border)" }}>
          <p className="text-[11px] font-semibold ui-text mb-2">Permissions</p>
          {permissionItems.map((perm) => (
            <label key={perm.id} className="flex items-center gap-2 text-[12px] py-0.5">
              <input type="checkbox" checked={permissionIds.includes(perm.id)} onChange={() => togglePermission(perm.id)} />
              {perm.transactionCode}:{perm.actionCode}
            </label>
          ))}
        </div>

        <div className="flex gap-2 pt-2">
          <PageAction variant="ghost" onClick={onClose}>Vazgec</PageAction>
          {mode === "create" ? (
            <PageAction
              onClick={() => {
                if (!createForm.userCode || !createForm.email || !createForm.password) {
                  toast.error("UserCode, Email ve Password zorunlu");
                  return;
                }
                createMut.mutate();
              }}
            >
              Kaydet
            </PageAction>
          ) : (
            <PageAction
              onClick={() => {
                if (!editForm.email) {
                  toast.error("Email zorunlu");
                  return;
                }
                updateMut.mutate();
              }}
            >
              Kaydet
            </PageAction>
          )}
        </div>
      </div>
    </aside>
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
  const debouncedSearch = useDebounce(search, 300);

  const [modalMode, setModalMode] = useState<"delete" | "deactivate" | "reactivate" | null>(null);
  const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

  const panelMode = (searchParams.get("mode") as PanelMode) ?? null;
  const panelUserId = searchParams.get("id") ? Number(searchParams.get("id")) : null;

  const { data, isLoading } = useQuery({
    queryKey: ["users", page, pageSize, debouncedSearch, sortBy, sortDir, isActiveFilter],
    queryFn: () => usersApi.list({
      page,
      pageSize,
      search: debouncedSearch || undefined,
      sortBy,
      sortDirection: sortDir,
      isActive: isActiveFilter === "all" ? undefined : isActiveFilter === "active",
    }),
    enabled: canView,
    placeholderData: (prev) => prev,
  });

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

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title="Kullanici Yonetimi"
        subtitle="Feature + mode tek ekran (list, create, edit, detail)"
        actions={canCreate ? <PageAction onClick={() => openPanel("create")}><UserPlus size={16} /> Yeni Kullanici</PageAction> : undefined}
      />

      <div className="flex flex-col lg:flex-row gap-4">
        <div className="flex-1 min-w-0 flex flex-col gap-3">
          <FilterBar
            search={{ value: search, onChange: setSearch, placeholder: "Kod veya e-posta ara..." }}
            filters={[{
              key: "isActive",
              label: "Durum",
              type: "boolean",
              value: isActiveFilter === "active" ? true : isActiveFilter === "inactive" ? false : "",
              onChange: (v) => { setIsActiveFilter(v === true ? "active" : v === false ? "inactive" : "all"); setPage(1); },
            }]}
            onReset={() => { setSearch(""); setIsActiveFilter("all"); setPage(1); }}
            activeCount={(search ? 1 : 0) + (isActiveFilter !== "all" ? 1 : 0)}
          />

          <DataGrid
            columns={columns}
            data={data?.items ?? []}
            isLoading={isLoading}
            totalCount={data?.totalCount}
            pagination={{ page, pageSize, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1); } }}
            sorting={{ sortBy: sortBy ?? "", sortDir, onSort: handleSort }}
            onRowClick={(row) => openPanel("detail", row.id)}
            emptyMessage="Kullanici bulunamadi"
          />
        </div>

        <UserPanel
          mode={panelMode}
          userId={panelUserId}
          canCreate={canCreate}
          canUpdate={canUpdate}
          onClose={() => openPanel(null)}
          onSaved={() => openPanel(null)}
        />
      </div>

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
