import { UserType } from './schema';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  department: string;
  userTypeId: string;
  userType?: UserType;
  fieldValues: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  department: string;
  userTypeId: string;
  fieldValues: Record<string, unknown>;
}

export interface UpdateUserRequest extends CreateUserRequest {
  id: string;
}

export interface UserSearchFilters {
  firstName?: string;
  lastName?: string;
  email?: string;
  department?: string;
  userTypeId?: string;
  fieldValues?: Record<string, unknown>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
