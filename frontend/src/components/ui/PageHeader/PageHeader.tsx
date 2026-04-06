export interface PageHeaderProps { title: string; subtitle?: string; actions?: React.ReactNode; }

const sans = "'Plus Jakarta Sans', sans-serif";

export default function PageHeader({ title, subtitle, actions }: PageHeaderProps) {
  return (
    <div className="mb-4 sm:mb-6 flex flex-col sm:flex-row sm:flex-wrap sm:items-start justify-between gap-3 sm:gap-4 pb-4 sm:pb-5"
      style={{ borderBottom: "1px solid #E2EBF3" }}>
      <div className="min-w-0">
        <h1 className="text-[16px] sm:text-[20px] font-medium tracking-[-0.02em]" style={{ color: "#1B3A5C", fontFamily: sans }}>{title}</h1>
        {subtitle && <p className="mt-0.5 sm:mt-1 text-[11px] sm:text-[12px]" style={{ color: "#7A96B0", fontFamily: sans }}>{subtitle}</p>}
      </div>
      {actions && <div className="flex items-center gap-2 w-full sm:w-auto sm:pt-1">{actions}</div>}
    </div>
  );
}

export function PageAction({ children, onClick, variant = "primary" }: { children: React.ReactNode; onClick?: () => void; variant?: "primary" | "ghost" }) {
  const isPrimary = variant === "primary";
  return (
    <button onClick={onClick}
      className="flex items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-[12px] font-medium transition-all w-full sm:w-auto min-h-[36px]"
      style={{ background: isPrimary ? "#1B3A5C" : "transparent", border: isPrimary ? "none" : "1px solid #D6E4F0", color: isPrimary ? "#FFFFFF" : "#7A96B0", fontFamily: "'JetBrains Mono', monospace" }}
      onMouseEnter={(e) => (e.currentTarget.style.background = isPrimary ? "#2E6DA4" : "#F7FAFD")}
      onMouseLeave={(e) => (e.currentTarget.style.background = isPrimary ? "#1B3A5C" : "transparent")}>
      {children}
    </button>
  );
}
