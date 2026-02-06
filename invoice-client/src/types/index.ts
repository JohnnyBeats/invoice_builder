export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CustomerResponse {
  id: string;
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  postalCode: string;
  vatTaxId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCustomerRequest {
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  postalCode: string;
  vatTaxId: string | null;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {}

export interface SenderResponse {
  id: string;
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  phone: string;
  vatTaxId: string | null;
  iban: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSenderRequest {
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  phone: string;
  vatTaxId: string | null;
  iban: string | null;
}

export interface UpdateSenderRequest extends CreateSenderRequest {}

export interface InvoiceLineItemResponse {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface InvoiceResponse {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  currency: string;
  taxRate: number;
  notes: string | null;
  status: string;
  senderId: string;
  senderCompanyName: string;
  customerId: string;
  customerCompanyName: string;
  lineItems: InvoiceLineItemResponse[];
  subTotal: number;
  taxAmount: number;
  grandTotal: number;
  createdAt: string;
  updatedAt: string;
}

export interface InvoiceListResponse {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  currency: string;
  status: string;
  senderCompanyName: string;
  customerCompanyName: string;
  grandTotal: number;
  createdAt: string;
}

export interface CreateInvoiceLineItemRequest {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateInvoiceRequest {
  invoiceDate: string;
  dueDate: string;
  currency: string;
  taxRate: number;
  notes: string | null;
  senderId: string;
  customerId: string;
  lineItems: CreateInvoiceLineItemRequest[];
}

export interface UpdateInvoiceRequest extends CreateInvoiceRequest {
  status: string;
}
