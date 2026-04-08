import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import apiClient from "@/api/client";
import { usersApi } from "./api";
import type { UserDetail } from "./api";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import {
  ArrowLeft, Save, Loader2, Check, UserPlus, Pencil,
  Mail, KeyRound, Building2, Shield, Users,
} from "lucide-react";

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

/* ─── Shared Styles ─── */

const cardCls = "rounded-xl border border-[#E2EBF3] bg-white";
const sectionTitleCls = "flex items-center gap-2 text-[13px] font-semibold text-[#1B3A5C] pb-3 mb-5 border-b border-[#F0F4F8]";
const inputCls =
  "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none transition-all bg-[#FAFCFF] border border-[#E2EBF3] text-[#1B3A5C] placeholder:text-[#C5CED8] focus:border-[#5B9BD5] focus:bg-white focus:shadow-[0_0_0_3px_rgba(91,155,213,0.08)]";
const inputErrorCls =
  "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none transition-all bg-[#FFFBFB] border border-[#F5C6C2] text-[#1B3A5C] placeholder:text-[#C5CED8] focus:border-[#E05252] focus:shadow-[0_0_0_3px_rgba(224,82,82,0.08)]";
const disabledInputCls =
  "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none bg-[#F5F7FA] border border-[#E8ECF1] text-[#94A3B8] cursor-not-allowed";

function Label({ children, required, htmlFor }: { children: React.ReactNode; required?: boolean; htmlFor?: string }) {
  return (
    <label htmlFor={htmlFor} className="block mb-1.5 text-[12px] font-medium text-[#4A6580]">
      {children} {required && <span className="text-[#E05252]">*</span>}
    </label>
  );
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <p className="mt-1.5 flex items-center gap-1 text-[11px] text-[#D94444]">
      <span className="inline-block w-1 h-1 rounded-full bg-[#E05252]" />
      {message}
    </p>
  );
}

function SectionIcon({ icon: Icon }: { icon: typeof Users }) {
  return (
    <span className="flex items-center justify-center w-6 h-6 rounded-md bg-[#EDF2F7]">
      <Icon size={13} className="text-[#5B9BD5]" />
    </span>
  );
}

/* ─── Main Component ─── */

export default function UserCreateEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEdit = !!id;

  const [form, setForm] = useState<UserFormData>({
    userCode: "", firstName: "", lastName: "", email: "",
    password: "", isActive: true, companyId: 1, roleIds: [],
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [saving, setSaving] = useState(false);

  const { data: lookups, isLoading: lookupsLoading } = useQuery({
    queryKey: ["user-lookups"],
    queryFn: () => apiClient.get<LookupsResponse>("/api/users/lookups").then((r) => r.data),
    staleTime: 60_000,
  });

  const { data: user, isLoading: userLoading } = useQuery({
    queryKey: ["users", id],
    queryFn: () => usersApi.getById(Number(id)),
    enabled: isEdit,
  });

  const { data: userRoles } = useQuery({
    queryKey: ["user-roles-edit", id],
    queryFn: () => apiClient.get<{ roleId: number }[]>(`/api/roles/users/${id}`).then((r) => r.data),
    enabled: isEdit,
  });

  useEffect(() => {
    if (!isEdit || !user) return;
    const u = user as UserDetail & { firstName?: string; lastName?: string };
    setForm({
      userCode: user.userCode, firstName: u.firstName ?? "", lastName: u.lastName ?? "",
      email: user.email, password: "", isActive: user.isActive, companyId: 1, roleIds: [],
    });
  }, [isEdit, user]);

  useEffect(() => {
    if (!userRoles) return;
    const ids = userRoles.map((r: { roleId?: number; id?: number }) => r.roleId ?? r.id ?? 0).filter(Boolean);
    setForm((prev) => ({ ...prev, roleIds: ids }));
  }, [userRoles]);

  function setField<K extends keyof UserFormData>(key: K, value: UserFormData[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
    setErrors((prev) => { const n = { ...prev }; delete n[key]; return n; });
  }

  function toggleRole(roleId: number) {
    setForm((prev) => ({
      ...prev,
      roleIds: prev.roleIds.includes(roleId)
        ? prev.roleIds.filter((x) => x !== roleId)
        : [...prev.roleIds, roleId],
    }));
  }

  function validate(): boolean {
    const errs: Record<string, string> = {};
    if (!form.userCode.trim()) errs.userCode = "Kullanıcı kodu zorunlu";
    if (!form.email.trim()) errs.email = "E-posta zorunlu";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errs.email = "Geçerli bir e-posta adresi giriniz";
    if (!isEdit && !form.password) errs.password = "Şifre zorunlu";
    if (!isEdit && form.password && form.password.length < 6) errs.password = "Şifre en az 6 karakter olmalıdır";
    setErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    setSaving(true);
    try {
      if (isEdit) {
        await apiClient.put(`/api/users/${id}`, {
          firstName: form.firstName || null, lastName: form.lastName || null,
          email: form.email, isActive: form.isActive, mustChangePassword: false,
          profileImageUrl: null, roleIds: form.roleIds,
        });
        toast.success("Kullanıcı güncellendi");
        queryClient.invalidateQueries({ queryKey: ["users"] });
        navigate(`/users/${id}`);
      } else {
        const { data } = await apiClient.post("/api/users", {
          userCode: form.userCode.trim().toUpperCase(),
          username: form.userCode.trim().toLowerCase(),
          firstName: form.firstName || null, lastName: form.lastName || null,
          email: form.email, password: form.password,
          companyId: form.companyId, notifyAdminByMail: false, roleIds: form.roleIds,
        });
        toast.success("Kullanıcı oluşturuldu");
        queryClient.invalidateQueries({ queryKey: ["users"] });
        navigate(`/users/${data.id ?? ""}`);
      }
    } catch (err: unknown) {
      const detail = (err as { response?: { data?: { detail?: string; title?: string } } }).response?.data;
      toast.error(detail?.detail ?? detail?.title ?? "İşlem başarısız");
    } finally { setSaving(false); }
  }

  /* ─── Loading state ─── */
  if (lookupsLoading || (isEdit && userLoading)) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-3">
        <Loader2 size={24} className="animate-spin text-[#5B9BD5]" />
        <span className="text-[13px] text-[#7A96B0]">Veriler yükleniyor...</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-5 max-w-[820px]">

      {/* ─── Page Header ─── */}
      <PageHeader
        title={isEdit ? "Kullanıcı Düzenle" : "Yeni Kullanıcı Oluştur"}
        subtitle={isEdit ? user?.userCode : "Sisteme yeni bir kullanıcı ekleyin"}
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate("/users")}>
              <ArrowLeft size={14} /> Vazgeç
            </PageAction>
            <PageAction onClick={() => (document.getElementById("user-form") as HTMLFormElement)?.requestSubmit()}>
              {saving
                ? <><Loader2 size={14} className="animate-spin" /> Kaydediliyor...</>
                : <><Save size={14} /> Kaydet</>}
            </PageAction>
          </div>
        }
      />

      <form id="user-form" onSubmit={handleSubmit} className="flex flex-col gap-5">

        {/* ═══ Section 1: Temel Bilgiler ═══ */}
        <div className={`${cardCls} p-6`}>
          <h3 className={sectionTitleCls}>
            <SectionIcon icon={isEdit ? Pencil : UserPlus} />
            Temel Bilgiler
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
            {/* Kullanıcı Kodu */}
            <div>
              <Label required htmlFor="userCode">Kullanıcı Kodu</Label>
              <input id="userCode" className={isEdit ? disabledInputCls : errors.userCode ? inputErrorCls : inputCls}
                value={form.userCode} disabled={isEdit} placeholder="USR001"
                onChange={(e) => setField("userCode", e.target.value.toUpperCase())} />
              <FieldError message={errors.userCode} />
              {!isEdit && <p className="mt-1 text-[11px] text-[#94A3B8]">Benzersiz, büyük harf. Değiştirilemez.</p>}
            </div>

            {/* E-posta */}
            <div>
              <Label required htmlFor="email">E-posta</Label>
              <div className="relative">
                <Mail size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-[#B0BEC5]" />
                <input id="email" className={(errors.email ? inputErrorCls : inputCls) + " pl-9"}
                  type="email" value={form.email} placeholder="kullanici@firma.com"
                  onChange={(e) => setField("email", e.target.value)} />
              </div>
              <FieldError message={errors.email} />
            </div>

            {/* Ad */}
            <div>
              <Label htmlFor="firstName">Ad</Label>
              <input id="firstName" className={inputCls} value={form.firstName} placeholder="Mithat"
                onChange={(e) => setField("firstName", e.target.value)} />
            </div>

            {/* Soyad */}
            <div>
              <Label htmlFor="lastName">Soyad</Label>
              <input id="lastName" className={inputCls} value={form.lastName} placeholder="Can"
                onChange={(e) => setField("lastName", e.target.value)} />
            </div>

            {/* Şifre (create only) */}
            {!isEdit && (
              <div>
                <Label required htmlFor="password">Şifre</Label>
                <div className="relative">
                  <KeyRound size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-[#B0BEC5]" />
                  <input id="password" className={(errors.password ? inputErrorCls : inputCls) + " pl-9"}
                    type="password" value={form.password} placeholder="En az 6 karakter"
                    onChange={(e) => setField("password", e.target.value)} />
                </div>
                <FieldError message={errors.password} />
              </div>
            )}

            {/* Şirket ID */}
            <div>
              <Label htmlFor="companyId">Şirket ID</Label>
              <div className="relative">
                <Building2 size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-[#B0BEC5]" />
                <input id="companyId" className={inputCls + " pl-9"} type="number" value={form.companyId}
                  onChange={(e) => setField("companyId", Number(e.target.value) || 1)} />
              </div>
            </div>

            {/* Aktif toggle (edit only) */}
            {isEdit && (
              <div className="sm:col-span-2 pt-2">
                <div className="flex items-center gap-3 rounded-lg px-4 py-3"
                  style={{ background: form.isActive ? "#F0FAF6" : "#F8F9FA", border: `1px solid ${form.isActive ? "#C3E6D0" : "#E2EBF3"}` }}>
                  <button type="button" role="switch" aria-checked={form.isActive}
                    onClick={() => setField("isActive", !form.isActive)}
                    className="relative h-[22px] w-[40px] rounded-full transition-colors shrink-0"
                    style={{ background: form.isActive ? "#1E8A6E" : "#CBD5E1" }}>
                    <span className="absolute top-[2px] left-[2px] h-[18px] w-[18px] rounded-full bg-white transition-transform shadow-sm"
                      style={{ transform: form.isActive ? "translateX(18px)" : "translateX(0)" }} />
                  </button>
                  <div>
                    <span className="text-[13px] font-medium" style={{ color: form.isActive ? "#1E8A6E" : "#64748B" }}>
                      {form.isActive ? "Aktif" : "Pasif"}
                    </span>
                    <p className="text-[11px]" style={{ color: form.isActive ? "#4A9E82" : "#94A3B8" }}>
                      {form.isActive ? "Kullanıcı sisteme giriş yapabilir" : "Kullanıcı sisteme giriş yapamaz"}
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* ═══ Section 2: Roller ═══ */}
        <div className={`${cardCls} p-6`}>
          <h3 className={sectionTitleCls}>
            <SectionIcon icon={Shield} />
            Roller
            {form.roleIds.length > 0 && (
              <span className="ml-auto text-[11px] font-normal px-2.5 py-1 rounded-full bg-[#EAF1FA] text-[#2E6DA4]">
                {form.roleIds.length} rol seçili
              </span>
            )}
          </h3>

          {!lookups?.roles?.length ? (
            <div className="flex flex-col items-center py-8 gap-2">
              <Users size={28} className="text-[#D6E4F0]" />
              <p className="text-[13px] text-[#94A3B8]">Tanımlı rol bulunamadı</p>
            </div>
          ) : (
            <div className="flex flex-wrap gap-2">
              {lookups.roles.map((role) => {
                const selected = form.roleIds.includes(role.id);
                return (
                  <button key={role.id} type="button" onClick={() => toggleRole(role.id)}
                    className={`flex items-center gap-1.5 rounded-lg px-3.5 py-2 text-[13px] font-medium transition-all border ${
                      selected
                        ? "bg-[#EAF1FA] border-[#5B9BD5] text-[#2E6DA4] shadow-[0_0_0_1px_rgba(91,155,213,0.15)]"
                        : "bg-white border-[#E2EBF3] text-[#7A96B0] hover:border-[#C5D5E3] hover:text-[#4A6580]"
                    }`}>
                    <span className={`flex items-center justify-center w-4 h-4 rounded border transition-all ${
                      selected ? "bg-[#2E6DA4] border-[#2E6DA4]" : "bg-white border-[#D6E4F0]"
                    }`}>
                      {selected && <Check size={10} className="text-white" />}
                    </span>
                    {role.name}
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {/* ═══ Mobile Submit ═══ */}
        <div className="sm:hidden pb-4">
          <button type="submit" disabled={saving}
            className="w-full flex items-center justify-center gap-2 rounded-xl h-[48px] text-[14px] font-semibold transition-colors disabled:opacity-50 bg-[#1B3A5C] text-white hover:bg-[#2E6DA4]">
            {saving ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </button>
        </div>
      </form>
    </div>
  );
}
