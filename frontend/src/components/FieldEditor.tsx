import type { FieldDefinition, FieldType, FieldOption } from '../types/schema';

interface FieldEditorProps {
  field: Partial<FieldDefinition>;
  index: number;
  onChange: (index: number, updated: Partial<FieldDefinition>) => void;
  onRemove: (index: number) => void;
}

const FIELD_TYPES: { value: FieldType; label: string }[] = [
  { value: 'text', label: 'Text' },
  { value: 'number', label: 'Number' },
  { value: 'date', label: 'Date' },
  { value: 'boolean', label: 'Checkbox' },
  { value: 'select', label: 'Dropdown' },
];

const inputClass =
  'block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

export function FieldEditor({ field, index, onChange, onRemove }: FieldEditorProps) {
  const handleOptionsChange = (raw: string) => {
    const options: FieldOption[] = raw
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean)
      .map((line) => ({ label: line, value: line.toLowerCase().replace(/\s+/g, '_') }));
    onChange(index, { options });
  };

  const optionsText = (field.options || []).map((o) => o.label).join('\n');

  return (
    <div className="rounded-lg border border-gray-200 bg-gray-50 p-4">
      <div className="flex items-start justify-between gap-4">
        <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Label</label>
            <input
              type="text"
              className={inputClass}
              placeholder="Display label"
              value={field.label || ''}
              onChange={(e) => onChange(index, { label: e.target.value })}
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Field name</label>
            <input
              type="text"
              className={inputClass}
              placeholder="camelCase key"
              value={field.name || ''}
              onChange={(e) =>
                onChange(index, { name: e.target.value.replace(/\s+/g, '_') })
              }
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Type</label>
            <select
              className={inputClass}
              value={field.fieldType || 'text'}
              onChange={(e) => onChange(index, { fieldType: e.target.value as FieldType })}
            >
              {FIELD_TYPES.map((t) => (
                <option key={t.value} value={t.value}>
                  {t.label}
                </option>
              ))}
            </select>
          </div>
          <div className="flex items-end pb-1">
            <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
              <input
                type="checkbox"
                className="h-4 w-4 rounded border-gray-300 text-blue-600"
                checked={field.required || false}
                onChange={(e) => onChange(index, { required: e.target.checked })}
              />
              Required
            </label>
          </div>
        </div>
        <button
          type="button"
          onClick={() => onRemove(index)}
          className="mt-5 text-gray-400 hover:text-red-500 transition-colors text-lg leading-none"
          aria-label="Remove field"
        >
          ✕
        </button>
      </div>

      {field.fieldType === 'select' && (
        <div className="mt-3">
          <label className="block text-xs font-medium text-gray-600 mb-1">
            Options (one per line)
          </label>
          <textarea
            className={inputClass}
            rows={3}
            placeholder={'Option A\nOption B\nOption C'}
            value={optionsText}
            onChange={(e) => handleOptionsChange(e.target.value)}
          />
        </div>
      )}
    </div>
  );
}
