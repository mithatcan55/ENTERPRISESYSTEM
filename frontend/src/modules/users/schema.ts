import { z } from "zod/v4";

export const createUserSchema = z.object({
  firstName: z.string().min(1, "Ad zorunlu"),
  lastName: z.string().min(1, "Soyad zorunlu"),
  userCode: z.string().min(2, "En az 2 karakter").max(20, "En fazla 20 karakter").regex(/^[A-Z0-9]+$/, "Sadece harf ve rakam"),
  email: z.email("Geçerli e-posta giriniz"),
  password: z.string().min(6, "En az 6 karakter"),
  companyId: z.number().min(1, "Şirket ID zorunlu"),
  notifyAdminByMail: z.boolean(),
  adminEmail: z.string().optional(),
  mustChangePassword: z.boolean().optional(),
});

export const updateUserSchema = z.object({
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  email: z.email("Geçerli e-posta giriniz"),
  isActive: z.boolean(),
  mustChangePassword: z.boolean(),
  passwordExpiresAt: z.string().nullable().optional(),
  profileImageUrl: z.string().nullable().optional(),
});

export type CreateUserForm = z.infer<typeof createUserSchema>;
export type UpdateUserForm = z.infer<typeof updateUserSchema>;
