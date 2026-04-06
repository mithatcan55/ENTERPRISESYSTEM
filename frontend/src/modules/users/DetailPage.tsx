import { useParams, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { tr } from "date-fns/locale";
import { usersApi } from "./api";
import { PageHeader, PageAction } from "@/components/ui/PageHeader";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { ArrowLeft, Pencil } from "lucide-react";

const mono = "'JetBrains Mono', monospace";

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-start justify-between py-2.5" style={{ borderBottom: "1px solid #F0F4F8" }}>
      <span className="text-[12px] shrink-0" style={{ color: "#7A96B0" }}>{label}</span>
      <span className="text-[13px] text-right" style={{ color: "#1B3A5C" }}>{children}</span>
    </div>
  );
}

function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-xl p-5" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}>
      <div className="text-[13px] font-semibold mb-4 pb-3" style={{ color: "#1B3A5C", borderBottom: "1px solid #F0F4F8" }}>{title}</div>
      {children}
    </div>
  );
}

export default function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: user, isLoading } = useQuery({
    queryKey: ["users", id],
    queryFn: () => usersApi.getById(Number(id)),
    enabled: !!id,
  });

  if (isLoading) return <p style={{ color: "#7A96B0" }}>Yükleniyor...</p>;
  if (!user) return <p style={{ color: "#7A96B0" }}>Kullanıcı bulunamadı.</p>;

  const fmtDate = (d: string | null) => d ? format(new Date(d), "dd.MM.yyyy HH:mm", { locale: tr }) : "—";

  return (
    <div className="flex flex-col gap-4">
      <PageHeader title={user.userCode} subtitle={user.email}
        actions={
          <div className="flex gap-2 w-full sm:w-auto">
            <PageAction variant="ghost" onClick={() => navigate(-1)}><ArrowLeft size={14} /> Geri</PageAction>
            <PageAction onClick={() => navigate(`/users`)}><Pencil size={14} /> Düzenle</PageAction>
          </div>
        }
      />

      {/* Top — Avatar + Status */}
      <div className="rounded-xl p-5 flex items-center gap-4" style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}>
        <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full"
          style={{ background: "#EAF1FA", color: "#2E6DA4", fontSize: 18, fontWeight: 600 }}>
          {user.username.slice(0, 2).toUpperCase()}
        </div>
        <div>
          <div className="text-[18px] font-semibold" style={{ color: "#1B3A5C" }}>{user.userCode}</div>
          <div className="text-[13px]" style={{ color: "#7A96B0", fontFamily: mono }}>{user.email}</div>
          <div className="flex gap-2 mt-2">
            <StatusBadge status={user.isActive ? "active" : "inactive"} />
            {user.mustChangePassword && <StatusBadge status="warning" />}
            {user.isDeleted && <StatusBadge status="deleted" />}
          </div>
        </div>
      </div>

      {/* Detail grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card title="Kimlik Bilgileri">
          <Field label="Kod"><span style={{ fontFamily: mono, color: "#2E6DA4" }}>{user.userCode}</span></Field>
          <Field label="Ad">{(user as unknown as { firstName?: string }).firstName ?? "—"}</Field>
          <Field label="Soyad">{(user as unknown as { lastName?: string }).lastName ?? "—"}</Field>
          <Field label="E-posta">{user.email}</Field>
          <Field label="Durum"><StatusBadge status={user.isActive ? "active" : "inactive"} /></Field>
          <Field label="Şifre Durumu">{user.mustChangePassword ? "Değişim zorunlu" : "Normal"}</Field>
          <Field label="Şifre Son">{fmtDate(user.passwordExpiresAt)}</Field>
        </Card>

        <Card title="Denetim Bilgileri">
          <Field label="Oluşturulma">{fmtDate(user.createdAt)}</Field>
          <Field label="Oluşturan">{user.createdBy ?? "—"}</Field>
          <Field label="Son Güncelleme">{fmtDate(user.modifiedAt)}</Field>
          <Field label="Güncelleyen">{user.modifiedBy ?? "—"}</Field>
          <Field label="Silindi">{user.isDeleted ? fmtDate(user.deletedAt) : "Hayır"}</Field>
          <Field label="Profil Resmi">{user.profileImageUrl ? "Var" : "—"}</Field>
        </Card>
      </div>
    </div>
  );
}
