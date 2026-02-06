import { api } from './client';
import type {
  PagedResult,
  CustomerResponse,
  CreateCustomerRequest,
  UpdateCustomerRequest,
} from '../types';

export const customersApi = {
  list: (page = 1, pageSize = 10, search?: string) =>
    api.get<PagedResult<CustomerResponse>>(
      `/customers?page=${page}&pageSize=${pageSize}${search ? `&search=${encodeURIComponent(search)}` : ''}`
    ),
  get: (id: string) => api.get<CustomerResponse>(`/customers/${id}`),
  create: (data: CreateCustomerRequest) =>
    api.post<CustomerResponse>('/customers', data),
  update: (id: string, data: UpdateCustomerRequest) =>
    api.put<CustomerResponse>(`/customers/${id}`, data),
  delete: (id: string) => api.delete<void>(`/customers/${id}`),
};
