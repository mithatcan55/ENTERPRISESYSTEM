import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { ArrowLeft, Check, ShieldCheck } from "lucide-react";
import { PageAction, PageHeader } from "@/components/ui/PageHeader";
import { usersApi } from "@/modules/users/api";
import { extractApiError } from "@/lib/api-error";

export default function AssignUserPermissionPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [selectedPermissionIds, setSelectedPermissionIds] = useState<number[]>([]);
  const [permissionSearch, setPermissionSearch] = useState("");

  const usersQuery = useQuery({
    queryKey: ["users", "permission-assign-picker"],
    queryFn: () => usersApi.list({ page: 1, pageSize: 200, includeDeleted: false }),
  });

  const lookupsQuery = useQuery({
    queryKey: ["user-lookups"],
    queryFn: usersApi.lookups,
  });

  const detailQuery = useQuery({
    queryKey: ["users", "detail", selectedUserId],
    queryFn: () => usersApi.getById(selectedUserId as number),
    enabled: selectedUserId !== null,
  });

  useEffect(() => {
    if (!detailQuery.data) {
      setSelectedPermissionIds([]);
      return;
    }

    setSelectedPermissionIds((detailQuery.data.directPermissions ?? []).map((p) => p.subModulePageId));
  }, [detailQuery.data]);

  const groupedPermissions = useMemo(() => {
    const permissions = lookupsQuery.data?.permissions ?? [];
    return permissions.reduce<Record<string, typeof permissions>>((acc, item) => {
      const key = item.transactionCode || "UNKNOWN";
      if (!acc[key]) acc[key] = [];
      acc[key].push(item);
      return acc;
    }, {});
  }, [lookupsQuery.data?.permissions]);

  const saveMutation = useMutation({
    mutationFn: async () => {
      if (!selectedUserId || !detailQuery.data) {
        throw new Error("Kullanici secilmedi");
      }

      await usersApi.update(selectedUserId, {
        firstName: detailQuery.data.firstName,
        lastName: detailQuery.data.lastName,
        email: detailQuery.data.email,
        isActive: detailQuery.data.isActive,
        mustChangePassword: detailQuery.data.mustChangePassword,
        roleIds: (detailQuery.data.roles ?? []).map((r) => r.roleId),
        permissionIds: selectedPermissionIds,
      });
    },
    onSuccess: () => {
      toast.success("Kullanici yetkileri kaydedildi");
      queryClient.invalidateQueries({ queryKey: ["users", "detail", selectedUserId] });
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
    onError: (err) => {
      toast.error(extractApiError(err));
    },
  });

  function togglePermission(permissionId: number) {
    setSelectedPermissionIds((prev) => (
      prev.includes(permissionId)
        ? prev.filter((x) => x !== permissionId)
        : [...prev, permissionId]
    ));
  }

  const users = usersQuery.data?.items ?? [];
  const normalizedSearch = permissionSearch.trim().toLowerCase();

  return (
    <div className="flex flex-col gap-4 max-w-[980px]">
      <PageHeader
        title="Kullaniciya Yetki Ata"
        subtitle="Kullanici secerek direkt yetkilerini yonet"
        actions={
          <div className="flex gap-2">
            <PageAction variant="ghost" onClick={() => navigate("/permissions")}>
              <ArrowLeft size={14} /> Yetkilere Don
            </PageAction>
            <PageAction onClick={() => {
              if (!selectedUserId || saveMutation.isPending || detailQuery.isLoading) return;
              saveMutation.mutate();
            }}>
              <Check size={14} /> Kaydet
            </PageAction>
          </div>
        }
      />

      <div className="rounded-xl p-4" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
        <label className="text-[12px] font-medium ui-text">Kullanici Sec</label>
        <select
          className="mt-1 w-full rounded-lg h-[40px] px-3 text-[13px] outline-none"
          style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)", color: "var(--ui-text)" }}
          value={selectedUserId ?? ""}
          onChange={(e) => setSelectedUserId(e.target.value ? Number(e.target.value) : null)}
        >
          <option value="">Kullanici seciniz</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>
              {u.userCode} - {u.displayName}
            </option>
          ))}
        </select>
        {usersQuery.isLoading && <p className="mt-2 text-[12px] ui-text-muted">Kullanicilar yukleniyor...</p>}
        {usersQuery.isError && <p className="mt-2 text-[12px]" style={{ color: "var(--ui-danger)" }}>Kullanicilar alinamadi: {extractApiError(usersQuery.error)}</p>}
      </div>

      {selectedUserId && (
        <div className="rounded-xl p-4" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
          <div className="flex items-center gap-2 mb-3">
            <ShieldCheck size={16} style={{ color: "var(--ui-primary)" }} />
            <h3 className="text-[14px] font-semibold ui-text">Yetki Atama</h3>
          </div>

          <input
            value={permissionSearch}
            onChange={(e) => setPermissionSearch(e.target.value)}
            placeholder="TCode, action veya yetki adinda ara..."
            className="mb-3 w-full rounded-lg h-[38px] px-3 text-[13px] outline-none"
            style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)", color: "var(--ui-text)" }}
          />

          <div className="flex flex-col gap-3">
            {Object.entries(groupedPermissions).map(([tCode, perms]) => {
              const filteredPerms = perms.filter((perm) => {
                if (!normalizedSearch) return true;
                return `${perm.transactionCode} ${perm.actionCode} ${perm.displayName} ${perm.permissionCode} ${perm.storedKey} ${perm.id}`.toLowerCase().includes(normalizedSearch);
              });

              if (filteredPerms.length === 0) return null;

              return (
              <div key={tCode} className="rounded-lg p-3" style={{ border: "1px solid var(--ui-border)", background: "var(--ui-surface-alt)" }}>
                <div className="text-[12px] font-semibold mb-2" style={{ color: "var(--ui-primary)" }}>{tCode}</div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {filteredPerms.map((perm) => {
                    const assigned = selectedPermissionIds.includes(perm.id);
                    return (
                      <button
                        key={perm.id}
                        type="button"
                        onClick={() => togglePermission(perm.id)}
                        className="w-full rounded-lg px-3 py-2 text-left border transition-colors"
                        style={{
                          borderColor: assigned
                            ? "color-mix(in srgb, var(--ui-primary) 30%, transparent)"
                            : "var(--ui-border)",
                          background: assigned
                            ? "color-mix(in srgb, var(--ui-primary) 12%, transparent)"
                            : "var(--ui-card-bg)",
                          color: assigned ? "var(--ui-primary)" : "var(--ui-text)",
                        }}
                      >
                        <div className="text-[13px] font-medium">{perm.permissionCode || perm.displayName}</div>
                        <div className="text-[11px]" style={{ color: "var(--ui-text-muted)" }}>
                          Nav: {perm.navigationCode} · Key: {perm.storedKey} · ID: {perm.id}
                        </div>
                      </button>
                    );
                  })}
                </div>
              </div>
              );
            })}
          </div>

          {lookupsQuery.isLoading && <p className="text-[12px] ui-text-muted">Yetki listesi yukleniyor...</p>}
          {lookupsQuery.isError && <p className="text-[12px]" style={{ color: "var(--ui-danger)" }}>Yetki listesi alinamadi: {extractApiError(lookupsQuery.error)}</p>}
        </div>
      )}

      {selectedUserId && detailQuery.isError && (
        <div className="rounded-lg px-3 py-2 text-[12px]" style={{ background: "var(--ui-danger-bg)", color: "var(--ui-danger)" }}>
          Kullanici detaylari alinamadi: {extractApiError(detailQuery.error)}
        </div>
      )}
    </div>
  );
}
