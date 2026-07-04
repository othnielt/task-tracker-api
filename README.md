# Task Tracker API

A small full-stack web API built with **C# / ASP.NET Core** and **Microsoft SQL Server** that tracks work items (tasks) through a simple workflow (To Do → In Progress → Review → Done).

Built to demonstrate a clean, layered .NET application with a relational SQL Server backend.

## Tech stack

- **C# / .NET 8 (ASP.NET Core Web API)**
- **Microsoft SQL Server** with **Entity Framework Core** (code-first migrations)
- **Swagger / OpenAPI** for interactive API documentation
- Layered architecture: **data / business / presentation**

## Architecture

```
Models/        Domain entity (WorkItem) + validation rules
Data/          EF Core DbContext - maps the model to SQL Server, seeds sample data
Services/      Business logic behind an interface (IWorkItemService) - unit-test friendly
Controllers/   REST API (presentation layer) - delegates all logic to the service
Program.cs     Entry point, dependency injection, request pipeline
```

This separation keeps business logic independent of the web layer, follows dependency-injection best practices, and makes each layer independently testable.

## Features

- Full **CRUD** for work items (create, read, update, delete)
- A **`/advance`** endpoint that moves an item to the next workflow stage (business logic)
- **Input validation** via data annotations (required fields, length limits)
- **Unique constraint** on the reference code, enforced at the database level
- Enums stored as **readable strings** in SQL Server
- **Seeded sample data** so the API returns results on first run

## API endpoints

| Method | Route                          | Description                        |
|--------|--------------------------------|------------------------------------|
| GET    | `/api/workitems`               | List all work items                |
| GET    | `/api/workitems/{id}`          | Get one work item                  |
| POST   | `/api/workitems`               | Create a work item                 |
| PUT    | `/api/workitems/{id}`          | Update a work item                 |
| POST   | `/api/workitems/{id}/advance`  | Advance to the next workflow stage |
| DELETE | `/api/workitems/{id}`          | Delete a work item                 |

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full) - or update the connection string in `appsettings.json`

### Run it

```bash
# restore dependencies
dotnet restore

# create the database from the EF Core migrations
dotnet ef database update

# run the app
dotnet run
```

Then open the Swagger UI (the URL is printed in the console, e.g. `https://localhost:5001/swagger`) to explore and test the API.

> If you don't have the EF Core CLI: `dotnet tool install --global dotnet-ef`

## Testing

Business logic lives in `WorkItemService` behind the `IWorkItemService` interface, so it can be unit-tested with an in-memory EF Core provider - no database required. See `Tests/WorkItemServiceTests.cs`.

## Notes

This is a compact demonstration project focused on showing clean C#/.NET structure and SQL Server integration. In a production system you would add authentication/authorization, pagination, logging, automated tests in CI, and DTOs to separate the API contract from the entity model.
