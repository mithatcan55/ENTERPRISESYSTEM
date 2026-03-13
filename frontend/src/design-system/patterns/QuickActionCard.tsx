import { ArrowRight } from "lucide-react";
import { Link } from "react-router-dom";

export function QuickActionCard({
  title,
  description,
  to
}: {
  title: string;
  description: string;
  to: string;
}) {
  return (
    <Link className="quick-action-card" to={to}>
      <div>
        <strong>{title}</strong>
        <p>{description}</p>
      </div>
      <ArrowRight size={18} />
    </Link>
  );
}
