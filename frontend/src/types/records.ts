import type { EmployeeType } from './schema';

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  department: string;
  employeeTypeId: string;
  employeeType?: EmployeeType;
  fieldValues: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  department: string;
  employeeTypeId: string;
  fieldValues: Record<string, unknown>;
}

export interface UpdateEmployeeRequest extends CreateEmployeeRequest {
  id: string;
}

export interface EmployeeSearchFilters {
  firstName?: string;
  lastName?: string;
  email?: string;
  department?: string;
  employeeTypeId?: string;
  fieldValues?: Record<string, unknown>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
