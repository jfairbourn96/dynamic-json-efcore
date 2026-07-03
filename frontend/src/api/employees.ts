import type { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeSearchFilters, PagedResult } from '../types/records';
import { api } from './client';

export const employeesApi = {
  getById: (id: string) => api.get<Employee>(`/employees/${id}`),
  search: (filters: EmployeeSearchFilters, pageNumber = 1, pageSize = 20) =>
    api.post<PagedResult<Employee>>('/employees/search', { ...filters, pageNumber, pageSize }),
  create: (body: CreateEmployeeRequest) => api.post<Employee>('/employees', body),
  update: (id: string, body: UpdateEmployeeRequest) => api.put<Employee>(`/employees/${id}`, body),
  delete: (id: string) => api.delete(`/employees/${id}`),
};
