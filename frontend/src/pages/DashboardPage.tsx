import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  BarChart,
  Bar,
  Line,
  Area,
  AreaChart,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import apiClient from "@/api/client";
import type { AuditDashboardSummary } from "@/types/ops";
import KpiCard from "@/components/KpiCard";
import TopBar from "@/components/TopBar";
import {
  gridProps,
  xAxisProps,
  yAxisProps,
  tooltipProps,
  barRadius,
  activeDotProps,
  accentPalette,
  gradientIds,
  gradientStops,
} from "@/lib/chart-theme";

const windowOptions = [
  { value: "6", label: "6s" },
  { value: "12", label: "12s" },
  { value: "24", label: "24s" },
  { value: "48", label: "48s" },
  { value: "72", label: "72s" },
];

function ChartPanel({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle: string;
  children: React.ReactNode;
}) {
  return (
    <div
      className="overflow-hidden rounded-[10px] p-5"
      style={{
        background: "#FFFFFF",
        border: "1px solid #E2EBF3",
      }}
    >
      <div className="mb-4">
        <div
          className="text-[14px] font-medium"
          style={{ color: "#1B3A5C" }}
        >
          {title}
        </div>
        <div
          className="mt-0.5 text-[12px]"
          style={{ color: "#7A96B0" }}
        >
          {subtitle}
        </div>
      </div>
      {children}
    </div>
  );
}

export default function DashboardPage() {
  const [windowHours, setWindowHours] = useState("24");

  const { data, isLoading } = useQuery({
    queryKey: ["audit-dashboard", windowHours],
    queryFn: async () => {
      const { data } = await apiClient.get<AuditDashboardSummary>(
        "/api/ops/audit/dashboard/summary",
        { params: { windowHours } },
      );
      return data;
    },
  });

  return (
    <div className="space-y-6">
      <TopBar
        title="Dashboard"
        subtitle="Audit & güvenlik özeti"
        windowOptions={windowOptions}
        activeWindow={windowHours}
        onWindowChange={setWindowHours}
        showLive
      />

      {/* KPI Cards */}
      {isLoading ? (
        <p style={{ color: "#7A96B0" }}>Yükleniyor...</p>
      ) : data ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <KpiCard
              variant="blue"
              label="Toplam Olay"
              value={data.totalEvents}
              unit="adet"
            />
            <KpiCard
              variant="teal"
              label="Tekil Kullanıcı"
              value={data.uniqueUsers}
              unit="adet"
            />
            <KpiCard
              variant="amber"
              label="Başarısız Giriş"
              value={data.failedLogins}
              unit="adet"
              delta={
                data.failedLogins > 0
                  ? { type: "up", text: `${data.failedLogins} başarısız deneme` }
                  : { type: "ok", text: "Başarısız giriş yok" }
              }
            />
            <KpiCard
              variant="red"
              label="Kritik Olay"
              value={data.criticalEvents}
              unit="adet"
              delta={
                data.criticalEvents > 0
                  ? { type: "up", text: `${data.criticalEvents} kritik` }
                  : { type: "ok", text: "Kritik olay yok" }
              }
            />
          </div>

          {/* Charts row */}
          <div className="grid gap-4 lg:grid-cols-2">
            {/* Event Breakdown — Bar */}
            {data.eventBreakdown && data.eventBreakdown.length > 0 && (
              <ChartPanel
                title="Olay Dağılımı"
                subtitle={`Son ${windowHours} saat içindeki olay tipleri`}
              >
                <ResponsiveContainer width="100%" height={200}>
                  <BarChart data={data.eventBreakdown}>
                    <CartesianGrid {...gridProps} />
                    <XAxis dataKey="label" {...xAxisProps} />
                    <YAxis {...yAxisProps} />
                    <Tooltip {...tooltipProps} />
                    <Bar
                      dataKey="count"
                      name="Adet"
                      fill={accentPalette.blue}
                      radius={barRadius}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </ChartPanel>
            )}

            {/* Failed Login Trend — Area + Line */}
            {data.eventBreakdown && data.eventBreakdown.length > 0 && (
              <ChartPanel
                title="Başarısız Giriş Trendi"
                subtitle="Zaman bazlı başarısız giriş denemeleri"
              >
                <ResponsiveContainer width="100%" height={200}>
                  <AreaChart data={data.eventBreakdown}>
                    <defs>
                      <linearGradient
                        id={gradientIds.red}
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="0%"
                          stopColor={gradientStops.red.start}
                        />
                        <stop
                          offset="100%"
                          stopColor={gradientStops.red.end}
                        />
                      </linearGradient>
                    </defs>
                    <CartesianGrid {...gridProps} />
                    <XAxis dataKey="label" {...xAxisProps} />
                    <YAxis {...yAxisProps} />
                    <Tooltip {...tooltipProps} />
                    <Area
                      type="monotone"
                      dataKey="count"
                      stroke="transparent"
                      fill={`url(#${gradientIds.red})`}
                    />
                    <Line
                      type="monotone"
                      dataKey="count"
                      name="Adet"
                      stroke={accentPalette.red}
                      strokeWidth={1.5}
                      dot={false}
                      activeDot={activeDotProps}
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </ChartPanel>
            )}
          </div>
        </>
      ) : null}
    </div>
  );
}
