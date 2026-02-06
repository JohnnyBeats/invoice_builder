import type { ReactNode } from 'react';

interface FormFieldProps {
  label: string;
  required?: boolean;
  error?: string;
  children: ReactNode;
}

const baseInputClass =
  'w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none';

export const inputClass = (error?: string) =>
  `${baseInputClass} ${error ? 'border-red-400 bg-red-50' : 'border-gray-300'}`;

export const selectClass = (error?: string) =>
  `${baseInputClass} bg-white ${error ? 'border-red-400 bg-red-50' : 'border-gray-300'}`;

export function FormField({ label, required, error, children }: FormFieldProps) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label}{required && ' *'}
      </label>
      {children}
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
}
