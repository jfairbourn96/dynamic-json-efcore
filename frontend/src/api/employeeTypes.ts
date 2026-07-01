import type { EmployeeType, CreateEmployeeTypeRequest, UpdateEmployeeTypeRequest } from '../types/schema';
import { api } from './client';

export const employeeTypesApi = {
  getAll: () => api.get<EmployeeType[]>('/employee-types'),
  getById: (id: string) => api.get<EmployeeType>(`/employee-types/${id}`),
  create: (body: CreateEmployeeTypeRequest) => api.post<EmployeeType>('/employee-types', body),
  update: (id: string, body: UpdateEmployeeTypeRequest) => api.put<EmployeeType>(`/employee-types/${id}`, body),
  delete: (id: string) => api.delete(`/employee-types/${id}`),
};
