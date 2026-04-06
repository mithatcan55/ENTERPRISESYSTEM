import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import { tr } from "date-fns/locale";
import type { UserListItem } from "./api";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { ProfileImageDisplay } from "@/components/ui/ProfileImage";
import { Pencil, Eye, UserX, UserCheck, Trash2 } from "lucide-react";

const mono = "'JetBrains Mono', monospace";

interface ColumnCallbacks {
  onEdit: (u: UserListItem) => void;
  onDetail: (u: UserListItem) => void;
  onDeactivate: (u: UserListItem) => void;
  onReactivate: (u: UserListItem) => void;
  onDelete: (u: UserListItem) => void;
}

export function getUserColumns(cb: ColumnCallbacks): ColumnDef<UserListItem>[] {
  return [
    {
      accessorKey: "userCode",
      header: "Kod",
      size: 120,
      meta: { sortKey: "usercode" },
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, fontWeight: 400, color: "#2E6DA4", letterSpacing: "0.01em" }}>
          {row.original.userCode}
        </span>
      ),
    },
    {
      id: "displayName",
      header: "Ad Soyad",
      meta: { sortKey: "username" },
      cell: ({ row }) => {
        const u = row.original;
        return (
          <div className="flex items-center gap-2">
            <ProfileImageDisplay src={u.profileImageUrl} displayName={u.username} size={28} />
            <span style={{ fontSize: 13, fontWeight: 500, color: "#1B3A5C" }}>{u.username}</span>
          </div>
        );
      },
    },
    {
      accessorKey: "email",
      header: "E-posta",
      meta: { hideOnMobile: true, sortKey: "email" },
      cell: ({ row }) => (
        <span style={{ fontSize: 12, fontWeight: 400, color: "#7A96B0" }}>{row.original.email}</span>
      ),
    },
    {
      accessorKey: "primaryRoleName",
      header: "Rol",
      cell: ({ row }) => {
        const role = row.original.primaryRoleName;
        if (!role) return <span style={{ color: "#D6E4F0" }}>—</span>;
        return (
          <span className="inline-flex items-center rounded-full px-2 py-0.5"
            style={{ background: "#EAF1FA", color: "#2E6DA4", border: "1px solid #BDD5EC", fontSize: 11 }}>
            {role}
          </span>
        );
      },
    },
    {
      accessorKey: "isActive",
      header: "Durum",
      size: 120,
      cell: ({ row }) => (
        <div className="flex flex-col gap-1">
          <StatusBadge status={row.original.isActive ? "active" : "inactive"} />
          {row.original.mustChangePassword && (
            <span className="inline-flex items-center rounded px-1.5 py-0.5"
              style={{ background: "#FEF3E2", color: "#D4891A", border: "1px solid #F5D99A", fontSize: 10 }}>
              Şifre değişmeli
            </span>
          )}
        </div>
      ),
    },
    {
      accessorKey: "createdAt",
      header: "Oluşturulma",
      meta: { hideOnMobile: true, sortKey: "createdat" },
      cell: ({ row }) => (
        <span style={{ fontSize: 12, color: "#7A96B0" }}>
          {format(new Date(row.original.createdAt), "dd MMM yyyy", { locale: tr })}
        </span>
      ),
    },
    {
      id: "actions",
      header: "",
      size: 48,
      cell: ({ row }) => {
        const u = row.original;
        return (
          <ActionMenu actions={[
            { label: "Düzenle", icon: Pencil, onClick: () => cb.onEdit(u) },
            { label: "Detay", icon: Eye, onClick: () => cb.onDetail(u) },
            { label: u.isActive ? "Pasife Al" : "Aktive Et", icon: u.isActive ? UserX : UserCheck, onClick: () => (u.isActive ? cb.onDeactivate(u) : cb.onReactivate(u)) },
            { label: "Sil", icon: Trash2, onClick: () => cb.onDelete(u), variant: "danger" },
          ]} />
        );
      },
    },
  ];
}
