import { Search, X } from "lucide-react";

interface SelectOption { value: string; label: string; }
export interface FilterConfig {
  key: string; label: string; type: "select" | "boolean" | "daterange";
  options?: SelectOption[]; value?: string | boolean | [string, string]; onChange?: (value: unknown) => void;
}
interface SearchConfig { value: string; onChange: (value: string) => void; placeholder?: string; }
export interface FilterBarProps { search?: SearchConfig; filters?: FilterConfig[]; onReset?: () => void; activeCount?: number; }

const mono = "'JetBrains Mono', monospace";
const inputBase: React.CSSProperties = {
  background: "#F7FAFD", border: "1px solid #D6E4F0", color: "#1B3A5C",
  fontSize: 13, height: 34, borderRadius: 8,
};

function focusB(e: React.FocusEvent<HTMLElement>) { (e.currentTarget as HTMLElement).style.borderColor = "#5B9BD5"; }
function blurB(e: React.FocusEvent<HTMLElement>) { (e.currentTarget as HTMLElement).style.borderColor = "#D6E4F0"; }

function SearchInput({ value, onChange, placeholder }: SearchConfig) {
  return (
    <div className="relative w-full sm:w-auto sm:min-w-[240px]">
      <Search className="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2" style={{ color: "#B0BEC5" }} />
      <input type="text" value={value} onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder ?? "Ara..."} className="w-full rounded-lg pl-8 pr-3 text-[13px] outline-none"
        style={inputBase} onFocus={focusB} onBlur={blurB} />
    </div>
  );
}

function SelectFilter({ config }: { config: FilterConfig }) {
  return (
    <select value={String(config.value ?? "")} onChange={(e) => config.onChange?.(e.target.value)}
      className="w-full sm:w-auto rounded-lg px-2.5 text-[13px] outline-none appearance-none cursor-pointer"
      style={inputBase} onFocus={focusB} onBlur={blurB}>
      <option value="" style={{ background: "#F7FAFD" }}>{config.label}</option>
      {config.options?.map((o) => <option key={o.value} value={o.value} style={{ background: "#F7FAFD" }}>{o.label}</option>)}
    </select>
  );
}

function BooleanFilter({ config }: { config: FilterConfig }) {
  const pills: { label: string; val: string | boolean }[] = [{ label: "Tümü", val: "" }, { label: "Aktif", val: true }, { label: "Pasif", val: false }];
  return (
    <div className="flex items-center gap-0.5 rounded-lg p-0.5 overflow-x-auto" style={{ border: "1px solid #D6E4F0" }}>
      {pills.map((p) => {
        const isActive = (p.val === "" && (config.value === undefined || config.value === "")) || config.value === p.val;
        return <button key={String(p.val)} onClick={() => config.onChange?.(p.val)}
          className="rounded-md px-2.5 py-1 text-[11px] font-medium transition-all whitespace-nowrap min-h-[28px]"
          style={{ fontFamily: mono, background: isActive ? "#EAF1FA" : "transparent", color: isActive ? "#2E6DA4" : "#7A96B0" }}>{p.label}</button>;
      })}
    </div>
  );
}

function DateRangeFilter({ config }: { config: FilterConfig }) {
  const [from, to] = (config.value as [string, string]) ?? ["", ""];
  return (
    <div className="flex items-center gap-1 w-full sm:w-auto">
      <input type="date" value={from} onChange={(e) => config.onChange?.([e.target.value, to])}
        className="flex-1 sm:flex-none rounded-lg px-2 text-[12px] outline-none" style={{ ...inputBase, fontFamily: mono }} onFocus={focusB} onBlur={blurB} />
      <span className="text-[11px]" style={{ color: "#B0BEC5" }}>–</span>
      <input type="date" value={to} onChange={(e) => config.onChange?.([from, e.target.value])}
        className="flex-1 sm:flex-none rounded-lg px-2 text-[12px] outline-none" style={{ ...inputBase, fontFamily: mono }} onFocus={focusB} onBlur={blurB} />
    </div>
  );
}

export default function FilterBar({ search, filters, onReset, activeCount }: FilterBarProps) {
  return (
    <div className="flex flex-col sm:flex-row sm:flex-wrap items-stretch sm:items-center gap-2">
      {search && <SearchInput {...search} />}
      {filters?.map((f) => {
        switch (f.type) {
          case "select": return <SelectFilter key={f.key} config={f} />;
          case "boolean": return <BooleanFilter key={f.key} config={f} />;
          case "daterange": return <DateRangeFilter key={f.key} config={f} />;
          default: return null;
        }
      })}
      {onReset && activeCount !== undefined && activeCount > 0 && (
        <button onClick={onReset}
          className="flex items-center justify-center gap-1.5 rounded-lg px-2.5 py-1 text-[11px] font-medium transition-all min-h-[34px]"
          style={{ fontFamily: mono, border: "1px solid #F5C6C2", background: "#FDECEA", color: "#C0392B" }}>
          <X className="h-3 w-3" />Temizle
          <span className="rounded-full px-1.5 py-0.5 text-[10px] font-semibold leading-none"
            style={{ background: "#F5C6C2", color: "#C0392B" }}>{activeCount}</span>
        </button>
      )}
    </div>
  );
}
