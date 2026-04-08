import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Shield, ShieldAlert, ShieldCheck, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { PageAction, PageHeader } from "@/components/ui/PageHeader";
import { FilterBar } from "@/components/ui/FilterBar";
import { DataGrid } from "@/components/ui/DataGrid";
import { CrudModal } from "@/components/ui/CrudModal";
import { permissionsApi } from "./api";
import { getPermissionColumns } from "./columns";
import type { PermissionFormState, UserActionPermission } from "./types";

type SortDir = "asc" | "desc";

const mono = "'JetBrains Mono', monospace";

const emptyForm: PermissionFormState = {
  userId: "",
  subModulePageId: "",
  transactionCode: "",
  actionCode: "",
  isAllowed: true,
};

function isPositiveInteger(value: string) {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0;
}

function extractError(err: unknown) {
  return (err as { response?: { data?: { detail?: string; message?: string; title?: string } } }).response?.data?.detail
    ?? (err as { response?: { data?: { message?: string; title?: string } } }).response?.data?.message
    ?? (err as { response?: { data?: { title?: string } } }).response?.data?.title
    ?? "İşlem başarısız";
}

export default function PermissionsListPage() {
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [sortBy, setSortBy] = useState("transactionCode");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const [search, setSearch] = useState("");
  const [userIdInput, setUserIdInput] = useState("");
  const [appliedUserId, setAppliedUserId] = useState("");
  const [subModulePageIdInput, setSubModulePageIdInput] = useState("");
  const [appliedSubModulePageId, setAppliedSubModulePageId] = useState("");
  const [transactionCode, setTransactionCode] = useState("");
  const [isAllowedFilter, setIsAllowedFilter] = useState<string>("");

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingPermission, setEditingPermission] = useState<UserActionPermission | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<UserActionPermission | null>(null);
  const [form, setForm] = useState<PermissionFormState>(emptyForm);
  const [formErrors, setFormErrors] = useState<Partial<Record<keyof PermissionFormState, string>>>({});

  const numericUserId = appliedUserId ? Number(appliedUserId) : 0;
  const numericPageId = appliedSubModulePageId ? Number(appliedSubModulePageId) : undefined;

  const permissionsQuery = useQuery({
    queryKey: ["permissions-actions", numericUserId, numericPageId, transactionCode.trim().toUpperCase()],
    queryFn: () => permissionsApi.list({
      userId: numericUserId,
      subModulePageId: numericPageId,
      transactionCode: transactionCode.trim() ? transactionCode.trim().toUpperCase() : undefined,
    }),
    enabled: numericUserId > 0,
  });

  const upsertMutation = useMutation({
    mutationFn: permissionsApi.upsert,
    onSuccess: () => {
      toast.success(editingPermission ? "Yetki güncellendi" : "Yetki oluşturuldu");
      closeFormModal();
      queryClient.invalidateQueries({ queryKey: ["permissions-actions"] });
    },
    onError: (err) => {
      toast.error(extractError(err));
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (permissionId: number) => permissionsApi.delete(permissionId),
    onSuccess: () => {
      toast.success("Yetki silindi");
      setDeleteTarget(null);
      queryClient.invalidateQueries({ queryKey: ["permissions-actions"] });
    },
    onError: (err) => {
      toast.error(extractError(err));
    },
  });

  const allPermissions = permissionsQuery.data ?? [];

  const filteredPermissions = useMemo(() => {
    const q = search.trim().toLowerCase();

    return allPermissions.filter((permission) => {
      if (isAllowedFilter === "true" && !permission.isAllowed) {
        return false;
      }

      if (isAllowedFilter === "false" && permission.isAllowed) {
        return false;
      }

      if (!q) {
        return true;
      }

      return [
        permission.id,
        permission.userId,
        permission.subModulePageId,
        permission.transactionCode,
        permission.actionCode,
        permission.isAllowed ? "izinli" : "engelli",
      ].some((value) => String(value).toLowerCase().includes(q));
    });
  }, [allPermissions, isAllowedFilter, search]);

  const sortedPermissions = useMemo(() => {
    const sorted = [...filteredPermissions];
    sorted.sort((a, b) => {
      const aValue = sortValue(a, sortBy);
      const bValue = sortValue(b, sortBy);

      if (aValue < bValue) {
        return sortDir === "asc" ? -1 : 1;
      }

      if (aValue > bValue) {
        return sortDir === "asc" ? 1 : -1;
      }

      return 0;
    });

    return sorted;
  }, [filteredPermissions, sortBy, sortDir]);

  const pagedPermissions = useMemo(() => {
    const start = (page - 1) * pageSize;
    return sortedPermissions.slice(start, start + pageSize);
  }, [page, pageSize, sortedPermissions]);

  const stats = useMemo(() => {
    const total = allPermissions.length;
    const allowed = allPermissions.filter((item) => item.isAllowed).length;
    return { total, allowed, blocked: total - allowed };
  }, [allPermissions]);

  const columns = useMemo(
    () => getPermissionColumns({
      onEdit: (permission) => {
        setEditingPermission(permission);
        setForm({
          userId: String(permission.userId),
          subModulePageId: String(permission.subModulePageId),
          transactionCode: permission.transactionCode,
          actionCode: permission.actionCode,
          isAllowed: permission.isAllowed,
        });
        setFormErrors({});
      },
      onDelete: (permission) => setDeleteTarget(permission),
    }),
    [],
  );

  function handleSort(column: string) {
    if (sortBy === column) {
      setSortDir((prev) => (prev === "asc" ? "desc" : "asc"));
    } else {
      setSortBy(column);
      setSortDir("asc");
    }
    setPage(1);
  }

  function applyServerFilters() {
    if (!userIdInput.trim() || !isPositiveInteger(userIdInput.trim())) {
      toast.error("Geçerli bir kullanıcı ID zorunludur");
      return;
    }

    if (subModulePageIdInput.trim() && !isPositiveInteger(subModulePageIdInput.trim())) {
      toast.error("Sayfa ID pozitif bir sayı olmalıdır");
      return;
    }

    setAppliedUserId(userIdInput.trim());
    setAppliedSubModulePageId(subModulePageIdInput.trim());
    setPage(1);
  }

  function validateForm() {
    const errors: Partial<Record<keyof PermissionFormState, string>> = {};

    if (!form.userId.trim() || !isPositiveInteger(form.userId.trim())) {
      errors.userId = "Geçerli kullanıcı ID zorunludur.";
    }

    if (!form.actionCode.trim()) {
      errors.actionCode = "Action code zorunludur.";
    }

    if (form.subModulePageId.trim() && !isPositiveInteger(form.subModulePageId.trim())) {
      errors.subModulePageId = "Sayfa ID pozitif bir sayı olmalıdır.";
    }

    if (!form.subModulePageId.trim() && !form.transactionCode.trim()) {
      errors.subModulePageId = "SubModulePageId veya TransactionCode zorunludur.";
      errors.transactionCode = "SubModulePageId veya TransactionCode zorunludur.";
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  }

  function closeFormModal() {
    if (upsertMutation.isPending) {
      return;
    }

    setIsCreateOpen(false);
    setEditingPermission(null);
    setForm(emptyForm);
    setFormErrors({});
  }

  function submitUpsert() {
    if (!validateForm()) {
      return;
    }

    upsertMutation.mutate({
      userId: Number(form.userId),
      subModulePageId: form.subModulePageId.trim() ? Number(form.subModulePageId) : undefined,
      transactionCode: form.transactionCode.trim() ? form.transactionCode.trim().toUpperCase() : undefined,
      actionCode: form.actionCode.trim().toUpperCase(),
      isAllowed: form.isAllowed,
    });
  }

  const activeCount =
    (search ? 1 : 0) +
    (transactionCode ? 1 : 0) +
    (isAllowedFilter !== "" ? 1 : 0) +
    (appliedSubModulePageId ? 1 : 0);

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title="Yetki Yönetimi"
        subtitle="Kullanıcı action izinleri"
        actions={<PageAction onClick={() => {
          setEditingPermission(null);
          setForm({
            ...emptyForm,
            userId: appliedUserId,
            subModulePageId: appliedSubModulePageId,
            transactionCode: transactionCode.trim().toUpperCase(),
          });
          setFormErrors({});
          setIsCreateOpen(true);
        }}><Plus size={16} /> Yeni Yetki</PageAction>}
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div className="rounded-[10px] px-4 py-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderTop: "3px solid #2E6DA4" }}>
          <div className="flex items-center justify-between">
            <span className="text-[22px] font-bold" style={{ color: "#1B3A5C" }}>{stats.total}</span>
            <Shield size={18} style={{ color: "#2E6DA4", opacity: 0.6 }} />
          </div>
          <span className="text-[11px]" style={{ color: "#7A96B0" }}>Yüklenen Yetki</span>
        </div>
        <div className="rounded-[10px] px-4 py-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderTop: "3px solid #1E8A6E" }}>
          <div className="flex items-center justify-between">
            <span className="text-[22px] font-bold" style={{ color: "#1B3A5C" }}>{stats.allowed}</span>
            <ShieldCheck size={18} style={{ color: "#1E8A6E", opacity: 0.6 }} />
          </div>
          <span className="text-[11px]" style={{ color: "#7A96B0" }}>İzinli</span>
        </div>
        <div className="rounded-[10px] px-4 py-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderTop: "3px solid #C0392B" }}>
          <div className="flex items-center justify-between">
            <span className="text-[22px] font-bold" style={{ color: "#1B3A5C" }}>{stats.blocked}</span>
            <ShieldAlert size={18} style={{ color: "#C0392B", opacity: 0.6 }} />
          </div>
          <span className="text-[11px]" style={{ color: "#7A96B0" }}>Engelli</span>
        </div>
      </div>

      <div className="rounded-[10px] p-4 flex flex-col gap-3" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}>
        <div className="grid grid-cols-1 md:grid-cols-[180px_180px_1fr_auto] gap-2">
          <label className="flex flex-col gap-1">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Kullanıcı ID *</span>
            <input
              value={userIdInput}
              onChange={(e) => setUserIdInput(e.target.value)}
              placeholder="Örn: 42"
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: "1px solid #D6E4F0", color: "#1B3A5C", fontFamily: mono }}
            />
          </label>
          <label className="flex flex-col gap-1">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Page ID</span>
            <input
              value={subModulePageIdInput}
              onChange={(e) => setSubModulePageIdInput(e.target.value)}
              placeholder="Opsiyonel"
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: "1px solid #D6E4F0", color: "#1B3A5C", fontFamily: mono }}
            />
          </label>
          <label className="flex flex-col gap-1">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Server-side TCode</span>
            <input
              value={transactionCode}
              onChange={(e) => {
                setTransactionCode(e.target.value);
                setPage(1);
              }}
              placeholder="Örn: SYS01"
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: "1px solid #D6E4F0", color: "#1B3A5C", fontFamily: mono }}
            />
          </label>
          <div className="flex items-end">
            <button
              onClick={applyServerFilters}
              className="flex items-center justify-center gap-1.5 rounded-md px-3 py-2 text-[12px] font-medium transition-all w-full min-h-[40px]"
              style={{ background: "#1B3A5C", color: "#FFFFFF", fontFamily: mono }}
            >
              Listele
            </button>
          </div>
        </div>

        <FilterBar
          search={{
            value: search,
            onChange: (value) => {
              setSearch(value);
              setPage(1);
            },
            placeholder: "ID, TCode, action veya durum ara...",
          }}
          filters={[
            {
              key: "isAllowed",
              label: "Durum",
              type: "select",
              value: isAllowedFilter,
              options: [
                { value: "true", label: "İzinli" },
                { value: "false", label: "Engelli" },
              ],
              onChange: (value) => {
                setIsAllowedFilter(String(value));
                setPage(1);
              },
            },
          ]}
          onReset={() => {
            setSearch("");
            setTransactionCode("");
            setSubModulePageIdInput("");
            setAppliedSubModulePageId("");
            setIsAllowedFilter("");
            setPage(1);
          }}
          activeCount={activeCount}
        />
      </div>

      {!appliedUserId && (
        <div className="rounded-[10px] px-4 py-3" style={{ background: "#F7FAFD", border: "1px solid #E2EBF3", color: "#7A96B0" }}>
          <span className="text-[13px]">Listeleme için önce bir kullanıcı ID girip filtreyi uygulayın.</span>
        </div>
      )}

      {permissionsQuery.isError && (
        <div className="rounded-[10px] px-4 py-3 flex items-center justify-between"
          style={{ background: "#FDECEA", border: "1px solid #F5C6C2", color: "#C0392B" }}>
          <span className="text-[13px]">Yetkiler yüklenemedi: {extractError(permissionsQuery.error)}</span>
          <button
            className="rounded-md px-3 py-1.5 text-[12px]"
            style={{ border: "1px solid #F5C6C2", background: "#FFFFFF", color: "#C0392B" }}
            onClick={() => permissionsQuery.refetch()}
          >
            Tekrar Dene
          </button>
        </div>
      )}

      <DataGrid
        columns={columns}
        data={pagedPermissions}
        isLoading={permissionsQuery.isLoading}
        totalCount={sortedPermissions.length}
        pagination={{
          page,
          pageSize,
          onPageChange: setPage,
          onPageSizeChange: (size) => {
            setPageSize(size);
            setPage(1);
          },
        }}
        sorting={{ sortBy, sortDir, onSort: handleSort }}
        emptyMessage={appliedUserId ? "Yetki bulunamadı" : "Listeleme için kullanıcı ID girin"}
      />

      <CrudModal
        mode={editingPermission ? "edit" : "create"}
        title={editingPermission ? "Yetki Düzenle" : "Yeni Yetki"}
        isOpen={isCreateOpen || Boolean(editingPermission)}
        onClose={closeFormModal}
        onSubmit={submitUpsert}
        isLoading={upsertMutation.isPending}
      >
        <div className="flex flex-col gap-3">
          <label className="flex flex-col gap-1.5">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Kullanıcı ID *</span>
            <input
              type="number"
              value={form.userId}
              onChange={(e) => {
                setForm((prev) => ({ ...prev, userId: e.target.value }));
                if (formErrors.userId) setFormErrors((prev) => ({ ...prev, userId: undefined }));
              }}
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: `1px solid ${formErrors.userId ? "#F5C6C2" : "#D6E4F0"}`, color: "#1B3A5C" }}
            />
            {formErrors.userId && <span className="text-[11px]" style={{ color: "#C0392B" }}>{formErrors.userId}</span>}
          </label>

          <label className="flex flex-col gap-1.5">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>SubModule Page ID</span>
            <input
              type="number"
              value={form.subModulePageId}
              onChange={(e) => {
                setForm((prev) => ({ ...prev, subModulePageId: e.target.value }));
                if (formErrors.subModulePageId) setFormErrors((prev) => ({ ...prev, subModulePageId: undefined }));
              }}
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: `1px solid ${formErrors.subModulePageId ? "#F5C6C2" : "#D6E4F0"}`, color: "#1B3A5C" }}
            />
            {formErrors.subModulePageId && <span className="text-[11px]" style={{ color: "#C0392B" }}>{formErrors.subModulePageId}</span>}
          </label>

          <label className="flex flex-col gap-1.5">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Transaction Code</span>
            <input
              type="text"
              value={form.transactionCode}
              onChange={(e) => {
                setForm((prev) => ({ ...prev, transactionCode: e.target.value.toUpperCase() }));
                if (formErrors.transactionCode) setFormErrors((prev) => ({ ...prev, transactionCode: undefined }));
              }}
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: `1px solid ${formErrors.transactionCode ? "#F5C6C2" : "#D6E4F0"}`, color: "#1B3A5C", fontFamily: mono }}
            />
            {formErrors.transactionCode && <span className="text-[11px]" style={{ color: "#C0392B" }}>{formErrors.transactionCode}</span>}
          </label>

          <label className="flex flex-col gap-1.5">
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>Action Code *</span>
            <input
              type="text"
              value={form.actionCode}
              onChange={(e) => {
                setForm((prev) => ({ ...prev, actionCode: e.target.value.toUpperCase() }));
                if (formErrors.actionCode) setFormErrors((prev) => ({ ...prev, actionCode: undefined }));
              }}
              className="w-full rounded-lg px-3 py-2 text-[13px] outline-none"
              style={{ background: "#F7FAFD", border: `1px solid ${formErrors.actionCode ? "#F5C6C2" : "#D6E4F0"}`, color: "#1B3A5C", fontFamily: mono }}
            />
            {formErrors.actionCode && <span className="text-[11px]" style={{ color: "#C0392B" }}>{formErrors.actionCode}</span>}
          </label>

          <label className="flex items-center justify-between rounded-lg px-3 py-2" style={{ background: "#F7FAFD", border: "1px solid #D6E4F0" }}>
            <span className="text-[12px]" style={{ color: "#2C4A6B" }}>İzin Durumu</span>
            <button
              type="button"
              role="switch"
              aria-checked={form.isAllowed}
              onClick={() => setForm((prev) => ({ ...prev, isAllowed: !prev.isAllowed }))}
              className="relative h-5 w-9 rounded-full transition-colors"
              style={{ background: form.isAllowed ? "#1E8A6E" : "#CBD5E1" }}
            >
              <span
                className="absolute top-0.5 left-0.5 h-4 w-4 rounded-full bg-white transition-transform shadow-sm"
                style={{ transform: form.isAllowed ? "translateX(16px)" : "translateX(0)" }}
              />
            </button>
          </label>
        </div>
      </CrudModal>

      <CrudModal
        mode="delete"
        title="Yetki Sil"
        isOpen={Boolean(deleteTarget)}
        onClose={() => {
          if (!deleteMutation.isPending) {
            setDeleteTarget(null);
          }
        }}
        onSubmit={() => {
          if (deleteTarget?.id) {
            deleteMutation.mutate(deleteTarget.id);
          }
        }}
        isLoading={deleteMutation.isPending}
      >
        <div className="flex flex-col gap-2">
          <p style={{ color: "#2C4A6B", fontSize: 14 }}>
            <strong>{deleteTarget?.transactionCode}</strong> / <strong>{deleteTarget?.actionCode}</strong> yetkisi silinecek.
          </p>
          <p style={{ color: "#7A96B0", fontSize: 12 }}>
            Kullanıcı ID: {deleteTarget?.userId} • Page ID: {deleteTarget?.subModulePageId}
          </p>
          <p style={{ color: "#C0392B", fontSize: 12 }} className="flex items-center gap-1">
            <Trash2 size={12} /> Bu işlem geri alınamaz.
          </p>
        </div>
      </CrudModal>
    </div>
  );
}

function sortValue(permission: UserActionPermission, sortBy: string) {
  switch (sortBy) {
    case "id":
      return permission.id;
    case "userId":
      return permission.userId;
    case "transactionCode":
      return permission.transactionCode.toLowerCase();
    case "actionCode":
      return permission.actionCode.toLowerCase();
    case "subModulePageId":
      return permission.subModulePageId;
    case "isAllowed":
      return permission.isAllowed ? 1 : 0;
    case "modifiedAt":
      return permission.modifiedAt ?? permission.createdAt;
    default:
      return permission.transactionCode.toLowerCase();
  }
}
