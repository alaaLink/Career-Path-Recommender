# Career Path Recommender

An ASP.NET Core MVC application that helps employees plan their career growth with AI-assisted recommendations, skill gap analysis, and actionable learning paths.

## Features
- Employee profiles with skills, experience, and goals
- AI-powered recommendations for courses, mentors, and projects
- Skill gap analysis with estimated timeline and milestones
- Interactive dashboard with responsive UI
- Authentication with ASP.NET Core Identity and Google OAuth (optional)
- Email notifications via Mailjet (configurable)
- Comprehensive logging with Serilog

## Architecture
- Presentation: `CareerPathRecommender.Web/` (ASP.NET Core MVC + Razor Pages)
- Application: `CareerPathRecommender.Application/` (DTOs, interfaces, use-cases, constants)
- Domain: `CareerPathRecommender.Domain/` (entities, enums, domain interfaces)
- Infrastructure: `CareerPathRecommender.Infrastructure/` (EF Core, repositories, services, migrations)
- Tests: `CareerPathRecommender.Tests/` (xUnit)

Key services:
- `RecommendationService` — ranks and creates course/mentor/project recommendations
- `MockAIService` — local, cost-free AI reasoning used via `IAIService`
- `CachedRecommendationService` — adds caching around recommendations

## Tech Stack
- .NET 8, ASP.NET Core MVC, Razor Pages
- Entity Framework Core with SQL Server (migrations in `Infrastructure`)
- Serilog for logging
- Bootstrap 5, jQuery, Font Awesome
- xUnit for tests

## Getting Started
### Prerequisites
- .NET 8 SDK: https://dotnet.microsoft.com/download
- SQL Server (LocalDB or full SQL Server)

### 1) Clone and Restore
```bash
# from the repository root
 dotnet --info
 dotnet restore
```

### 2) Configure App Settings
Update `CareerPathRecommender.Web/appsettings.json` (or use user secrets/environment variables):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CareerPathRecommender;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Authentication": {
    "Google": {
      "ClientId": "<your_google_client_id>",
      "ClientSecret": "<your_google_client_secret>"
    }
  },
  "Mailjet": {
    "ApiKey": "<your_mailjet_api_key>",
    "SecretKey": "<your_mailjet_secret>",
    "FromEmail": "no-reply@example.com",
    "FromName": "Career Path Recommender"
  },
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
```
Notes:
- Google OAuth is optional. If not configured, local email/password works.
- Mailjet is optional; emailing features will be disabled without keys.

### 3) Build and Run the Web App
Migrations are applied automatically at startup and seed data is inserted if empty.
```bash
# build
 dotnet build

# run the web project
 dotnet run --project CareerPathRecommender.Web/CareerPathRecommender.Web.csproj
```
The app runs on `https://localhost:5001` or `https://localhost:7142` (check console output).

### 4) Login and Explore
- Navigate to the login page (default route points to `Account/Login`).
- Use Google sign-in if configured or register a new account.
- Open the Dashboard to view recommendations and skill gap analysis.

## Tests
Run all tests:
```bash
 dotnet test
```

## Configuration Reference
- `Program.cs` registers:
  - `IAIService` => `MockAIService` (local AI reasoning)
  - `IRecommendationService` via `CachedRecommendationService`
  - EF Core SQL Server using `DefaultConnection`
- On startup, the app:
  - Applies migrations automatically (`context.Database.MigrateAsync()`)
  - Seeds data if no employees exist (`DataSeeder.SeedDataAsync(...)`)

## Replacing the Mock AI
To integrate a real AI provider:
1. Implement `IAIService` in a new class, e.g. `OpenAIAIService`.
2. Register it in `CareerPathRecommender.Web/Program.cs`:
```csharp
builder.Services.AddScoped<IAIService, OpenAIAIService>();
```
3. Provide API keys via configuration and update the implementation accordingly.

## Project Structure (Top-Level)
```
CareerPathRecommender.Application/
CareerPathRecommender.Domain/
CareerPathRecommender.Infrastructure/
CareerPathRecommender.Tests/
CareerPathRecommender.Web/
```

## Logs
Serilog writes rolling log files to `CareerPathRecommender.Web/logs/` (configured in `Program.cs`).

## Troubleshooting
- Database connection errors: verify `DefaultConnection` and SQL Server instance.
- Migrations issues: delete the DB and re-run, or run `dotnet ef database update` targeting the Infrastructure migrations assembly.
- OAuth sign-in not working: confirm Google credentials and callback URLs.

## License
This project is for demonstration/competition purposes. Add a license file if you intend to distribute.
