import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { userTypesApi } from '../api/userTypes';
import { usersApi } from '../api/users';
import { DynamicForm } from '../components/DynamicForm';
import { CreateUserRequest } from '../types/records';

const CORE_FIELDS: { name: keyof Omit<CreateUserRequest, 'userTypeId' | 'fieldValues'>; label: string; type: string; required: boolean }[] = [
  { name: 'firstName', label: 'First Name', type: 'text', required: true },
  { name: 'lastName', label: 'Last Name', type: 'text', required: true },
  { name: 'email', label: 'Email', type: 'email', required: true },
  { name: 'hireDate', label: 'Hire Date', type: 'date', required: true },
  { name: 'department', label: 'Department', type: 'text', required: false },
];

const inputClass =
  'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

export function AddUserPage() {
  const navigate = useNavigate();
  const [selectedTypeId, setSelectedTypeId] = useState('');
  const [coreValues, setCoreValues] = useState<Record<string, string>>({});
  const [dynamicValues, setDynamicValues] = useState<Record<string, unknown>>({});

  const { data: userTypes = [] } = useQuery({
    queryKey: ['user-types'],
    queryFn: userTypesApi.getAll,
  });

  const selectedType = userTypes.find((ut) => ut.id === selectedTypeId) || null;

  const createMutation = useMutation({
    mutationFn: (req: CreateUserRequest) => usersApi.create(req),
    onSuccess: (user) => navigate(`/users/${user.id}`),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTypeId) {
      return;
    }
    createMutation.mutate({
      firstName: coreValues.firstName || '',
      lastName: coreValues.lastName || '',
      email: coreValues.email || '',
      hireDate: coreValues.hireDate || '',
      department: coreValues.department || '',
      userTypeId: selectedTypeId,
      fieldValues: dynamicValues,
    });
  };

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Add User</h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label className="block text-sm font-medium text-gray-700">
            User Type <span className="text-red-500">*</span>
          </label>
          <select
            className={inputClass}
            value={selectedTypeId}
            onChange={(e) => {
              setSelectedTypeId(e.target.value);
              setDynamicValues({});
            }}
            required
          >
            <option value="">Select a user type...</option>
            {userTypes.map((ut) => (
              <option key={ut.id} value={ut.id}>
                {ut.name}
              </option>
            ))}
          </select>
        </div>

        <div className="rounded-lg border border-gray-200 p-4 space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">
            Core Information
          </h2>
          {CORE_FIELDS.map((field) => (
            <div key={field.name}>
              <label className="block text-sm font-medium text-gray-700">
                {field.label}
                {field.required && <span className="ml-1 text-red-500">*</span>}
              </label>
              <input
                type={field.type}
                className={inputClass}
                value={coreValues[field.name] || ''}
                onChange={(e) =>
                  setCoreValues((prev) => ({ ...prev, [field.name]: e.target.value }))
                }
                required={field.required}
              />
            </div>
          ))}
        </div>

        {selectedType && selectedType.fields.length > 0 && (
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-4">
              {selectedType.name} Fields
            </h2>
            <DynamicForm
              fields={selectedType.fields}
              values={dynamicValues}
              onChange={(name, value) =>
                setDynamicValues((prev) => ({ ...prev, [name]: value }))
              }
            />
          </div>
        )}

        <div className="flex items-center gap-3">
          <button
            type="submit"
            disabled={createMutation.isPending || !selectedTypeId}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {createMutation.isPending ? 'Saving...' : 'Add User'}
          </button>
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>

        {createMutation.isError && (
          <p className="text-sm text-red-600">{(createMutation.error as Error).message}</p>
        )}
      </form>
    </div>
  );
}
