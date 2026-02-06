import { useState, useEffect, useMemo } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import { invoicesApi } from '../api/invoices';
import { customersApi } from '../api/customers';
import { sendersApi } from '../api/senders';
import type {
  CustomerResponse,
  SenderResponse,
  CreateInvoiceLineItemRequest,
  InvoiceResponse,
} from '../types';
import { Modal } from '../components/Modal';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorAlert } from '../components/ErrorAlert';
import { FormField, inputClass, selectClass } from '../components/FormField';
import { useToastContext } from '../context/ToastContext';
import { validateInvoice, hasInvoiceErrors, type FieldErrors } from '../utils/validation';

interface InvoiceFormModalProps {
  editingId: string | null;
  onClose: (saved: boolean) => void;
}

const STATUSES = ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'];
const CURRENCIES = ['USD', 'EUR', 'GBP', 'ZAR', 'AUD', 'CAD'];

const emptyLineItem: CreateInvoiceLineItemRequest = {
  description: '',
  quantity: 1,
  unitPrice: 0,
};

function toDateInputValue(dateStr: string): string {
  return new Date(dateStr).toISOString().split('T')[0];
}

function todayStr(): string {
  return new Date().toISOString().split('T')[0];
}

function in30Days(): string {
  const d = new Date();
  d.setDate(d.getDate() + 30);
  return d.toISOString().split('T')[0];
}

export function InvoiceFormModal({ editingId, onClose }: InvoiceFormModalProps) {
  const { addToast } = useToastContext();
  const [loadingData, setLoadingData] = useState(true);
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [senders, setSenders] = useState<SenderResponse[]>([]);
  const [existing, setExisting] = useState<InvoiceResponse | null>(null);

  const [customerId, setCustomerId] = useState('');
  const [senderId, setSenderId] = useState('');
  const [invoiceDate, setInvoiceDate] = useState(todayStr());
  const [dueDate, setDueDate] = useState(in30Days());
  const [currency, setCurrency] = useState('USD');
  const [taxRate, setTaxRate] = useState(15);
  const [notes, setNotes] = useState('');
  const [status, setStatus] = useState('Draft');
  const [lineItems, setLineItems] = useState<CreateInvoiceLineItemRequest[]>([
    { ...emptyLineItem },
  ]);

  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [lineItemErrors, setLineItemErrors] = useState<FieldErrors[]>([]);

  useEffect(() => {
    const load = async () => {
      setLoadingData(true);
      try {
        const [custResult, senderResult] = await Promise.all([
          customersApi.list(1, 100),
          sendersApi.list(1, 100),
        ]);
        setCustomers(custResult.items);
        setSenders(senderResult.items);

        if (editingId) {
          const inv = await invoicesApi.get(editingId);
          setExisting(inv);
          setCustomerId(inv.customerId);
          setSenderId(inv.senderId);
          setInvoiceDate(toDateInputValue(inv.invoiceDate));
          setDueDate(toDateInputValue(inv.dueDate));
          setCurrency(inv.currency);
          setTaxRate(inv.taxRate);
          setNotes(inv.notes || '');
          setStatus(inv.status);
          setLineItems(
            inv.lineItems.map((li) => ({
              description: li.description,
              quantity: li.quantity,
              unitPrice: li.unitPrice,
            }))
          );
        }
      } catch {
        setFormError('Failed to load form data');
      } finally {
        setLoadingData(false);
      }
    };
    load();
  }, [editingId]);

  const subTotal = useMemo(
    () => lineItems.reduce((sum, li) => sum + li.quantity * li.unitPrice, 0),
    [lineItems]
  );
  const taxAmount = useMemo(() => subTotal * (taxRate / 100), [subTotal, taxRate]);
  const grandTotal = useMemo(() => subTotal + taxAmount, [subTotal, taxAmount]);

  const addLineItem = () => {
    setLineItems((prev) => [...prev, { ...emptyLineItem }]);
  };

  const removeLineItem = (index: number) => {
    setLineItems((prev) => prev.filter((_, i) => i !== index));
  };

  const updateLineItem = (
    index: number,
    field: keyof CreateInvoiceLineItemRequest,
    value: string | number
  ) => {
    setLineItems((prev) =>
      prev.map((li, i) => (i === index ? { ...li, [field]: value } : li))
    );
  };

  const handleSave = async () => {
    const errors = validateInvoice({
      customerId,
      senderId,
      invoiceDate,
      dueDate,
      currency,
      taxRate,
      notes,
      lineItems,
    });
    setFieldErrors(errors.fields);
    setLineItemErrors(errors.lineItems);
    if (hasInvoiceErrors(errors)) return;

    setSaving(true);
    setFormError(null);
    try {
      if (editingId) {
        await invoicesApi.update(editingId, {
          invoiceDate,
          dueDate,
          currency,
          taxRate,
          notes: notes || null,
          status,
          senderId,
          customerId,
          lineItems,
        });
        addToast('Invoice updated successfully', 'success');
      } else {
        await invoicesApi.create({
          invoiceDate,
          dueDate,
          currency,
          taxRate,
          notes: notes || null,
          senderId,
          customerId,
          lineItems,
        });
        addToast('Invoice created successfully', 'success');
      }
      onClose(true);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to save invoice';
      setFormError(message);
    } finally {
      setSaving(false);
    }
  };

  const formatMoney = (amount: number) =>
    new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);

  return (
    <Modal
      open
      onClose={() => onClose(false)}
      title={editingId ? `Edit Invoice${existing ? ` ${existing.invoiceNumber}` : ''}` : 'New Invoice'}
      wide
    >
      {loadingData ? (
        <LoadingSpinner />
      ) : (
        <div className="space-y-6">
          {formError && <ErrorAlert message={formError} />}

          {/* Top section: Customer, Sender, Dates */}
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Customer" required error={fieldErrors.customerId}>
              <select
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                className={selectClass(fieldErrors.customerId)}
              >
                <option value="">Select a customer...</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.companyName}
                  </option>
                ))}
              </select>
            </FormField>
            <FormField label="Sender" required error={fieldErrors.senderId}>
              <select
                value={senderId}
                onChange={(e) => setSenderId(e.target.value)}
                className={selectClass(fieldErrors.senderId)}
              >
                <option value="">Select a sender...</option>
                {senders.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.companyName}
                  </option>
                ))}
              </select>
            </FormField>
          </div>

          <div className="grid grid-cols-4 gap-4">
            <FormField label="Invoice Date" required error={fieldErrors.invoiceDate}>
              <input
                type="date"
                value={invoiceDate}
                onChange={(e) => setInvoiceDate(e.target.value)}
                className={inputClass(fieldErrors.invoiceDate)}
              />
            </FormField>
            <FormField label="Due Date" required error={fieldErrors.dueDate}>
              <input
                type="date"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
                className={inputClass(fieldErrors.dueDate)}
              />
            </FormField>
            <FormField label="Currency" required error={fieldErrors.currency}>
              <select
                value={currency}
                onChange={(e) => setCurrency(e.target.value)}
                className={selectClass(fieldErrors.currency)}
              >
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </FormField>
            <FormField label="Tax Rate (%)" required error={fieldErrors.taxRate}>
              <input
                type="number"
                min={0}
                max={100}
                step={0.01}
                value={taxRate}
                onChange={(e) => setTaxRate(parseFloat(e.target.value) || 0)}
                className={inputClass(fieldErrors.taxRate)}
              />
            </FormField>
          </div>

          {editingId && (
            <div className="w-48">
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none bg-white"
              >
                {STATUSES.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Line Items */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <div>
                <h3 className="text-sm font-semibold text-gray-900">Line Items</h3>
                {fieldErrors.lineItems && <p className="text-xs text-red-600 mt-0.5">{fieldErrors.lineItems}</p>}
              </div>
              <button
                type="button"
                onClick={addLineItem}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-indigo-600 bg-indigo-50 rounded-lg hover:bg-indigo-100 transition-colors"
              >
                <Plus className="w-3.5 h-3.5" />
                Add Row
              </button>
            </div>
            <div className="bg-gray-50 rounded-lg border border-gray-200 overflow-hidden" data-cy="line-items">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left text-xs font-medium text-gray-500 uppercase px-4 py-2.5 w-[45%]">
                      Description
                    </th>
                    <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5 w-[15%]">
                      Qty
                    </th>
                    <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5 w-[18%]">
                      Unit Price
                    </th>
                    <th className="text-right text-xs font-medium text-gray-500 uppercase px-4 py-2.5 w-[17%]">
                      Total
                    </th>
                    <th className="w-[5%] px-2"></th>
                  </tr>
                </thead>
                <tbody>
                  {lineItems.map((item, index) => (
                    <tr key={index} className="border-b border-gray-100 last:border-b-0">
                      <td className="px-3 py-2">
                        <input
                          type="text"
                          value={item.description}
                          onChange={(e) =>
                            updateLineItem(index, 'description', e.target.value)
                          }
                          placeholder="Item description"
                          className={`w-full px-2.5 py-1.5 border rounded-md text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none ${lineItemErrors[index]?.description ? 'border-red-400 bg-red-50' : 'border-gray-300'}`}
                        />
                        {lineItemErrors[index]?.description && <p className="text-xs text-red-600 mt-0.5">{lineItemErrors[index].description}</p>}
                      </td>
                      <td className="px-3 py-2">
                        <input
                          type="number"
                          min={0}
                          step={0.01}
                          value={item.quantity}
                          onChange={(e) =>
                            updateLineItem(index, 'quantity', parseFloat(e.target.value) || 0)
                          }
                          className={`w-full px-2.5 py-1.5 border rounded-md text-sm text-right focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none ${lineItemErrors[index]?.quantity ? 'border-red-400 bg-red-50' : 'border-gray-300'}`}
                        />
                        {lineItemErrors[index]?.quantity && <p className="text-xs text-red-600 mt-0.5">{lineItemErrors[index].quantity}</p>}
                      </td>
                      <td className="px-3 py-2">
                        <input
                          type="number"
                          min={0}
                          step={0.01}
                          value={item.unitPrice}
                          onChange={(e) =>
                            updateLineItem(index, 'unitPrice', parseFloat(e.target.value) || 0)
                          }
                          className={`w-full px-2.5 py-1.5 border rounded-md text-sm text-right focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none ${lineItemErrors[index]?.unitPrice ? 'border-red-400 bg-red-50' : 'border-gray-300'}`}
                        />
                        {lineItemErrors[index]?.unitPrice && <p className="text-xs text-red-600 mt-0.5">{lineItemErrors[index].unitPrice}</p>}
                      </td>
                      <td className="px-4 py-2 text-sm text-right font-mono text-gray-700">
                        {formatMoney(item.quantity * item.unitPrice)}
                      </td>
                      <td className="px-2 py-2">
                        {lineItems.length > 1 && (
                          <button
                            type="button"
                            onClick={() => removeLineItem(index)}
                            className="p-1 text-gray-400 hover:text-red-500 rounded transition-colors"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Summary */}
          <div className="flex justify-end">
            <div className="w-72 bg-gray-50 rounded-lg border border-gray-200 p-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Subtotal</span>
                <span className="font-mono text-gray-900">{currency} {formatMoney(subTotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Tax ({taxRate}%)</span>
                <span className="font-mono text-gray-900">{currency} {formatMoney(taxAmount)}</span>
              </div>
              <div className="border-t border-gray-300 pt-2 flex justify-between text-base font-semibold">
                <span className="text-gray-900">Grand Total</span>
                <span className="font-mono text-indigo-600">{currency} {formatMoney(grandTotal)}</span>
              </div>
            </div>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              placeholder="Payment terms, additional notes..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none resize-none"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              onClick={() => onClose(false)}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="px-5 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50"
            >
              {saving ? 'Saving...' : editingId ? 'Update Invoice' : 'Create Invoice'}
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
