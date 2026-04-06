import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import apiClient from "@/api/client";
import type {
  ActionPermissionsResponse,
  TCodeAccessResult,
  UpsertPermissionRequest,
} from "@/types/permission";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ShieldCheck, ShieldX, AlertTriangle, Search } from "lucide-react";

/* ─── Action Permissions Query Tab ─── */

function ActionPermissionsTab() {
  const [userId, setUserId] = useState("");
  const [transactionCode, setTransactionCode] = useState("");
  const [queryParams, setQueryParams] = useState<{
    userId: string;
    transactionCode: string;
  } | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["permissions-actions", queryParams],
    queryFn: async () => {
      const { data } = await apiClient.get<ActionPermissionsResponse>(
        "/api/permissions/actions",
        { params: queryParams! },
      );
      return data;
    },
    enabled: !!queryParams,
  });

  function handleSearch() {
    if (!userId || !transactionCode) {
      toast.error("Kullanıcı ID ve işlem kodu gerekli");
      return;
    }
    setQueryParams({ userId, transactionCode });
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-3">
        <div className="grid gap-1.5">
          <Label>Kullanıcı ID</Label>
          <Input
            placeholder="Kullanıcı ID"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            className="w-56"
          />
        </div>
        <div className="grid gap-1.5">
          <Label>İşlem Kodu (TCode)</Label>
          <Input
            placeholder="ör: MM01"
            value={transactionCode}
            onChange={(e) => setTransactionCode(e.target.value)}
            className="w-56"
          />
        </div>
        <Button onClick={handleSearch} disabled={isLoading}>
          <Search className="mr-2 h-4 w-4" />
          Sorgula
        </Button>
      </div>

      {data && (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Aksiyon Kodu</TableHead>
                <TableHead>İzin Durumu</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.actions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={2} className="text-center py-6">
                    Kayıt bulunamadı.
                  </TableCell>
                </TableRow>
              ) : (
                data.actions.map((action) => (
                  <TableRow key={action.actionCode}>
                    <TableCell className="font-mono">
                      {action.actionCode}
                    </TableCell>
                    <TableCell>
                      {action.isAllowed ? (
                        <Badge variant="default">İzinli</Badge>
                      ) : (
                        <Badge variant="destructive">Engelli</Badge>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

/* ─── Upsert Permission Tab ─── */

const upsertSchema = z.object({
  userId: z.string().min(1, "Kullanıcı ID gerekli"),
  transactionCode: z.string().min(1, "İşlem kodu gerekli"),
  actionCode: z.string().min(1, "Aksiyon kodu gerekli"),
  isAllowed: z.boolean(),
});

type UpsertForm = z.infer<typeof upsertSchema>;

function UpsertPermissionTab() {
  const queryClient = useQueryClient();
  const [allowed, setAllowed] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<UpsertForm>({
    resolver: zodResolver(upsertSchema),
    defaultValues: { isAllowed: true },
  });

  const mutation = useMutation({
    mutationFn: (data: UpsertPermissionRequest) =>
      apiClient.put("/api/permissions/actions", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["permissions-actions"] });
      toast.success("İzin güncellendi");
      reset();
      setAllowed(true);
    },
    onError: (err: unknown) => {
      const message =
        (err as { response?: { data?: { message?: string } } }).response?.data
          ?.message ?? "İzin güncellenemedi";
      toast.error(message);
    },
  });

  return (
    <form
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      className="grid gap-4 max-w-lg"
    >
      <div className="grid gap-2">
        <Label>Kullanıcı ID</Label>
        <Input {...register("userId")} />
        {errors.userId && (
          <p className="text-sm text-destructive">{errors.userId.message}</p>
        )}
      </div>
      <div className="grid gap-2">
        <Label>İşlem Kodu (TCode)</Label>
        <Input {...register("transactionCode")} />
        {errors.transactionCode && (
          <p className="text-sm text-destructive">
            {errors.transactionCode.message}
          </p>
        )}
      </div>
      <div className="grid gap-2">
        <Label>Aksiyon Kodu</Label>
        <Input {...register("actionCode")} />
        {errors.actionCode && (
          <p className="text-sm text-destructive">
            {errors.actionCode.message}
          </p>
        )}
      </div>
      <div className="flex items-center gap-2">
        <Switch
          checked={allowed}
          onCheckedChange={(val) => {
            setAllowed(val);
            setValue("isAllowed", val);
          }}
        />
        <Label>{allowed ? "İzinli" : "Engelli"}</Label>
      </div>
      <Button type="submit" disabled={mutation.isPending} className="w-fit">
        {mutation.isPending ? "Kaydediliyor..." : "Kaydet"}
      </Button>
    </form>
  );
}

/* ─── TCode Access Test Tab ─── */

function TCodeTestTab() {
  const [code, setCode] = useState("");
  const [testCode, setTestCode] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["tcode-access", testCode],
    queryFn: async () => {
      const { data } = await apiClient.get<TCodeAccessResult>(
        `/api/tcode/${testCode}`,
      );
      return data;
    },
    enabled: !!testCode,
  });

  function handleTest() {
    if (!code) {
      toast.error("TCode gerekli");
      return;
    }
    setTestCode(code);
  }

  return (
    <div className="space-y-4">
      <div className="flex items-end gap-3">
        <div className="grid gap-1.5">
          <Label>TCode</Label>
          <Input
            placeholder="ör: MM01"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            className="w-56"
          />
        </div>
        <Button onClick={handleTest} disabled={isLoading}>
          <Search className="mr-2 h-4 w-4" />
          Test Et
        </Button>
      </div>

      {data && <TCodeResultCard result={data} />}
    </div>
  );
}

function TCodeResultCard({ result }: { result: TCodeAccessResult }) {
  const actionEntries = Object.entries(result.actions);

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center gap-3">
          {result.isAllowed ? (
            <ShieldCheck className="h-8 w-8 text-success" />
          ) : (
            <ShieldX className="h-8 w-8 text-destructive" />
          )}
          <div>
            <CardTitle className="flex items-center gap-2">
              Erişim Sonucu
              <Badge variant={result.isAllowed ? "default" : "destructive"}>
                {result.isAllowed ? "İZİNLİ" : "ENGELLİ"}
              </Badge>
            </CardTitle>
            <CardDescription>6 seviyeli TCode erişim kontrolü</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Denied info */}
        {!result.isAllowed && (
          <div className="rounded-md bg-destructive/10 p-3 space-y-1">
            {result.deniedAtLevel && (
              <p className="text-sm">
                <span className="font-medium">Engellenme Seviyesi:</span>{" "}
                {result.deniedAtLevel}
              </p>
            )}
            {result.deniedReason && (
              <p className="text-sm">
                <span className="font-medium">Sebep:</span>{" "}
                {result.deniedReason}
              </p>
            )}
          </div>
        )}

        {/* Actions map */}
        {actionEntries.length > 0 && (
          <>
            <Separator />
            <div>
              <h4 className="text-sm font-medium mb-2">Aksiyonlar</h4>
              <div className="flex flex-wrap gap-2">
                {actionEntries.map(([action, allowed]) => (
                  <Badge
                    key={action}
                    variant={allowed ? "default" : "secondary"}
                    className="font-mono"
                  >
                    {action}: {allowed ? "✓" : "✗"}
                  </Badge>
                ))}
              </div>
            </div>
          </>
        )}

        {/* Missing context fields */}
        {result.missingContextFields.length > 0 && (
          <>
            <Separator />
            <div className="flex items-start gap-2">
              <AlertTriangle className="h-4 w-4 text-warning mt-0.5 shrink-0" />
              <div>
                <h4 className="text-sm font-medium">Eksik Context Alanları</h4>
                <div className="flex flex-wrap gap-1 mt-1">
                  {result.missingContextFields.map((field) => (
                    <Badge key={field} variant="outline" className="font-mono">
                      {field}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

/* ─── Main Page ─── */

export default function PermissionsPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">İzin Yönetimi</h1>
      <Tabs defaultValue="query">
        <TabsList>
          <TabsTrigger value="query">Sorgula</TabsTrigger>
          <TabsTrigger value="upsert">İzin Ekle/Güncelle</TabsTrigger>
          <TabsTrigger value="tcode">TCode Test</TabsTrigger>
        </TabsList>
        <TabsContent value="query" className="mt-4">
          <ActionPermissionsTab />
        </TabsContent>
        <TabsContent value="upsert" className="mt-4">
          <UpsertPermissionTab />
        </TabsContent>
        <TabsContent value="tcode" className="mt-4">
          <TCodeTestTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}
