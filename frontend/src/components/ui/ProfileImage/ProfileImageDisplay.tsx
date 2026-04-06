import { UserCircle } from "lucide-react";

export interface ProfileImageDisplayProps {
  src: string | null | undefined;
  displayName?: string;
  size?: number;
}

function getInitials(name: string): string {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2);
}

export default function ProfileImageDisplay({ src, displayName, size = 40 }: ProfileImageDisplayProps) {
  const fontSize = Math.max(10, Math.round(size * 0.35));

  if (src) {
    return (
      <img src={src} alt={displayName ?? ""} className="shrink-0 rounded-full object-cover"
        style={{ width: size, height: size }}
        onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = "none"; }} />
    );
  }

  if (displayName) {
    return (
      <div className="flex shrink-0 items-center justify-center rounded-full"
        style={{ width: size, height: size, background: "#EAF1FA", color: "#2E6DA4", fontSize, fontWeight: 600 }}>
        {getInitials(displayName)}
      </div>
    );
  }

  return <UserCircle size={size} style={{ color: "#D6E4F0" }} />;
}
