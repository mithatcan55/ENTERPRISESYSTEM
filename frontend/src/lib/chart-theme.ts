import type { CSSProperties } from "react";

export const gridProps = {
  stroke: "#E2EBF3",
  strokeDasharray: "none",
  vertical: false,
} as const;

const axisTick = {
  fill: "#7A96B0",
  fontSize: 10,
  fontFamily: "'JetBrains Mono', monospace",
};

export const xAxisProps = {
  tick: axisTick,
  stroke: "#E2EBF3",
  tickLine: false,
  axisLine: false,
} as const;

export const yAxisProps = {
  tick: axisTick,
  stroke: "#E2EBF3",
  tickLine: false,
  axisLine: false,
} as const;

export const tooltipContentStyle: CSSProperties = {
  background: "#FFFFFF",
  border: "1px solid #D6E4F0",
  borderRadius: 8,
};

export const tooltipLabelStyle: CSSProperties = {
  color: "#7A96B0",
  fontSize: 11,
  fontFamily: "'JetBrains Mono', monospace",
};

export const tooltipItemStyle: CSSProperties = {
  color: "#1B3A5C",
  fontSize: 12,
};

export const tooltipProps = {
  contentStyle: tooltipContentStyle,
  labelStyle: tooltipLabelStyle,
  itemStyle: tooltipItemStyle,
} as const;

export const barRadius: [number, number, number, number] = [3, 3, 0, 0];

export const activeDotProps = {
  fill: "#E05252",
  r: 4,
  stroke: "#FFFFFF",
  strokeWidth: 2,
};

export const accentPalette = {
  blue: "#2E6DA4",
  red: "#E05252",
  teal: "#1E8A6E",
  amber: "#D4891A",
  purple: "#7B61C2",
} as const;

export const gradientIds = {
  red: "chartGradientRed",
  blue: "chartGradientBlue",
  teal: "chartGradientTeal",
  amber: "chartGradientAmber",
} as const;

export const gradientStops: Record<
  keyof typeof gradientIds,
  { start: string; end: string }
> = {
  red: { start: "rgba(224,82,82,0.12)", end: "rgba(224,82,82,0)" },
  blue: { start: "rgba(46,109,164,0.12)", end: "rgba(46,109,164,0)" },
  teal: { start: "rgba(30,138,110,0.12)", end: "rgba(30,138,110,0)" },
  amber: { start: "rgba(212,137,26,0.12)", end: "rgba(212,137,26,0)" },
};
