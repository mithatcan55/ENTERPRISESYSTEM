import { useState, useCallback } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

/* ─── Types ─── */

type ModalMode = "create" | "edit" | "delete" | null;

export interface UseCrudOptions<T = unknown> {
  queryKey: string;
  createFn?: (data: unknown) => Promise<T>;
  updateFn?: (id: unknown, data: unknown) => Promise<T>;
  deleteFn?: (id: unknown) => Promise<unknown>;
}

function extractError(err: unknown): string {
  const resp = (err as { response?: { data?: { detail?: string; message?: string } } })
    .response?.data;
  return resp?.detail ?? resp?.message ?? "İşlem başarısız";
}

/* ─── Hook ─── */

export function useCrud<T = unknown>({
  queryKey,
  createFn,
  updateFn,
  deleteFn,
}: UseCrudOptions<T>) {
  const queryClient = useQueryClient();

  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selectedItem, setSelectedItem] = useState<T | null>(null);

  // Invalidate list query on success
  const onSuccess = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: [queryKey] });
  }, [queryClient, queryKey]);

  /* ─── Create ─── */

  const createMutation = useMutation({
    mutationFn: (data: unknown) => {
      if (!createFn) throw new Error("createFn not provided");
      return createFn(data);
    },
    onSuccess: () => {
      toast.success("Kayıt oluşturuldu");
      onSuccess();
      closeModal();
    },
    onError: (err: unknown) => toast.error(extractError(err)),
  });

  /* ─── Update ─── */

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: unknown; data: unknown }) => {
      if (!updateFn) throw new Error("updateFn not provided");
      return updateFn(id, data);
    },
    onSuccess: () => {
      toast.success("Kayıt güncellendi");
      onSuccess();
      closeModal();
    },
    onError: (err: unknown) => toast.error(extractError(err)),
  });

  /* ─── Delete ─── */

  const deleteMutation = useMutation({
    mutationFn: (id: unknown) => {
      if (!deleteFn) throw new Error("deleteFn not provided");
      return deleteFn(id);
    },
    onSuccess: () => {
      toast.success("Kayıt silindi");
      onSuccess();
      closeModal();
    },
    onError: (err: unknown) => toast.error(extractError(err)),
  });

  /* ─── Modal controls ─── */

  const openCreate = useCallback(() => {
    setSelectedItem(null);
    setModalMode("create");
  }, []);

  const openEdit = useCallback((item: T) => {
    setSelectedItem(item);
    setModalMode("edit");
  }, []);

  const openDelete = useCallback((item: T) => {
    setSelectedItem(item);
    setModalMode("delete");
  }, []);

  const closeModal = useCallback(() => {
    setModalMode(null);
    setSelectedItem(null);
  }, []);

  /* ─── Public API ─── */

  return {
    // Mutations
    create: (data: unknown) => createMutation.mutateAsync(data),
    update: (id: unknown, data: unknown) =>
      updateMutation.mutateAsync({ id, data }),
    delete: (id: unknown) => deleteMutation.mutateAsync(id),

    // Loading states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Modal
    modalMode,
    openCreate,
    openEdit,
    openDelete,
    closeModal,
    selectedItem,
  };
}
