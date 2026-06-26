import { User, CreateUserRequest, UpdateUserRequest, UserSearchFilters, PagedResult } from '../types/records';
import { api } from './client';

export const usersApi = {
  getById: (id: string) => api.get<User>(`/users/${id}`),
  search: (filters: UserSearchFilters, pageNumber = 1, pageSize = 20) =>
    api.post<PagedResult<User>>('/users/search', { ...filters, pageNumber, pageSize }),
  create: (body: CreateUserRequest) => api.post<User>('/users', body),
  update: (id: string, body: UpdateUserRequest) => api.put<User>(`/users/${id}`, body),
  delete: (id: string) => api.delete(`/users/${id}`),
};
