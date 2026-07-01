import type { FieldDefinition, FieldType } from '../types/schema';
import { cn } from '../lib/utils';

interface DynamicFormProps {
  fields: FieldDefinition[];
  values: Record<string, unknown>;
  onChange: (name: string, value: unknown) => void;
  disabled?: boolean;
}

function renderInput(
  field: FieldDefinition,
  value: unknown,
  onChange: (name: string, value: unknown) => void,
  disabled: boolean,
) {
  const base =
    'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:bg-gray-50 disabled:text-gray-500';

  switch (field.fieldType as FieldType) {
    case 'text':
      return (
        <input
          type="text"
          id={field.name}
          className={base}
          value={(value as string) || ''}
          onChange={(e) => onChange(field.name, e.target.value)}
          disabled={disabled}
          required={field.required}
        />
      );
    case 'number':
      return (
        <input
          type="number"
          id={field.name}
          className={base}
          value={(value as number) ?? ''}
          onChange={(e) => onChange(field.name, e.target.valueAsNumber)}
          disabled={disabled}
          required={field.required}
        />
      );
    case 'date':
      return (
        <input
          type="date"
          id={field.name}
          className={base}
          value={(value as string) || ''}
          onChange={(e) => onChange(field.name, e.target.value)}
          disabled={disabled}
          required={field.required}
        />
      );
    case 'boolean':
      return (
        <div className="mt-1 flex items-center gap-2">
          <input
            type="checkbox"
            id={field.name}
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 disabled:opacity-50"
            checked={(value as boolean) || false}
            onChange={(e) => onChange(field.name, e.target.checked)}
            disabled={disabled}
          />
          <span className="text-sm text-gray-600">{field.label}</span>
        </div>
      );
    case 'select':
      return (
        <select
          id={field.name}
          className={base}
          value={(value as string) || ''}
          onChange={(e) => onChange(field.name, e.target.value)}
          disabled={disabled}
          required={field.required}
        >
          <option value="">Select...</option>
          {(field.options || []).map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      );
    default:
      return null;
  }
}

export function DynamicForm({ fields, values, onChange, disabled = false }: DynamicFormProps) {
  const sorted = [...fields].sort((a, b) => a.order - b.order);

  if (sorted.length === 0) {
    return <p className="text-sm text-gray-500 italic">No additional fields for this user type.</p>;
  }

  return (
    <div className="space-y-4">
      {sorted.map((field) => (
        <div key={field.id} className={cn(field.fieldType === 'boolean' && 'flex items-start')}>
          {field.fieldType !== 'boolean' && (
            <label htmlFor={field.name} className="block text-sm font-medium text-gray-700">
              {field.label}
              {field.required && <span className="ml-1 text-red-500">*</span>}
            </label>
          )}
          {renderInput(field, values[field.name], onChange, disabled)}
        </div>
      ))}
    </div>
  );
}
