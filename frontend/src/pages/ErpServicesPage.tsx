import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import apiClient from "@/api/client";
import type { ErpService, ErpParam } from "@/types/erp";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Globe, Search } from "lucide-react";

export default function ErpServicesPage() {
  const [search, setSearch] = useState("");
  const [activeCategory, setActiveCategory] = useState<string | null>(null);
  const [selectedService, setSelectedService] = useState<ErpService | null>(
    null,
  );

  const { data: services, isLoading } = useQuery({
    queryKey: ["erp-services"],
    queryFn: async () => {
      const { data } = await apiClient.get<ErpService[]>("/api/erp/services");
      return data;
    },
  });

  const categories = useMemo(() => {
    if (!services) return [];
    return [...new Set(services.map((s) => s.category))].sort();
  }, [services]);

  const filtered = useMemo(() => {
    if (!services) return [];
    return services.filter((s) => {
      if (activeCategory && s.category !== activeCategory) return false;
      if (search) {
        const q = search.toLowerCase();
        return (
          s.name.toLowerCase().includes(q) ||
          s.endpoint.toLowerCase().includes(q) ||
          s.description.toLowerCase().includes(q)
        );
      }
      return true;
    });
  }, [services, activeCategory, search]);

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">ERP Servisleri</h1>

      {/* Search */}
      <div className="relative max-w-sm">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Servis ara..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Category filter badges */}
      <div className="flex flex-wrap gap-2">
        <Badge
          variant={activeCategory === null ? "default" : "outline"}
          className="cursor-pointer"
          onClick={() => setActiveCategory(null)}
        >
          Tümü
        </Badge>
        {categories.map((cat) => (
          <Badge
            key={cat}
            variant={activeCategory === cat ? "default" : "outline"}
            className="cursor-pointer"
            onClick={() =>
              setActiveCategory(activeCategory === cat ? null : cat)
            }
          >
            {cat}
          </Badge>
        ))}
      </div>

      {/* Service cards grid */}
      {isLoading ? (
        <p className="text-muted-foreground">Yükleniyor...</p>
      ) : filtered.length === 0 ? (
        <p className="text-muted-foreground">Servis bulunamadı.</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {filtered.map((svc) => (
            <Card
              key={svc.endpoint}
              className="cursor-pointer transition-shadow hover:shadow-md"
              onClick={() => setSelectedService(svc)}
            >
              <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-sm font-medium">
                    {svc.name}
                  </CardTitle>
                  <Badge variant="secondary" className="text-xs">
                    {svc.category}
                  </Badge>
                </div>
                <CardDescription className="text-xs truncate">
                  {svc.description}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span className="flex items-center gap-1 font-mono">
                    <Globe className="h-3 w-3" />
                    {svc.endpoint}
                  </span>
                  <span>{svc.parameterCount} param</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Detail dialog */}
      <ServiceDetailDialog
        service={selectedService}
        onClose={() => setSelectedService(null)}
      />
    </div>
  );
}

function ServiceDetailDialog({
  service,
  onClose,
}: {
  service: ErpService | null;
  onClose: () => void;
}) {
  const { data: params, isLoading } = useQuery({
    queryKey: ["erp-params", service?.endpoint],
    queryFn: async () => {
      const { data } = await apiClient.get<ErpParam[]>(
        `/api/erp/params/${encodeURIComponent(service!.endpoint)}`,
      );
      return data;
    },
    enabled: !!service,
  });

  return (
    <Dialog open={!!service} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-lg max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{service?.name}</DialogTitle>
          <DialogDescription>
            <span className="font-mono">{service?.endpoint}</span>
            {" — "}
            {service?.description}
          </DialogDescription>
        </DialogHeader>
        {isLoading ? (
          <p className="text-sm text-muted-foreground py-4">Yükleniyor...</p>
        ) : !params || params.length === 0 ? (
          <p className="text-sm text-muted-foreground py-4">
            Parametre bulunamadı.
          </p>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Parametre</TableHead>
                  <TableHead>Tip</TableHead>
                  <TableHead>Zorunlu</TableHead>
                  <TableHead>Açıklama</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {params.map((p) => (
                  <TableRow key={p.name}>
                    <TableCell className="font-mono text-xs">
                      {p.name}
                    </TableCell>
                    <TableCell className="text-xs">{p.type}</TableCell>
                    <TableCell>
                      <Badge
                        variant={p.isRequired ? "default" : "secondary"}
                        className="text-xs"
                      >
                        {p.isRequired ? "Evet" : "Hayır"}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-xs">{p.description}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
