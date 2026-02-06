export type FieldErrors = Record<string, string>;

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function required(value: string | null | undefined, fieldName: string): string | null {
  if (!value || value.trim() === '') return `${fieldName} is required.`;
  return null;
}

function maxLength(value: string | null | undefined, max: number, fieldName: string): string | null {
  if (value && value.length > max) return `${fieldName} must be at most ${max} characters.`;
  return null;
}

function email(value: string | null | undefined, fieldName: string): string | null {
  if (value && !EMAIL_REGEX.test(value)) return `${fieldName} must be a valid email address.`;
  return null;
}

// --- Customer ---

export interface CustomerFormData {
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  postalCode: string;
  vatTaxId: string | null;
}

export function validateCustomer(form: CustomerFormData): FieldErrors {
  const errors: FieldErrors = {};

  const companyName = required(form.companyName, 'Company name') || maxLength(form.companyName, 200, 'Company name');
  if (companyName) errors.companyName = companyName;

  const contactPerson = required(form.contactPerson, 'Contact person') || maxLength(form.contactPerson, 200, 'Contact person');
  if (contactPerson) errors.contactPerson = contactPerson;

  const address = required(form.address, 'Address') || maxLength(form.address, 500, 'Address');
  if (address) errors.address = address;

  const emailErr = required(form.email, 'Email') || email(form.email, 'Email') || maxLength(form.email, 200, 'Email');
  if (emailErr) errors.email = emailErr;

  const postalCode = required(form.postalCode, 'Postal code') || maxLength(form.postalCode, 20, 'Postal code');
  if (postalCode) errors.postalCode = postalCode;

  const vatTaxId = maxLength(form.vatTaxId, 50, 'VAT/Tax ID');
  if (vatTaxId) errors.vatTaxId = vatTaxId;

  return errors;
}

// --- Sender ---

export interface SenderFormData {
  companyName: string;
  contactPerson: string;
  address: string;
  email: string;
  phone: string;
  vatTaxId: string | null;
  iban: string | null;
}

export function validateSender(form: SenderFormData): FieldErrors {
  const errors: FieldErrors = {};

  const companyName = required(form.companyName, 'Company name') || maxLength(form.companyName, 200, 'Company name');
  if (companyName) errors.companyName = companyName;

  const contactPerson = required(form.contactPerson, 'Contact person') || maxLength(form.contactPerson, 200, 'Contact person');
  if (contactPerson) errors.contactPerson = contactPerson;

  const address = required(form.address, 'Address') || maxLength(form.address, 500, 'Address');
  if (address) errors.address = address;

  const emailErr = required(form.email, 'Email') || email(form.email, 'Email') || maxLength(form.email, 200, 'Email');
  if (emailErr) errors.email = emailErr;

  const phone = required(form.phone, 'Phone') || maxLength(form.phone, 50, 'Phone');
  if (phone) errors.phone = phone;

  const vatTaxId = maxLength(form.vatTaxId, 50, 'VAT/Tax ID');
  if (vatTaxId) errors.vatTaxId = vatTaxId;

  const iban = maxLength(form.iban, 50, 'IBAN');
  if (iban) errors.iban = iban;

  return errors;
}

// --- Invoice ---

export interface InvoiceLineItemFormData {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface InvoiceFormData {
  customerId: string;
  senderId: string;
  invoiceDate: string;
  dueDate: string;
  currency: string;
  taxRate: number;
  notes: string;
  lineItems: InvoiceLineItemFormData[];
}

export interface InvoiceErrors {
  fields: FieldErrors;
  lineItems: FieldErrors[];
}

export function validateInvoice(form: InvoiceFormData): InvoiceErrors {
  const fields: FieldErrors = {};

  if (!form.customerId) fields.customerId = 'Customer is required.';
  if (!form.senderId) fields.senderId = 'Sender is required.';
  if (!form.invoiceDate) fields.invoiceDate = 'Invoice date is required.';
  if (!form.dueDate) fields.dueDate = 'Due date is required.';

  if (form.invoiceDate && form.dueDate && form.dueDate < form.invoiceDate) {
    fields.dueDate = 'Due date must be on or after the invoice date.';
  }

  if (!form.currency || form.currency.length !== 3) fields.currency = 'Currency must be a 3-letter code.';

  if (form.taxRate < 0) fields.taxRate = 'Tax rate cannot be negative.';
  if (form.taxRate > 100) fields.taxRate = 'Tax rate cannot exceed 100%.';

  const notesErr = maxLength(form.notes, 1000, 'Notes');
  if (notesErr) fields.notes = notesErr;

  if (form.lineItems.length === 0) fields.lineItems = 'At least one line item is required.';

  const lineItems: FieldErrors[] = form.lineItems.map((li) => {
    const errs: FieldErrors = {};
    if (!li.description || li.description.trim() === '') errs.description = 'Description is required.';
    if (li.description && li.description.length > 500) errs.description = 'Max 500 characters.';
    if (li.quantity <= 0) errs.quantity = 'Must be > 0.';
    if (li.unitPrice < 0) errs.unitPrice = 'Cannot be negative.';
    return errs;
  });

  return { fields, lineItems };
}

export function hasErrors(errors: FieldErrors): boolean {
  return Object.keys(errors).length > 0;
}

export function hasInvoiceErrors(errors: InvoiceErrors): boolean {
  return hasErrors(errors.fields) || errors.lineItems.some(hasErrors);
}
