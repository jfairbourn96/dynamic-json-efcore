import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { queryClient } from './lib/queryClient';
import { EmployeeTypesPage } from './pages/EmployeeTypesPage';
import { AddEmployeePage } from './pages/AddEmployeePage';
import { SearchEmployeesPage } from './pages/SearchEmployeesPage';
import { ViewEmployeePage } from './pages/ViewEmployeePage';

const NAV_LINKS = [
  { to: '/employee-types', label: 'Employee Types' },
  { to: '/employees/add', label: 'Add Employee' },
  { to: '/employees/search', label: 'Search Employees' },
];

function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-100">
      <nav className="bg-white border-b border-gray-200 shadow-sm">
        <div className="mx-auto max-w-5xl px-4 flex items-center gap-6 h-14">
          <span className="font-semibold text-gray-900">Dynamic HR</span>
          {NAV_LINKS.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `text-sm font-medium transition-colors ${
                  isActive ? 'text-blue-600' : 'text-gray-600 hover:text-gray-900'
                }`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </div>
      </nav>
      <main className="mx-auto max-w-5xl px-4 py-8">{children}</main>
    </div>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<EmployeeTypesPage />} />
            <Route path="/employee-types" element={<EmployeeTypesPage />} />
            <Route path="/employees/add" element={<AddEmployeePage />} />
            <Route path="/employees/search" element={<SearchEmployeesPage />} />
            <Route path="/employees/:id" element={<ViewEmployeePage />} />
          </Routes>
        </Layout>
      </BrowserRouter>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
