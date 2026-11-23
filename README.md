# Online Learning Platform

C# Console application using Entity Framework Core 8 and SQL Server.

## Requirements
- .NET 8 SDK
- SQL Server (Developer or Express) with a default instance (`Server=.`)

## Run
1. Open `OnlineLearningPlatform.csproj` in Visual Studio.
2. Ensure SQL Server is installed and its Database Engine service is running.
3. If your SQL Server instance name differs, update the connection string in `AppDbContext.OnConfiguring` in `Program.cs`.
4. Restore NuGet packages and run the project. `Main` applies the included migrations automatically.

Database: `OnlineLearningDB`.

## Included
- Entities and enums: Instructor, Category, Course, Student, Enrollment.
- Fluent configurations, keys, indexes, relationships, delete behaviors and check constraints.
- CRUD and LINQ demonstrations.
- `InitialModel` migration creates the schema.
- `SeedData` migration inserts the initial records.

The project intentionally targets .NET 8 and uses matching EF Core 8 packages. SQL Server is a required runtime dependency; the project can be built without connecting to it, but the console demonstration requires a running SQL Server instance.
