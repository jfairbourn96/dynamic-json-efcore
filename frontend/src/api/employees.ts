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

    appendIfPresent('firstName', filters.firstName);
    appendIfPresent('lastName', filters.lastName);
    appendIfPresent('email', filters.email);
    appendIfPresent('department', filters.department);
    appendIfPresent('employeeTypeId', filters.employeeTypeId);
    appendIfPresent('pageNumber', pageNumber);
    appendIfPresent('pageSize', pageSize);

    Object.entries(filters.fieldValues ?? {}).forEach(([name, value]) => {
      appendIfPresent(`fieldValues.${name}`, value);
    });

    return api.get<PagedResult<Employee>>(`/employees/search?${params.toString()}`);
  },
  create: (body: CreateEmployeeRequest) => api.post<Employee>('/employees', body),
  update: (id: string, body: UpdateEmployeeRequest) => api.put<Employee>(`/employees/${id}`, body),
  delete: (id: string) => api.delete(`/employees/${id}`),
};
