export type OfficeLocation = {
  id: string;
  name: string;
  address: string;
  timezone: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type LocationListItem = {
  id: string;
  name: string;
  address: string;
  createdAt: string;
  departmentCount: number;
  totalCount: number;
};

export type PagedResult<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  totalCount: number;
};
