import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { employeesApi } from '../api/employees';
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

export function ViewEmployeePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: employee, isLoading, isError } = useQuery({
    queryKey: ['employees', id],
    queryFn: () => employeesApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <p className="text-gray-500">Loading...</p>;
  }

  if (isError || !employee) {
    return (
      <div>
        <p className="text-red-600">Failed to load employee record.</p>
        <button onClick={() => navigate(-1)} className="mt-2 text-sm text-blue-600 hover:underline">
          ← Back
        </button>
      </div>
    );
  }

  const employeeType = employee.employeeType;

  return (
    <div className="max-w-2xl">
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">
          ← Back to results
        </button>
        {employeeType && (
          <span className="rounded-full bg-blue-100 px-3 py-0.5 text-xs font-medium text-blue-700">
            {employeeType.name}
          </span>
        )}
      </div>

      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        {employee.firstName} {employee.lastName}
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
                {(employee as unknown as Record<string, string>)[field.key] || '—'}
              </div>
            </div>
          ))}
        </div>

        {employeeType && employeeType.fields.length > 0 && (
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-4">
              {employeeType.name} Fields
            </h2>
            <DynamicForm
              fields={employeeType.fields}
              values={employee.fieldValues}
              onChange={() => undefined}
              disabled
            />
          </div>
        )}

        <p className="text-xs text-gray-400">
          Created {new Date(employee.createdAt).toLocaleDateString()} · Last updated{' '}
          {new Date(employee.updatedAt).toLocaleDateString()}
        </p>
      </div>
    </div>
  );
}
