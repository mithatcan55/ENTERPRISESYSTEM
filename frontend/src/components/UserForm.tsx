import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import apiClient from "@/api/client";
import type { CreateUserRequest } from "@/types/user";
import {
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";

const userSchema = z.object({
  userCode: z.string().min(1, "Kullanıcı kodu gerekli"),
  userName: z.string().min(1, "Kullanıcı adı gerekli"),
  email: z.email("Geçerli bir e-posta girin"),
  password: z.string().min(6, "Şifre en az 6 karakter olmalı"),
  companyId: z.string().min(1, "Şirket ID gerekli"),
  notifyAdminByMail: z.boolean(),
});

type UserFormValues = z.infer<typeof userSchema>;

interface UserFormProps {
  onSuccess: () => void;
}

export default function UserForm({ onSuccess }: UserFormProps) {
  const queryClient = useQueryClient();
  const [notify, setNotify] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<UserFormValues>({
    resolver: zodResolver(userSchema),
    defaultValues: { notifyAdminByMail: false },
  });

  const mutation = useMutation({
    mutationFn: (data: CreateUserRequest) =>
      apiClient.post("/api/users", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast.success("Kullanıcı oluşturuldu");
      reset();
      onSuccess();
    },
    onError: (err: unknown) => {
      const message =
        (err as { response?: { data?: { message?: string } } }).response?.data
          ?.message ?? "Kullanıcı oluşturulamadı";
      toast.error(message);
    },
  });

  function onSubmit(values: UserFormValues) {
    mutation.mutate(values);
  }

  return (
    <DialogContent className="sm:max-w-md">
      <DialogHeader>
        <DialogTitle>Yeni Kullanıcı</DialogTitle>
        <DialogDescription>Kullanıcı bilgilerini doldurun.</DialogDescription>
      </DialogHeader>
      <form onSubmit={handleSubmit(onSubmit)} className="grid gap-4 py-2">
        <div className="grid gap-2">
          <Label htmlFor="userCode">Kullanıcı Kodu</Label>
          <Input id="userCode" {...register("userCode")} />
          {errors.userCode && (
            <p className="text-sm text-destructive">{errors.userCode.message}</p>
          )}
        </div>
        <div className="grid gap-2">
          <Label htmlFor="userName">Kullanıcı Adı</Label>
          <Input id="userName" {...register("userName")} />
          {errors.userName && (
            <p className="text-sm text-destructive">{errors.userName.message}</p>
          )}
        </div>
        <div className="grid gap-2">
          <Label htmlFor="email">E-posta</Label>
          <Input id="email" type="email" {...register("email")} />
          {errors.email && (
            <p className="text-sm text-destructive">{errors.email.message}</p>
          )}
        </div>
        <div className="grid gap-2">
          <Label htmlFor="password">Şifre</Label>
          <Input id="password" type="password" {...register("password")} />
          {errors.password && (
            <p className="text-sm text-destructive">
              {errors.password.message}
            </p>
          )}
        </div>
        <div className="grid gap-2">
          <Label htmlFor="companyId">Şirket ID</Label>
          <Input id="companyId" {...register("companyId")} />
          {errors.companyId && (
            <p className="text-sm text-destructive">
              {errors.companyId.message}
            </p>
          )}
        </div>
        <div className="flex items-center gap-2">
          <Switch
            id="notifyAdminByMail"
            checked={notify}
            onCheckedChange={(val) => {
              setNotify(val);
              setValue("notifyAdminByMail", val);
            }}
          />
          <Label htmlFor="notifyAdminByMail">Admin'e mail gönder</Label>
        </div>
        <DialogFooter>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  );
}
