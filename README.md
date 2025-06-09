# Order Management System

A modern, scalable order management system built using Clean Architecture principles and .NET 8.0.

## 🏗️ Architecture

This solution follows Clean Architecture principles with a clear separation of concerns:

- **Domain Layer**: Core business logic and entities
- **Application Layer**: Business rules and use cases
- **Infrastructure Layer**: External concerns and implementations
- **API Layer**: Web API endpoints and controllers

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (for production) or SQLite (for development)
- Required NuGet packages:
  ```bash
  dotnet add package NSwag.AspNetCore --version 14.4.0
  ```

### Building the Solution

```bash
dotnet build -tl
```

### Running the Application

```bash
cd src/Api
dotnet watch run
```

The application will be available at https://localhost:5001

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **Architecture**: Clean Architecture
- **Database**: Entity Framework Core 8.0
- **API Documentation**: Swagger/OpenAPI
- **Validation**: FluentValidation
- **Testing**: xUnit, NUnit, FluentAssertions
- **Code Quality**: EditorConfig for consistent coding styles

## 📦 Key Dependencies

- MediatR for CQRS pattern implementation
- FluentValidation for request validation
- Entity Framework Core for data access

## 🧪 Testing

The solution includes unit tests covering:

- Command and Query Handlers
- Validators
- Business Logic

Run the tests with:

```bash
dotnet test
```

## 🎯 Areas for Improvement

The following areas have been identified for future enhancement:

1. **API Documentation**

   - Enhance Swagger UI with more detailed endpoint descriptions, resolve the issue where the swagger doc says My Title, even though a title has been defined in program.cs
   - Add request/response examples
   - Include authentication requirements in documentation

2. **Test Coverage**

   - Add Integration tests for API endpoints
   - Implement API tests to verify end-to-end functionality
   - Expand test coverage beyond the Application layer

3. **Security**

   - Implement Authentication using ASP.NET Core Identity
   - Add role-based authorization
   - Secure API endpoints

4. **Performance Optimization**

   - Move daily report filtering to the database layer
   - Add additional caching where appropriate

5. **API Response Handling**

   - Introduce additional appropriate HTTP status codes for different scenarios

6. **Additional functionality**

   - Add logic around deletion of order
