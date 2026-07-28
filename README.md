# Clean Architecture ASP.NET Core 9 Web API

A production-quality ASP.NET Core 9 Web API built with Clean Architecture principles, following Microsoft best practices.

## Architecture Overview

```
├── src/
│   ├── CleanArchitecture.Domain/          # Enterprise business rules
│   │   ├── Common/                        # Base entity
│   │   ├── Entities/                      # Domain entities (User)
│   │   ├── Exceptions/                    # Domain exceptions
│   │   └── Interfaces/                    # Repository interfaces
│   │
│   ├── CleanArchitecture.Application/     # Application business rules
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   │   ├── Requests/                  # Request DTOs
│   │   │   └── Responses/                 # Response DTOs
│   │   ├── Interfaces/                    # Service interfaces
│   │   └── Services/                      # Application services
│   │
│   ├── CleanArchitecture.Infrastructure/  # External concerns
│   │   ├── Data/                          # EF Core DbContext, UnitOfWork
│   │   ├── Migrations/                    # EF Core migrations
│   │   ├── Repositories/                  # Repository implementations
│   │   └── Services/                      # JWT, password hashing
│   │
│   └── CleanArchitecture.API/             # Presentation layer
│       ├── Controllers/                   # API controllers
│       └── Middleware/                    # Global exception handler
│
└── tests/
    └── CleanArchitecture.UnitTests/       # xUnit unit tests
        ├── Repositories/
        └── Services/
```

## Features

- ✅ **Clean Architecture** – Domain → Application → Infrastructure → API
- ✅ **Entity Framework Core 9** with SQL Server
- ✅ **Repository & Unit of Work** pattern
- ✅ **JWT Authentication** with role-based authorization
- ✅ **Swagger / OpenAPI** documentation
- ✅ **Global exception handling** middleware
- ✅ **Structured logging** (console + debug)
- ✅ **Dependency Injection** throughout
- ✅ **xUnit unit tests** (16 tests, Moq + FluentAssertions)
- ✅ **Docker support** (Dockerfile + docker-compose)
- ✅ **Password hashing** with BCrypt

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | 9.0+ |
| [SQL Server](https://www.microsoft.com/en-us/sql-server) | 2019+ (or Docker) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Optional |

## Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone <repo-url>
cd <repo-directory>

# Start all services (API + SQL Server)
docker compose up --build
```

The API will be available at **http://localhost:8080** with Swagger UI at the root.

To override the default passwords, create a `.env` file:

```env
SA_PASSWORD=YourStrong@Passw0rd
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters
```

### Option 2: Local Development

**1. Configure the database connection**

Update `src/CleanArchitecture.API/appsettings.Development.json` with your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitectureDb;User Id=sa;******;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-at-least-32-characters"
  }
}
```

> **Security Note:** Never commit real credentials. Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:
> ```bash
> dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key" --project src/CleanArchitecture.API
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project src/CleanArchitecture.API
> ```

**2. Run the application**

```bash
dotnet run --project src/CleanArchitecture.API
```

The app runs at **https://localhost:5001** / **http://localhost:5000**.  
In Development mode, Swagger UI is available at the root URL.

## Database Migrations

Migrations are applied automatically on startup in Development. To manage manually:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/CleanArchitecture.Infrastructure \
  --startup-project src/CleanArchitecture.API

# Apply migrations
dotnet ef database update \
  --project src/CleanArchitecture.Infrastructure \
  --startup-project src/CleanArchitecture.API

# Remove the last migration
dotnet ef migrations remove \
  --project src/CleanArchitecture.Infrastructure \
  --startup-project src/CleanArchitecture.API
```

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register a new user | No |
| `POST` | `/api/auth/login` | Login and get JWT token | No |

### Users

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/users` | Get all users | Admin role |
| `GET` | `/api/users/{id}` | Get user by ID | Any authenticated |
| `PUT` | `/api/users/{id}` | Update user | Any authenticated |
| `DELETE` | `/api/users/{id}` | Delete user | Admin role |

### Example: Register a user

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"john","email":"john@example.com","password":"Password123!"}'
```

### Example: Login

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"Password123!"}'
```

Use the returned `token` in subsequent requests:

```bash
curl http://localhost:8080/api/users \
  -H "Authorization: ******"
```

## Running Tests

```bash
# Run all unit tests
dotnet test tests/CleanArchitecture.UnitTests

# With verbose output
dotnet test tests/CleanArchitecture.UnitTests -v normal

# With coverage (requires coverlet)
dotnet test tests/CleanArchitecture.UnitTests --collect:"XPlat Code Coverage"
```

## Configuration Reference

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | — |
| `JwtSettings:SecretKey` | JWT signing key (≥ 32 chars) | — |
| `JwtSettings:Issuer` | JWT issuer | `CleanArchitectureAPI` |
| `JwtSettings:Audience` | JWT audience | `CleanArchitectureAPI` |
| `JwtSettings:ExpiryMinutes` | Token lifetime in minutes | `60` |

## Project Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | EF Core SQL Server provider |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT authentication |
| `Swashbuckle.AspNetCore` | Swagger / OpenAPI |
| `BCrypt.Net-Next` | Password hashing |
| `xUnit` | Unit testing framework |
| `Moq` | Mocking library |
| `FluentAssertions` | Readable test assertions |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory DB for tests |

## License

MIT
