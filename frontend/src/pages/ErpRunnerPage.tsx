import { useMemo, useState } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  type ColumnDef,
} from "@tanstack/react-table";
import { toast } from "sonner";
import apiClient from "@/api/client";
import type { ErpService, ErpParam, ErpRunResponse } from "@/types/erp";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import KpiCard from "@/components/KpiCard";
import { Play, Download } from "lucide-react";

export default function ErpRunnerPage() {
  const [selectedEndpoint, setSelectedEndpoint] = useState("");
  const [formValues, setFormValues] = useState<Record<string, string>>({});
  const [result, setResult] = useState<ErpRunResponse | null>(null);

  /* ── Services list ── */
  const { data: services } = useQuery({
    queryKey: ["erp-services"],
    queryFn: async () => {
      const { data } = await apiClient.get<ErpService[]>("/api/erp/services");
      return data;
    },
  });

  /* ── Params for selected service ── */
  const { data: params } = useQuery({
    queryKey: ["erp-params", selectedEndpoint],
    queryFn: async () => {
      const { data } = await apiClient.get<ErpParam[]>(
        `/api/erp/params/${encodeURIComponent(selectedEndpoint)}`,
      );
      return data;
    },
    enabled: !!selectedEndpoint,
  });

  // Reset form when service changes
  function handleServiceChange(endpoint: string) {
    setSelectedEndpoint(endpoint);
    setFormValues({});
    setResult(null);
  }

  function setField(name: string, value: string) {
    setFormValues((prev) => ({ ...prev, [name]: value }));
  }

  /* ── Run query ── */
  const runMutation = useMutation({
    mutationFn: async () => {
      const payload: Record<string, string> = {};
      for (const [k, v] of Object.entries(formValues)) {
        if (v) payload[k] = v;
      }
      const { data } = await apiClient.post<ErpRunResponse>("/api/erp/run", {
        endpoint: selectedEndpoint,
        parameters: payload,
      });
      return data;
    },
    onSuccess: (data) => {
      setResult(data);
      toast.success(`${data.rowCount} satır, ${data.duration}`);
    },
    onError: (err: unknown) => {
      const message =
        (err as { response?: { data?: { message?: string } } }).response?.data
          ?.message ?? "Sorgu başarısız";
      toast.error(message);
    },
  });

  /* ── Export Excel ── */
  const exportMutation = useMutation({
    mutationFn: async () => {
      const payload: Record<string, string> = {};
      for (const [k, v] of Object.entries(formValues)) {
        if (v) payload[k] = v;
      }
      const { data } = await apiClient.post(
        "/api/erp/export-excel",
        { endpoint: selectedEndpoint, parameters: payload },
        { responseType: "blob" },
      );
      return data as Blob;
    },
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `${selectedEndpoint || "export"}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
      toast.success("Excel indirildi");
    },
    onError: () => {
      toast.error("Excel export başarısız");
    },
  });

  /* ── Handle submit ── */
  function handleRun() {
    if (!selectedEndpoint) {
      toast.error("Servis seçin");
      return;
    }
    // Validate required fields
    const missing = (params ?? []).filter(
      (p) => p.isRequired && !formValues[p.name],
    );
    if (missing.length > 0) {
      toast.error(
        `Zorunlu alanlar: ${missing.map((p) => p.name).join(", ")}`,
      );
      return;
    }
    runMutation.mutate();
  }

  /* ── Dynamic columns for result table ── */
  const columns = useMemo<ColumnDef<Record<string, unknown>>[]>(() => {
    if (!result?.columns) return [];
    return result.columns.map((col) => ({
      accessorKey: col,
      header: col,
      cell: ({ row }) => {
        const val = row.original[col];
        return val == null ? "—" : String(val);
      },
    }));
  }, [result?.columns]);

  const table = useReactTable({
    data: result?.rows ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="flex gap-6 h-[calc(100vh-7rem)]">
      {/* Left panel — service selector + dynamic form */}
      <div className="w-80 shrink-0 space-y-4 overflow-y-auto">
        <h1 className="text-2xl font-semibold">ERP Sorgu</h1>

        <div className="grid gap-2">
          <Label>Servis</Label>
          <Select value={selectedEndpoint} onValueChange={handleServiceChange}>
            <SelectTrigger>
              <SelectValue placeholder="Servis seçin..." />
            </SelectTrigger>
            <SelectContent>
              {(services ?? []).map((svc) => (
                <SelectItem key={svc.endpoint} value={svc.endpoint}>
                  {svc.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Dynamic param form */}
        {params && params.length > 0 && (
          <div className="space-y-3">
            {params.map((p) => (
              <div key={p.name} className="grid gap-1">
                <Label className="flex items-center gap-1 text-xs">
                  {p.name}
                  {p.isRequired && (
                    <Badge variant="destructive" className="text-[10px] px-1 py-0">
                      *
                    </Badge>
                  )}
                </Label>
                <Input
                  placeholder={p.description || p.defaultValue || p.type}
                  value={formValues[p.name] ?? ""}
                  onChange={(e) => setField(p.name, e.target.value)}
                />
              </div>
            ))}
          </div>
        )}

        <div className="flex gap-2 pt-2">
          <Button
            onClick={handleRun}
            disabled={runMutation.isPending || !selectedEndpoint}
            className="flex-1"
          >
            <Play className="mr-2 h-4 w-4" />
            {runMutation.isPending ? "Çalışıyor..." : "Çalıştır"}
          </Button>
          <Button
            variant="outline"
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending || !result}
          >
            <Download className="mr-2 h-4 w-4" />
            Excel
          </Button>
        </div>
      </div>

      {/* Right panel — result */}
      <div className="flex-1 overflow-hidden flex flex-col gap-3">
        {result && (
          <>
            {/* Stats */}
            <div className="flex gap-3">
              <KpiCard
                variant="teal"
                label="Satır Sayısı"
                value={result.rowCount}
                unit="adet"
                className="flex-1"
              />
              <KpiCard
                variant="blue"
                label="Süre"
                value={result.duration}
                className="flex-1"
              />
            </div>

            {/* Result table */}
            <div className="flex-1 overflow-auto rounded-md border">
              <Table>
                <TableHeader>
                  {table.getHeaderGroups().map((hg) => (
                    <TableRow key={hg.id}>
                      {hg.headers.map((header) => (
                        <TableHead key={header.id} className="whitespace-nowrap">
                          {header.isPlaceholder
                            ? null
                            : flexRender(
                                header.column.columnDef.header,
                                header.getContext(),
                              )}
                        </TableHead>
                      ))}
                    </TableRow>
                  ))}
                </TableHeader>
                <TableBody>
                  {table.getRowModel().rows.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={columns.length || 1}
                        className="text-center py-8"
                      >
                        Sonuç yok.
                      </TableCell>
                    </TableRow>
                  ) : (
                    table.getRowModel().rows.map((row) => (
                      <TableRow key={row.id}>
                        {row.getVisibleCells().map((cell) => (
                          <TableCell
                            key={cell.id}
                            className="whitespace-nowrap text-sm"
                          >
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext(),
                            )}
                          </TableCell>
                        ))}
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </>
        )}

        {!result && (
          <div className="flex-1 flex items-center justify-center text-muted-foreground">
            Servis seçip çalıştırın.
          </div>
        )}
      </div>
    </div>
  );
}
