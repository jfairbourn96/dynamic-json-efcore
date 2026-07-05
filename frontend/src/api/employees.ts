import type { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeSearchFilters, PagedResult } from '../types/records';
import { api } from './client';

export const employeesApi = {
  getById: (id: string) => api.get<Employee>(`/employees/${id}`),
  search: (filters: EmployeeSearchFilters, pageNumber = 1, pageSize = 20) => {
    const params = new URLSearchParams();

    const appendIfPresent = (key: string, value: unknown) => {
      if (value === undefined || value === null || value === '') {
        return;
      }

      params.append(key, String(value));
    };

    Object.entries(filters).forEach(([key, value]) => {
      appendIfPresent(key, value);
    });

    appendIfPresent('pageNumber', pageNumber);
    appendIfPresent('pageSize', pageSize);

    return api.get<PagedResult<Employee>>(`/employees/search?${params.toString()}`);
  },
  create: (body: CreateEmployeeRequest) => api.post<Employee>('/employees', body),
  update: (id: string, body: UpdateEmployeeRequest) => api.put<Employee>(`/employees/${id}`, body),
  delete: (id: string) => api.delete(`/employees/${id}`),
};
