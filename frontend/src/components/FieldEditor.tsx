import { Plus, Trash2, X } from 'lucide-react';
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
  const options = field.options?.length ? field.options : [{ value: '', label: '' }];

  const handleTypeChange = (fieldType: FieldType) => {
    onChange(index, {
      fieldType,
      options: fieldType === 'select' ? options : [],
    });
  };

  const handleOptionChange = (
    optionIndex: number,
    key: keyof FieldOption,
    value: string,
  ) => {
    const updatedOptions = options.map((option, i) =>
      i === optionIndex ? { ...option, [key]: value } : option,
    );

    onChange(index, { options: updatedOptions });
  };

  const handleOptionAdd = () => {
    onChange(index, { options: [...options, { value: '', label: '' }] });
  };

  const handleOptionRemove = (optionIndex: number) => {
    const updatedOptions = options.filter((_, i) => i !== optionIndex);
    onChange(index, {
      options: updatedOptions.length ? updatedOptions : [{ value: '', label: '' }],
    });
  };

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
              onChange={(e) => handleTypeChange(e.target.value as FieldType)}
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
          className="mt-5 rounded p-1 text-gray-400 transition-colors hover:bg-red-50 hover:text-red-500"
          aria-label="Remove field"
          title="Remove field"
        >
          <X className="h-4 w-4" />
        </button>
      </div>

      {field.fieldType === 'select' && (
        <div className="mt-3">
          <div className="mb-2 grid grid-cols-[1fr_1fr_2rem] gap-2">
            <span className="text-xs font-medium text-gray-600">Value</span>
            <span className="text-xs font-medium text-gray-600">Label</span>
            <span className="sr-only">Remove</span>
          </div>

          <div className="space-y-2">
            {options.map((option, optionIndex) => (
              <div
                key={optionIndex}
                className="grid grid-cols-[1fr_1fr_2rem] items-center gap-2"
              >
                <input
                  type="text"
                  className={inputClass}
                  placeholder="admin"
                  value={option.value}
                  onChange={(e) =>
                    handleOptionChange(optionIndex, 'value', e.target.value)
                  }
                />
                <input
                  type="text"
                  className={inputClass}
                  placeholder="Administrator"
                  value={option.label}
                  onChange={(e) =>
                    handleOptionChange(optionIndex, 'label', e.target.value)
                  }
                />
                <button
                  type="button"
                  onClick={() => handleOptionRemove(optionIndex)}
                  className="flex h-8 w-8 items-center justify-center rounded border border-gray-300 text-gray-500 transition-colors hover:border-red-300 hover:bg-red-50 hover:text-red-600"
                  aria-label="Remove option"
                  title="Remove option"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            ))}
          </div>

          <button
            type="button"
            onClick={handleOptionAdd}
            className="mt-2 inline-flex items-center gap-1 rounded border border-gray-300 px-2.5 py-1.5 text-xs font-medium text-gray-700 transition-colors hover:bg-white"
          >
            <Plus className="h-3.5 w-3.5" />
            Add option
          </button>
        </div>
      )}
    </div>
  );
}
