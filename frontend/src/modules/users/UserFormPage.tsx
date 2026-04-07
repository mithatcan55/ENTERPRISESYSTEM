import { useState, useEffect, useMemo, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { DndContext, DragOverlay, useDroppable, useDraggable, type DragEndEvent } from "@dnd-kit/core";
import apiClient from "@/api/client";
import { usersApi } from "./api";
import type { UserDetail } from "./api";
import { createUserSchema, updateUserSchema } from "./schema";
import type { CreateUserForm, UpdateUserForm } from "./schema";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import {
  User, UserCog, Shield, GripVertical, X as XIcon,
  Loader2, Check, ArrowLeft, Wand2, Pencil,
} from "lucide-react";
import { PasswordField as PasswordFieldComponent } from "@/components/ui/PasswordField";
import { ProfileImageEditor } from "@/components/ui/ProfileImage";

/* ═══════════════════════════════════════ */
/*  TYPES                                  */
/* ═══════════════════════════════════════ */

interface RoleItem { id: number; code: string; name: string; description: string | null; isSystemRole: boolean; }
interface ActionPerm { id: number; userId: number; transactionCode: string; actionCode: string; isAllowed: boolean; }

const TCODE_DEFS = [
  { code: "SYS01", label: "Kullanıcı İşlemleri", actions: ["CREATE", "UPDATE", "DELETE", "DEACTIVATE", "REACTIVATE"] },
  { code: "SYS02", label: "Kullanıcı Güncelleme", actions: ["UPDATE"] },
  { code: "SYS03", label: "Kullanıcı Görüntüleme", actions: ["READ", "VIEW"] },
  { code: "SYS04", label: "Kullanıcı Listeleme", actions: ["READ"] },
  { code: "SYS05", label: "Kullanıcı Rolleri", actions: ["MANAGE"] },
  { code: "SYS06", label: "Kullanıcı Yetkileri", actions: ["MANAGE", "PERMISSIONS_READ"] },
];

/* ═══════════════════════════════════════ */
/*  STYLES                                 */
/* ═══════════════════════════════════════ */

const mono = "'JetBrains Mono', monospace";
const inputCls = "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none transition-all bg-[#FAFCFF] border-[1.5px] border-[#E2EBF3] text-[#1B3A5C] placeholder:text-[#B0BEC5] focus:border-[#5B9BD5] focus:ring-2 focus:ring-[#5B9BD5]/12";
const labelCls = "block mb-1 text-[12px] font-medium";
const errCls = "mt-1 text-[11px]";
const sectionCls = "flex items-center gap-2 text-[12px] font-semibold tracking-[0.04em] pb-2.5 mb-4";

function extractErr(e: unknown) {
  return (e as { response?: { data?: { detail?: string; message?: string } } }).response?.data?.detail ?? "İşlem başarısız";
}

/* ─── UserCode auto-generator ─── */

const trMap: Record<string, string> = {
  ç: "C", Ç: "C", ğ: "G", Ğ: "G", ı: "I", İ: "I",
  ö: "O", Ö: "O", ş: "S", Ş: "S", ü: "U", Ü: "U",
};

function normalizeTr(str: string): string {
  return str.split("").map((c) => trMap[c] ?? c).join("").toUpperCase().replace(/[^A-Z0-9]/g, "");
}

function generateUserCode(firstName: string, lastName: string): string {
  const f = firstName.trim();
  const l = lastName.trim();
  if (!f || !l) return "";
  const firstChar = normalizeTr(f)[0] ?? "";
  const firstSurname = normalizeTr(l.split(" ")[0]);
  return firstChar + firstSurname;
}

/* ═══════════════════════════════════════ */
/*  TOGGLE SWITCH                          */
/* ═══════════════════════════════════════ */

function Toggle({ checked, onChange, label, color }: { checked: boolean; onChange: (v: boolean) => void; label: string; color?: string }) {
  return (
    <label className="flex items-center gap-2.5 cursor-pointer select-none">
      <button type="button" role="switch" aria-checked={checked} onClick={() => onChange(!checked)}
        className="relative h-5 w-9 rounded-full transition-colors" style={{ background: checked ? (color ?? "#5B9BD5") : "#D6E4F0" }}>
        <span className="absolute top-0.5 left-0.5 h-4 w-4 rounded-full bg-white transition-transform shadow-sm"
          style={{ transform: checked ? "translateX(16px)" : "translateX(0)" }} />
      </button>
      <span className="text-[12px] font-medium" style={{ color: "#2C4A6B" }}>{label}</span>
    </label>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 1: BİLGİLER                        */
/* ═══════════════════════════════════════ */

function InfoTab({ mode, user, onSaved }: { mode: "create" | "edit"; user: UserDetail | null; onSaved: (id: number) => void }) {
  const [notify, setNotify] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [mustChange, setMustChange] = useState(false);
  const [isTemporary, setIsTemporary] = useState(true);
  const [autoGen, setAutoGen] = useState(false);

  const createForm = useForm<CreateUserForm>({ resolver: zodResolver(createUserSchema), defaultValues: { notifyAdminByMail: false, companyId: 1, mustChangePassword: true } });
  const editForm = useForm<UpdateUserForm>({ resolver: zodResolver(updateUserSchema) });

  useEffect(() => {
    if (mode === "edit" && user) {
      editForm.reset({ firstName: (user as unknown as { firstName?: string }).firstName ?? "", lastName: (user as unknown as { lastName?: string }).lastName ?? "", email: user.email, isActive: user.isActive, mustChangePassword: user.mustChangePassword, profileImageUrl: user.profileImageUrl });
      setIsActive(user.isActive);
      setMustChange(user.mustChangePassword);
    }
  }, [mode, user, editForm]);

  const createMut = useMutation({
    mutationFn: (d: CreateUserForm) => usersApi.create(d as unknown as Parameters<typeof usersApi.create>[0]),
    onSuccess: (res: { id?: number }) => { toast.success("Kullanıcı oluşturuldu"); onSaved(res?.id ?? 0); },
    onError: (e) => toast.error(extractErr(e)),
  });

  const updateMut = useMutation({
    mutationFn: (d: UpdateUserForm) => usersApi.update(user!.id, d as unknown as Parameters<typeof usersApi.update>[1]),
    onSuccess: () => { toast.success("Bilgiler güncellendi"); },
    onError: (e) => toast.error(extractErr(e)),
  });

  const isSaving = createMut.isPending || updateMut.isPending;

  if (mode === "create") {
    const { register, handleSubmit, setValue, watch, formState: { errors } } = createForm;
    const watchFirst = watch("firstName") ?? "";
    const watchLast = watch("lastName") ?? "";
    const watchCode = watch("userCode") ?? "";
    const watchPw = watch("password") ?? "";
    const preview = generateUserCode(watchFirst, watchLast);
    const isAutoMatch = preview && watchCode === preview;

    const handleGenerate = () => {
      if (!preview) return;
      setValue("userCode", preview);
      setAutoGen(true);
      toast.success(`Kod oluşturuldu: ${preview}`, { duration: 2000 });
    };

    return (
      <form onSubmit={handleSubmit((d) => createMut.mutate(d))} className="space-y-6">
        <div style={{ borderBottom: "1px solid #F0F4F8" }} className={sectionCls}>
          <User size={14} style={{ color: "#5B9BD5" }} /><span style={{ color: "#1B3A5C" }}>Temel Bilgiler</span>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
          {/* Row 1: Ad + Soyad */}
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Ad *</label>
            <input {...register("firstName")} className={inputCls} placeholder="Mithat" />
            {errors.firstName && <p style={{ color: "#E05252" }} className={errCls}>{errors.firstName.message}</p>}
          </div>
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Soyad *</label>
            <input {...register("lastName")} className={inputCls} placeholder="Can" />
            {errors.lastName && <p style={{ color: "#E05252" }} className={errCls}>{errors.lastName.message}</p>}
          </div>

          {/* Live preview hint */}
          {(watchFirst || watchLast) && (
            <div className="sm:col-span-2 flex items-center gap-1.5" style={{ fontSize: 11, color: "#7A96B0", marginTop: -8 }}>
              <Wand2 size={11} style={{ color: "#B0BEC5" }} />
              <span>Otomatik kod: </span>
              <span style={{ fontFamily: mono, fontWeight: 500, color: "#2E6DA4" }}>{preview || "—"}</span>
              {preview && watchCode !== preview && (
                <button type="button" onClick={handleGenerate} style={{ fontSize: 11, color: "#5B9BD5", background: "none", border: "none", cursor: "pointer", padding: 0 }}>Uygula</button>
              )}
            </div>
          )}

          {/* Row 2: Kod + E-posta */}
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Kod *</label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <input {...register("userCode")} className={inputCls + " pr-16"} placeholder="Örn: MCAN"
                  style={{ fontFamily: mono, fontSize: 14, letterSpacing: "0.05em", textTransform: "uppercase" }}
                  onChange={(e) => { setValue("userCode", normalizeTr(e.target.value)); setAutoGen(false); }} />
                <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
                  {isAutoMatch && autoGen ? (<><Check size={12} style={{ color: "#1E8A6E" }} /><span className="text-[10px] px-1.5 py-0.5 rounded-full" style={{ background: "#E8F5EE", color: "#1E8A6E" }}>otomatik</span></>)
                    : watchCode ? <Pencil size={12} style={{ color: "#B0BEC5" }} /> : null}
                </div>
              </div>
              <button type="button" onClick={handleGenerate} disabled={!preview}
                className="flex items-center gap-1.5 shrink-0 rounded-lg px-3.5 h-[40px] text-[12px] font-medium transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                style={{ background: "#EAF1FA", border: "1px solid #BDD5EC", color: "#2E6DA4" }}
                onMouseEnter={(e) => { if (preview) { e.currentTarget.style.background = "#2E6DA4"; e.currentTarget.style.color = "#fff"; } }}
                onMouseLeave={(e) => { e.currentTarget.style.background = "#EAF1FA"; e.currentTarget.style.color = "#2E6DA4"; }}
                title={!preview ? "Önce Ad ve Soyad girin" : undefined}>
                <Wand2 size={14} /> Oluştur
              </button>
            </div>
            {errors.userCode && <p style={{ color: "#E05252" }} className={errCls}>{errors.userCode.message}</p>}
          </div>
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>E-posta *</label>
            <input {...register("email")} type="email" className={inputCls} placeholder="kullanici@firma.com" />
            {errors.email && <p style={{ color: "#E05252" }} className={errCls}>{errors.email.message}</p>}
          </div>

          {/* Row 3: Password (full width) */}
          <div className="sm:col-span-2">
            <PasswordFieldComponent value={watchPw} onChange={(v) => setValue("password", v, { shouldValidate: true })}
              error={errors.password?.message} isTemporary={isTemporary}
              onTemporaryChange={(v) => { setIsTemporary(v); setValue("mustChangePassword", v); }} />
          </div>

          {/* Row 4: Şirket ID + Profil */}
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Şirket ID *</label>
            <input {...register("companyId", { valueAsNumber: true })} type="number" className={inputCls} defaultValue={1} />
            {errors.companyId && <p style={{ color: "#E05252" }} className={errCls}>{errors.companyId.message}</p>}
          </div>
          <div>
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Profil Resmi URL</label>
            <input className={inputCls} placeholder="https://..." disabled />
          </div>
        </div>

        <div style={{ borderBottom: "1px solid #F0F4F8" }} className={sectionCls + " mt-8"}>
          <Shield size={14} style={{ color: "#D4891A" }} /><span style={{ color: "#1B3A5C" }}>Bildirim Ayarları</span>
        </div>
        <Toggle checked={notify} onChange={(v) => { setNotify(v); setValue("notifyAdminByMail", v); }} label="Yeni kullanıcı için admin'e bildirim e-postası gönder" />
        {notify && (
          <div className="max-w-sm">
            <label style={{ color: "#2C4A6B" }} className={labelCls}>Admin E-posta</label>
            <input {...register("adminEmail")} type="email" className={inputCls} placeholder="admin@firma.com" />
          </div>
        )}

        <div className="flex justify-end pt-4">
          <button type="submit" disabled={isSaving}
            className="flex items-center gap-2 rounded-lg px-5 py-2.5 text-[13px] font-medium transition-colors disabled:opacity-60"
            style={{ background: "#1B3A5C", color: "#fff" }}>
            {isSaving ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
            {isSaving ? "Kaydediliyor..." : "Kaydet ve Devam Et"}
          </button>
        </div>
      </form>
    );
  }

  // EDIT mode
  const { register, handleSubmit, setValue, watch, formState: { errors } } = editForm;
  return (
    <form onSubmit={handleSubmit((d) => updateMut.mutate(d))} className="space-y-6">
      <div style={{ borderBottom: "1px solid #F0F4F8" }} className={sectionCls}>
        <User size={14} style={{ color: "#5B9BD5" }} /><span style={{ color: "#1B3A5C" }}>Temel Bilgiler</span>
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
        <div>
          <label style={{ color: "#2C4A6B" }} className={labelCls}>Ad</label>
          <input {...register("firstName")} className={inputCls} placeholder="Mithat" />
        </div>
        <div>
          <label style={{ color: "#2C4A6B" }} className={labelCls}>Soyad</label>
          <input {...register("lastName")} className={inputCls} placeholder="Can" />
        </div>
        <div>
          <label style={{ color: "#2C4A6B" }} className={labelCls}>E-posta *</label>
          <input {...register("email")} type="email" className={inputCls} />
          {errors.email && <p style={{ color: "#E05252" }} className={errCls}>{errors.email.message}</p>}
        </div>
        {user?.passwordExpiresAt && (
          <div className="flex items-center" style={{ fontSize: 12, color: "#7A96B0", padding: "8px 12px", background: "#F7FAFD", borderRadius: 8, border: "1px solid #E2EBF3" }}>
            Şifre geçerliliği:{" "}
            <strong style={{ color: "#1B3A5C", marginLeft: 4 }}>
              {new Date(user.passwordExpiresAt).toLocaleDateString("tr-TR")}
            </strong>
          </div>
        )}
        <div className="sm:col-span-2">
          <label style={{ color: "#2C4A6B", fontSize: 12, fontWeight: 500, marginBottom: 8, display: "block" }}>Profil Fotoğrafı</label>
          <ProfileImageEditor
            value={watch("profileImageUrl") ?? null}
            displayName={`${watch("firstName") ?? ""} ${watch("lastName") ?? ""}`.trim() || user?.userCode}
            onChange={(val) => setValue("profileImageUrl", val ?? "")}
          />
        </div>
      </div>

      <div style={{ borderBottom: "1px solid #F0F4F8" }} className={sectionCls + " mt-8"}>
        <Shield size={14} style={{ color: "#1E8A6E" }} /><span style={{ color: "#1B3A5C" }}>Hesap Durumu</span>
      </div>
      <div className="flex flex-wrap gap-6">
        <Toggle checked={isActive} onChange={(v) => { setIsActive(v); setValue("isActive", v); }} label="Aktif kullanıcı" color={isActive ? "#1E8A6E" : undefined} />
        <Toggle checked={mustChange} onChange={(v) => { setMustChange(v); setValue("mustChangePassword", v); }} label="Şifre değişimi zorunlu" color={mustChange ? "#D4891A" : undefined} />
      </div>

      <div className="flex justify-end pt-4">
        <button type="submit" disabled={isSaving}
          className="flex items-center gap-2 rounded-lg px-5 py-2.5 text-[13px] font-medium transition-colors disabled:opacity-60"
          style={{ background: "#1B3A5C", color: "#fff" }}>
          {isSaving ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
          {isSaving ? "Kaydediliyor..." : "Kaydet"}
        </button>
      </div>
    </form>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 2: ROL ATAMA                       */
/* ═══════════════════════════════════════ */

function DraggableRole({ role, side }: { role: RoleItem; side: "available" | "assigned" }) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({ id: `${side}-${role.id}`, data: { role, side } });
  return (
    <div ref={setNodeRef} {...listeners} {...attributes}
      className="flex items-center gap-2.5 rounded-lg p-2.5 transition-all"
      style={{ background: "#fff", border: "1px solid #E2EBF3", opacity: isDragging ? 0.4 : 1, cursor: "grab" }}>
      <GripVertical size={14} style={{ color: "#D6E4F0" }} />
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-1.5">
          <span className="text-[11px]" style={{ fontFamily: mono, color: "#5B9BD5" }}>{role.code || "KOD YOK"}</span>
          {role.isSystemRole && <span className="text-[10px] px-1.5 py-0.5 rounded" style={{ background: "#EAF1FA", color: "#2E6DA4" }}>Sistem</span>}
        </div>
        <div className="text-[13px] font-medium" style={{ color: "#1B3A5C" }}>{role.name || "İSİM YOK"}</div>
        {role.description && <div className="text-[11px] truncate" style={{ color: "#7A96B0" }}>{role.description}</div>}
      </div>
    </div>
  );
}

function DroppableZone({ id, children, label }: { id: string; children: React.ReactNode; label: string }) {
  const { setNodeRef, isOver } = useDroppable({ id });
  return (
    <div ref={setNodeRef} className="flex flex-col gap-2 min-h-[200px] rounded-lg p-3 transition-colors"
      style={{ background: isOver ? "#F7FAFD" : "#FAFCFF", border: isOver ? "1.5px dashed #5B9BD5" : "1.5px dashed #E2EBF3" }}>
      {children}
      {!React.Children.count(children) && (
        <div className="flex-1 flex items-center justify-center text-[12px]" style={{ color: "#B0BEC5" }}>{label}</div>
      )}
    </div>
  );
}

import React from "react";

function RolesTab({ userId }: { userId: number | null }) {
  const [search, setSearch] = useState("");
  const [draggedRole, setDraggedRole] = useState<RoleItem | null>(null);
  const [availableRoles, setAvailableRoles] = useState<RoleItem[]>([]);
  const [assignedRoles, setAssignedRoles] = useState<RoleItem[]>([]);
  const rolesInitialized = useRef(false);

  const { data: allRoles } = useQuery({
    queryKey: ["roles-all"],
    queryFn: () => apiClient.get<RoleItem[]>("/api/roles").then((r) => r.data),
    enabled: !!userId,
    staleTime: 30_000,
  });
  const { data: userRoles } = useQuery({
    queryKey: ["user-roles", userId],
    queryFn: () => apiClient.get<RoleItem[]>(`/api/roles/users/${userId}`).then((r) => r.data),
    enabled: !!userId,
    staleTime: 30_000,
  });

  // Build full role objects from allRoles — runs only once
  useEffect(() => {
    if (!allRoles || !userRoles) return;
    if (rolesInitialized.current) return;
    rolesInitialized.current = true;
    const assignedIds = new Set(userRoles.map((r) => r.id));
    setAssignedRoles(allRoles.filter((r) => assignedIds.has(r.id)));
    setAvailableRoles(allRoles.filter((r) => !assignedIds.has(r.id)));
  }, [allRoles, userRoles]);

  const filteredAvailable = useMemo(() => {
    if (!search) return availableRoles;
    const q = search.toLowerCase();
    return availableRoles.filter((r) => (r.name || "").toLowerCase().includes(q) || (r.code || "").toLowerCase().includes(q));
  }, [availableRoles, search]);

  if (!userId) return (
    <div className="text-center py-16">
      <p className="text-[14px]" style={{ color: "#7A96B0" }}>Önce kullanıcı oluşturun, ardından rol atayabilirsiniz.</p>
    </div>
  );

  async function handleAssign(roleId: number) {
    const fullRole = allRoles?.find((r) => r.id === roleId);
    if (!fullRole) { toast.error("Rol bulunamadı"); return; }
    // Optimistic
    setAvailableRoles((prev) => prev.filter((r) => r.id !== roleId));
    setAssignedRoles((prev) => [...prev, fullRole]);
    try {
      await apiClient.post(`/api/roles/${roleId}/assign/${userId}`);
      toast.success(`"${fullRole.name}" rolü atandı`);
    } catch (e) {
      setAssignedRoles((prev) => prev.filter((r) => r.id !== roleId));
      setAvailableRoles((prev) => [...prev, fullRole]);
      toast.error(extractErr(e));
    }
  }

  async function handleUnassign(roleId: number) {
    const fullRole = allRoles?.find((r) => r.id === roleId);
    if (!fullRole) { toast.error("Rol bulunamadı"); return; }
    // Optimistic
    setAssignedRoles((prev) => prev.filter((r) => r.id !== roleId));
    setAvailableRoles((prev) => [...prev, fullRole]);
    try {
      await apiClient.delete(`/api/roles/${roleId}/assign/${userId}`);
      toast.success(`"${fullRole.name}" rolü kaldırıldı`);
    } catch (e) {
      setAvailableRoles((prev) => prev.filter((r) => r.id !== roleId));
      setAssignedRoles((prev) => [...prev, fullRole]);
      toast.error(extractErr(e));
    }
  }

  function handleDragEnd(e: DragEndEvent) {
    setDraggedRole(null);
    const { active, over } = e;
    if (!over) return;
    const data = active.data.current as { role: RoleItem; side: string };
    if (data.side === "available" && over.id === "assigned") handleAssign(data.role.id);
    if (data.side === "assigned" && over.id === "available") handleUnassign(data.role.id);
  }

  return (
    <DndContext onDragStart={(e) => setDraggedRole((e.active.data.current as { role: RoleItem }).role)} onDragEnd={handleDragEnd}>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Available */}
        <div className="rounded-xl p-4" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
          <div className="flex items-center justify-between mb-3">
            <span className="text-[13px] font-semibold" style={{ color: "#1B3A5C" }}>Mevcut Roller</span>
            <span className="text-[11px] px-2 py-0.5 rounded-full" style={{ background: "#F0F4F8", color: "#7A96B0" }}>{filteredAvailable.length}</span>
          </div>
          <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Rol ara..." className={inputCls + " mb-3 h-[32px] text-[12px]"} />
          <DroppableZone id="available" label="Tüm roller atandı">
            {filteredAvailable.map((r) => <DraggableRole key={r.id} role={r} side="available" />)}
          </DroppableZone>
        </div>

        {/* Assigned */}
        <div className="rounded-xl p-4" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
          <div className="flex items-center justify-between mb-3">
            <span className="text-[13px] font-semibold" style={{ color: "#1B3A5C" }}>Atanmış Roller</span>
            <span className="text-[11px] px-2 py-0.5 rounded-full" style={{ background: "#EAF1FA", color: "#2E6DA4" }}>{assignedRoles.length}</span>
          </div>
          <DroppableZone id="assigned" label="Buraya sürükleyin">
            {assignedRoles.map((r) => (
              <div key={r.id} className="flex items-center gap-2">
                <div className="flex-1"><DraggableRole role={r} side="assigned" /></div>
                <button onClick={() => handleUnassign(r.id)} className="shrink-0 p-1 rounded hover:bg-[#FDECEA] transition-colors" style={{ color: "#D6E4F0" }}>
                  <XIcon size={14} />
                </button>
              </div>
            ))}
          </DroppableZone>
        </div>
      </div>
      <DragOverlay>{draggedRole && <div className="rounded-lg p-2.5 shadow-lg" style={{ background: "#fff", border: "1px solid #5B9BD5", width: 280 }}>
        <span className="text-[13px] font-medium" style={{ color: "#1B3A5C" }}>{draggedRole.name || "—"}</span>
      </div>}</DragOverlay>
    </DndContext>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 3: YETKİ ATAMA                     */
/* ═══════════════════════════════════════ */

function PermissionsTab({ userId }: { userId: number | null }) {
  const [perms, setPerms] = useState<ActionPerm[]>([]);
  const [loadingChips, setLoadingChips] = useState<Set<string>>(new Set());

  const { data } = useQuery({
    queryKey: ["user-permissions", userId],
    queryFn: () => apiClient.get<ActionPerm[]>("/api/permissions/actions", { params: { userId } }).then((r) => r.data),
    enabled: !!userId,
  });

  useEffect(() => { if (data) setPerms(data); }, [data]);

  if (!userId) return (
    <div className="text-center py-16">
      <p className="text-[14px]" style={{ color: "#7A96B0" }}>Önce kullanıcı oluşturun, ardından yetki atayabilirsiniz.</p>
    </div>
  );

  function isAllowed(tcode: string, action: string) {
    return perms.some((p) => p.transactionCode === tcode && p.actionCode === action && p.isAllowed);
  }

  function chipKey(tcode: string, action: string) { return `${tcode}:${action}`; }

  async function toggleAction(tcode: string, action: string) {
    const key = chipKey(tcode, action);
    setLoadingChips((s) => new Set(s).add(key));
    try {
      if (isAllowed(tcode, action)) {
        const perm = perms.find((p) => p.transactionCode === tcode && p.actionCode === action);
        if (perm) {
          await apiClient.delete(`/api/permissions/actions/${perm.id}`);
          setPerms((prev) => prev.filter((p) => p.id !== perm.id));
          toast.success("Yetki kaldırıldı");
        }
      } else {
        const { data: newPerm } = await apiClient.post<ActionPerm>("/api/permissions/actions", { userId, transactionCode: tcode, actionCode: action, isAllowed: true });
        setPerms((prev) => [...prev, newPerm]);
        toast.success("Yetki atandı");
      }
    } catch (e) { toast.error(extractErr(e)); }
    setLoadingChips((s) => { const n = new Set(s); n.delete(key); return n; });
  }

  async function toggleAll(tcode: string, actions: string[]) {
    const allOn = actions.every((a) => isAllowed(tcode, a));
    for (const a of actions) {
      if (allOn ? isAllowed(tcode, a) : !isAllowed(tcode, a)) await toggleAction(tcode, a);
    }
  }

  return (
    <div className="space-y-4">
      {TCODE_DEFS.map((tc) => {
        const allOn = tc.actions.every((a) => isAllowed(tc.code, a));
        return (
          <div key={tc.code} className="rounded-xl p-5" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-2">
                <span className="text-[11px] px-2 py-0.5 rounded" style={{ fontFamily: mono, background: "#EAF1FA", color: "#2E6DA4" }}>{tc.code}</span>
                <span className="text-[13px] font-medium" style={{ color: "#1B3A5C" }}>{tc.label}</span>
              </div>
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input type="checkbox" checked={allOn} onChange={() => toggleAll(tc.code, tc.actions)}
                  className="h-4 w-4 rounded border-[#D6E4F0] accent-[#2E6DA4]" />
                <span className="text-[11px]" style={{ color: "#7A96B0" }}>Tümünü Seç</span>
              </label>
            </div>
            <div className="flex flex-wrap gap-2">
              {tc.actions.map((action) => {
                const on = isAllowed(tc.code, action);
                const loading = loadingChips.has(chipKey(tc.code, action));
                return (
                  <button key={action} onClick={() => toggleAction(tc.code, action)} disabled={loading}
                    className="flex items-center gap-1.5 rounded-full px-3 py-1.5 text-[12px] font-medium transition-all disabled:opacity-60"
                    style={on ? { background: "#EAF1FA", border: "1px solid #BDD5EC", color: "#2E6DA4" } : { background: "#F0F4F8", border: "1px solid #E2EBF3", color: "#7A96B0" }}>
                    {loading ? <Loader2 size={12} className="animate-spin" /> : on ? <Check size={12} /> : null}
                    {action}
                  </button>
                );
              })}
            </div>
          </div>
        );
      })}
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  MAIN PAGE                              */
/* ═══════════════════════════════════════ */

const TABS = [
  { key: "info", label: "Bilgiler", Icon: User },
  { key: "roles", label: "Rol Atama", Icon: UserCog },
  { key: "perms", label: "Yetki Atama", Icon: Shield },
] as const;

type TabKey = (typeof TABS)[number]["key"];

export default function UserFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;
  const [activeTab, setActiveTab] = useState<TabKey>("info");
  const [createdUserId, setCreatedUserId] = useState<number | null>(null);

  const userId = isEdit ? Number(id) : createdUserId;

  const { data: user } = useQuery({
    queryKey: ["users", id],
    queryFn: () => usersApi.getById(Number(id)),
    enabled: isEdit,
  });

  // Role/perm counts for badges
  const { data: userRoles } = useQuery({ queryKey: ["user-roles", userId], queryFn: () => apiClient.get<RoleItem[]>(`/api/roles/users/${userId}`).then((r) => r.data), enabled: !!userId });
  const { data: userPerms } = useQuery({ queryKey: ["user-permissions", userId], queryFn: () => apiClient.get<ActionPerm[]>("/api/permissions/actions", { params: { userId } }).then((r) => r.data), enabled: !!userId });

  function handleCreated(newId: number) {
    setCreatedUserId(newId);
    setActiveTab("roles");
  }

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title={isEdit ? `${user?.username ?? "..."} — Düzenle` : "Yeni Kullanıcı"}
        subtitle="Kullanıcı bilgilerini, rollerini ve yetkilerini yönetin"
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate("/users")}><ArrowLeft size={14} /> İptal</PageAction>
            {userId && <PageAction onClick={() => navigate(`/users/${userId}`)}><Check size={14} /> Tamamla</PageAction>}
          </div>
        }
      />

      {/* Tab bar */}
      <div className="rounded-[10px] px-5 overflow-x-auto" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
        <div className="flex" style={{ borderBottom: "1px solid #F0F4F8" }}>
          {TABS.map((tab) => {
            const active = activeTab === tab.key;
            const count = tab.key === "roles" ? (userRoles?.length ?? 0) : tab.key === "perms" ? (userPerms?.filter((p) => p.isAllowed).length ?? 0) : 0;
            return (
              <button key={tab.key} onClick={() => setActiveTab(tab.key)}
                className="flex items-center gap-2 px-5 py-3.5 text-[13px] font-medium transition-all whitespace-nowrap"
                style={{ color: active ? "#1B3A5C" : "#7A96B0", borderBottom: active ? "2px solid #2E6DA4" : "2px solid transparent" }}>
                <tab.Icon size={15} />
                {tab.label}
                {count > 0 && <span className="text-[11px] px-1.5 py-0.5 rounded-full" style={{ background: "#EAF1FA", color: "#2E6DA4" }}>{count}</span>}
              </button>
            );
          })}
        </div>
      </div>

      {/* Tab content */}
      <div className="rounded-[10px] p-6 sm:p-8" style={{ background: "#fff", border: "1px solid #E2EBF3" }}>
        {activeTab === "info" && <InfoTab mode={isEdit ? "edit" : "create"} user={user ?? null} onSaved={handleCreated} />}
        {activeTab === "roles" && <RolesTab userId={userId} />}
        {activeTab === "perms" && <PermissionsTab userId={userId} />}
      </div>
    </div>
  );
}
