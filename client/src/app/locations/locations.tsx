"use client";

import { locationsApi } from "@/entities/locations/api";
import type { LocationListItem, PagedResult } from "@/entities/locations/type";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Spinner } from "@/shared/components/ui/spinner";
import {
  CircleAlert,
  MapPin,
  Plus,
  Search,
  SlidersHorizontal,
} from "lucide-react";
import { useEffect, useState } from "react";

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export default function Locations() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE_OPTIONS[0]);
  const [data, setData] = useState<PagedResult<LocationListItem> | null>(null);

  const [retryCount, setRetryCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    locationsApi
      .getLocations({ page, pageSize }, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return;
        setData(result);
      })
      .catch((error) => {
        if (controller.signal.aborted) return;

        setError(
          error instanceof Error
            ? error.message
            : "Не удалось загрузить локации",
        );
      })
      .finally(() => {
        if (controller.signal.aborted) return;
        setIsLoading(false);
      });

    return () => {
      controller.abort();
    };
  }, [page, pageSize, retryCount]);

  const changePage = (nextPage: number) => {
    setIsLoading(true);
    setError(null);
    setPage(nextPage);
  };

  const changePageSize = (value: string) => {
    setIsLoading(true);
    setError(null);
    setPageSize(Number(value));
    setPage(1);
  };

  return (
    <div className="mx-auto w-full max-w-7xl px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
      <div className="flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="mb-3 flex items-center gap-2 text-sm text-muted-foreground">
            <MapPin className="size-4" aria-hidden="true" />
            Справочник компании
          </div>
          <h1 className="text-3xl font-semibold tracking-tight">
            Офисы и локации
          </h1>
          <p className="mt-2 max-w-xl text-sm text-muted-foreground">
            Адреса офисов, часовые пояса и доступность для сотрудников.
          </p>
        </div>
        <Button size="lg" className="self-start sm:self-auto">
          <Plus className="size-4" aria-hidden="true" />
          Добавить локацию
        </Button>
      </div>

      {error ? (
        <div
          role="alert"
          className="mt-7 flex min-h-56 flex-col items-center justify-center gap-4 rounded-md border border-destructive/20 bg-muted/20 px-6 py-10 text-center"
        >
          <div className="flex max-w-xl flex-col items-center">
            <CircleAlert
              className="mb-3 size-5 text-destructive"
              aria-hidden="true"
            />
            <p className="font-medium">Не удалось загрузить локации</p>
            <p className="mt-1 text-sm text-muted-foreground">{error}</p>
          </div>
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              setError(null);
              setIsLoading(true);
              setRetryCount((count) => count + 1);
            }}
          >
            Повторить
          </Button>
        </div>
      ) : (
        <section className="mt-7" aria-label="Список офисных локаций">
          <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-base font-semibold">Все локации</h2>
              <p className="mt-1 text-xs text-muted-foreground">
                {data && <span>{data.totalCount} записей в справочнике</span>}
              </p>
            </div>
            <div className="flex flex-col gap-2 sm:flex-row">
              <label className="relative block sm:w-64">
                <Search
                  className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                  aria-hidden="true"
                />
                <Input
                  type="search"
                  placeholder="Поиск по локациям"
                  aria-label="Поиск по локациям"
                  className="h-9 w-full rounded-md bg-background pl-9 pr-3 text-sm"
                />
              </label>
              <Button
                type="button"
                variant="outline"
                size="lg"
                aria-label="Фильтры"
              >
                <SlidersHorizontal className="size-4" aria-hidden="true" />
                Фильтры
              </Button>
            </div>
          </div>

          <div className="overflow-hidden rounded-md border border-border">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[1000px] border-collapse text-left text-sm">
                <thead className="bg-muted/40 text-xs font-medium text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">ID</th>
                    <th className="px-4 py-3">Локация</th>
                    <th className="px-4 py-3">Адрес</th>
                    <th className="px-4 py-3">Количество отделов</th>
                    <th className="px-4 py-3">Дата создания</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {isLoading ? (
                    <tr>
                      <td colSpan={5} className="py-10 text-center">
                        <Spinner className="mx-auto text-muted-foreground" />
                      </td>
                    </tr>
                  ) : data?.items.length === 0 ? (
                    <tr>
                      <td
                        colSpan={5}
                        className="py-10 text-center text-sm text-muted-foreground"
                      >
                        Локации не добавлены
                      </td>
                    </tr>
                  ) : (
                    data?.items.map((location) => (
                      <tr key={location.id}>
                        <td>{location.id}</td>
                        <td>{location.name}</td>
                        <td>{location.address}</td>
                        <td>{location.departmentCount}</td>
                        <td>
                          {new Date(location.createdAt).toLocaleDateString(
                            "ru-RU",
                          )}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
            <div className="flex flex-col gap-3 border-t border-border px-4 py-3 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between sm:px-5">
              <div className="flex items-center gap-2">
                <label htmlFor="locations-page-size">Показывать по</label>
                <select
                  id="locations-page-size"
                  value={pageSize}
                  onChange={(event) => changePageSize(event.target.value)}
                  className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  aria-label="Размер страницы"
                >
                  {PAGE_SIZE_OPTIONS.map((option) => (
                    <option key={option} value={option}>
                      {option}
                    </option>
                  ))}
                </select>
              </div>
              {data && data.pageCount > 0 && (
                <div className="flex items-center gap-2 self-end sm:self-auto">
                  <Button
                    type="button"
                    disabled={data.pageNumber <= 1 || isLoading}
                    variant="outline"
                    size="sm"
                    onClick={() => changePage(page - 1)}
                  >
                    Назад
                  </Button>
                  <span className="px-1 text-foreground">
                    {data.pageNumber} / {data.pageCount}
                  </span>
                  <Button
                    type="button"
                    disabled={data.pageNumber >= data.pageCount || isLoading}
                    variant="outline"
                    size="sm"
                    onClick={() => changePage(page + 1)}
                  >
                    Вперёд
                  </Button>
                </div>
              )}
            </div>
          </div>
        </section>
      )}
    </div>
  );
}
