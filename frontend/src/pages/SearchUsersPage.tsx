import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { userTypesApi } from '../api/userTypes';
import { usersApi } from '../api/users';
import { DynamicSearch } from '../components/DynamicSearch';
import { UserSearchFilters, User, PagedResult } from '../types/records';
import { FieldDefinition } from '../types/schema';

export function SearchUsersPage() {
  const navigate = useNavigate();
  const [selectedTypeId, setSelectedTypeId] = useState('');
  const [results, setResults] = useState<PagedResult<User> | null>(null);

  const { data: userTypes = [] } = useQuery({
    queryKey: ['user-types'],
    queryFn: userTypesApi.getAll,
  });

  const selectedType = userTypes.find((ut) => ut.id === selectedTypeId) || null;
  const dynamicFields: FieldDefinition[] = selectedType ? selectedType.fields : [];

  const searchMutation = useMutation({
    mutationFn: (filters: UserSearchFilters) =>
      usersApi.search({ ...filters, userTypeId: selectedTypeId || undefined }),
    onSuccess: (data) => setResults(data),
  });

  const handleSearch = (filters: UserSearchFilters) => {
    searchMutation.mutate(filters);
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Search Users</h1>

      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Filter by User Type
        </label>
        <select
          className="block w-64 rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          value={selectedTypeId}
          onChange={(e) => {
            setSelectedTypeId(e.target.value);
            setResults(null);
          }}
        >
          <option value="">All types</option>
          {userTypes.map((ut) => (
            <option key={ut.id} value={ut.id}>
              {ut.name}
            </option>
          ))}
        </select>
      </div>

      <div className="rounded-lg border border-gray-200 bg-white p-6 mb-6">
        <DynamicSearch
          fields={dynamicFields}
          onSearch={handleSearch}
          isLoading={searchMutation.isPending}
        />
      </div>

      {searchMutation.isError && (
        <p className="text-sm text-red-600 mb-4">{(searchMutation.error as Error).message}</p>
      )}

      {results && (
        <div>
          <p className="text-sm text-gray-500 mb-3">
            {results.totalCount} result{results.totalCount !== 1 ? 's' : ''}
          </p>

          {results.items.length === 0 ? (
            <div className="rounded-lg border-2 border-dashed border-gray-200 p-8 text-center">
              <p className="text-gray-500">No users match your search.</p>
            </div>
          ) : (
            <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    {['Name', 'Email', 'Department', 'User Type', 'Hire Date', ''].map((h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wide text-gray-500"
                      >
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {results.items.map((user) => (
                    <tr key={user.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm font-medium text-gray-900">
                        {user.firstName} {user.lastName}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{user.email}</td>
                      <td className="px-4 py-3 text-sm text-gray-600">{user.department || '—'}</td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {user.userType ? user.userType.name : '—'}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{user.hireDate}</td>
                      <td className="px-4 py-3 text-right">
                        <button
                          onClick={() => navigate(`/users/${user.id}`)}
                          className="text-sm text-blue-600 hover:underline"
                        >
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
