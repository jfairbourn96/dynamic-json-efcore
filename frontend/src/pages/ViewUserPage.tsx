import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { usersApi } from '../api/users';
import { DynamicForm } from '../components/DynamicForm';

const CORE_FIELDS: { key: string; label: string }[] = [
  { key: 'firstName', label: 'First Name' },
  { key: 'lastName', label: 'Last Name' },
  { key: 'email', label: 'Email' },
  { key: 'hireDate', label: 'Hire Date' },
  { key: 'department', label: 'Department' },
];

const inputClass =
  'mt-1 block w-full rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-700 cursor-default';

export function ViewUserPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: user, isLoading, isError } = useQuery({
    queryKey: ['users', id],
    queryFn: () => usersApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <p className="text-gray-500">Loading...</p>;
  }

  if (isError || !user) {
    return (
      <div>
        <p className="text-red-600">Failed to load user record.</p>
        <button onClick={() => navigate(-1)} className="mt-2 text-sm text-blue-600 hover:underline">
          ← Back
        </button>
      </div>
    );
  }

  const userType = user.userType;

  return (
    <div className="max-w-2xl">
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">
          ← Back to results
        </button>
        {userType && (
          <span className="rounded-full bg-blue-100 px-3 py-0.5 text-xs font-medium text-blue-700">
            {userType.name}
          </span>
        )}
      </div>

      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        {user.firstName} {user.lastName}
      </h1>

      <div className="space-y-6">
        <div className="rounded-lg border border-gray-200 p-4 space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">
            Core Information
          </h2>
          {CORE_FIELDS.map((field) => (
            <div key={field.key}>
              <label className="block text-sm font-medium text-gray-700">{field.label}</label>
              <div className={inputClass}>
                {(user as unknown as Record<string, string>)[field.key] || '—'}
              </div>
            </div>
          ))}
        </div>

        {userType && userType.fields.length > 0 && (
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-4">
              {userType.name} Fields
            </h2>
            <DynamicForm
              fields={userType.fields}
              values={user.fieldValues}
              onChange={() => undefined}
              disabled
            />
          </div>
        )}

        <p className="text-xs text-gray-400">
          Created {new Date(user.createdAt).toLocaleDateString()} · Last updated{' '}
          {new Date(user.updatedAt).toLocaleDateString()}
        </p>
      </div>
    </div>
  );
}
