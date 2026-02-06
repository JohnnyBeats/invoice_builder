import { useState, useEffect } from 'react';
import { FileDown } from 'lucide-react';
import { invoicesApi } from '../api/invoices';
import type { InvoiceResponse } from '../types';
import { Modal } from '../components/Modal';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorAlert } from '../components/ErrorAlert';
import { StatusBadge } from '../components/StatusBadge';

interface InvoiceViewModalProps {
  invoiceId: string;
  onClose: () => void;
  onDownloadPdf: (id: string, invoiceNumber: string) => void;
}

export function InvoiceViewModal({ invoiceId, onClose, onDownloadPdf }: InvoiceViewModalProps) {
  const [invoice, setInvoice] = useState<InvoiceResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const data = await invoicesApi.get(invoiceId);
        setInvoice(data);
      } catch {
        setError('Failed to load invoice');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [invoiceId]);

  const formatMoney = (amount: number, currency: string) =>
    new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
      minimumFractionDigits: 2,
    }).format(amount);

  const formatDate = (dateStr: string) =>
    new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });

  return (
    <Modal open onClose={onClose} title="Invoice Details" wide>
      {loading && <LoadingSpinner />}
      {error && <ErrorAlert message={error} />}
      {invoice && (
        <div className="space-y-6">
          {/* Header */}
          <div className="flex items-start justify-between">
            <div>
              <h3 className="text-xl font-bold text-gray-900 font-mono">
                {invoice.invoiceNumber}
              </h3>
              <div className="mt-1">
                <StatusBadge status={invoice.status} />
              </div>
            </div>
            <button
              onClick={() => onDownloadPdf(invoice.id, invoice.invoiceNumber)}
              className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 transition-colors shadow-sm"
            >
              <FileDown className="w-4 h-4" />
              Download PDF
            </button>
          </div>

          {/* Parties */}
          <div className="grid grid-cols-2 gap-6">
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                From (Sender)
              </h4>
              <p className="font-medium text-gray-900">{invoice.senderCompanyName}</p>
            </div>
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Bill To (Customer)
              </h4>
              <p className="font-medium text-gray-900">{invoice.customerCompanyName}</p>
            </div>
          </div>

          {/* Details */}
          <div className="grid grid-cols-4 gap-4">
            <div>
              <span className="text-xs font-medium text-gray-500 uppercase">Invoice Date</span>
              <p className="text-sm text-gray-900 mt-0.5">{formatDate(invoice.invoiceDate)}</p>
            </div>
            <div>
              <span className="text-xs font-medium text-gray-500 uppercase">Due Date</span>
              <p className="text-sm text-gray-900 mt-0.5">{formatDate(invoice.dueDate)}</p>
            </div>
            <div>
              <span className="text-xs font-medium text-gray-500 uppercase">Currency</span>
              <p className="text-sm text-gray-900 mt-0.5">{invoice.currency}</p>
            </div>
            <div>
              <span className="text-xs font-medium text-gray-500 uppercase">Tax Rate</span>
              <p className="text-sm text-gray-900 mt-0.5">{invoice.taxRate}%</p>
            </div>
          </div>

          {/* Line Items */}
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left text-xs font-medium text-gray-500 uppercase px-4 py-2.5">
                    #
                  </th>
                  <th className="text-left text-xs font-medium text-gray-500 uppercase px-4 py-2.5">
                    Description
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5">
                    Qty
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5">
                    Unit Price
                  </th>
                  <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5">
                    Total
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {invoice.lineItems.map((li, i) => (
                  <tr key={li.id}>
                    <td className="px-4 py-3 text-sm text-gray-500">{i + 1}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">{li.description}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 text-right font-mono">
                      {li.quantity}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700 text-right font-mono">
                      {formatMoney(li.unitPrice, invoice.currency)}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900 text-right font-mono font-medium">
                      {formatMoney(li.total, invoice.currency)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Totals */}
          <div className="flex justify-end">
            <div className="w-72 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Subtotal</span>
                <span className="font-mono">{formatMoney(invoice.subTotal, invoice.currency)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Tax ({invoice.taxRate}%)</span>
                <span className="font-mono">{formatMoney(invoice.taxAmount, invoice.currency)}</span>
              </div>
              <div className="border-t border-gray-300 pt-2 flex justify-between text-lg font-bold">
                <span className="text-gray-900">Grand Total</span>
                <span className="font-mono text-indigo-600">
                  {formatMoney(invoice.grandTotal, invoice.currency)}
                </span>
              </div>
            </div>
          </div>

          {/* Notes */}
          {invoice.notes && (
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <h4 className="text-xs font-semibold text-amber-700 uppercase tracking-wider mb-1">
                Notes
              </h4>
              <p className="text-sm text-amber-900">{invoice.notes}</p>
            </div>
          )}
        </div>
      )}
    </Modal>
  );
}
