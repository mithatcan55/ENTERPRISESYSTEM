import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import apiClient from "@/api/client";
import { useAuthStore } from "@/store/auth-store";
import { decodeJwt } from "@/lib/jwt";
import type { LoginResponse, UserRole } from "@/types/auth";
import { User, Lock, Loader2, Users, GitBranch, BarChart3 } from "lucide-react";

const loginSchema = z.object({
  identifier: z.string().min(1, "Kullanıcı adı gerekli"),
  password: z.string().min(1, "Şifre gerekli"),
});
type LoginForm = z.infer<typeof loginSchema>;

const inputCls =
  "w-full bg-[#FAFCFF] border-[1.5px] border-[#E2EBF3] text-[#1B3A5C] rounded-[10px] h-[44px] pl-10 pr-3 text-[14px] placeholder:text-[#B0BEC5] focus:border-[#5B9BD5] focus:ring-2 focus:ring-[#5B9BD5]/10 focus:bg-white focus:outline-none transition-all";

const features = [
  { iconBg: "rgba(46,109,164,0.22)", Icon: Users, iconColor: "#5B9BD5", title: "Kimlik & Yetki Yönetimi", sub: "6 seviyeli TCode sistemi, rol tabanlı erişim kontrolü" },
  { iconBg: "rgba(30,138,110,0.22)", Icon: GitBranch, iconColor: "#1E8A6E", title: "Onay & Delegasyon Akışı", sub: "Dinamik iş akışı, çok adımlı onay süreçleri" },
  { iconBg: "rgba(212,137,26,0.22)", Icon: BarChart3, iconColor: "#D4891A", title: "Audit & ERP Entegrasyonu", sub: "Gerçek zamanlı log izleme, Canias ERP bağlantısı" },
];

const stats = [
  { value: "6", label: "YETKİ SEVİYESİ" },
  { value: "18+", label: "API MODÜLÜ" },
  { value: "100%", label: "DENETLENEBİLİR" },
];

export default function LoginPage() {
  const navigate = useNavigate();
  const { setTokens, setUser } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) });

  async function onSubmit(values: LoginForm) {
    setLoading(true);
    try {
      const { data } = await apiClient.post<LoginResponse>("/api/auth/login", values);
      setTokens(data.accessToken, data.refreshToken);
      const jwt = decodeJwt(data.accessToken);

      let roles: UserRole[] = [];
      if (data.effectiveAuthorization?.roles?.length) {
        roles = data.effectiveAuthorization.roles as UserRole[];
      } else {
        const jwtRole = jwt.role;
        if (Array.isArray(jwtRole)) roles = jwtRole as UserRole[];
        else if (typeof jwtRole === "string") roles = [jwtRole as UserRole];
      }

      setUser({
        id: String(data.userId ?? jwt.sub ?? ""),
        userName: data.username ?? (jwt.username as string) ?? "",
        displayName: data.username ?? (jwt.username as string) ?? "",
        roles,
      });
      navigate("/dashboard", { replace: true });
    } catch (err: unknown) {
      const message = (err as { response?: { data?: { message?: string } } }).response?.data?.message ?? "Giriş başarısız";
      toast.error(message);
    } finally { setLoading(false); }
  }

  return (
    <div className="flex min-h-screen" style={{ background: "#F0F4F8" }}>

      {/* ═══ Left panel — branding ═══ */}
      <div className="hidden lg:flex flex-col justify-center flex-1 relative overflow-hidden"
        style={{ background: "#1B3A5C" }}>

        {/* Geometric background SVG */}
        <svg style={{ position: "absolute", inset: 0, width: "100%", height: "100%", pointerEvents: "none", zIndex: 0 }}
          viewBox="0 0 800 650" preserveAspectRatio="xMidYMid slice" fill="none">
          <circle cx="780" cy="90" r="280" stroke="rgba(91,155,213,0.07)" strokeWidth="70" />
          <circle cx="780" cy="90" r="180" stroke="rgba(91,155,213,0.06)" strokeWidth="1" />
          <circle cx="780" cy="90" r="110" stroke="rgba(91,155,213,0.08)" strokeWidth="1" />
          <circle cx="780" cy="90" r="55" stroke="rgba(91,155,213,0.05)" strokeWidth="1" />
          <circle cx="-60" cy="560" r="260" stroke="rgba(91,155,213,0.05)" strokeWidth="60" />
          <circle cx="-60" cy="560" r="160" stroke="rgba(91,155,213,0.04)" strokeWidth="1" />
          <line x1="0" y1="300" x2="800" y2="160" stroke="rgba(91,155,213,0.04)" strokeWidth="1" />
          <line x1="0" y1="380" x2="800" y2="240" stroke="rgba(91,155,213,0.03)" strokeWidth="1" />
          <rect x="80" y="460" width="130" height="130" rx="18" stroke="rgba(91,155,213,0.06)" strokeWidth="1" transform="rotate(18 145 525)" />
          <circle cx="120" cy="210" r="3" fill="rgba(91,155,213,0.35)" />
          <circle cx="360" cy="340" r="2.5" fill="rgba(91,155,213,0.25)" />
          <circle cx="680" cy="230" r="2" fill="rgba(91,155,213,0.20)" />
          <circle cx="220" cy="500" r="2" fill="rgba(91,155,213,0.20)" />
          <circle cx="500" cy="120" r="1.5" fill="rgba(91,155,213,0.15)" />
        </svg>

        {/* Content */}
        <div className="px-12 xl:px-16" style={{ position: "relative", zIndex: 1 }}>
          {/* Logo */}
          <img src="/hm-aygun.png" alt="Hermann Müller / AYGÜN" className="brightness-0 invert mb-4" style={{ maxHeight: 38 }} />
          <p className="text-[14px] font-light tracking-wide mb-10" style={{ color: "rgba(255,255,255,0.42)" }}>
            Enterprise Management System
          </p>

          {/* Feature list */}
          <div className="flex flex-col gap-4 mb-9">
            {features.map((f) => (
              <div key={f.title} className="flex items-start gap-3">
                <div className="flex items-center justify-center shrink-0" style={{ width: 36, height: 36, borderRadius: 10, background: f.iconBg }}>
                  <f.Icon size={16} color={f.iconColor} />
                </div>
                <div>
                  <div className="text-[13px] font-semibold" style={{ color: "rgba(255,255,255,0.85)" }}>{f.title}</div>
                  <div className="text-[11px] leading-[1.5]" style={{ color: "rgba(255,255,255,0.38)" }}>{f.sub}</div>
                </div>
              </div>
            ))}
          </div>

          {/* Stats row */}
          <div className="flex pt-6" style={{ borderTop: "1px solid rgba(255,255,255,0.07)" }}>
            {stats.map((s, i) => (
              <div key={s.label} className="flex-1 text-center" style={{ borderRight: i < stats.length - 1 ? "1px solid rgba(255,255,255,0.07)" : "none" }}>
                <div className="text-[22px] font-bold tracking-[-0.02em]" style={{ color: "#FFFFFF" }}>{s.value}</div>
                <div className="text-[9px] tracking-[0.1em] mt-1" style={{ color: "rgba(255,255,255,0.32)" }}>{s.label}</div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* ═══ Right panel — form ═══ */}
      <div className="login-right flex flex-col justify-center items-center w-full lg:w-[480px] lg:shrink-0 px-6 sm:px-12"
        style={{ background: "#FFFFFF" }}>
        <div className="w-full max-w-[360px]">

          {/* Logo — mobile only */}
          <div className="lg:hidden flex justify-center mb-8">
            <img src="/hm-aygun.png" alt="Hermann Müller / AYGÜN" style={{ maxHeight: 40 }} />
          </div>

          {/* Tag pill */}
          <div className="inline-flex items-center gap-1.5 mb-5 px-3 py-1"
            style={{ background: "#EAF1FA", border: "1px solid #BDD5EC", borderRadius: 20 }}>
            <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
              <rect x="1" y="1" width="11" height="11" rx="2.5" stroke="#2E6DA4" strokeWidth="1.2" />
              <path d="M4.5 6.5h4M6.5 4.5v4" stroke="#2E6DA4" strokeWidth="1.2" strokeLinecap="round" />
            </svg>
            <span className="text-[11px] font-medium" style={{ color: "#2E6DA4" }}>Güvenli Kurumsal Giriş</span>
          </div>

          {/* Title */}
          <h1 className="text-[22px] font-semibold tracking-[-0.02em] mb-1" style={{ color: "#1B3A5C" }}>
            Hoş Geldiniz
          </h1>
          <p className="text-[13px] mb-8" style={{ color: "#7A96B0" }}>
            Hesabınıza giriş yapın
          </p>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
            {/* Identifier */}
            <div>
              <label className="block mb-1.5 text-[12px] font-medium" style={{ color: "#2C4A6B" }}>
                Kullanıcı Adı
              </label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4" style={{ color: "#A8C8E8" }} />
                <input {...register("identifier")} type="text" placeholder="kullanıcı adı veya e-posta"
                  className={inputCls} autoComplete="username" />
              </div>
              {errors.identifier && <p className="mt-1 text-[11px]" style={{ color: "#E05252" }}>{errors.identifier.message}</p>}
            </div>

            {/* Password */}
            <div>
              <label className="block mb-1.5 text-[12px] font-medium" style={{ color: "#2C4A6B" }}>
                Şifre
              </label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4" style={{ color: "#A8C8E8" }} />
                <input {...register("password")} type="password" placeholder="••••••••"
                  className={inputCls} autoComplete="current-password" />
              </div>
              {errors.password && <p className="mt-1 text-[11px]" style={{ color: "#E05252" }}>{errors.password.message}</p>}
            </div>

            {/* Submit */}
            <button type="submit" disabled={loading}
              className="w-full h-[46px] bg-[#1B3A5C] hover:bg-[#2E6DA4] text-white font-semibold text-[14px] rounded-[10px] border-none transition-colors duration-150 cursor-pointer disabled:opacity-80 flex items-center justify-center gap-2">
              {loading && <Loader2 className="h-4 w-4 animate-spin text-white" />}
              {loading ? "Giriş yapılıyor..." : "Giriş Yap"}
            </button>
          </form>

          {/* Footer */}
          <div className="mt-5 text-center text-[11px] leading-[1.6]" style={{ color: "#B0BEC5" }}>
            JWT + Refresh Token + Audit Log
            <br />
            <span style={{ color: "#5B9BD5" }}>TLS 1.3</span>
            {" · "}
            <span>Oturum izleme aktif</span>
          </div>
        </div>
      </div>
    </div>
  );
}
