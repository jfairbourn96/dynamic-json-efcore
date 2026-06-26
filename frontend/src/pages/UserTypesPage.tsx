import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { UserType, FieldDefinition, CreateUserTypeRequest } from '../types/schema';
import { userTypesApi } from '../api/userTypes';
import { FieldEditor } from '../components/FieldEditor';

type Mode = 'list' | 'create' | 'edit';

const emptyField = (): Partial<FieldDefinition> => ({
  label: '',
  name: '',
  fieldType: 'text',
  required: false,
  options: [],
  order: 0,
});

function buildRequest(
  name: string,
  description: string,
  fields: Partial<FieldDefinition>[],
): CreateUserTypeRequest {
  return {
    name,
    description: description || undefined,
    fields: fields.map((f, i) => ({
      name: f.name || '',
      label: f.label || '',
      fieldType: f.fieldType || 'text',
      required: f.required || false,
      options: f.options || [],
      order: i,
    })),
  };
}

export function UserTypesPage() {
  const qc = useQueryClient();
  const [mode, setMode] = useState<Mode>('list');
  const [selected, setSelected] = useState<UserType | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [fields, setFields] = useState<Partial<FieldDefinition>[]>([]);

  const { data: userTypes = [], isLoading } = useQuery({
    queryKey: ['user-types'],
    queryFn: userTypesApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: (req: CreateUserTypeRequest) => userTypesApi.create(req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['user-types'] });
      resetToList();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, req }: { id: string; req: CreateUserTypeRequest }) =>
      userTypesApi.update(id, { id, ...req }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['user-types'] });
      resetToList();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: userTypesApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['user-types'] }),
  });

  const resetToList = () => {
    setMode('list');
    setSelected(null);
    setName('');
    setDescription('');
    setFields([]);
  };

  const openCreate = () => {
    setSelected(null);
    setName('');
    setDescription('');
    setFields([emptyField()]);
    setMode('create');
  };

  const openEdit = (ut: UserType) => {
    setSelected(ut);
    setName(ut.name);
    setDescription(ut.description || '');
    setFields(ut.fields.map((f) => ({ ...f })));
    setMode('edit');
  };

  const handleFieldChange = (index: number, updated: Partial<FieldDefinition>) => {
    setFields((prev) => prev.map((f, i) => (i === index ? { ...f, ...updated } : f)));
  };

  const handleFieldRemove = (index: number) => {
    setFields((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const req = buildRequest(name, description, fields);
    if (mode === 'create') {
      createMutation.mutate(req);
    } else if (selected) {
      updateMutation.mutate({ id: selected.id, req });
    }
  };

  const inputClass =
    'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

  if (mode === 'list') {
    return (
      <div>
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold text-gray-900">User Types</h1>
          <button
            onClick={openCreate}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            + New User Type
          </button>
        </div>

        {isLoading && <p className="text-gray-500">Loading...</p>}

        {!isLoading && userTypes.length === 0 && (
          <div className="rounded-lg border-2 border-dashed border-gray-300 p-12 text-center">
            <p className="text-gray-500">No user types yet. Create one to get started.</p>
          </div>
        )}

        <div className="space-y-3">
          {userTypes.map((ut) => (
            <div
              key={ut.id}
              className="flex items-center justify-between rounded-lg border border-gray-200 bg-white p-4 shadow-sm"
            >
              <div>
                <p className="font-medium text-gray-900">{ut.name}</p>
                {ut.description && (
                  <p className="text-sm text-gray-500">{ut.description}</p>
                )}
                <p className="text-xs text-gray-400 mt-1">
                  {ut.fields.length} field{ut.fields.length !== 1 ? 's' : ''}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => openEdit(ut)}
                  className="rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50"
                >
                  Edit
                </button>
                <button
                  onClick={() => {
                    if (confirm(`Delete "${ut.name}"?`)) {
                      deleteMutation.mutate(ut.id);
                    }
                  }}
                  className="rounded border border-red-300 px-3 py-1.5 text-sm text-red-600 hover:bg-red-50"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <button onClick={resetToList} className="text-sm text-blue-600 hover:underline">
          ← User Types
        </button>
        <h1 className="text-2xl font-bold text-gray-900">
          {mode === 'create' ? 'New User Type' : `Edit: ${selected?.name}`}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 max-w-3xl">
        <div>
          <label className="block text-sm font-medium text-gray-700">Name</label>
          <input
            type="text"
            className={inputClass}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            placeholder="e.g. Engineer, Project Manager"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Description</label>
          <input
            type="text"
            className={inputClass}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Optional description"
          />
        </div>

        <div>
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">
              Custom Fields
            </h2>
            <button
              type="button"
              onClick={() => setFields((prev) => [...prev, emptyField()])}
              className="text-sm text-blue-600 hover:underline"
            >
              + Add field
            </button>
          </div>
          <div className="space-y-3">
            {fields.map((field, i) => (
              <FieldEditor
                key={i}
                field={field}
                index={i}
                onChange={handleFieldChange}
                onRemove={handleFieldRemove}
              />
            ))}
          </div>
        </div>

        <div className="flex items-center gap-3 pt-2">
          <button
            type="submit"
            disabled={createMutation.isPending || updateMutation.isPending}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {mode === 'create' ? 'Create User Type' : 'Save Changes'}
          </button>
          <button
            type="button"
            onClick={resetToList}
            className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
