# Invoice Builder

A full-stack invoicing application built with ASP.NET Core 10 (Minimal APIs) and React (TypeScript). Businesses can manage customers, senders, create invoices with line items, and export them as PDF documents.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 10 Minimal APIs, Entity Framework Core, FluentValidation |
| **Frontend** | React 19, TypeScript, Vite, Tailwind CSS v4, React Router v7 |
| **Database** | PostgreSQL 16 |
| **PDF Generation** | PuppeteerSharp (Chromium-based HTML-to-PDF) |
| **Backend Tests** | xUnit, TestContainers (real PostgreSQL) |
| **Frontend Tests** | Cypress (component + E2E) |
| **Deployment** | Docker Compose |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/) (local dev) **or** [Docker](https://www.docker.com/) (for containerised setup)
- Docker is also required for backend integration tests (TestContainers)

---

## Getting Started (Local Development)

### 1. Database

Start a local PostgreSQL instance. The default connection string expects:

| Setting | Value |
|---------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `invoice_db` |
| Username | `postgres` |
| Password | `postgres` |

You can change these in `InvoiceService/appsettings.json` under `ConnectionStrings:DefaultConnection`.

### 2. Backend API

```bash
cd InvoiceService
dotnet restore
dotnet run
```

The API starts at **http://localhost:5083** and automatically applies EF Core migrations on startup.

API endpoints:

| Resource | URL |
|----------|-----|
| Customers | `GET/POST /api/customers`, `GET/PUT/DELETE /api/customers/{id}` |
| Senders | `GET/POST /api/senders`, `GET/PUT/DELETE /api/senders/{id}` |
| Invoices | `GET/POST /api/invoices`, `GET/PUT/DELETE /api/invoices/{id}` |
| PDF Export | `GET /api/invoices/{id}/pdf` |

All list endpoints support pagination via `?page=1&pageSize=10` query parameters.

### 3. Frontend

```bash
cd invoice-client
npm install
npm run dev
```

The frontend starts at **http://localhost:5173** and proxies `/api` requests to the backend automatically.

---

## Getting Started (Docker Compose)

Run the entire stack with a single command from the project root:

```bash
docker compose up --build
```

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | React app served via nginx |
| **API** | http://localhost:8080 | ASP.NET Core API |
| **PostgreSQL** | localhost:5432 | Database with health checks |

The API container includes Chromium for PDF generation. The database schema is applied automatically on startup.

To stop:

```bash
docker compose down
```

To stop and remove data:

```bash
docker compose down -v
```

---

## Running Tests

### Backend Integration Tests (xUnit + TestContainers)

Requires Docker running (TestContainers spins up a real PostgreSQL container automatically).

```bash
cd InvoiceService.Tests
dotnet test
```

**Coverage:**
- Customer CRUD operations (create, read, update, delete, pagination, validation, 404)
- Invoice creation with line items and calculation verification (subtotal, tax, grand total)
- Invoice update with status changes and line item modifications
- PDF generation endpoint (validates PDF content type and magic bytes)

### Frontend Component Tests (Cypress)

```bash
cd invoice-client
npx cypress run --component
```

**Coverage:** Pagination, StatusBadge, ConfirmDialog, Modal, EmptyState components (29 tests).

### Frontend E2E Tests (Cypress)

Requires the frontend dev server running (`npm run dev`).

```bash
# Terminal 1
cd invoice-client
npm run dev

# Terminal 2
cd invoice-client
npx cypress run --e2e
```

**Coverage:**
- Navigation between tabs, active tab highlighting, root redirect
- Customer CRUD: list, create, edit, delete, empty state, error state
- Sender CRUD: list, create, edit, delete, empty state
- Invoice: list with status badges and totals, view details, create with customer/sender dropdowns and dynamic line items with live calculations, edit with status change, delete, PDF download, empty state

### Interactive Cypress UI

```bash
cd invoice-client
npx cypress open
```

---

## Project Structure

```
├── InvoiceService/                  # ASP.NET Core API
│   ├── Data/                        # DbContext and EF Core configurations
│   ├── Modules/
│   │   ├── Customers/               # Customer module (Domain, DTOs, Validators, Repository, Services, Endpoints)
│   │   ├── Senders/                 # Sender module (same structure)
│   │   └── Invoices/                # Invoice module (includes PdfService)
│   ├── Shared/                      # Base entities, exceptions, middleware, pagination
│   ├── Migrations/                  # EF Core migrations
│   ├── Dockerfile                   # Multi-stage build with Chromium for PDF
│   └── Program.cs                   # App entry point with DI and endpoint mapping
│
├── InvoiceService.Tests/            # Backend integration tests
│   ├── Infrastructure/              # WebApplicationFactory with TestContainers
│   └── Tests/                       # Customer, Invoice, PDF export tests
│
├── invoice-client/                  # React frontend
│   ├── src/
│   │   ├── api/                     # Typed API client (customers, senders, invoices)
│   │   ├── components/              # Reusable UI components
│   │   ├── context/                 # Toast notification context
│   │   ├── hooks/                   # Custom hooks (useAsync, useToast)
│   │   ├── pages/                   # Page components (Customers, Senders, Invoices)
│   │   └── types/                   # TypeScript interfaces matching API DTOs
│   ├── cypress/
│   │   ├── component/               # Component tests
│   │   └── e2e/                     # E2E integration tests
│   ├── Dockerfile                   # Multi-stage build (Node → nginx)
│   └── nginx.conf                   # Production nginx config with API proxy
│
├── docker-compose.yml               # Full stack: PostgreSQL + API + Frontend
└── README.md
```

## Architecture

The backend follows a **Modular Monolith** pattern. Each module (Customers, Senders, Invoices) is self-contained with its own:

- **Domain** — Entity classes
- **DTOs** — Request/response records
- **Validators** — FluentValidation rules
- **Repository** — Data access (EF Core)
- **Services** — Business logic
- **Endpoints** — Minimal API route definitions

Shared concerns (base entity, pagination, exceptions, middleware) live in the `Shared` folder.
