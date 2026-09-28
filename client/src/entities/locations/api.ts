import type { LocationListItem, PagedResult } from "./type";
import { apiClient } from "@/shared/api/axios-instance";

export type CreateLocationRequest = {
  name: string;
  address: string;
  timezone: string;
};

export type GetLocationsRequest = {
  search?: string;
  minDepartmentCount?: number;
  SortBy?: string;
  SortDirection?: string;
  page: number;
  pageSize: number;
};

export type Envelope<T> = {
  result: T | null;
  errors: ApiError[] | null;
  timeGenerated: string;
};

export type ApiError = {
  code: string;
  message: string;
  type: ErrorType;
  invalidField: string | null;
};

// Значения enum ErrorType из backend.
// В текущей конфигурации они сериализуются числами.
export type ErrorType =
  | 0 // VALIDATION
  | 1 // NOT_FOUND
  | 2 // FAILURE
  | 3; // CONFLICT

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest,
    signal?: AbortSignal,
  ): Promise<PagedResult<LocationListItem>> => {
    const response = await apiClient.get<
      Envelope<PagedResult<LocationListItem>>
    >("/Locations", {
      params: request,
      signal,
    });

    const result = response.data.result;

    if (result === null) {
      throw new Error("Не удалось получить список локаций");
    }

    return result;
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post("/Locations", request);
    return response.data;
  },
};
