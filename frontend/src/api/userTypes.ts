import { UserType, CreateUserTypeRequest, UpdateUserTypeRequest } from '../types/schema';
import { api } from './client';

export const userTypesApi = {
  getAll: () => api.get<UserType[]>('/user-types'),
  getById: (id: string) => api.get<UserType>(`/user-types/${id}`),
  create: (body: CreateUserTypeRequest) => api.post<UserType>('/user-types', body),
  update: (id: string, body: UpdateUserTypeRequest) => api.put<UserType>(`/user-types/${id}`, body),
  delete: (id: string) => api.delete(`/user-types/${id}`),
};
