import { api } from './client';
import type {
  PagedResult,
  SenderResponse,
  CreateSenderRequest,
  UpdateSenderRequest,
} from '../types';

export const sendersApi = {
  list: (page = 1, pageSize = 10) =>
    api.get<PagedResult<SenderResponse>>(
      `/senders?page=${page}&pageSize=${pageSize}`
    ),
  get: (id: string) => api.get<SenderResponse>(`/senders/${id}`),
  create: (data: CreateSenderRequest) =>
    api.post<SenderResponse>('/senders', data),
  update: (id: string, data: UpdateSenderRequest) =>
    api.put<SenderResponse>(`/senders/${id}`, data),
  delete: (id: string) => api.delete<void>(`/senders/${id}`),
};
