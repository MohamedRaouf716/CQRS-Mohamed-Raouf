# Products API — Clean Architecture & CQRS

A small Products API that demonstrates **Clean Architecture**, **CQRS with MediatR**, **EF Core + SQL Server**, and **FluentValidation**.

## Requirements covered

| # | Requirement | Where |
|---|-------------|-------|
| 1 | `Product` entity with `Id`, `Name`, `Price` | `src/Products.Domain/Entities/Product.cs` |
| 2 | `POST /api/products`, `GET /api/products/{id}`, `GET /api/products` | `src/Products.Api/Controllers/ProductsController.cs` |
| 3 | Separation of Domain / Application / Infrastructure / API | Four independent projects under `src/` |
| 4 | EF Core + SQL Server | `src/Products.Infrastructure` |
| 5 | DTOs + Validation | `ProductDto`, FluentValidation validators + MediatR pipeline behavior |

## Architecture

```
Products.Api              (presentation)
    │  references Application + Infrastructure (composition root only)
    ├──> Products.Application   (use cases / CQRS)
    │        └──> Products.Domain
    └──> Products.Infrastructure (EF Core, SQL Server)
             └──> Products.Application
```

- **Domain** — `Product` entity, invariants, no external dependencies.
- **Application** — commands, queries, handlers, DTOs, validators, repository abstraction. Depends only on Domain.
- **Infrastructure** — `ApplicationDbContext`, entity configuration, EF Core migration, repository implementation.
- **API** — controllers, DI composition, Swagger, global exception handling.

### CQRS flow

```
HTTP request → ProductsController → ISender.Send(...)
             → ValidationBehavior (FluentValidation)
             → Command/Query handler
             → IProductRepository → EF Core → SQL Server
```

- `CreateProductCommand` → `CreateProductCommandHandler` → returns `ProductDto`
- `GetProductByIdQuery` → `GetProductByIdQueryHandler` → returns `ProductDto`
- `GetProductsQuery` → `GetProductsQueryHandler` → returns `IReadOnlyList<ProductDto>`

## Tech stack

- .NET 10 / ASP.NET Core controllers
- MediatR 12.5
- FluentValidation 12.2
- EF Core 10.0.13 + SQL Server
- Swashbuckle (Swagger UI)

## Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server (any edition, LocalDB, or Docker)
- `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`)

### 1. Configure the connection string

Edit `src/Products.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\MSSQLSERVER1;Database=ProductsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Examples for other setups:

```text
Server=(localdb)\\MSSQLLocalDB;Database=ProductsDb;Trusted_Connection=True;TrustServerCertificate=True
Server=localhost,1433;Database=ProductsDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True
```

### 2. Apply the migration

```bash
dotnet ef database update --project src/Products.Infrastructure --startup-project src/Products.Api
```

This creates the `ProductsDb` database, the `Products` table, and seeds three products.

### 3. Run

```bash
dotnet run --project src/Products.Api --launch-profile http
```

- API: <http://localhost:5051>
- Swagger UI: <http://localhost:5051/swagger>

Swagger opens automatically when the project is launched from Visual Studio or VS Code
(`launchBrowser` and `launchUrl` are enabled in `Properties/launchSettings.json`).

## Endpoints

| Method | Route | Description | Success | Errors |
|--------|-------|-------------|---------|--------|
| `POST` | `/api/products` | Create a product | `201 Created` + `ProductDto` | `400` validation |
| `GET` | `/api/products/{id}` | Get one product | `200 OK` + `ProductDto` | `404` |
| `GET` | `/api/products` | Get all products | `200 OK` + `ProductDto[]` | — |

### Create

```http
POST /api/products
Content-Type: application/json

{
  "name": "4K Monitor",
  "price": 349.99
}
```

`201 Created`

```json
{
  "id": 4,
  "name": "4K Monitor",
  "price": 349.99
}
```

### Get all

```http
GET /api/products
```

```json
[
  { "id": 1, "name": "Laptop", "price": 1499.99 },
  { "id": 3, "name": "Mechanical Keyboard", "price": 129.50 },
  { "id": 2, "name": "Wireless Mouse", "price": 49.99 }
]
```

### Get by id

```http
GET /api/products/1
```

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 1499.99
}
```

## Validation

`CreateProductCommandValidator` enforces:

- `Name` is required and at most 200 characters.
- `Price` is greater than zero and has at most 2 decimal places.

Validation runs through a MediatR `ValidationBehavior`, so every command is validated before its handler executes. Failures are returned as RFC 7807 `ValidationProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required."],
    "Price": ["Price must be greater than zero."]
  }
}
```

A missing product returns `404` with `ProblemDetails`, and unexpected errors return `500` without leaking exception details.

## Project structure

```
CQRS Task/
├── CQRS Task.slnx
├── README.md
└── src/
    ├── Products.Domain/
    │   ├── Common/BaseEntity.cs
    │   └── Entities/Product.cs
    ├── Products.Application/
    │   ├── Common/
    │   │   ├── Behaviors/ValidationBehavior.cs
    │   │   ├── Exceptions/NotFoundException.cs
    │   │   └── Interfaces/IProductRepository.cs
    │   ├── Features/Products/
    │   │   ├── Commands/CreateProduct/
    │   │   ├── Queries/GetProductById/
    │   │   ├── Queries/GetProducts/
    │   │   ├── Dtos/ProductDto.cs
    │   │   └── ProductMappings.cs
    │   └── DependencyInjection.cs
    ├── Products.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext.cs
    │   │   ├── Configurations/ProductConfiguration.cs
    │   │   ├── Migrations/
    │   │   └── Repositories/ProductRepository.cs
    │   └── DependencyInjection.cs
    └── Products.Api/
        ├── Controllers/ProductsController.cs
        ├── Middleware/GlobalExceptionHandler.cs
        ├── Program.cs
        ├── appsettings.json
        └── Products.Api.http
```

## Design notes

- The domain entity is never exposed by the API; `ProductDto` is the contract.
- `Id` is an `int` generated by SQL Server (`ValueGeneratedOnAdd()` / identity column).
- Repository read methods use `AsNoTracking()` for query-side performance.
- `IProductRepository` lives in Application, so use cases do not depend on EF Core.
- Migrations live in Infrastructure and are applied with the API as the startup project.
