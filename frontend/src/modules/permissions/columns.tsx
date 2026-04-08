import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import { tr } from "date-fns/locale";
import { Pencil, Trash2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { UserActionPermission } from "./types";

const mono = "'JetBrains Mono', monospace";

interface ColumnCallbacks {
  onEdit: (permission: UserActionPermission) => void;
  onDelete: (permission: UserActionPermission) => void;
}

export function getPermissionColumns(cb: ColumnCallbacks): ColumnDef<UserActionPermission>[] {
  return [
    {
      accessorKey: "id",
      header: "ID",
      size: 84,
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, color: "#7A96B0" }}>
          {row.original.id}
        </span>
      ),
    },
    {
      accessorKey: "userId",
      header: "Kullanıcı ID",
      size: 110,
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, color: "#1B3A5C" }}>
          {row.original.userId}
        </span>
      ),
    },
    {
      accessorKey: "transactionCode",
      header: "TCode",
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, color: "#2E6DA4" }}>
          {row.original.transactionCode}
        </span>
      ),
    },
    {
      accessorKey: "actionCode",
      header: "Action",
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, color: "#1B3A5C" }}>
          {row.original.actionCode}
        </span>
      ),
    },
    {
      accessorKey: "subModulePageId",
      header: "Page ID",
      meta: { hideOnMobile: true },
      cell: ({ row }) => (
        <span style={{ fontFamily: mono, fontSize: 12, color: "#7A96B0" }}>
          {row.original.subModulePageId}
        </span>
      ),
    },
    {
      accessorKey: "isAllowed",
      header: "Durum",
      size: 120,
      cell: ({ row }) => {
        const isAllowed = row.original.isAllowed;
        return (
          <span
            className="inline-flex items-center rounded-md px-2 py-0.5 text-[11px] font-medium"
            style={isAllowed
              ? { background: "#E8F5EE", color: "#1E8A6E", border: "1px solid #C3E6D0" }
              : { background: "#FDECEA", color: "#C0392B", border: "1px solid #F5C6C2" }}
          >
            {isAllowed ? "İzinli" : "Engelli"}
          </span>
        );
      },
    },
    {
      accessorKey: "modifiedAt",
      header: "Güncelleme",
      meta: { hideOnTablet: true },
      cell: ({ row }) => {
        const value = row.original.modifiedAt ?? row.original.createdAt;
        return (
          <span style={{ fontSize: 12, color: "#7A96B0" }}>
            {format(new Date(value), "dd MMM yyyy", { locale: tr })}
          </span>
        );
      },
    },
    {
      id: "actions",
      header: "",
      size: 48,
      cell: ({ row }) => (
        <ActionMenu
          actions={[
            { label: "Düzenle", icon: Pencil, onClick: () => cb.onEdit(row.original) },
            { label: "Sil", icon: Trash2, onClick: () => cb.onDelete(row.original), variant: "danger" },
          ]}
        />
      ),
    },
  ];
}
