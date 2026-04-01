type UserAvatarProps = {
  username: string;
  profileImageUrl?: string | null;
  size?: "sm" | "md" | "lg" | "xl";
};

function getInitials(username: string): string {
  const parts = username.trim().split(/[\s._-]+/).filter(Boolean);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }
  return username.slice(0, 2).toUpperCase();
}

function getColorFromUsername(username: string): string {
  const palette = [
    "#0c446d", "#1f6fb2", "#1f8a4c", "#7b3fa0",
    "#c98900", "#c62828", "#00695c", "#37474f",
  ];
  let hash = 0;
  for (let i = 0; i < username.length; i++) {
    hash = username.charCodeAt(i) + ((hash << 5) - hash);
  }
  return palette[Math.abs(hash) % palette.length];
}

const sizeMap = {
  sm: 28,
  md: 36,
  lg: 48,
  xl: 80,
};

export function UserAvatar({ username, profileImageUrl, size = "md" }: UserAvatarProps) {
  const px = sizeMap[size];
  const fontSize = Math.round(px * 0.38);
  const style: React.CSSProperties = {
    width: px,
    height: px,
    borderRadius: "50%",
    flexShrink: 0,
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    fontSize,
    fontWeight: 600,
    letterSpacing: "0.02em",
    overflow: "hidden",
    userSelect: "none",
  };

  if (profileImageUrl) {
    return (
      <span style={style}>
        <img
          src={profileImageUrl}
          alt={username}
          style={{ width: "100%", height: "100%", objectFit: "cover" }}
        />
      </span>
    );
  }

  return (
    <span
      style={{
        ...style,
        background: getColorFromUsername(username),
        color: "#fff",
      }}
      title={username}
    >
      {getInitials(username)}
    </span>
  );
}
