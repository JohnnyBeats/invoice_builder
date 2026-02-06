import { api } from './client';
import type {
  PagedResult,
  InvoiceResponse,
  InvoiceListResponse,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
} from '../types';

export const invoicesApi = {
  list: (page = 1, pageSize = 10, search?: string) =>
    api.get<PagedResult<InvoiceListResponse>>(
      `/invoices?page=${page}&pageSize=${pageSize}${search ? `&search=${encodeURIComponent(search)}` : ''}`
    ),
  get: (id: string) => api.get<InvoiceResponse>(`/invoices/${id}`),
  create: (data: CreateInvoiceRequest) =>
    api.post<InvoiceResponse>('/invoices', data),
  update: (id: string, data: UpdateInvoiceRequest) =>
    api.put<InvoiceResponse>(`/invoices/${id}`, data),
  delete: (id: string) => api.delete<void>(`/invoices/${id}`),
  downloadPdf: (id: string) => api.getBlob(`/invoices/${id}/pdf`),
};
