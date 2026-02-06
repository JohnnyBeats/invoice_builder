import { useState, useEffect, useCallback } from 'react';
import { Plus, Pencil, Trash2, FileDown, Eye } from 'lucide-react';
import { invoicesApi } from '../api/invoices';
import type { InvoiceListResponse, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorAlert } from '../components/ErrorAlert';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { StatusBadge } from '../components/StatusBadge';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { SearchBar } from '../components/SearchBar';
import { useToastContext } from '../context/ToastContext';
import { InvoiceFormModal } from './InvoiceFormModal';
import { InvoiceViewModal } from './InvoiceViewModal';

export function InvoicesPage() {
  const { addToast } = useToastContext();
  const [data, setData] = useState<PagedResult<InvoiceListResponse> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');

  const [formOpen, setFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [viewingId, setViewingId] = useState<string | null>(null);

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await invoicesApi.list(page, 10, search || undefined);
      setData(result);
    } catch {
      setError('Failed to load invoices');
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
    setFormOpen(true);
  };

  const openEdit = (id: string) => {
    setEditingId(id);
    setFormOpen(true);
  };

  const handleFormClose = (saved: boolean) => {
    setFormOpen(false);
    setEditingId(null);
    if (saved) fetchData();
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try {
      await invoicesApi.delete(deleteId);
      addToast('Invoice deleted', 'success');
      setDeleteId(null);
      fetchData();
    } catch {
      addToast('Failed to delete invoice', 'error');
    } finally {
      setDeleting(false);
    }
  };

  const handleDownloadPdf = async (id: string, invoiceNumber: string) => {
    try {
      addToast('Generating PDF...', 'info');
      const blob = await invoicesApi.downloadPdf(id);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${invoiceNumber}.pdf`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      addToast('PDF downloaded', 'success');
    } catch {
      addToast('Failed to generate PDF', 'error');
    }
  };

  const formatCurrency = (amount: number, currency: string) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Invoices</h1>
          <p className="text-sm text-gray-500 mt-1">Create and manage your invoices</p>
        </div>
        <button
          onClick={openCreate}
          className="inline-flex items-center gap-2 px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" />
          New Invoice
        </button>
      </div>

      <div className="mb-4 max-w-sm">
        <SearchBar value={search} onChange={handleSearch} placeholder="Search invoices..." />
      </div>

      {loading && <LoadingSpinner />}
      {error && <ErrorAlert message={error} onRetry={fetchData} />}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState
          title="No invoices yet"
          description="Create your first invoice to get started."
          action={
            <button
              onClick={openCreate}
              className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700"
            >
              <Plus className="w-4 h-4" />
              New Invoice
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
                    Invoice
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Customer
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Date
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Due Date
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Status
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Total
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((invoice) => (
                  <tr key={invoice.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="font-medium text-gray-900 font-mono text-sm">
                        {invoice.invoiceNumber}
                      </div>
                      <div className="text-xs text-gray-500">{invoice.senderCompanyName}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">
                      {invoice.customerCompanyName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">
                      {formatDate(invoice.invoiceDate)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">
                      {formatDate(invoice.dueDate)}
                    </td>
                    <td className="px-6 py-4">
                      <StatusBadge status={invoice.status} />
                    </td>
                    <td className="px-6 py-4 text-sm font-medium text-gray-900 text-right font-mono">
                      {formatCurrency(invoice.grandTotal, invoice.currency)}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setViewingId(invoice.id)}
                          className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="View"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => openEdit(invoice.id)}
                          className="p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                          title="Edit"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() =>
                            handleDownloadPdf(invoice.id, invoice.invoiceNumber)
                          }
                          className="p-2 text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors"
                          title="Download PDF"
                        >
                          <FileDown className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => setDeleteId(invoice.id)}
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

      {formOpen && (
        <InvoiceFormModal editingId={editingId} onClose={handleFormClose} />
      )}

      {viewingId && (
        <InvoiceViewModal
          invoiceId={viewingId}
          onClose={() => setViewingId(null)}
          onDownloadPdf={handleDownloadPdf}
        />
      )}

      <ConfirmDialog
        open={!!deleteId}
        title="Delete Invoice"
        message="Are you sure you want to delete this invoice? This action cannot be undone."
        onConfirm={handleDelete}
        onCancel={() => setDeleteId(null)}
        loading={deleting}
      />
    </div>
  );
}
