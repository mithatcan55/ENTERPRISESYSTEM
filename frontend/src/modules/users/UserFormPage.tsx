import { useState, useEffect, useRef, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import apiClient from "@/api/client";
import { usersApi } from "./api";
import type { UserDetail, LookupItem, PermissionLookupItem } from "./api";
import { createUserSchema, updateUserSchema } from "./schema";
import type { CreateUserForm, UpdateUserForm } from "./schema";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { PasswordField as PasswordFieldComponent } from "@/components/ui/PasswordField";
import { ProfileImageEditor } from "@/components/ui/ProfileImage";
import {
  User, UserCog, Shield, Check, ArrowLeft, Wand2, Pencil,
} from "lucide-react";

/* ═══════════════════════════════════════ */
/*  HELPERS                                */
/* ═══════════════════════════════════════ */

const mono = "'JetBrains Mono', monospace";
const inputCls = "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none transition-all bg-[#FAFCFF] border border-[#E2EBF3] text-[#1B3A5C] placeholder:text-[#C5CED8] focus:border-[#5B9BD5] focus:bg-white focus:shadow-[0_0_0_3px_rgba(91,155,213,0.08)]";
const labelCls = "block mb-1 text-[12px] font-medium text-[#4A6580]";
const errCls = "mt-1 text-[11px] text-[#E05252]";
const sectionCls = "flex items-center gap-2 text-[12px] font-semibold tracking-[0.04em] pb-2.5 mb-4 border-b border-[#F0F4F8] text-[#1B3A5C]";

const trMap: Record<string, string> = { ç: "C", Ç: "C", ğ: "G", Ğ: "G", ı: "I", İ: "I", ö: "O", Ö: "O", ş: "S", Ş: "S", ü: "U", Ü: "U" };
function normalizeTr(str: string): string {
  return str.split("").map((c) => trMap[c] ?? c).join("").toUpperCase().replace(/[^A-Z0-9]/g, "");
}
function generateUserCode(firstName: string, lastName: string): string {
  const f = firstName.trim(), l = lastName.trim();
  if (!f || !l) return "";
  return (normalizeTr(f)[0] ?? "") + normalizeTr(l.split(" ")[0]);
}

function Toggle({ checked, onChange, label, color }: { checked: boolean; onChange: (v: boolean) => void; label: string; color?: string }) {
  return (
    <label className="flex items-center gap-2.5 cursor-pointer select-none">
      <button type="button" role="switch" aria-checked={checked} onClick={() => onChange(!checked)}
        className="relative h-5 w-9 rounded-full transition-colors" style={{ background: checked ? (color ?? "#5B9BD5") : "#D6E4F0" }}>
        <span className="absolute top-0.5 left-0.5 h-4 w-4 rounded-full bg-white transition-transform shadow-sm"
          style={{ transform: checked ? "translateX(16px)" : "translateX(0)" }} />
      </button>
      <span className="text-[12px] font-medium text-[#2C4A6B]">{label}</span>
    </label>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 1: BİLGİLER                        */
/* ═══════════════════════════════════════ */

function InfoTab({ mode, user, onSaved, formRef }: {
  mode: "create" | "edit"; user: UserDetail | null;
  onSaved: (id: number) => void; formRef?: React.RefObject<HTMLFormElement | null>;
}) {
  const [notify, setNotify] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [mustChange, setMustChange] = useState(false);
  const [isTemporary, setIsTemporary] = useState(true);
  const [autoGen, setAutoGen] = useState(false);

  const createForm = useForm<CreateUserForm>({ resolver: zodResolver(createUserSchema), defaultValues: { notifyAdminByMail: false, companyId: 1, mustChangePassword: true } });
  const editForm = useForm<UpdateUserForm>({ resolver: zodResolver(updateUserSchema) });

  useEffect(() => {
    if (mode === "edit" && user) {
      editForm.reset({ firstName: user.firstName ?? "", lastName: user.lastName ?? "", email: user.email, isActive: user.isActive, mustChangePassword: user.mustChangePassword, profileImageUrl: user.profileImageUrl });
      setIsActive(user.isActive);
      setMustChange(user.mustChangePassword);
    }
  }, [mode, user, editForm]);

  const createMut = async (d: CreateUserForm) => {
    try {
      const res = await usersApi.create({
        userCode: d.userCode, username: d.userCode.toLowerCase(),
        firstName: d.firstName || null, lastName: d.lastName || null,
        email: d.email, password: d.password, companyId: d.companyId,
        notifyAdminByMail: d.notifyAdminByMail, adminEmail: d.adminEmail,
      });
      toast.success("Kullanıcı oluşturuldu");
      onSaved(res?.id ?? 0);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { detail?: string } } }).response?.data?.detail ?? "Oluşturma başarısız";
      toast.error(msg);
    }
  };

  const updateMut = async (d: UpdateUserForm) => {
    if (!user) return;
    try {
      await usersApi.update(user.id, {
        firstName: d.firstName || null, lastName: d.lastName || null,
        email: d.email, isActive: d.isActive, mustChangePassword: d.mustChangePassword,
        profileImageUrl: d.profileImageUrl || null,
      });
      toast.success("Bilgiler güncellendi");
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { detail?: string } } }).response?.data?.detail ?? "Güncelleme başarısız";
      toast.error(msg);
    }
  };

  // ─── CREATE ───
  if (mode === "create") {
    const { register, handleSubmit, setValue, watch, formState: { errors } } = createForm;
    const wFirst = watch("firstName") ?? "", wLast = watch("lastName") ?? "";
    const wCode = watch("userCode") ?? "", wPw = watch("password") ?? "";
    const preview = generateUserCode(wFirst, wLast);

    return (
      <form ref={formRef} onSubmit={handleSubmit(createMut)} className="space-y-6">
        {/* Profile image + name section */}
        <div className="flex flex-col sm:flex-row items-start gap-5">
          <ProfileImageEditor value={watch("profileImageUrl") ?? null}
            displayName={`${wFirst} ${wLast}`.trim() || undefined}
            onChange={(val) => setValue("profileImageUrl", val ?? "")} />
          <div className="flex-1 grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4 w-full">
            <div>
              <label className={labelCls}>Ad *</label>
              <input {...register("firstName")} className={inputCls} placeholder="Mithat" />
              {errors.firstName && <p className={errCls}>{errors.firstName.message}</p>}
            </div>
            <div>
              <label className={labelCls}>Soyad *</label>
              <input {...register("lastName")} className={inputCls} placeholder="Can" />
              {errors.lastName && <p className={errCls}>{errors.lastName.message}</p>}
            </div>
          </div>
        </div>

        {/* Code hint */}
        {(wFirst || wLast) && (
          <div className="flex items-center gap-1.5 text-[11px] text-[#7A96B0]">
            <Wand2 size={11} className="text-[#B0BEC5]" />
            <span>Kod: </span>
            <span style={{ fontFamily: mono, fontWeight: 500, color: "#2E6DA4" }}>{preview || "—"}</span>
            {preview && wCode !== preview && (
              <button type="button" onClick={() => { setValue("userCode", preview); setAutoGen(true); toast.success(`Kod: ${preview}`, { duration: 1500 }); }}
                className="text-[#5B9BD5] hover:underline" style={{ background: "none", border: "none", cursor: "pointer", padding: 0, fontSize: 11 }}>Uygula</button>
            )}
          </div>
        )}

        <div className={sectionCls}><User size={14} className="text-[#5B9BD5]" /> Hesap Bilgileri</div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
          <div>
            <label className={labelCls}>Kod *</label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <input {...register("userCode")} className={inputCls + " pr-16"} placeholder="MCAN"
                  style={{ fontFamily: mono, letterSpacing: "0.05em", textTransform: "uppercase" }}
                  onChange={(e) => { setValue("userCode", normalizeTr(e.target.value)); setAutoGen(false); }} />
                {wCode && (
                  <div className="absolute right-2 top-1/2 -translate-y-1/2">
                    {preview && wCode === preview && autoGen
                      ? <span className="text-[10px] px-1.5 py-0.5 rounded-full bg-[#E8F5EE] text-[#1E8A6E]">otomatik</span>
                      : <Pencil size={12} className="text-[#B0BEC5]" />}
                  </div>
                )}
              </div>
              <button type="button" disabled={!preview} onClick={() => { if (preview) { setValue("userCode", preview); setAutoGen(true); toast.success(`Kod: ${preview}`, { duration: 1500 }); } }}
                className="shrink-0 rounded-lg px-3.5 h-[40px] text-[12px] font-medium transition-all disabled:opacity-40 bg-[#EAF1FA] border border-[#BDD5EC] text-[#2E6DA4] hover:bg-[#2E6DA4] hover:text-white">
                <Wand2 size={14} />
              </button>
            </div>
            {errors.userCode && <p className={errCls}>{errors.userCode.message}</p>}
          </div>
          <div>
            <label className={labelCls}>E-posta *</label>
            <input {...register("email")} type="email" className={inputCls} placeholder="kullanici@firma.com" />
            {errors.email && <p className={errCls}>{errors.email.message}</p>}
          </div>
          <div className="sm:col-span-2">
            <PasswordFieldComponent value={wPw} onChange={(v) => setValue("password", v, { shouldValidate: true })}
              error={errors.password?.message} isTemporary={isTemporary}
              onTemporaryChange={(v) => { setIsTemporary(v); setValue("mustChangePassword", v); }} />
          </div>
          <div>
            <label className={labelCls}>Şirket ID *</label>
            <input {...register("companyId", { valueAsNumber: true })} type="number" className={inputCls} defaultValue={1} />
            {errors.companyId && <p className={errCls}>{errors.companyId.message}</p>}
          </div>
        </div>
        <div className={sectionCls + " mt-6"}><Shield size={14} className="text-[#D4891A]" /> Bildirim</div>
        <Toggle checked={notify} onChange={(v) => { setNotify(v); setValue("notifyAdminByMail", v); }} label="Admin'e bildirim gönder" />
        {notify && (
          <div className="max-w-sm">
            <label className={labelCls}>Admin E-posta</label>
            <input {...register("adminEmail")} type="email" className={inputCls} placeholder="admin@firma.com" />
          </div>
        )}
      </form>
    );
  }

  // ─── EDIT ───
  const { register, handleSubmit, setValue, watch, formState: { errors } } = editForm;
  return (
    <form ref={formRef} onSubmit={handleSubmit(updateMut)} className="space-y-6">
      {/* Profile image + name section */}
      <div className="flex flex-col sm:flex-row items-start gap-5">
        <ProfileImageEditor value={watch("profileImageUrl") ?? null}
          displayName={`${watch("firstName") ?? ""} ${watch("lastName") ?? ""}`.trim() || user?.userCode}
          onChange={(val) => setValue("profileImageUrl", val ?? "")} />
        <div className="flex-1 grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4 w-full">
          <div>
            <label className={labelCls}>Ad *</label>
            <input {...register("firstName")} className={inputCls} />
            {errors.firstName && <p className={errCls}>{errors.firstName.message}</p>}
          </div>
          <div>
            <label className={labelCls}>Soyad *</label>
            <input {...register("lastName")} className={inputCls} />
            {errors.lastName && <p className={errCls}>{errors.lastName.message}</p>}
          </div>
        </div>
      </div>

      <div className={sectionCls}><User size={14} className="text-[#5B9BD5]" /> Hesap Bilgileri</div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
        <div>
          <label className={labelCls}>E-posta *</label>
          <input {...register("email")} type="email" className={inputCls} />
          {errors.email && <p className={errCls}>{errors.email.message}</p>}
        </div>
        {user?.passwordExpiresAt && (
          <div className="flex items-center text-[12px] text-[#7A96B0] px-3 py-2 rounded-lg bg-[#F7FAFD] border border-[#E2EBF3]">
            Şifre geçerliliği: <strong className="ml-1 text-[#1B3A5C]">{new Date(user.passwordExpiresAt).toLocaleDateString("tr-TR")}</strong>
          </div>
        )}
      </div>
      <div className={sectionCls + " mt-6"}><Shield size={14} className="text-[#1E8A6E]" /> Hesap Durumu</div>
      <div className="flex flex-wrap gap-6">
        <Toggle checked={isActive} onChange={(v) => { setIsActive(v); setValue("isActive", v); }} label="Aktif" color={isActive ? "#1E8A6E" : undefined} />
        <Toggle checked={mustChange} onChange={(v) => { setMustChange(v); setValue("mustChangePassword", v); }} label="Şifre değişimi zorunlu" color={mustChange ? "#D4891A" : undefined} />
      </div>
    </form>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 2: ROL ATAMA                       */
/* ═══════════════════════════════════════ */

function RolesTab({ userId, allRoles, userRoleIds }: { userId: number | null; allRoles: LookupItem[]; userRoleIds: number[] }) {
  const [assignedIds, setAssignedIds] = useState<Set<number>>(new Set());
  const [search, setSearch] = useState("");
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current) return;
    if (userRoleIds.length > 0 || allRoles.length > 0) {
      initialized.current = true;
      setAssignedIds(new Set(userRoleIds));
    }
  }, [userRoleIds, allRoles]);

  if (!userId) return <p className="text-center py-12 text-[13px] text-[#7A96B0]">Önce kullanıcı oluşturun.</p>;

  async function toggle(roleId: number) {
    const wasAssigned = assignedIds.has(roleId);
    const next = new Set(assignedIds);
    if (wasAssigned) next.delete(roleId); else next.add(roleId);
    setAssignedIds(next);
    try {
      if (wasAssigned) await apiClient.delete(`/api/roles/${roleId}/assign/${userId}`);
      else await apiClient.post(`/api/roles/${roleId}/assign/${userId}`);
      toast.success(wasAssigned ? "Rol kaldırıldı" : "Rol atandı");
    } catch {
      // revert
      const rev = new Set(assignedIds);
      if (wasAssigned) rev.add(roleId); else rev.delete(roleId);
      setAssignedIds(rev);
      toast.error("İşlem başarısız");
    }
  }

  const filtered = search ? allRoles.filter((r) => r.name.toLowerCase().includes(search.toLowerCase())) : allRoles;

  return (
    <div className="space-y-4">
      <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Rol ara..." className={inputCls + " max-w-xs"} />
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        {filtered.map((role) => {
          const on = assignedIds.has(role.id);
          return (
            <button key={role.id} type="button" onClick={() => toggle(role.id)}
              className={`flex items-center gap-3 rounded-lg px-4 py-3 text-left transition-all border ${
                on ? "bg-[#EAF1FA] border-[#5B9BD5] shadow-[0_0_0_1px_rgba(91,155,213,0.15)]" : "bg-white border-[#E2EBF3] hover:border-[#C5D5E3]"
              }`}>
              <span className={`flex items-center justify-center w-5 h-5 rounded border transition-all ${on ? "bg-[#2E6DA4] border-[#2E6DA4]" : "bg-white border-[#D6E4F0]"}`}>
                {on && <Check size={12} className="text-white" />}
              </span>
              <span className={`text-[13px] font-medium ${on ? "text-[#2E6DA4]" : "text-[#7A96B0]"}`}>{role.name}</span>
            </button>
          );
        })}
      </div>
      {!filtered.length && <p className="text-[13px] text-[#94A3B8] text-center py-8">Rol bulunamadı</p>}
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 3: YETKİ ATAMA                     */
/* ═══════════════════════════════════════ */

function PermissionsTab({ userId, allPermissions, userPermIds }: { userId: number | null; allPermissions: PermissionLookupItem[]; userPermIds: number[] }) {
  const [assignedIds, setAssignedIds] = useState<Set<number>>(new Set());
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current) return;
    if (userPermIds.length > 0 || allPermissions.length > 0) {
      initialized.current = true;
      setAssignedIds(new Set(userPermIds));
    }
  }, [userPermIds, allPermissions]);

  if (!userId) return <p className="text-center py-12 text-[13px] text-[#7A96B0]">Önce kullanıcı oluşturun.</p>;

  // Group by transactionCode
  const groups = useMemo(() => {
    const map = new Map<string, PermissionLookupItem[]>();
    for (const p of allPermissions) {
      const key = p.transactionCode;
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(p);
    }
    return Array.from(map.entries());
  }, [allPermissions]);

  async function toggle(permId: number) {
    const wasOn = assignedIds.has(permId);
    const next = new Set(assignedIds);
    if (wasOn) next.delete(permId); else next.add(permId);
    setAssignedIds(next);
    try {
      if (wasOn) {
        // find the action permission ID from user's directPermissions — for now use subModulePageId approach
        await apiClient.delete(`/api/permissions/actions/${permId}`);
      } else {
        const perm = allPermissions.find((p) => p.id === permId);
        if (perm) {
          await apiClient.post("/api/permissions/actions", {
            userId, transactionCode: perm.transactionCode, actionCode: "ALL", isAllowed: true,
          });
        }
      }
      toast.success(wasOn ? "İzin kaldırıldı" : "İzin atandı");
    } catch {
      const rev = new Set(assignedIds);
      if (wasOn) rev.add(permId); else rev.delete(permId);
      setAssignedIds(rev);
      toast.error("İşlem başarısız");
    }
  }

  return (
    <div className="space-y-4">
      {groups.length === 0 && <p className="text-[13px] text-[#94A3B8] text-center py-8">İzin tanımı bulunamadı</p>}
      {groups.map(([tcode, perms]) => (
        <div key={tcode} className="rounded-xl p-4 bg-white border border-[#E2EBF3]">
          <div className="flex items-center gap-2 mb-3">
            <span className="text-[11px] px-2 py-0.5 rounded bg-[#EAF1FA] text-[#2E6DA4]" style={{ fontFamily: mono }}>{tcode}</span>
            <span className="text-[13px] font-medium text-[#1B3A5C]">{perms[0]?.displayName ?? tcode}</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {perms.map((p) => {
              const on = assignedIds.has(p.id);
              return (
                <button key={p.id} type="button" onClick={() => toggle(p.id)}
                  className={`flex items-center gap-1.5 rounded-full px-3 py-1.5 text-[12px] font-medium transition-all border ${
                    on ? "bg-[#FEF3E2] border-[#F5D99A] text-[#D4891A]" : "bg-[#F0F4F8] border-[#E2EBF3] text-[#7A96B0] hover:border-[#C5D5E3]"
                  }`}>
                  {on && <Check size={12} />}
                  {p.actionCode}
                </button>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  MAIN PAGE                              */
/* ═══════════════════════════════════════ */

const TABS = [
  { key: "info" as const, label: "Bilgiler", Icon: User },
  { key: "roles" as const, label: "Rol Atama", Icon: UserCog },
  { key: "perms" as const, label: "Yetki Atama", Icon: Shield },
];

type TabKey = "info" | "roles" | "perms";

export default function UserFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;
  const [activeTab, setActiveTab] = useState<TabKey>("info");
  const [createdUserId, setCreatedUserId] = useState<number | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  const userId = isEdit ? Number(id) : createdUserId;

  // User detail
  const { data: user } = useQuery({ queryKey: ["users", id], queryFn: () => usersApi.getById(Number(id)), enabled: isEdit });

  // Lookups
  const { data: lookups } = useQuery({ queryKey: ["user-lookups"], queryFn: () => usersApi.lookups(), staleTime: 60_000 });

  // Derived
  const userRoleIds = useMemo(() => (user?.roles ?? []).map((r) => r.roleId), [user]);
  const userPermIds = useMemo(() => (user?.directPermissions ?? []).map((p) => p.subModulePageId), [user]);
  const roleCount = isEdit ? userRoleIds.length : 0;
  const permCount = isEdit ? userPermIds.length : 0;

  function handleCreated(newId: number) {
    setCreatedUserId(newId);
    setActiveTab("roles");
  }

  return (
    <div className="flex flex-col gap-4 max-w-[820px]">
      <PageHeader
        title={isEdit ? `${user?.userCode ?? "..."} — Düzenle` : "Yeni Kullanıcı"}
        subtitle="Kullanıcı bilgilerini, rollerini ve yetkilerini yönetin"
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate("/users")}><ArrowLeft size={14} /> Vazgeç</PageAction>
            <PageAction onClick={() => {
              if (activeTab === "info") formRef.current?.requestSubmit();
              else if (userId) navigate(`/users/${userId}`);
            }}><Check size={14} /> Kaydet</PageAction>
          </div>
        }
      />

      {/* Tab bar */}
      <div className="rounded-xl px-5 overflow-x-auto bg-white border border-[#E2EBF3]">
        <div className="flex border-b border-[#F0F4F8]">
          {TABS.map((tab) => {
            const active = activeTab === tab.key;
            const count = tab.key === "roles" ? roleCount : tab.key === "perms" ? permCount : 0;
            return (
              <button key={tab.key} onClick={() => setActiveTab(tab.key)}
                className="flex items-center gap-2 px-5 py-3.5 text-[13px] font-medium transition-all whitespace-nowrap"
                style={{ color: active ? "#1B3A5C" : "#7A96B0", borderBottom: active ? "2px solid #2E6DA4" : "2px solid transparent" }}>
                <tab.Icon size={15} />
                {tab.label}
                {count > 0 && <span className="text-[11px] px-1.5 py-0.5 rounded-full bg-[#EAF1FA] text-[#2E6DA4]">{count}</span>}
              </button>
            );
          })}
        </div>
      </div>

      {/* Tab content */}
      <div className="rounded-xl p-6 sm:p-8 bg-white border border-[#E2EBF3]">
        {activeTab === "info" && <InfoTab mode={isEdit ? "edit" : "create"} user={user ?? null} onSaved={handleCreated} formRef={formRef} />}
        {activeTab === "roles" && <RolesTab userId={userId} allRoles={lookups?.roles ?? []} userRoleIds={userRoleIds} />}
        {activeTab === "perms" && <PermissionsTab userId={userId} allPermissions={lookups?.permissions ?? []} userPermIds={userPermIds} />}
      </div>
    </div>
  );
}
