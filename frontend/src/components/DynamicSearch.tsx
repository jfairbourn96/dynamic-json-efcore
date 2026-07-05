import { useState } from 'react';
import type { FieldDefinition } from '../types/schema';
import type { EmployeeSearchFilters } from '../types/records';

type TextOperator = 'contains' | 'startsWith' | 'exact';
type NumberOperator = 'lt' | 'lte' | 'eq' | 'gt' | 'gte';

interface TextFilterState {
  operator: TextOperator;
  value: string;
}

interface DateRangeState {
  startDate: string;
  endDate: string;
}

interface NumberFilterState {
  operator: NumberOperator;
  value: string;
}

interface DynamicSearchProps {
  fields: FieldDefinition[];
  onSearch: (filters: EmployeeSearchFilters) => void;
  isLoading?: boolean;
}

const CORE_TEXT_FIELDS = [
  { name: 'firstName', label: 'First Name' },
  { name: 'lastName', label: 'Last Name' },
  { name: 'department', label: 'Department' },
] as const;

const TEXT_OPERATORS: { value: TextOperator; label: string }[] = [
  { value: 'contains', label: 'Contains' },
  { value: 'startsWith', label: 'Starts With' },
  { value: 'exact', label: 'Exact Match' },
];

const NUMBER_OPERATORS: { value: NumberOperator; label: string }[] = [
  { value: 'lt', label: 'Less than' },
  { value: 'lte', label: 'Less than or equal to' },
  { value: 'eq', label: 'Equal to' },
  { value: 'gt', label: 'Greater than' },
  { value: 'gte', label: 'Greater than or equal to' },
];

const defaultTextFilter = (): TextFilterState => ({ operator: 'contains', value: '' });
const defaultDateRange = (): DateRangeState => ({ startDate: '', endDate: '' });
const defaultNumberFilter = (): NumberFilterState => ({ operator: 'eq', value: '' });

export function DynamicSearch({ fields, onSearch, isLoading = false }: DynamicSearchProps) {
  const [coreTextFilters, setCoreTextFilters] = useState<Record<string, TextFilterState>>({});
  const [emailFilter, setEmailFilter] = useState('');
  const [hireDateFilter, setHireDateFilter] = useState<DateRangeState>(defaultDateRange);
  const [dynamicTextFilters, setDynamicTextFilters] = useState<Record<string, TextFilterState>>({});
  const [dynamicDateFilters, setDynamicDateFilters] = useState<Record<string, DateRangeState>>({});
  const [dynamicBooleanFilters, setDynamicBooleanFilters] = useState<Record<string, boolean>>({});
  const [dynamicSelectFilters, setDynamicSelectFilters] = useState<Record<string, string>>({});
  const [dynamicNumberFilters, setDynamicNumberFilters] = useState<Record<string, NumberFilterState>>({});

  const inputClass =
    'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

  const selectClass =
    'mt-1 block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

  const getTextQueryKey = (fieldName: string, operator: TextOperator) =>
    `${fieldName}_${operator}`;

  const getNumberQueryKey = (fieldName: string, operator: NumberOperator) =>
    operator === 'eq' ? fieldName : `${fieldName}_${operator}`;

  const addIfPresent = (
    filters: EmployeeSearchFilters,
    key: string,
    value: string | number | boolean | undefined,
  ) => {
    if (value === undefined || value === '') {
      return;
    }

    filters[key] = value;
  };

  const setCoreTextFilter = (name: string, patch: Partial<TextFilterState>) => {
    setCoreTextFilters((prev) => ({
      ...prev,
      [name]: { ...(prev[name] ?? defaultTextFilter()), ...patch },
    }));
  };

  const setDynamicTextFilter = (name: string, patch: Partial<TextFilterState>) => {
    setDynamicTextFilters((prev) => ({
      ...prev,
      [name]: { ...(prev[name] ?? defaultTextFilter()), ...patch },
    }));
  };

  const setDynamicDateFilter = (name: string, patch: Partial<DateRangeState>) => {
    setDynamicDateFilters((prev) => ({
      ...prev,
      [name]: { ...(prev[name] ?? defaultDateRange()), ...patch },
    }));
  };

  const setDynamicNumberFilter = (name: string, patch: Partial<NumberFilterState>) => {
    setDynamicNumberFilters((prev) => ({
      ...prev,
      [name]: { ...(prev[name] ?? defaultNumberFilter()), ...patch },
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const filters: EmployeeSearchFilters = {};

    CORE_TEXT_FIELDS.forEach(({ name }) => {
      const filter = coreTextFilters[name] ?? defaultTextFilter();
      addIfPresent(filters, getTextQueryKey(name, filter.operator), filter.value.trim());
    });

    addIfPresent(filters, 'email', emailFilter.trim());
    addIfPresent(filters, 'hireDate_startDate', hireDateFilter.startDate);
    addIfPresent(filters, 'hireDate_endDate', hireDateFilter.endDate);

    fields.forEach((field) => {
      switch (field.fieldType) {
        case 'text': {
          const filter = dynamicTextFilters[field.name] ?? defaultTextFilter();
          addIfPresent(filters, getTextQueryKey(field.name, filter.operator), filter.value.trim());
          break;
        }
        case 'date': {
          const filter = dynamicDateFilters[field.name] ?? defaultDateRange();
          addIfPresent(filters, `${field.name}_startDate`, filter.startDate);
          addIfPresent(filters, `${field.name}_endDate`, filter.endDate);
          break;
        }
        case 'boolean':
          filters[field.name] = dynamicBooleanFilters[field.name] ?? false;
          break;
        case 'select':
          addIfPresent(filters, field.name, dynamicSelectFilters[field.name]);
          break;
        case 'number': {
          const filter = dynamicNumberFilters[field.name] ?? defaultNumberFilter();
          addIfPresent(filters, getNumberQueryKey(field.name, filter.operator), filter.value);
          break;
        }
        default:
          break;
      }
    });

    onSearch(filters);
  };

  const handleReset = () => {
    setCoreTextFilters({});
    setEmailFilter('');
    setHireDateFilter(defaultDateRange());
    setDynamicTextFilters({});
    setDynamicDateFilters({});
    setDynamicBooleanFilters({});
    setDynamicSelectFilters({});
    setDynamicNumberFilters({});
    onSearch({});
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
          Core Fields
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {CORE_TEXT_FIELDS.map((field) => {
            const filter = coreTextFilters[field.name] ?? defaultTextFilter();

            return (
              <div key={field.name}>
                <label className="block text-sm font-medium text-gray-700">{field.label}</label>
                <div className="grid grid-cols-[minmax(8rem,0.75fr)_minmax(0,1fr)] gap-2">
                  <select
                    className={selectClass}
                    value={filter.operator}
                    onChange={(e) =>
                      setCoreTextFilter(field.name, { operator: e.target.value as TextOperator })
                    }
                  >
                    {TEXT_OPERATORS.map((operator) => (
                      <option key={operator.value} value={operator.value}>
                        {operator.label}
                      </option>
                    ))}
                  </select>
                  <input
                    type="text"
                    className={inputClass}
                    value={filter.value}
                    onChange={(e) => setCoreTextFilter(field.name, { value: e.target.value })}
                    placeholder={`Filter by ${field.label.toLowerCase()}...`}
                  />
                </div>
              </div>
            );
          })}

          <div>
            <label className="block text-sm font-medium text-gray-700">Email</label>
            <input
              type="text"
              className={inputClass}
              value={emailFilter}
              onChange={(e) => setEmailFilter(e.target.value)}
              placeholder="Filter by email..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Hire Date</label>
            <div className="grid grid-cols-2 gap-2">
              <input
                type="date"
                className={inputClass}
                value={hireDateFilter.startDate}
                onChange={(e) =>
                  setHireDateFilter((prev) => ({ ...prev, startDate: e.target.value }))
                }
              />
              <input
                type="date"
                className={inputClass}
                value={hireDateFilter.endDate}
                onChange={(e) =>
                  setHireDateFilter((prev) => ({ ...prev, endDate: e.target.value }))
                }
              />
            </div>
          </div>
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
              .map((field) => {
                const label = (
                  <label className="block text-sm font-medium text-gray-700">{field.label}</label>
                );

                if (field.fieldType === 'text') {
                  const filter = dynamicTextFilters[field.name] ?? defaultTextFilter();

                  return (
                    <div key={field.id}>
                      {label}
                      <div className="grid grid-cols-[minmax(8rem,0.75fr)_minmax(0,1fr)] gap-2">
                        <select
                          className={selectClass}
                          value={filter.operator}
                          onChange={(e) =>
                            setDynamicTextFilter(field.name, {
                              operator: e.target.value as TextOperator,
                            })
                          }
                        >
                          {TEXT_OPERATORS.map((operator) => (
                            <option key={operator.value} value={operator.value}>
                              {operator.label}
                            </option>
                          ))}
                        </select>
                        <input
                          type="text"
                          className={inputClass}
                          value={filter.value}
                          onChange={(e) =>
                            setDynamicTextFilter(field.name, { value: e.target.value })
                          }
                          placeholder={`Filter by ${field.label.toLowerCase()}...`}
                        />
                      </div>
                    </div>
                  );
                }

                if (field.fieldType === 'date') {
                  const filter = dynamicDateFilters[field.name] ?? defaultDateRange();

                  return (
                    <div key={field.id}>
                      {label}
                      <div className="grid grid-cols-2 gap-2">
                        <input
                          type="date"
                          className={inputClass}
                          value={filter.startDate}
                          onChange={(e) =>
                            setDynamicDateFilter(field.name, { startDate: e.target.value })
                          }
                        />
                        <input
                          type="date"
                          className={inputClass}
                          value={filter.endDate}
                          onChange={(e) =>
                            setDynamicDateFilter(field.name, { endDate: e.target.value })
                          }
                        />
                      </div>
                    </div>
                  );
                }

                if (field.fieldType === 'boolean') {
                  return (
                    <div key={field.id} className="flex items-center gap-2 pt-6">
                      <input
                        id={`search-${field.id}`}
                        type="checkbox"
                        className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        checked={dynamicBooleanFilters[field.name] ?? false}
                        onChange={(e) =>
                          setDynamicBooleanFilters((prev) => ({
                            ...prev,
                            [field.name]: e.target.checked,
                          }))
                        }
                      />
                      <label htmlFor={`search-${field.id}`} className="text-sm font-medium text-gray-700">
                        {field.label}
                      </label>
                    </div>
                  );
                }

                if (field.fieldType === 'select') {
                  return (
                    <div key={field.id}>
                      {label}
                      <select
                        className={selectClass}
                        value={dynamicSelectFilters[field.name] ?? ''}
                        onChange={(e) =>
                          setDynamicSelectFilters((prev) => ({
                            ...prev,
                            [field.name]: e.target.value,
                          }))
                        }
                      >
                        <option value="">Any</option>
                        {(field.options ?? []).map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                  );
                }

                if (field.fieldType === 'number') {
                  const filter = dynamicNumberFilters[field.name] ?? defaultNumberFilter();

                  return (
                    <div key={field.id}>
                      {label}
                      <div className="grid grid-cols-[minmax(10rem,0.9fr)_minmax(0,1fr)] gap-2">
                        <select
                          className={selectClass}
                          value={filter.operator}
                          onChange={(e) =>
                            setDynamicNumberFilter(field.name, {
                              operator: e.target.value as NumberOperator,
                            })
                          }
                        >
                          {NUMBER_OPERATORS.map((operator) => (
                            <option key={operator.value} value={operator.value}>
                              {operator.label}
                            </option>
                          ))}
                        </select>
                        <input
                          type="number"
                          className={inputClass}
                          value={filter.value}
                          onChange={(e) =>
                            setDynamicNumberFilter(field.name, { value: e.target.value })
                          }
                          placeholder="Value"
                        />
                      </div>
                    </div>
                  );
                }

                return null;
              })}
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
