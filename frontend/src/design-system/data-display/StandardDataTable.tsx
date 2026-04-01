import type { ReactNode } from "react";
import { ChevronDown, ChevronUp, ChevronsUpDown, MoreHorizontal } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

export type TableColumn<TItem> = {
  key: string;
  header: string;
  mobileLabel?: string;
  sortable?: boolean;
  align?: "left" | "center" | "right";
  cell: (item: TItem) => ReactNode;
};

export type TableAction<TItem> = {
  key: string;
  label: string;
  onClick: (item: TItem) => void;
  hidden?: (item: TItem) => boolean;
  tone?: "default" | "danger";
};

type StandardDataTableProps<TItem> = {
  columns: Array<TableColumn<TItem>>;
  items: TItem[];
  rowKey: (item: TItem) => string | number;
  loading?: boolean;
  emptyTitle: string;
  emptyDescription: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  totalCount?: number;
  page?: number;
  pageSize?: number;
  onPageChange?: (page: number) => void;
  sortKey?: string;
  sortDirection?: "asc" | "desc";
  onSortChange?: (key: string) => void;
  actions?: Array<TableAction<TItem>>;
  onRowClick?: (item: TItem) => void;
  selectedRowKey?: string | number | null;
};

export function StandardDataTable<TItem>({
  columns,
  items,
  rowKey,
  loading,
  emptyTitle,
  emptyDescription,
  searchValue,
  onSearchChange,
  searchPlaceholder,
  totalCount,
  page = 1,
  pageSize = 10,
  onPageChange,
  sortKey,
  sortDirection,
  onSortChange,
  actions,
  onRowClick,
  selectedRowKey,
}: StandardDataTableProps<TItem>) {
  const { t } = useTranslation(["common"]);
  const [activeRowMenu, setActiveRowMenu] = useState<string | number | null>(null);
  const pageCount = totalCount ? Math.max(1, Math.ceil(totalCount / pageSize)) : 1;

  return (
    <section className="standard-table">
      <div className="standard-table__toolbar">
        {onSearchChange ? (
          <input
            className="standard-table__search"
            value={searchValue ?? ""}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder={searchPlaceholder}
          />
        ) : (
          <div />
        )}

        {totalCount !== undefined ? <span className="standard-table__count">{totalCount}</span> : null}
      </div>

      <div className="standard-table__desktop">
        <table>
          <thead>
            <tr>
              {columns.map((column) => (
                <th key={column.key} className={`table-align-${column.align ?? "left"}`}>
                  {column.sortable && onSortChange ? (
                    <button
                      className="standard-table__sort-button"
                      type="button"
                      onClick={() => onSortChange(column.key)}
                    >
                      <span>{column.header}</span>
                      {sortKey === column.key ? (
                        sortDirection === "asc" ? (
                          <ChevronUp size={15} />
                        ) : (
                          <ChevronDown size={15} />
                        )
                      ) : (
                        <ChevronsUpDown size={15} />
                      )}
                    </button>
                  ) : (
                    column.header
                  )}
                </th>
              ))}
              {actions?.length ? <th className="table-align-right">{t("common:tableActions")}</th> : null}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={columns.length + (actions?.length ? 1 : 0)}>
                  <div className="standard-table__empty">{t("common:loading")}</div>
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={columns.length + (actions?.length ? 1 : 0)}>
                  <div className="standard-table__empty">
                    <strong>{emptyTitle}</strong>
                    <span>{emptyDescription}</span>
                  </div>
                </td>
              </tr>
            ) : (
              items.map((item) => {
                const key = rowKey(item);
                const visibleActions = actions?.filter((action) => !action.hidden?.(item)) ?? [];
                const isSelected = selectedRowKey != null && key === selectedRowKey;

                return (
                  <tr
                    key={key}
                    className={[
                      onRowClick ? "standard-table__row--clickable" : "",
                      isSelected ? "standard-table__row--selected" : "",
                    ].filter(Boolean).join(" ") || undefined}
                    onClick={onRowClick ? () => onRowClick(item) : undefined}
                  >
                    {columns.map((column) => (
                      <td key={column.key} className={`table-align-${column.align ?? "left"}`}>
                        {column.cell(item)}
                      </td>
                    ))}
                    {actions?.length ? (
                      <td className="table-align-right" onClick={(e) => e.stopPropagation()}>
                        {visibleActions.length ? (
                          <div className="standard-table__row-menu">
                            <button
                              className="standard-table__icon-button"
                              type="button"
                              onClick={() => setActiveRowMenu(activeRowMenu === key ? null : key)}
                            >
                              <MoreHorizontal size={16} />
                            </button>
                            {activeRowMenu === key ? (
                              <div className="standard-table__row-menu-popover">
                                {visibleActions.map((action) => (
                                  <button
                                    key={action.key}
                                    className={action.tone === "danger" ? "row-action--danger" : undefined}
                                    type="button"
                                    onClick={() => {
                                      setActiveRowMenu(null);
                                      action.onClick(item);
                                    }}
                                  >
                                    {action.label}
                                  </button>
                                ))}
                              </div>
                            ) : null}
                          </div>
                        ) : null}
                      </td>
                    ) : null}
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      <div className="standard-table__mobile">
        {loading ? (
          <div className="standard-table__empty">{t("common:loading")}</div>
        ) : items.length === 0 ? (
          <div className="standard-table__empty">
            <strong>{emptyTitle}</strong>
            <span>{emptyDescription}</span>
          </div>
        ) : (
          items.map((item) => {
            const key = rowKey(item);
            const visibleActions = actions?.filter((action) => !action.hidden?.(item)) ?? [];

            return (
              <article
                key={key}
                className="standard-table__mobile-card"
                onClick={onRowClick ? () => onRowClick(item) : undefined}
              >
                {columns.map((column) => (
                  <div key={column.key} className="standard-table__mobile-row">
                    <span>{column.mobileLabel ?? column.header}</span>
                    <strong>{column.cell(item)}</strong>
                  </div>
                ))}

                {visibleActions.length ? (
                  <div className="standard-table__mobile-actions">
                    {visibleActions.map((action) => (
                      <button
                        key={action.key}
                        className={action.tone === "danger" ? "row-action--danger" : undefined}
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          action.onClick(item);
                        }}
                      >
                        {action.label}
                      </button>
                    ))}
                  </div>
                ) : null}
              </article>
            );
          })
        )}
      </div>

      {onPageChange && pageCount > 1 ? (
        <div className="standard-table__pagination">
          <button type="button" onClick={() => onPageChange(Math.max(1, page - 1))} disabled={page <= 1}>
            {t("common:previous")}
          </button>
          <span>
            {page} / {pageCount}
          </span>
          <button
            type="button"
            onClick={() => onPageChange(Math.min(pageCount, page + 1))}
            disabled={page >= pageCount}
          >
            {t("common:next")}
          </button>
        </div>
      ) : null}
    </section>
  );
}
