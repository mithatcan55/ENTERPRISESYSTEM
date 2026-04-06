import { useNavigate } from "react-router-dom";
import { ShieldX } from "lucide-react";

export default function ForbiddenPage() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
      <ShieldX className="h-16 w-16" style={{ color: "#E05252" }} />
      <h1
        className="text-[24px] font-medium tracking-[-0.02em]"
        style={{ color: "#FFFFFF" }}
      >
        Erişim Engellendi
      </h1>
      <p
        className="text-[14px]"
        style={{ color: "#7A96B0" }}
      >
        Bu sayfaya erişim yetkiniz bulunmamaktadır.
      </p>
      <button
        onClick={() => navigate("/dashboard")}
        className="mt-2 rounded-md px-4 py-2 text-[13px] font-medium transition-all"
        style={{
          background: "#1B3A5C",
          border: "none",
          color: "#FFFFFF",
          fontFamily: "'JetBrains Mono', monospace",
        }}
      >
        Dashboard'a Dön
      </button>
    </div>
  );
}
