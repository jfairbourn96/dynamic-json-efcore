import { useState } from 'react';
import type { FieldDefinition } from '../types/schema';
import type { EmployeeSearchFilters } from '../types/records';

interface DynamicSearchProps {
  fields: FieldDefinition[];
  onSearch: (filters: EmployeeSearchFilters) => void;
  isLoading?: boolean;
}

export function DynamicSearch({ fields, onSearch, isLoading = false }: DynamicSearchProps) {
  const [coreFilters, setCoreFilters] = useState<Partial<EmployeeSearchFilters>>({});
  const [dynamicFilters, setDynamicFilters] = useState<Record<string, unknown>>({});

  const handleCoreChange = (name: keyof EmployeeSearchFilters, value: string) => {
    setCoreFilters((prev) => ({ ...prev, [name]: value || undefined }));
  };

  const handleDynamicChange = (name: string, value: unknown) => {
    setDynamicFilters((prev) => ({ ...prev, [name]: value || undefined }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch({ ...coreFilters, fieldValues: dynamicFilters });
  };

  const handleReset = () => {
    setCoreFilters({});
    setDynamicFilters({});
    onSearch({});
  };

  const inputClass =
    'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
          Core Fields
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {(['firstName', 'lastName', 'email', 'department'] as const).map((field) => (
            <div key={field}>
              <label className="block text-sm font-medium text-gray-700 capitalize">
                {field.replace(/([A-Z])/g, ' $1')}
              </label>
              <input
                type="text"
                className={inputClass}
                value={(coreFilters[field] as string) || ''}
                onChange={(e) => handleCoreChange(field, e.target.value)}
                placeholder={`Filter by ${field}...`}
              />
            </div>
          ))}
        </div>
      </div>

      {fields.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
            Type-Specific Fields
          </h3>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {[...fields]
              .sort((a, b) => a.order - b.order)
              .map((field) => (
                <div key={field.id}>
                  <label className="block text-sm font-medium text-gray-700">{field.label}</label>
                  <input
                    type={field.fieldType === 'number' ? 'number' : 'text'}
                    className={inputClass}
                    value={(dynamicFilters[field.name] as string) || ''}
                    onChange={(e) => handleDynamicChange(field.name, e.target.value)}
                    placeholder={`Filter by ${field.label}...`}
                  />
                </div>
              ))}
          </div>
        </div>
      )}

      <div className="flex items-center gap-3">
        <button
          type="submit"
          disabled={isLoading}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
        >
          {isLoading ? 'Searching...' : 'Search'}
        </button>
        <button
          type="button"
          onClick={handleReset}
          className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Reset
        </button>
      </div>
    </form>
  );
}
