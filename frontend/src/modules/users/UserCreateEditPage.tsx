import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import apiClient from "@/api/client";
import { usersApi } from "./api";
import type { UserDetail } from "./api";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { ArrowLeft, Save, Loader2, Check } from "lucide-react";

/* ─── Types ─── */

interface LookupItem { id: number; name: string; }
interface LookupsResponse { roles: LookupItem[]; permissions: LookupItem[]; }

interface UserFormData {
  userCode: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  isActive: boolean;
  companyId: number;
  roleIds: number[];
}

/* ─── Styles ─── */

const inputCls =
  "w-full rounded-lg h-[42px] px-3 text-[14px] outline-none transition-all bg-[#FAFCFF] border-[1.5px] border-[#E2EBF3] text-[#1B3A5C] placeholder:text-[#B0BEC5] focus:border-[#5B9BD5] focus:ring-2 focus:ring-[#5B9BD5]/12";

const labelCls = "block mb-1.5 text-[13px] font-medium";

function Label({ children, required }: { children: React.ReactNode; required?: boolean }) {
  return (
    <label className={labelCls} style={{ color: "#2C4A6B" }}>
      {children} {required && <span style={{ color: "#E05252" }}>*</span>}
    </label>
  );
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return <p className="mt-1 text-[12px]" style={{ color: "#E05252" }}>{message}</p>;
}

/* ─── Main Component ─── */

export default function UserCreateEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEdit = !!id;

  // Form state
  const [form, setForm] = useState<UserFormData>({
    userCode: "", firstName: "", lastName: "", email: "",
    password: "", isActive: true, companyId: 1, roleIds: [],
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [saving, setSaving] = useState(false);

  // Fetch lookups
  const { data: lookups, isLoading: lookupsLoading } = useQuery({
    queryKey: ["user-lookups"],
    queryFn: () => apiClient.get<LookupsResponse>("/api/users/lookups").then((r) => r.data),
    staleTime: 60_000,
  });

  // Fetch user for edit mode
  const { data: user, isLoading: userLoading } = useQuery({
    queryKey: ["users", id],
    queryFn: () => usersApi.getById(Number(id)),
    enabled: isEdit,
  });

  // Fetch user roles for edit
  const { data: userRoles } = useQuery({
    queryKey: ["user-roles-edit", id],
    queryFn: () => apiClient.get<{ roleId: number }[]>(`/api/roles/users/${id}`).then((r) => r.data),
    enabled: isEdit,
  });

  // Prefill form in edit mode
  useEffect(() => {
    if (!isEdit || !user) return;
    const u = user as UserDetail & { firstName?: string; lastName?: string };
    setForm({
      userCode: user.userCode,
      firstName: u.firstName ?? "",
      lastName: u.lastName ?? "",
      email: user.email,
      password: "",
      isActive: user.isActive,
      companyId: 1,
      roleIds: [],
    });
  }, [isEdit, user]);

  // Prefill roles
  useEffect(() => {
    if (!userRoles) return;
    const ids = userRoles.map((r: { roleId?: number; id?: number }) => r.roleId ?? r.id ?? 0).filter(Boolean);
    setForm((prev) => ({ ...prev, roleIds: ids }));
  }, [userRoles]);

  // Field change
  function setField<K extends keyof UserFormData>(key: K, value: UserFormData[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
    setErrors((prev) => { const n = { ...prev }; delete n[key]; return n; });
  }

  // Toggle role
  function toggleRole(roleId: number) {
    setForm((prev) => ({
      ...prev,
      roleIds: prev.roleIds.includes(roleId)
        ? prev.roleIds.filter((id) => id !== roleId)
        : [...prev.roleIds, roleId],
    }));
  }

  // Validation
  function validate(): boolean {
    const errs: Record<string, string> = {};
    if (!form.userCode.trim()) errs.userCode = "Kullanıcı kodu zorunlu";
    if (!form.email.trim()) errs.email = "E-posta zorunlu";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errs.email = "Geçerli e-posta giriniz";
    if (!isEdit && !form.password) errs.password = "Şifre zorunlu";
    if (!isEdit && form.password && form.password.length < 6) errs.password = "En az 6 karakter";
    setErrors(errs);
    return Object.keys(errs).length === 0;
  }

  // Submit
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    setSaving(true);

    try {
      if (isEdit) {
        await apiClient.put(`/api/users/${id}`, {
          firstName: form.firstName || null,
          lastName: form.lastName || null,
          email: form.email,
          isActive: form.isActive,
          mustChangePassword: false,
          profileImageUrl: null,
          roleIds: form.roleIds,
        });
        toast.success("Kullanıcı güncellendi");
        queryClient.invalidateQueries({ queryKey: ["users"] });
        navigate(`/users/${id}`);
      } else {
        const { data } = await apiClient.post("/api/users", {
          userCode: form.userCode.trim().toUpperCase(),
          username: form.userCode.trim().toLowerCase(),
          firstName: form.firstName || null,
          lastName: form.lastName || null,
          email: form.email,
          password: form.password,
          companyId: form.companyId,
          notifyAdminByMail: false,
          roleIds: form.roleIds,
        });
        toast.success("Kullanıcı oluşturuldu");
        queryClient.invalidateQueries({ queryKey: ["users"] });
        navigate(`/users/${data.id ?? ""}`);
      }
    } catch (err: unknown) {
      const detail = (err as { response?: { data?: { detail?: string; title?: string } } }).response?.data;
      toast.error(detail?.detail ?? detail?.title ?? "İşlem başarısız");
    } finally {
      setSaving(false);
    }
  }

  if (lookupsLoading || (isEdit && userLoading)) {
    return <div className="flex items-center gap-2 py-12 justify-center" style={{ color: "#7A96B0" }}><Loader2 size={18} className="animate-spin" /> Yükleniyor...</div>;
  }

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title={isEdit ? "Kullanıcı Düzenle" : "Yeni Kullanıcı"}
        subtitle={isEdit ? `${user?.userCode ?? ""}` : "Yeni kullanıcı oluştur"}
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate("/users")}><ArrowLeft size={14} /> Geri</PageAction>
            <PageAction onClick={() => (document.getElementById("user-form") as HTMLFormElement)?.requestSubmit()}>
              <Save size={14} /> {saving ? "Kaydediliyor..." : "Kaydet"}
            </PageAction>
          </div>
        }
      />

      <form id="user-form" onSubmit={handleSubmit}>
        {/* Basic Info Card */}
        <div className="rounded-xl p-6 mb-4" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
          <h3 className="text-[14px] font-semibold mb-4 pb-3" style={{ color: "#1B3A5C", borderBottom: "1px solid #F0F4F8" }}>
            Temel Bilgiler
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
            <div>
              <Label required>Kullanıcı Kodu</Label>
              <input className={inputCls} value={form.userCode} disabled={isEdit}
                style={isEdit ? { background: "#F0F4F8", color: "#7A96B0" } : undefined}
                placeholder="USR001" onChange={(e) => setField("userCode", e.target.value.toUpperCase())} />
              <FieldError message={errors.userCode} />
            </div>
            <div>
              <Label>Ad</Label>
              <input className={inputCls} value={form.firstName} placeholder="Mithat"
                onChange={(e) => setField("firstName", e.target.value)} />
            </div>
            <div>
              <Label>Soyad</Label>
              <input className={inputCls} value={form.lastName} placeholder="Can"
                onChange={(e) => setField("lastName", e.target.value)} />
            </div>
            <div>
              <Label required>E-posta</Label>
              <input className={inputCls} type="email" value={form.email} placeholder="user@firma.com"
                onChange={(e) => setField("email", e.target.value)} />
              <FieldError message={errors.email} />
            </div>
            {!isEdit && (
              <div>
                <Label required>Şifre</Label>
                <input className={inputCls} type="password" value={form.password} placeholder="••••••••"
                  onChange={(e) => setField("password", e.target.value)} />
                <FieldError message={errors.password} />
              </div>
            )}
            <div>
              <Label>Şirket ID</Label>
              <input className={inputCls} type="number" value={form.companyId}
                onChange={(e) => setField("companyId", Number(e.target.value) || 1)} />
            </div>
            {isEdit && (
              <div className="flex items-end pb-1">
                <label className="flex items-center gap-2.5 cursor-pointer select-none">
                  <button type="button" role="switch" aria-checked={form.isActive}
                    onClick={() => setField("isActive", !form.isActive)}
                    className="relative h-5 w-9 rounded-full transition-colors"
                    style={{ background: form.isActive ? "#1E8A6E" : "#D6E4F0" }}>
                    <span className="absolute top-0.5 left-0.5 h-4 w-4 rounded-full bg-white transition-transform shadow-sm"
                      style={{ transform: form.isActive ? "translateX(16px)" : "translateX(0)" }} />
                  </button>
                  <span className="text-[13px] font-medium" style={{ color: "#2C4A6B" }}>Aktif</span>
                </label>
              </div>
            )}
          </div>
        </div>

        {/* Roles Card */}
        <div className="rounded-xl p-6 mb-4" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
          <h3 className="text-[14px] font-semibold mb-4 pb-3" style={{ color: "#1B3A5C", borderBottom: "1px solid #F0F4F8" }}>
            Roller
            {form.roleIds.length > 0 && (
              <span className="ml-2 text-[11px] px-2 py-0.5 rounded-full" style={{ background: "#EAF1FA", color: "#2E6DA4" }}>
                {form.roleIds.length} seçili
              </span>
            )}
          </h3>
          {!lookups?.roles?.length ? (
            <p className="text-[13px]" style={{ color: "#7A96B0" }}>Tanımlı rol bulunamadı.</p>
          ) : (
            <div className="flex flex-wrap gap-2">
              {lookups.roles.map((role) => {
                const selected = form.roleIds.includes(role.id);
                return (
                  <button key={role.id} type="button" onClick={() => toggleRole(role.id)}
                    className="flex items-center gap-1.5 rounded-lg px-3 py-2 text-[13px] font-medium transition-all"
                    style={selected
                      ? { background: "#EAF1FA", border: "1.5px solid #5B9BD5", color: "#2E6DA4" }
                      : { background: "#FAFCFF", border: "1.5px solid #E2EBF3", color: "#7A96B0" }
                    }>
                    {selected && <Check size={14} />}
                    {role.name}
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {/* Submit button (mobile) */}
        <div className="sm:hidden">
          <button type="submit" disabled={saving}
            className="w-full flex items-center justify-center gap-2 rounded-lg h-[46px] text-[14px] font-medium transition-colors disabled:opacity-60"
            style={{ background: "#1B3A5C", color: "#fff" }}>
            {saving ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </button>
        </div>
      </form>
    </div>
  );
}
