import { useState, useEffect, useRef, useMemo } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { usersApi } from "./api";
import type { UserDetail, LookupItem, PermissionLookupItem } from "./api";
import { createUserSchema, updateUserSchema } from "./schema";
import type { CreateUserForm, UpdateUserForm } from "./schema";
import { extractApiError } from "@/lib/api-error";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { PasswordField as PasswordFieldComponent } from "@/components/ui/PasswordField";
import { ProfileImageEditor } from "@/components/ui/ProfileImage";
import {
  DndContext,
  PointerSensor,
  useDraggable,
  useDroppable,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core";
import { CSS } from "@dnd-kit/utilities";
import {
  User, UserCog, Shield, Check, ArrowLeft, Wand2, Pencil, GripVertical,
} from "lucide-react";

/* ═══════════════════════════════════════ */
/*  HELPERS                                */
/* ═══════════════════════════════════════ */

const mono = "'JetBrains Mono', monospace";
const inputCls = "w-full rounded-lg h-[40px] px-3 text-[13px] outline-none transition-all bg-[var(--ui-surface-alt)] border border-[var(--ui-border)] text-[var(--ui-text)] placeholder:text-[var(--ui-text-muted)] focus:border-[var(--ui-primary)] focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--ui-primary)_18%,transparent)]";
const labelCls = "block mb-1 text-[12px] font-medium ui-text";
const errCls = "mt-1 text-[11px]";
const sectionCls = "flex items-center gap-2 text-[12px] font-semibold tracking-[0.04em] pb-2.5 mb-4 border-b border-[var(--ui-border)] ui-text";

type UserSaveState = {
  firstName: string | null;
  lastName: string | null;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  profileImageUrl: string | null;
};

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
        className="relative h-5 w-9 rounded-full transition-colors" style={{ background: checked ? (color ?? "var(--ui-primary)") : "var(--ui-border)" }}>
        <span className="absolute top-0.5 left-0.5 h-4 w-4 rounded-full bg-white transition-transform shadow-sm"
          style={{ transform: checked ? "translateX(16px)" : "translateX(0)" }} />
      </button>
      <span className="text-[12px] font-medium ui-text">{label}</span>
    </label>
  );
}

function AssignmentDropZone({
  id,
  title,
  hint,
  count,
  emptyText,
  children,
}: {
  id: string;
  title: string;
  hint: string;
  count: number;
  emptyText: string;
  children: React.ReactNode;
}) {
  const { setNodeRef, isOver } = useDroppable({ id });

  return (
    <div
      ref={setNodeRef}
      className="rounded-xl border min-h-[280px] transition-all"
      style={{
        borderColor: isOver ? "var(--ui-primary)" : "var(--ui-border)",
        background: "var(--ui-card-bg)",
        boxShadow: isOver ? "0 0 0 3px color-mix(in srgb, var(--ui-primary) 18%, transparent)" : "none",
      }}
    >
      <div className="flex items-center justify-between px-4 py-3" style={{ borderBottom: "1px solid var(--ui-border)" }}>
        <div>
          <p className="text-[13px] font-semibold ui-text">{title}</p>
          <p className="text-[11px] ui-text-muted">{hint}</p>
        </div>
        <span className="text-[11px] px-2 py-0.5 rounded-full" style={{ background: "color-mix(in srgb, var(--ui-primary) 18%, transparent)", color: "var(--ui-primary)" }}>{count}</span>
      </div>

      <div className="p-3 space-y-2">
        {count === 0 ? (
          <p className="text-[12px] text-center py-10 border border-dashed rounded-lg ui-text-muted" style={{ borderColor: "var(--ui-border)", background: "var(--ui-surface-alt)" }}>
            {emptyText}
          </p>
        ) : children}
      </div>
    </div>
  );
}

function DraggableAssignmentItem({
  id,
  title,
  subtitle,
  activeTone,
  onClick,
  actionLabel,
}: {
  id: string;
  title: string;
  subtitle?: string;
  activeTone: "available" | "assigned";
  onClick: () => void;
  actionLabel: string;
}) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({ id });

  return (
    <div
      ref={setNodeRef}
      className="flex items-center justify-between gap-2 rounded-lg border px-3 py-2.5"
      style={{
        borderColor: activeTone === "assigned" ? "color-mix(in srgb, var(--ui-primary) 30%, transparent)" : "var(--ui-border)",
        background: activeTone === "assigned" ? "color-mix(in srgb, var(--ui-primary) 12%, transparent)" : "var(--ui-card-bg)",
        transform: CSS.Translate.toString(transform),
        opacity: isDragging ? 0.7 : 1,
      }}
    >
      <div className="flex items-start gap-2 min-w-0">
        <button
          type="button"
          className="mt-0.5 rounded p-0.5 ui-text-muted"
          {...listeners}
          {...attributes}
          aria-label="Sürükle"
        >
          <GripVertical size={14} />
        </button>
        <div className="min-w-0">
          <p className="text-[12px] font-medium ui-text truncate">{title}</p>
          {subtitle && <p className="text-[11px] ui-text-muted truncate">{subtitle}</p>}
        </div>
      </div>

      <button
        type="button"
        onClick={onClick}
        className="shrink-0 rounded-md px-2 py-1 text-[11px] border"
        style={activeTone === "assigned"
          ? { borderColor: "color-mix(in srgb, var(--ui-danger) 35%, transparent)", color: "var(--ui-danger)", background: "var(--ui-danger-bg)" }
          : { borderColor: "color-mix(in srgb, var(--ui-primary) 30%, transparent)", color: "var(--ui-primary)", background: "color-mix(in srgb, var(--ui-primary) 12%, transparent)" }}
      >
        {actionLabel}
      </button>
    </div>
  );
}

function groupPermissions(items: PermissionLookupItem[]) {
  const map = new Map<string, PermissionLookupItem[]>();
  for (const item of items) {
    const key = item.transactionCode || "GENEL";
    if (!map.has(key)) {
      map.set(key, []);
    }
    map.get(key)!.push(item);
  }

  return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b));
}

/* ═══════════════════════════════════════ */
/*  TAB 1: BİLGİLER                        */
/* ═══════════════════════════════════════ */

function InfoTab({ mode, user, onSaved, onStateSaved, selectedRoleIds, selectedPermissionIds, formRef }: {
  mode: "create" | "edit"; user: UserDetail | null;
  onSaved: (id: number) => void;
  onStateSaved: (state: UserSaveState) => void;
  selectedRoleIds: number[];
  selectedPermissionIds: number[];
  formRef?: React.RefObject<HTMLFormElement | null>;
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
        userCode: d.userCode,
        firstName: d.firstName || null, lastName: d.lastName || null,
        email: d.email, password: d.password, companyId: d.companyId,
        notifyAdminByMail: d.notifyAdminByMail, adminEmail: d.adminEmail,
        roleIds: selectedRoleIds,
        permissionIds: selectedPermissionIds,
      });
      onStateSaved({
        firstName: d.firstName || null,
        lastName: d.lastName || null,
        email: d.email,
        isActive: true,
        mustChangePassword: d.mustChangePassword ?? true,
        profileImageUrl: d.profileImageUrl || null,
      });
      toast.success("Kullanıcı oluşturuldu");
      onSaved(res?.id ?? 0);
    } catch (e: unknown) {
      toast.error(extractApiError(e, "Oluşturma başarısız"));
    }
  };

  const updateMut = async (d: UpdateUserForm) => {
    if (!user) return;
    try {
      const nextState: UserSaveState = {
        firstName: d.firstName || null,
        lastName: d.lastName || null,
        email: d.email,
        isActive: d.isActive,
        mustChangePassword: d.mustChangePassword,
        profileImageUrl: d.profileImageUrl || null,
      };
      await usersApi.update(user.id, {
        ...nextState,
        roleIds: selectedRoleIds,
        permissionIds: selectedPermissionIds,
      });
      onStateSaved(nextState);
      toast.success("Bilgiler güncellendi");
    } catch (e: unknown) {
      toast.error(extractApiError(e, "Güncelleme başarısız"));
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
          <div className="flex items-center gap-1.5 text-[11px] ui-text-muted">
            <Wand2 size={11} className="ui-text-muted" />
            <span>Kod: </span>
            <span style={{ fontFamily: mono, fontWeight: 500, color: "var(--ui-primary)" }}>{preview || "—"}</span>
            {preview && wCode !== preview && (
              <button type="button" onClick={() => { setValue("userCode", preview); setAutoGen(true); toast.success(`Kod: ${preview}`, { duration: 1500 }); }}
                className="hover:underline" style={{ background: "none", border: "none", cursor: "pointer", padding: 0, fontSize: 11, color: "var(--ui-primary)" }}>Uygula</button>
            )}
          </div>
        )}

        <div className={sectionCls}><User size={14} style={{ color: "var(--ui-primary)" }} /> Hesap Bilgileri</div>
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
                      ? <span className="text-[10px] px-1.5 py-0.5 rounded-full" style={{ background: "var(--ui-success-bg)", color: "var(--ui-success)" }}>otomatik</span>
                      : <Pencil size={12} className="ui-text-muted" />}
                  </div>
                )}
              </div>
              <button type="button" disabled={!preview} onClick={() => { if (preview) { setValue("userCode", preview); setAutoGen(true); toast.success(`Kod: ${preview}`, { duration: 1500 }); } }}
                className="shrink-0 rounded-lg px-3.5 h-[40px] text-[12px] font-medium transition-all disabled:opacity-40"
                style={{ background: "color-mix(in srgb, var(--ui-primary) 14%, transparent)", border: "1px solid color-mix(in srgb, var(--ui-primary) 30%, transparent)", color: "var(--ui-primary)" }}>
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
        <div className={sectionCls + " mt-6"}><Shield size={14} style={{ color: "var(--ui-warning)" }} /> Bildirim</div>
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

      <div className={sectionCls}><User size={14} style={{ color: "var(--ui-primary)" }} /> Hesap Bilgileri</div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-5 gap-y-4">
        <div>
          <label className={labelCls}>E-posta *</label>
          <input {...register("email")} type="email" className={inputCls} />
          {errors.email && <p className={errCls}>{errors.email.message}</p>}
        </div>
        {user?.passwordExpiresAt && (
          <div className="flex items-center text-[12px] px-3 py-2 rounded-lg ui-text-muted" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
            Şifre geçerliliği: <strong className="ml-1 ui-text">{new Date(user.passwordExpiresAt).toLocaleDateString("tr-TR")}</strong>
          </div>
        )}
      </div>
      <div className={sectionCls + " mt-6"}><Shield size={14} style={{ color: "var(--ui-success)" }} /> Hesap Durumu</div>
      <div className="flex flex-wrap gap-6">
        <Toggle checked={isActive} onChange={(v) => { setIsActive(v); setValue("isActive", v); }} label="Aktif" color={isActive ? "var(--ui-success)" : undefined} />
        <Toggle checked={mustChange} onChange={(v) => { setMustChange(v); setValue("mustChangePassword", v); }} label="Şifre değişimi zorunlu" color={mustChange ? "var(--ui-warning)" : undefined} />
      </div>
    </form>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 2: ROL ATAMA                       */
/* ═══════════════════════════════════════ */

function RolesTab({ userId, allRoles, selectedRoleIds, onToggleRole }: { userId: number | null; allRoles: LookupItem[]; selectedRoleIds: number[]; onToggleRole: (roleId: number) => void }) {
  const [search, setSearch] = useState("");
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 8 } }));

  const searchQuery = search.trim().toLowerCase();
  const assigned = allRoles.filter((r) => selectedRoleIds.includes(r.id));
  const available = allRoles.filter((r) => !selectedRoleIds.includes(r.id));
  const filteredAvailable = searchQuery
    ? available.filter((r) => r.name.toLowerCase().includes(searchQuery))
    : available;

  function handleDragEnd(event: DragEndEvent) {
    const activeId = String(event.active.id);
    const overId = event.over ? String(event.over.id) : "";
    if (!activeId.startsWith("role:")) {
      return;
    }

    const roleId = Number(activeId.replace("role:", ""));
    if (!Number.isFinite(roleId)) {
      return;
    }

    if (overId === "roles:assigned" && !selectedRoleIds.includes(roleId)) {
      onToggleRole(roleId);
    }

    if (overId === "roles:available" && selectedRoleIds.includes(roleId)) {
      onToggleRole(roleId);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Mevcut rollerde ara..." className={inputCls + " max-w-xs"} />
        <p className="text-[12px] ui-text-muted">{userId ? "Rolleri sürükleyip bırakarak atayın" : "Seçimler kullanıcı oluşturulduğunda uygulanır"}</p>
      </div>

      <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <AssignmentDropZone
            id="roles:available"
            title="Mevcut Roller"
            hint="Kullanıcıya atanabilir roller"
            count={filteredAvailable.length}
            emptyText={searchQuery ? "Aramaya uygun rol bulunamadı" : "Atanmamış rol bulunmuyor"}
          >
            {filteredAvailable.map((role) => (
              <DraggableAssignmentItem
                key={role.id}
                id={`role:${role.id}`}
                title={role.name}
                subtitle={`Rol ID: ${role.id}`}
                activeTone="available"
                actionLabel="Ekle"
                onClick={() => onToggleRole(role.id)}
              />
            ))}
          </AssignmentDropZone>

          <AssignmentDropZone
            id="roles:assigned"
            title="Atanan Roller"
            hint="Kaydet ile kullanıcıya uygulanır"
            count={assigned.length}
            emptyText="Henüz rol atanmadı"
          >
            {assigned.map((role) => (
              <DraggableAssignmentItem
                key={role.id}
                id={`role:${role.id}`}
                title={role.name}
                subtitle={`Rol ID: ${role.id}`}
                activeTone="assigned"
                actionLabel="Çıkar"
                onClick={() => onToggleRole(role.id)}
              />
            ))}
          </AssignmentDropZone>
        </div>
      </DndContext>
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  TAB 3: YETKİ ATAMA                     */
/* ═══════════════════════════════════════ */

function PermissionsTab({ userId, allPermissions, selectedPermissionIds, onTogglePermission }: { userId: number | null; allPermissions: PermissionLookupItem[]; selectedPermissionIds: number[]; onTogglePermission: (permissionId: number) => void }) {
  const [search, setSearch] = useState("");
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 8 } }));

  const searchQuery = search.trim().toLowerCase();
  const assigned = allPermissions.filter((p) => selectedPermissionIds.includes(p.id));
  const available = allPermissions.filter((p) => !selectedPermissionIds.includes(p.id));
  const filteredAvailable = searchQuery
    ? available.filter((p) => {
      const haystack = `${p.transactionCode} ${p.actionCode} ${p.displayName} ${p.permissionCode} ${p.storedKey}`.toLowerCase();
      return haystack.includes(searchQuery);
    })
    : available;

  const availableGroups = useMemo(() => groupPermissions(filteredAvailable), [filteredAvailable]);
  const assignedGroups = useMemo(() => groupPermissions(assigned), [assigned]);

  function handleDragEnd(event: DragEndEvent) {
    const activeId = String(event.active.id);
    const overId = event.over ? String(event.over.id) : "";
    if (!activeId.startsWith("perm:")) {
      return;
    }

    const permissionId = Number(activeId.replace("perm:", ""));
    if (!Number.isFinite(permissionId)) {
      return;
    }

    if (overId === "perms:assigned" && !selectedPermissionIds.includes(permissionId)) {
      onTogglePermission(permissionId);
    }

    if (overId === "perms:available" && selectedPermissionIds.includes(permissionId)) {
      onTogglePermission(permissionId);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="TCode, action veya ad ile ara..." className={inputCls + " max-w-xs"} />
        <p className="text-[12px] ui-text-muted">{userId ? "Yetkileri gruplu şekilde sürükleyip bırakın" : "Seçimler kullanıcı oluşturulduğunda uygulanır"}</p>
      </div>

      <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <AssignmentDropZone
            id="perms:available"
            title="Mevcut Yetkiler"
            hint="Atanmamış izinler"
            count={filteredAvailable.length}
            emptyText={searchQuery ? "Aramaya uygun yetki bulunamadı" : "Atanabilir yetki yok"}
          >
            {availableGroups.map(([tcode, perms]) => (
              <div key={`available-${tcode}`} className="rounded-lg border" style={{ borderColor: "var(--ui-border)", background: "var(--ui-surface-alt)" }}>
                <div className="px-3 py-2 flex items-center justify-between" style={{ borderBottom: "1px solid var(--ui-border)" }}>
                  <span className="text-[11px] px-1.5 py-0.5 rounded" style={{ fontFamily: mono, background: "color-mix(in srgb, var(--ui-primary) 14%, transparent)", color: "var(--ui-primary)" }}>{tcode}</span>
                  <span className="text-[11px] ui-text-muted">{perms.length}</span>
                </div>
                <div className="p-2 space-y-2">
                  {perms.map((p) => (
                    <DraggableAssignmentItem
                      key={p.id}
                      id={`perm:${p.id}`}
                      title={p.permissionCode || p.displayName || `${p.transactionCode} ${p.actionCode}`}
                      subtitle={`Nav: ${p.navigationCode} · Key: ${p.storedKey}`}
                      activeTone="available"
                      actionLabel="Ekle"
                      onClick={() => onTogglePermission(p.id)}
                    />
                  ))}
                </div>
              </div>
            ))}
          </AssignmentDropZone>

          <AssignmentDropZone
            id="perms:assigned"
            title="Atanan Yetkiler"
            hint="Kaydet ile kullanıcıya uygulanır"
            count={assigned.length}
            emptyText="Henüz yetki atanmadı"
          >
            {assignedGroups.map(([tcode, perms]) => (
              <div key={`assigned-${tcode}`} className="rounded-lg border" style={{ borderColor: "var(--ui-border)", background: "var(--ui-surface-alt)" }}>
                <div className="px-3 py-2 flex items-center justify-between" style={{ borderBottom: "1px solid var(--ui-border)" }}>
                  <span className="text-[11px] px-1.5 py-0.5 rounded" style={{ fontFamily: mono, background: "var(--ui-warning-bg)", color: "var(--ui-warning)" }}>{tcode}</span>
                  <span className="text-[11px] ui-text-muted">{perms.length}</span>
                </div>
                <div className="p-2 space-y-2">
                  {perms.map((p) => (
                    <DraggableAssignmentItem
                      key={p.id}
                      id={`perm:${p.id}`}
                      title={p.permissionCode || p.displayName || `${p.transactionCode} ${p.actionCode}`}
                      subtitle={`Nav: ${p.navigationCode} · Key: ${p.storedKey}`}
                      activeTone="assigned"
                      actionLabel="Çıkar"
                      onClick={() => onTogglePermission(p.id)}
                    />
                  ))}
                </div>
              </div>
            ))}
          </AssignmentDropZone>
        </div>
      </DndContext>

      {allPermissions.length === 0 && (
        <p className="text-[13px] ui-text-muted text-center py-8">İzin tanımı bulunamadı</p>
      )}
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
  const { id: routeId } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const queryMode = searchParams.get("mode");
  const queryId = searchParams.get("id");
  const isFeatureMode = queryMode === "create" || queryMode === "edit";

  const isEdit = !!routeId || queryMode === "edit";
  const parsedRouteId = routeId ? Number(routeId) : null;
  const routeUserId = Number.isFinite(parsedRouteId) ? parsedRouteId : null;
  const queryUserId = queryId ? Number(queryId) : null;
  const effectiveEditUserId = routeUserId ?? (Number.isFinite(queryUserId) ? queryUserId : null);
  const [activeTab, setActiveTab] = useState<TabKey>("info");
  const [createdUserId, setCreatedUserId] = useState<number | null>(null);
  const [selectedRoleIds, setSelectedRoleIds] = useState<number[]>([]);
  const [selectedPermissionIds, setSelectedPermissionIds] = useState<number[]>([]);
  const [savedState, setSavedState] = useState<UserSaveState | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  const userId = isEdit ? effectiveEditUserId : createdUserId;

  const { data: user } = useQuery({
    queryKey: ["users", userId],
    queryFn: () => usersApi.getById(userId as number),
    enabled: !!userId,
  });

  const { data: lookups } = useQuery({ queryKey: ["user-lookups"], queryFn: () => usersApi.lookups(), staleTime: 60_000 });

  useEffect(() => {
    if (!user) return;
    setSavedState({
      firstName: user.firstName ?? null,
      lastName: user.lastName ?? null,
      email: user.email,
      isActive: user.isActive,
      mustChangePassword: user.mustChangePassword,
      profileImageUrl: user.profileImageUrl ?? null,
    });
    setSelectedRoleIds((user.roles ?? []).map((r) => r.roleId));
    setSelectedPermissionIds((user.directPermissions ?? []).map((p) => p.subModulePageId));
  }, [user?.id]);

  function handleCreated(newId: number) {
    setCreatedUserId(newId);
    setActiveTab("roles");
  }

  function toggleRole(roleId: number) {
    setSelectedRoleIds((prev) => prev.includes(roleId)
      ? prev.filter((x) => x !== roleId)
      : [...prev, roleId]);
  }

  function togglePermission(permissionId: number) {
    setSelectedPermissionIds((prev) => prev.includes(permissionId)
      ? prev.filter((x) => x !== permissionId)
      : [...prev, permissionId]);
  }

  async function saveAssignments() {
    if (!userId || !savedState) {
      toast.error("Önce kullanıcı bilgilerini kaydedin");
      return;
    }

    // Validate permission IDs exist in lookups
    if (selectedPermissionIds.length > 0 && lookups?.permissions) {
      const validIds = new Set(lookups.permissions.map((p) => p.id));
      const invalidIds = selectedPermissionIds.filter((id) => !validIds.has(id));
      
      if (invalidIds.length > 0) {
        toast.error(`Geçersiz yetki ID'leri: ${invalidIds.join(", ")}`);
        return;
      }
    }

    // Validate role IDs exist in lookups
    if (selectedRoleIds.length > 0 && lookups?.roles) {
      const validIds = new Set(lookups.roles.map((r) => r.id));
      const invalidIds = selectedRoleIds.filter((id) => !validIds.has(id));
      
      if (invalidIds.length > 0) {
        toast.error(`Geçersiz rol ID'leri: ${invalidIds.join(", ")}`);
        return;
      }
    }

    try {
      await usersApi.update(userId, {
        ...savedState,
        roleIds: selectedRoleIds,
        permissionIds: selectedPermissionIds,
      });
      toast.success("Rol ve yetkiler kaydedildi");
      navigate(isFeatureMode ? `/users?mode=detail&id=${userId}` : `/users/${userId}`);
    } catch (e: unknown) {
      const errorData = (e as { response?: { data?: { detail?: string; errors?: Record<string, string[]> } } }).response?.data;
      
      // Handle validation errors
      if (errorData?.errors?.permissionIds) {
        toast.error(`Yetki Hatası: ${errorData.errors.permissionIds.join(", ")}`);
      } else if (errorData?.errors?.roleIds) {
        toast.error(`Rol Hatası: ${errorData.errors.roleIds.join(", ")}`);
      } else {
        const msg = errorData?.detail ?? "Kaydetme başarısız";
        toast.error(msg);
      }
    }
  }

  const roleCount = selectedRoleIds.length;
  const permCount = selectedPermissionIds.length;

  return (
    <div className="flex flex-col gap-4 max-w-[820px]">
      <PageHeader
        title={isEdit ? `${user?.userCode ?? "..."} — Düzenle` : "Yeni Kullanıcı"}
        subtitle="Kullanıcı bilgilerini, rollerini ve yetkilerini yönetin"
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate("/users")}><ArrowLeft size={14} /> Vazgeç</PageAction>
            <PageAction onClick={() => {
              if (activeTab === "info") {
                formRef.current?.requestSubmit();
                return;
              }

              if (!userId) {
                setActiveTab("info");
                requestAnimationFrame(() => formRef.current?.requestSubmit());
                return;
              }

              void saveAssignments();
            }}><Check size={14} /> Kaydet</PageAction>
          </div>
        }
      />

      <div className="rounded-xl px-5 overflow-x-auto" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
        <div className="flex" style={{ borderBottom: "1px solid var(--ui-border)" }}>
          {TABS.map((tab) => {
            const active = activeTab === tab.key;
            const count = tab.key === "roles" ? roleCount : tab.key === "perms" ? permCount : 0;
            return (
              <button key={tab.key} onClick={() => setActiveTab(tab.key)}
                className="flex items-center gap-2 px-5 py-3.5 text-[13px] font-medium transition-all whitespace-nowrap"
                style={{ color: active ? "var(--ui-text)" : "var(--ui-text-muted)", borderBottom: active ? "2px solid var(--ui-primary)" : "2px solid transparent" }}>
                <tab.Icon size={15} />
                {tab.label}
                {count > 0 && <span className="text-[11px] px-1.5 py-0.5 rounded-full" style={{ background: "color-mix(in srgb, var(--ui-primary) 14%, transparent)", color: "var(--ui-primary)" }}>{count}</span>}
              </button>
            );
          })}
        </div>
      </div>

      <div className="rounded-xl p-6 sm:p-8" style={{ background: "var(--ui-card-bg)", border: "1px solid var(--ui-border)" }}>
        {activeTab === "info" && (
          <InfoTab
            mode={isEdit ? "edit" : "create"}
            user={user ?? null}
            onSaved={handleCreated}
            onStateSaved={setSavedState}
            selectedRoleIds={selectedRoleIds}
            selectedPermissionIds={selectedPermissionIds}
            formRef={formRef}
          />
        )}
        {activeTab === "roles" && (
          <RolesTab
            userId={userId}
            allRoles={lookups?.roles ?? []}
            selectedRoleIds={selectedRoleIds}
            onToggleRole={toggleRole}
          />
        )}
        {activeTab === "perms" && (
          <PermissionsTab
            userId={userId}
            allPermissions={lookups?.permissions ?? []}
            selectedPermissionIds={selectedPermissionIds}
            onTogglePermission={togglePermission}
          />
        )}
      </div>
    </div>
  );
}
