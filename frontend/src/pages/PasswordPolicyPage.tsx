import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import apiClient from "@/api/client";
import type { PasswordPolicy, PasswordPolicyValidation } from "@/types/ops";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ShieldCheck, ShieldX, FlaskConical } from "lucide-react";

const policySchema = z.object({
  minLength: z.number().min(1, "En az 1 olmalı"),
  requireUppercase: z.boolean(),
  requireLowercase: z.boolean(),
  requireDigit: z.boolean(),
  requireSpecialCharacter: z.boolean(),
  historyCount: z.number().min(0),
  minimumPasswordAgeMinutes: z.number().min(0),
});

type PolicyForm = z.infer<typeof policySchema>;

export default function PasswordPolicyPage() {
  const queryClient = useQueryClient();
  const [testPassword, setTestPassword] = useState("");
  const [testResult, setTestResult] = useState<{
    valid: boolean;
    errors: string[];
  } | null>(null);

  const { data: currentPolicy, isLoading } = useQuery({
    queryKey: ["password-policy"],
    queryFn: async () => {
      const { data } = await apiClient.get<PasswordPolicy>(
        "/api/ops/security/password-policy",
      );
      return data;
    },
  });

  const [reqUpper, setReqUpper] = useState(false);
  const [reqLower, setReqLower] = useState(false);
  const [reqDigit, setReqDigit] = useState(false);
  const [reqSpecial, setReqSpecial] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<PolicyForm>({
    resolver: zodResolver(policySchema),
    defaultValues: {
      minLength: 8,
      requireUppercase: false,
      requireLowercase: false,
      requireDigit: false,
      requireSpecialCharacter: false,
      historyCount: 0,
      minimumPasswordAgeMinutes: 0,
    },
  });

  // Load current policy into form when fetched
  const [loaded, setLoaded] = useState(false);
  if (currentPolicy && !loaded) {
    reset(currentPolicy);
    setReqUpper(currentPolicy.requireUppercase);
    setReqLower(currentPolicy.requireLowercase);
    setReqDigit(currentPolicy.requireDigit);
    setReqSpecial(currentPolicy.requireSpecialCharacter);
    setLoaded(true);
  }

  const dryRunMutation = useMutation({
    mutationFn: async (data: PolicyForm) => {
      const { data: result } =
        await apiClient.put<PasswordPolicyValidation>(
          "/api/ops/security/password-policy",
          data,
        );
      return result;
    },
    onSuccess: (result) => {
      if (result.isValidConfiguration) {
        queryClient.invalidateQueries({ queryKey: ["password-policy"] });
        toast.success("Politika güncellendi");
      } else {
        toast.error("Politika geçersiz");
      }
    },
    onError: (err: unknown) => {
      const message =
        (err as { response?: { data?: { message?: string } } }).response?.data
          ?.message ?? "İşlem başarısız";
      toast.error(message);
    },
  });

  function handleTestPassword() {
    if (!testPassword) {
      toast.error("Test şifresi girin");
      return;
    }
    const policy = currentPolicy;
    if (!policy) return;

    const errs: string[] = [];
    if (testPassword.length < policy.minLength)
      errs.push(`En az ${policy.minLength} karakter olmalı`);
    if (policy.requireUppercase && !/[A-Z]/.test(testPassword))
      errs.push("Büyük harf gerekli");
    if (policy.requireLowercase && !/[a-z]/.test(testPassword))
      errs.push("Küçük harf gerekli");
    if (policy.requireDigit && !/\d/.test(testPassword))
      errs.push("Rakam gerekli");
    if (
      policy.requireSpecialCharacter &&
      !/[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(testPassword)
    )
      errs.push("Özel karakter gerekli");

    setTestResult({ valid: errs.length === 0, errors: errs });
  }

  if (isLoading) {
    return <p className="text-muted-foreground">Yükleniyor...</p>;
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <h1 className="text-2xl font-semibold">Şifre Politikası</h1>

      {/* Current Policy Display */}
      {currentPolicy && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Mevcut Politika</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                Min. Uzunluk:{" "}
                <span className="font-medium">{currentPolicy.minLength}</span>
              </div>
              <div>
                Büyük Harf:{" "}
                <Badge
                  variant={
                    currentPolicy.requireUppercase ? "default" : "secondary"
                  }
                >
                  {currentPolicy.requireUppercase ? "Evet" : "Hayır"}
                </Badge>
              </div>
              <div>
                Küçük Harf:{" "}
                <Badge
                  variant={
                    currentPolicy.requireLowercase ? "default" : "secondary"
                  }
                >
                  {currentPolicy.requireLowercase ? "Evet" : "Hayır"}
                </Badge>
              </div>
              <div>
                Rakam:{" "}
                <Badge
                  variant={
                    currentPolicy.requireDigit ? "default" : "secondary"
                  }
                >
                  {currentPolicy.requireDigit ? "Evet" : "Hayır"}
                </Badge>
              </div>
              <div>
                Özel Karakter:{" "}
                <Badge
                  variant={
                    currentPolicy.requireSpecialCharacter
                      ? "default"
                      : "secondary"
                  }
                >
                  {currentPolicy.requireSpecialCharacter ? "Evet" : "Hayır"}
                </Badge>
              </div>
              <div>
                Geçmiş Sayısı:{" "}
                <span className="font-medium">
                  {currentPolicy.historyCount}
                </span>
              </div>
              <div>
                Min. Şifre Yaşı:{" "}
                <span className="font-medium">
                  {currentPolicy.minimumPasswordAgeMinutes} dk
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Separator />

      {/* Update Form */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Politika Güncelle</CardTitle>
          <CardDescription>
            Değerleri değiştirin ve kaydedin.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit((v) => dryRunMutation.mutate(v))}
            className="grid gap-4"
          >
            <div className="grid gap-2">
              <Label>Min. Uzunluk</Label>
              <Input type="number" {...register("minLength", { valueAsNumber: true })} />
              {errors.minLength && (
                <p className="text-sm text-destructive">
                  {errors.minLength.message}
                </p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="flex items-center gap-2">
                <Switch
                  checked={reqUpper}
                  onCheckedChange={(v) => {
                    setReqUpper(v);
                    setValue("requireUppercase", v);
                  }}
                />
                <Label>Büyük Harf</Label>
              </div>
              <div className="flex items-center gap-2">
                <Switch
                  checked={reqLower}
                  onCheckedChange={(v) => {
                    setReqLower(v);
                    setValue("requireLowercase", v);
                  }}
                />
                <Label>Küçük Harf</Label>
              </div>
              <div className="flex items-center gap-2">
                <Switch
                  checked={reqDigit}
                  onCheckedChange={(v) => {
                    setReqDigit(v);
                    setValue("requireDigit", v);
                  }}
                />
                <Label>Rakam</Label>
              </div>
              <div className="flex items-center gap-2">
                <Switch
                  checked={reqSpecial}
                  onCheckedChange={(v) => {
                    setReqSpecial(v);
                    setValue("requireSpecialCharacter", v);
                  }}
                />
                <Label>Özel Karakter</Label>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label>Geçmiş Sayısı</Label>
                <Input type="number" {...register("historyCount", { valueAsNumber: true })} />
              </div>
              <div className="grid gap-2">
                <Label>Min. Şifre Yaşı (dk)</Label>
                <Input
                  type="number"
                  {...register("minimumPasswordAgeMinutes", { valueAsNumber: true })}
                />
              </div>
            </div>

            <Button
              type="submit"
              disabled={dryRunMutation.isPending}
              className="w-fit"
            >
              {dryRunMutation.isPending ? "Kaydediliyor..." : "Kaydet"}
            </Button>

            {dryRunMutation.data && (
              <div className="mt-2">
                <Badge
                  variant={
                    dryRunMutation.data.isValidConfiguration
                      ? "default"
                      : "destructive"
                  }
                  className="text-sm"
                >
                  {dryRunMutation.data.isValidConfiguration
                    ? "Geçerli Konfigürasyon"
                    : "Geçersiz Konfigürasyon"}
                </Badge>
                {dryRunMutation.data.errors.length > 0 && (
                  <ul className="mt-2 text-sm text-destructive list-disc pl-4">
                    {dryRunMutation.data.errors.map((e, i) => (
                      <li key={i}>{e}</li>
                    ))}
                  </ul>
                )}
              </div>
            )}
          </form>
        </CardContent>
      </Card>

      <Separator />

      {/* Password Test */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <FlaskConical className="h-4 w-4" />
            Şifre Test
          </CardTitle>
          <CardDescription>
            Mevcut politikaya göre örnek şifre test edin.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex gap-2">
            <Input
              placeholder="Test şifresi girin..."
              value={testPassword}
              onChange={(e) => {
                setTestPassword(e.target.value);
                setTestResult(null);
              }}
              className="max-w-sm"
            />
            <Button variant="outline" onClick={handleTestPassword}>
              Test Et
            </Button>
          </div>

          {testResult && (
            <div className="flex items-start gap-2 mt-2">
              {testResult.valid ? (
                <>
                  <ShieldCheck className="h-5 w-5 text-success shrink-0 mt-0.5" />
                  <Badge variant="default">Geçerli</Badge>
                </>
              ) : (
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <ShieldX className="h-5 w-5 text-destructive shrink-0" />
                    <Badge variant="destructive">Geçersiz</Badge>
                  </div>
                  <ul className="text-sm text-destructive list-disc pl-4">
                    {testResult.errors.map((e, i) => (
                      <li key={i}>{e}</li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
