import { useState, useMemo, useCallback } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebounce } from "@/hooks/use-debounce";

/* ─── Types ─── */

export interface ListParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  search?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface UseListOptions<TFilters extends Record<string, unknown>> {
  queryKey: string;
  fetcher: (params: ListParams & TFilters) => Promise<PagedResult<unknown>>;
  defaultFilters?: TFilters;
  defaultPageSize?: number;
}

/* ─── Hook ─── */

export function useList<TFilters extends Record<string, unknown> = Record<string, never>>({
  queryKey,
  fetcher,
  defaultFilters,
  defaultPageSize = 20,
}: UseListOptions<TFilters>) {
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(defaultPageSize);
  const [sortBy, setSortBy] = useState<string | undefined>();
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
  const [search, setSearch] = useState("");
  const [filters, setFilters] = useState<TFilters>(
    (defaultFilters ?? {}) as TFilters,
  );

  const debouncedSearch = useDebounce(search, 300);

  // Build full params
  const params = useMemo<ListParams & TFilters>(
    () => ({
      page,
      pageSize,
      sortBy,
      sortDir,
      search: debouncedSearch || undefined,
      ...filters,
    }),
    [page, pageSize, sortBy, sortDir, debouncedSearch, filters],
  );

  // Cache key includes all params for proper invalidation
  const cacheKey = useMemo(
    () => [queryKey, params] as const,
    [queryKey, params],
  );

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: cacheKey,
    queryFn: () => fetcher(params),
  });

  // Sort toggle
  const setSort = useCallback(
    (column: string) => {
      if (sortBy === column) {
        setSortDir((d) => (d === "asc" ? "desc" : "asc"));
      } else {
        setSortBy(column);
        setSortDir("asc");
      }
      setPage(1);
    },
    [sortBy],
  );

  // Filter helpers
  const setFilter = useCallback(
    <K extends keyof TFilters>(key: K, value: TFilters[K]) => {
      setFilters((prev) => ({ ...prev, [key]: value }));
      setPage(1);
    },
    [],
  );

  const resetFilters = useCallback(() => {
    setFilters((defaultFilters ?? {}) as TFilters);
    setSearch("");
    setSortBy(undefined);
    setSortDir("asc");
    setPage(1);
  }, [defaultFilters]);

  // Page size change resets to page 1
  const changePageSize = useCallback((size: number) => {
    setPageSize(size);
    setPage(1);
  }, []);

  const invalidate = useCallback(
    () => queryClient.invalidateQueries({ queryKey: [queryKey] }),
    [queryClient, queryKey],
  );

  // Active filter count (non-empty, non-default values)
  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (debouncedSearch) count++;
    const defaults = (defaultFilters ?? {}) as Record<string, unknown>;
    for (const [key, val] of Object.entries(filters)) {
      if (val !== undefined && val !== "" && val !== defaults[key]) count++;
    }
    return count;
  }, [debouncedSearch, filters, defaultFilters]);

  return {
    // Data
    data: (data?.items ?? []) as unknown[],
    isLoading,
    totalCount: data?.totalCount ?? 0,
    error,

    // Pagination
    page,
    pageSize,
    setPage,
    setPageSize: changePageSize,

    // Sorting
    sortBy,
    sortDir,
    setSort,

    // Search
    search,
    setSearch,

    // Filters
    filters,
    setFilter,
    resetFilters,
    activeFilterCount,

    // Actions
    refetch,
    invalidate,
  };
}
