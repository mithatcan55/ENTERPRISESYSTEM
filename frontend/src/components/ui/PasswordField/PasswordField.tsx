import { useState } from "react";
import { Lock, Eye, EyeOff, Copy, Wand2, ShieldCheck, Shield } from "lucide-react";
import { toast } from "sonner";
import { Switch } from "@/components/ui/switch";

export interface PasswordFieldProps {
  value: string;
  onChange: (val: string) => void;
  error?: string;
  isTemporary: boolean;
  onTemporaryChange: (val: boolean) => void;
}

const mono = "'JetBrains Mono', monospace";

function generatePassword(): string {
  const upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
  const lower = "abcdefghjkmnpqrstuvwxyz";
  const digits = "23456789";
  const special = "!@#$%&*";
  const all = upper + lower + digits + special;
  const pick = (s: string) => s[Math.floor(Math.random() * s.length)];
  const result = [pick(upper), pick(upper), pick(lower), pick(lower), pick(digits), pick(digits), pick(special)];
  while (result.length < 12) result.push(pick(all));
  return result.sort(() => Math.random() - 0.5).join("");
}

function getStrength(v: string) {
  let score = 0;
  if (v.length >= 8) score++;
  if (/[A-Z]/.test(v) && /[a-z]/.test(v)) score++;
  if (/[0-9]/.test(v)) score++;
  if (/[!@#$%^&*()_+\-=[\]{}|;':",.<>?]/.test(v)) score++;
  const map: Record<number, { colors: string[]; label: string; color: string }> = {
    0: { colors: ["#E05252", "#F0F4F8", "#F0F4F8", "#F0F4F8"], label: "Çok zayıf", color: "#E05252" },
    1: { colors: ["#E05252", "#F0F4F8", "#F0F4F8", "#F0F4F8"], label: "Çok zayıf", color: "#E05252" },
    2: { colors: ["#D4891A", "#D4891A", "#F0F4F8", "#F0F4F8"], label: "Zayıf", color: "#D4891A" },
    3: { colors: ["#2E6DA4", "#2E6DA4", "#2E6DA4", "#F0F4F8"], label: "İyi", color: "#2E6DA4" },
    4: { colors: ["#1E8A6E", "#1E8A6E", "#1E8A6E", "#1E8A6E"], label: "Güçlü", color: "#1E8A6E" },
  };
  return map[score];
}

export default function PasswordField({ value, onChange, error, isTemporary, onTemporaryChange }: PasswordFieldProps) {
  const [show, setShow] = useState(false);
  const [focused, setFocused] = useState(false);
  const strength = value.length > 0 ? getStrength(value) : null;

  function handleGenerate() {
    const pwd = generatePassword();
    onChange(pwd);
    setShow(true);
    navigator.clipboard.writeText(pwd);
    toast.success("Şifre oluşturuldu ve kopyalandı", { description: pwd, duration: 4000 });
  }

  function handleCopy() {
    if (!value) return;
    navigator.clipboard.writeText(value);
    toast.success("Kopyalandı", { duration: 1500 });
  }

  const borderColor = error ? "#E05252" : focused ? "#5B9BD5" : "#E2EBF3";

  return (
    <div>
      {/* A) Label row */}
      <div className="flex items-center justify-between mb-1">
        <label className="text-[12px] font-medium" style={{ color: "#2C4A6B" }}>Şifre *</label>
        <button type="button" onClick={handleGenerate}
          className="flex items-center gap-1 text-[12px] transition-colors hover:underline"
          style={{ color: "#5B9BD5", background: "none", border: "none", cursor: "pointer", padding: 0 }}>
          <Wand2 size={13} /> Otomatik Üret
        </button>
      </div>

      {/* B) Input row */}
      <div className="flex gap-2">
        <div className="relative flex-1">
          <Lock size={14} className="absolute left-2.5 top-1/2 -translate-y-1/2" style={{ color: "#A8C8E8" }} />
          <input
            type={show ? "text" : "password"}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            onFocus={() => setFocused(true)}
            onBlur={() => setFocused(false)}
            placeholder="••••••••"
            className="w-full outline-none transition-all"
            style={{
              height: 42, borderRadius: 8, fontFamily: mono, fontSize: 14, letterSpacing: "0.08em",
              background: "#FAFCFF", padding: "0 44px 0 36px", color: "#1B3A5C",
              border: `1.5px solid ${borderColor}`,
              boxShadow: focused ? "0 0 0 3px rgba(91,155,213,0.12)" : "none",
            }}
          />
          <button type="button" onClick={() => setShow(!show)}
            className="absolute right-2.5 top-1/2 -translate-y-1/2" style={{ color: "#B0BEC5", background: "none", border: "none", cursor: "pointer" }}>
            {show ? <EyeOff size={15} /> : <Eye size={15} />}
          </button>
        </div>
        <button type="button" onClick={handleCopy} title="Panoya kopyala"
          className="flex items-center justify-center shrink-0 transition-colors hover:bg-[#E2EBF3]"
          style={{ height: 42, width: 42, background: "#F0F4F8", border: "1px solid #E2EBF3", borderRadius: 8 }}>
          <Copy size={14} style={{ color: "#7A96B0" }} />
        </button>
      </div>
      {error && <p className="mt-1 text-[11px]" style={{ color: "#E05252" }}>{error}</p>}

      {/* C) Strength bar */}
      {strength && (
        <div className="flex items-center gap-1 mt-1.5">
          <div className="flex gap-[3px] flex-1">
            {strength.colors.map((c, i) => (
              <div key={i} style={{ flex: 1, height: 3, borderRadius: 2, background: c, transition: "background 0.3s" }} />
            ))}
          </div>
          <span className="text-[11px] ml-2" style={{ color: strength.color }}>{strength.label}</span>
        </div>
      )}

      {/* D) Temporary toggle */}
      <div className="mt-2 flex items-center justify-between rounded-lg cursor-pointer transition-all"
        onClick={() => onTemporaryChange(!isTemporary)}
        style={{
          padding: "10px 14px",
          border: `1.5px solid ${isTemporary ? "#1E8A6E" : "#E2EBF3"}`,
          background: isTemporary ? "#E8F5EE" : "#FAFCFF",
        }}>
        <div className="flex items-center gap-2.5">
          {isTemporary ? <ShieldCheck size={15} style={{ color: "#1E8A6E" }} /> : <Shield size={15} style={{ color: "#B0BEC5" }} />}
          <div>
            <div className="text-[13px] font-medium" style={{ color: isTemporary ? "#1E8A6E" : "#1B3A5C" }}>Geçici şifre</div>
            <div className="text-[11px]" style={{ color: isTemporary ? "#1E8A6E" : "#7A96B0" }}>
              {isTemporary ? "Kullanıcı ilk girişte değiştirmek zorunda kalacak" : "Kalıcı — kullanıcı serbestçe kullanabilir"}
            </div>
          </div>
        </div>
        <Switch checked={isTemporary} onCheckedChange={onTemporaryChange} onClick={(e) => e.stopPropagation()} />
      </div>
    </div>
  );
}
