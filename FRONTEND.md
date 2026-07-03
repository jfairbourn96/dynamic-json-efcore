# Frontend Architecture

## Overview

The frontend is a React 19 + TypeScript single-page application (SPA) built with Vite and styled with Tailwind CSS. It communicates with the ASP.NET Core backend API to manage dynamic employee types and employee records.

---

## Tech Stack & Build

### Vite
Lightning-fast build tool and dev server:
- Configured with React plugin in `vite.config.ts`
- Path alias set up: `@/` maps to `src/` for cleaner imports
- Dev server runs on `http://localhost:5173` (configurable)
- Build produces optimized production bundles
- Hot Module Replacement (HMR) for instant feedback during development

### TypeScript
All source code is written in TypeScript with strict mode enabled:
- Type-safe across all components, API calls, and data structures
- Compilation runs before Vite build: `tsc -b && vite build`
- Config files: `tsconfig.json`, `tsconfig.app.json`, `tsconfig.node.json`

### Tailwind CSS
Utility-first styling framework:
- Scans `./src/**/*.{ts,tsx}` for Tailwind class usage
- PostCSS pipeline includes Tailwind and Autoprefixer for vendor prefixes
- Global styles in `index.css` (includes Tailwind directives: `@tailwind` base/components/utilities)
- Configuration in `tailwind.config.js` with default theme extensions

### Oxlint
Fast, modern JavaScript linter for code quality:
- Run with `npm run lint`
- Configured for TypeScript and React best practices

---

## Routing & Layout

### React Router v7
Client-side routing via `react-router-dom`:
- Declarative route definitions in `App.tsx`
- `NavLink` components for navigation with automatic active state styling
- Nested layouts support via shared `Layout` wrapper

### Route Structure
| Route | Component | Purpose |
|-------|-----------|---------|
| `/` | EmployeeTypesPage | Home/default view |
| `/employee-types` | EmployeeTypesPage | Create, edit, and list employee types with dynamic fields |
| `/employees/add` | AddEmployeePage | Create new employees based on selected type |
| `/employees/search` | SearchEmployeesPage | Search and filter existing employees |
| `/employees/:id` | ViewEmployeePage | View single employee details with all dynamic fields |

### Layout Component
Wraps all routes with:
- Global navigation bar (`nav` with Tailwind styling)
- Active link highlighting using `NavLink`
- Responsive max-width container (`max-w-5xl`)
- Consistent page styling (gray background, centered content)

---

## Data Management & API

### TanStack React Query (v5)
Server state management with automatic caching and synchronization:
- **Configured in** `lib/queryClient.ts`
- **Default Options:**
  - **Stale Time:** 5 minutes — data is considered fresh for 5 minutes before automatic refetch
  - **Retry:** 1 retry on failure
- **DevTools:** Included in dev mode (`ReactQueryDevtools`) for debugging cache state and queries
- **Benefits:**
  - Automatic request deduplication
  - Background refetching
  - Cache invalidation on mutations
  - TypeScript-safe queries and mutations

### API Client
**File:** `api/client.ts`

Centralized HTTP client wrapping the Fetch API:
```typescript
const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) => request<T>(path, { method: 'POST', ... }),
  put: <T>(path: string, body: unknown) => request<T>(path, { method: 'PUT', ... }),
  delete: (path: string) => request<void>(path, { method: 'DELETE' }),
};
```

**Features:**
- Base URL: `VITE_API_BASE_URL` environment variable or `http://localhost:5000/api`
- Automatic JSON serialization/deserialization
- Generic typing for type-safe responses
- Error handling with meaningful error messages
- Handles 204 No Content responses correctly

### Domain-Specific API Modules
**Location:** `api/`

Each module wraps the centralized `api` client with domain logic:
- **`employeeTypes.ts`** — Employee Type CRUD operations
- **`employees.ts`** — Employee CRUD operations
- Each exports functions that return React Query hooks (`useQuery`, `useMutation`)

---

## Components

**Location:** `src/components/`

Reusable, composable UI building blocks:

### DynamicForm.tsx
Renders HTML form fields based on a schema definition:
- **Props:**
  - `fields: FieldDefinition[]` — Metadata describing field types, validation, etc.
  - `values: Record<string, unknown>` — Current form state
  - `onChange: (name: string, value: unknown) => void` — Change handler
  - `disabled?: boolean` — Disable all inputs
- **Supported Field Types:** text, number, date, boolean, select
- **Styling:** Tailwind base input classes with focus and disabled states
- **Used By:** AddEmployeePage, ViewEmployeePage

### FieldEditor.tsx
Builder UI for creating and editing employee type field definitions:
- Allows users to define custom fields for employee types
- Field configuration: name, label, type, required, options, order
- Used in EmployeeTypesPage for dynamic schema management

### DynamicSearch.tsx
Search and filter interface for employees:
- Used in SearchEmployeesPage
- Filters employees based on field values

---

## Pages

**Location:** `src/pages/`

Route-level container components managing data flow and business logic:

### EmployeeTypesPage.tsx
**Mode Management:** list | create | edit

Manages the employee type lifecycle:
- **List Mode:** Display all employee types in a table/grid
- **Create Mode:** Form to add new employee types
- **Edit Mode:** Update existing employee type with FieldEditor
- **React Query Integration:**
  - `useQuery` for fetching all employee types
  - `useMutation` for create, update, delete with automatic cache invalidation
- **Features:**
  - FieldEditor for dynamic field management
  - Real-time field reordering and configuration

### AddEmployeePage.tsx
Creates a new employee:
- Fetches available employee types
- Uses DynamicForm for dynamic field rendering
- Submits form data to backend via mutation

### SearchEmployeesPage.tsx
Search and discover employees:
- DynamicSearch component for filtering
- Results display with pagination or scrolling
- Link to ViewEmployeePage for individual records

### ViewEmployeePage.tsx
Displays a single employee record:
- URL parameter `:id` for employee identification
- All dynamic fields rendered via DynamicForm (likely in read-only mode)
- Edit mode option (if needed)

---

## Types & Schema

**File:** `src/types/schema.ts`

Defines the contract between frontend and backend with TypeScript interfaces:

```typescript
type FieldType = 'text' | 'number' | 'date' | 'boolean' | 'select';

interface FieldOption {
  label: string;
  value: string;
}

interface FieldDefinition {
  id: string;
  name: string;
  label: string;
  fieldType: FieldType;
  required: boolean;
  options?: FieldOption[];
  order: number;
}

interface EmployeeType {
  id: string;
  name: string;
  description?: string;
  parentTypeId?: string | null;
  fields: FieldDefinition[];
  createdAt: string;
  updatedAt: string;
}
```

**Key Points:**
- `FieldType` — Enum of supported form field types
- `FieldDefinition` — Metadata for a single form field
- `EmployeeType` — Complete schema for an employee type
- Requests have separate types for POST vs PUT operations

---

## Styling Approach

### Tailwind CSS
- Utility-first styling for all layouts and components
- No separate CSS files in components (all inline Tailwind classes)
- Responsive modifiers: `md:`, `lg:`, `hover:`, etc.

### Radix UI
Accessible component primitives:
- `@radix-ui/react-dialog` — Modal dialogs
- `@radix-ui/react-label` — Form labels with accessibility
- `@radix-ui/react-select` — Select dropdowns
- `@radix-ui/react-separator` — Visual dividers
- `@radix-ui/react-slot` — Polymorphic slot composition

### Lucide React
Consistent, high-quality SVG icon library:
- Used for UI icons throughout the application
- Tree-shakable (only imported icons included in bundle)

### Utility Libraries
- **clsx** — Conditional class name composition
- **tailwind-merge** — Merge Tailwind classes intelligently (resolve conflicts)
- **class-variance-authority (CVA)** — Component style variants and slots

---

## Development Workflow

### Available Scripts

```bash
npm run dev      # Start Vite dev server with HMR on http://localhost:5173
npm run build    # TypeScript check + Vite build (outputs to dist/)
npm run lint     # Run Oxlint for code quality
npm run preview  # Preview production build locally before deployment
```

### Environment Variables

Create `.env.local` or `.env.development.local` for local overrides:
```env
VITE_API_BASE_URL=http://localhost:5000/api
```

### Hot Module Replacement (HMR)
- Changes to component files are instantly reflected in the browser
- Component state is preserved during updates
- Very fast development feedback loop

---

## Directory Structure

```
frontend/
├── src/
│   ├── api/                    # API client and domain modules
│   │   ├── client.ts           # Centralized HTTP client
│   │   ├── employees.ts        # Employee API hooks
│   │   └── employeeTypes.ts    # Employee Type API hooks
│   ├── components/             # Reusable UI components
│   │   ├── DynamicForm.tsx
│   │   ├── DynamicSearch.tsx
│   │   └── FieldEditor.tsx
│   ├── lib/                    # Utilities and configuration
│   │   ├── queryClient.ts      # React Query configuration
│   │   └── utils.ts            # Helper functions
│   ├── pages/                  # Route-level pages
│   │   ├── EmployeeTypesPage.tsx
│   │   ├── AddEmployeePage.tsx
│   │   ├── SearchEmployeesPage.tsx
│   │   └── ViewEmployeePage.tsx
│   ├── types/                  # TypeScript type definitions
│   │   ├── schema.ts           # API contract types
│   │   └── records.ts          # Record/data types
│   ├── App.tsx                 # Router and Layout
│   ├── App.css
│   ├── index.css               # Global Tailwind styles
│   └── main.tsx                # React entry point
├── public/                     # Static assets
├── vite.config.ts              # Vite configuration
├── tailwind.config.js          # Tailwind configuration
├── postcss.config.js           # PostCSS configuration
├── tsconfig.json               # TypeScript root config
├── tsconfig.app.json           # TypeScript app config
├── tsconfig.node.json          # TypeScript node config
├── package.json                # Dependencies and scripts
└── index.html                  # HTML entry point
```

---

## Key Design Patterns

### Separation of Concerns
- **API Layer** (`api/`) — Handles all HTTP communication
- **Components** — Focused on UI rendering and user interaction
- **Pages** — Orchestrate data fetching and component composition
- **Types** — Centralized schema definitions

### React Query Integration
- Mutations automatically invalidate related queries
- DevTools provide visibility into cache state
- Queries deduped within stale time window
- Optimistic updates possible for better UX

### TypeScript First
- All props, state, and API responses are typed
- Catches errors at compile time
- IDE autocomplete and refactoring support

### Tailwind + Radix UI
- Accessible components out of the box
- Consistent styling across the application
- Easy to customize via Tailwind config
- Minimal JavaScript overhead (Radix is lightweight)

---

## Performance Considerations

- **Code Splitting:** Vite automatically chunks components
- **Lazy Loading:** Consider `React.lazy()` for large page components
- **Image Optimization:** Use Next.js Image or similar if needed later
- **Query Caching:** React Query prevents unnecessary API calls
- **Build Optimization:** `npm run build` generates minified, tree-shaken bundles

---

## Common Tasks

### Adding a New Page
1. Create component in `src/pages/PageName.tsx`
2. Define API hooks in `src/api/`
3. Add route in `App.tsx` Routes
4. Update `NAV_LINKS` if needed

### Adding a New Component
1. Create file in `src/components/ComponentName.tsx`
2. Define TypeScript props interface
3. Use Tailwind for styling
4. Export from component file

### Styling
- Use Tailwind utility classes directly in JSX
- Create Tailwind `@apply` rules in `index.css` for repeated patterns
- Use CVA for complex component variants

### Fetching Data
1. Define types in `src/types/schema.ts`
2. Create API function in `src/api/domain.ts`
3. Use `useQuery()` hook in pages/components
4. Handle loading/error states

