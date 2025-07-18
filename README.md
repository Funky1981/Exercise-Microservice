# Exercise Microservice

A .NET 9 microservice for managing exercise data, integrating with RapidAPI's Exercise Database.

## Project Structure

This project follows Clean Architecture principles:

Exercise-Microservice/
├── Exercise.API/           # API layer, endpoints and controllers
├── Exercise.Domain/        # Domain entities and business logic
├── Exercise.Application/   # Application services and interfaces
├── Exercise.Infrastructure/# External services and data access
└── Exercise-Microservice.sln

## Features

- Minimal API approach for lightweight endpoints
- Clean Architecture for maintainable, testable code
- Docker support with Windows containers
- OpenAPI/Swagger documentation
- JWT Bearer authentication
- RapidAPI integration for exercise data

## Current Endpoints

### GET /exercises/{bodyPart}
Retrieves exercises for a specific body part from the external exercise database.

## Technology Stack

- .NET 9
- ASP.NET Core
- Docker
- RapidAPI Exercise Database

## Configuration

The application requires the following configuration in appsettings.json:

{
  "RapidApi": {
    "Host": "exercisedb.p.rapidapi.com",
    "Key": "your-api-key"
  }
}

## Getting Started

1. Clone the repository
2. Set up RapidAPI credentials in user secrets:
   dotnet user-secrets set "RapidApi:Key" "your-api-key"
3. Run the application:
   dotnet run --project Exercise.API

## Docker Support

Build and run with Docker:
docker build -t exercise-microservice .
docker run -p 8080:80 exercise-microservice

## Project Status

- ✅ Initial API project setup
- ✅ Basic endpoint implementation
- ✅ External API integration
- 🔄 Domain model (In Progress)
- 🔄 Application services (In Progress)
- 🔄 Infrastructure setup (In Progress)

## Next Steps

- Implement Domain entities and business logic
- Set up Application layer services
- Complete Infrastructure implementation
- Add comprehensive error handling
- Implement remaining CRUD operations
- Add API documentation