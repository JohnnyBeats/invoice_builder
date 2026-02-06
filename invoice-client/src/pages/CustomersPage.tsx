import { useState, useEffect, useCallback } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { customersApi } from '../api/customers';
import type { CustomerResponse, CreateCustomerRequest, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorAlert } from '../components/ErrorAlert';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Modal } from '../components/Modal';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { FormField, inputClass } from '../components/FormField';
import { SearchBar } from '../components/SearchBar';
import { useToastContext } from '../context/ToastContext';
import { validateCustomer, hasErrors, type FieldErrors } from '../utils/validation';

const emptyForm: CreateCustomerRequest = {
  companyName: '',
  contactPerson: '',
  address: '',
  email: '',
  postalCode: '',
  vatTaxId: null,
};

export function CustomersPage() {
  const { addToast } = useToastContext();
  const [data, setData] = useState<PagedResult<CustomerResponse> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');

  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<CreateCustomerRequest>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await customersApi.list(page, 10, search || undefined);
      setData(result);
    } catch {
      setError('Failed to load customers');
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  const handleSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setFormErrors(null);
    setFieldErrors({});
    setModalOpen(true);
  };

  const openEdit = (customer: CustomerResponse) => {
    setEditingId(customer.id);
    setForm({
      companyName: customer.companyName,
      contactPerson: customer.contactPerson,
      address: customer.address,
      email: customer.email,
      postalCode: customer.postalCode,
      vatTaxId: customer.vatTaxId,
    });
    setFormErrors(null);
    setFieldErrors({});
    setModalOpen(true);
  };

  const handleSave = async () => {
    const errors = validateCustomer(form);
    setFieldErrors(errors);
    if (hasErrors(errors)) return;

    setSaving(true);
    setFormErrors(null);
    try {
      if (editingId) {
        await customersApi.update(editingId, form);
        addToast('Customer updated successfully', 'success');
      } else {
        await customersApi.create(form);
        addToast('Customer created successfully', 'success');
      }
      setModalOpen(false);
      fetchData();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to save customer';
      setFormErrors(message);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try {
      await customersApi.delete(deleteId);
      addToast('Customer deleted', 'success');
      setDeleteId(null);
      fetchData();
    } catch {
      addToast('Failed to delete customer', 'error');
    } finally {
      setDeleting(false);
    }
  };

  const updateField = (field: keyof CreateCustomerRequest, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value || null }));
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Customers</h1>
          <p className="text-sm text-gray-500 mt-1">Manage your customer records</p>
        </div>
        <button
          onClick={openCreate}
          className="inline-flex items-center gap-2 px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" />
          Add Customer
        </button>
      </div>

      <div className="mb-4 max-w-sm">
        <SearchBar value={search} onChange={handleSearch} placeholder="Search customers..." />
      </div>

      {loading && <LoadingSpinner />}
      {error && <ErrorAlert message={error} onRetry={fetchData} />}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState
          title="No customers yet"
          description="Get started by adding your first customer."
          action={
            <button
              onClick={openCreate}
              className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700"
            >
              <Plus className="w-4 h-4" />
              Add Customer
            </button>
          }
        />
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Company
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Contact
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Email
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    VAT/Tax ID
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((customer) => (
                  <tr key={customer.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="font-medium text-gray-900">{customer.companyName}</div>
                      <div className="text-sm text-gray-500">{customer.address}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">{customer.contactPerson}</td>
                    <td className="px-6 py-4 text-sm text-gray-700">{customer.email}</td>
                    <td className="px-6 py-4 text-sm text-gray-500">{customer.vatTaxId || '—'}</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => openEdit(customer)}
                          className="p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                          title="Edit"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => setDeleteId(customer.id)}
                          className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Delete"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            pageSize={data.pageSize}
            onPageChange={setPage}
          />
        </>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editingId ? 'Edit Customer' : 'New Customer'}
      >
        <div className="space-y-4">
          {formErrors && <ErrorAlert message={formErrors} />}
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Company Name" required error={fieldErrors.companyName}>
              <input
                type="text"
                value={form.companyName}
                onChange={(e) => updateField('companyName', e.target.value)}
                className={inputClass(fieldErrors.companyName)}
              />
            </FormField>
            <FormField label="Contact Person" required error={fieldErrors.contactPerson}>
              <input
                type="text"
                value={form.contactPerson}
                onChange={(e) => updateField('contactPerson', e.target.value)}
                className={inputClass(fieldErrors.contactPerson)}
              />
            </FormField>
          </div>
          <FormField label="Address" required error={fieldErrors.address}>
            <input
              type="text"
              value={form.address}
              onChange={(e) => updateField('address', e.target.value)}
              className={inputClass(fieldErrors.address)}
            />
          </FormField>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Email" required error={fieldErrors.email}>
              <input
                type="email"
                value={form.email}
                onChange={(e) => updateField('email', e.target.value)}
                className={inputClass(fieldErrors.email)}
              />
            </FormField>
            <FormField label="Postal Code" required error={fieldErrors.postalCode}>
              <input
                type="text"
                value={form.postalCode}
                onChange={(e) => updateField('postalCode', e.target.value)}
                className={inputClass(fieldErrors.postalCode)}
              />
            </FormField>
          </div>
          <FormField label="VAT/Tax ID" error={fieldErrors.vatTaxId}>
            <input
              type="text"
              value={form.vatTaxId || ''}
              onChange={(e) => updateField('vatTaxId', e.target.value)}
              className={inputClass(fieldErrors.vatTaxId)}
            />
          </FormField>
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              onClick={() => setModalOpen(false)}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50"
            >
              {saving ? 'Saving...' : editingId ? 'Update' : 'Create'}
            </button>
          </div>
        </div>
      </Modal>

      <ConfirmDialog
        open={!!deleteId}
        title="Delete Customer"
        message="Are you sure you want to delete this customer? This action cannot be undone."
        onConfirm={handleDelete}
        onCancel={() => setDeleteId(null)}
        loading={deleting}
      />
    </div>
  );
}
