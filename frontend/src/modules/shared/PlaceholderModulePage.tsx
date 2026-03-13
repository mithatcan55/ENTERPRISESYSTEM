import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";

export function PlaceholderModulePage({
  title,
  description
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="page-grid">
      <PageHeader title={title} description={description} />
      <PanelCard title="Modul Iskeleti" subtitle="Starter-kit hazir">
        <p>
          Bu alan frontend foundation tamamlandiktan sonra standard data table, standard form ve responsive shell
          davranislari ile doldurulacak.
        </p>
      </PanelCard>
    </div>
  );
}
