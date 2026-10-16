RaceDay API
Overview
RaceDay API is an ASP.NET Core Web API project for managing race events and related data. It uses C#, Entity Framework Core, and Microsoft SQL Server. The API includes registration and login for Organiser and Participant roles, session-based authentication, and role-aware access control.
Technology Stack
- ASP.NET Core Web API (net10.0)
- C#
- Entity Framework Core
- Microsoft SQL Server / LocalDB
- Cookie authentication and server-side sessions
- Password hashing with ASP.NET Core Identity
- Swagger / OpenAPI for API exploration
- xUnit-based test project (see RaceDay.Tests)
Project Structure
RaceDay.API/
├── RaceDay.API/
│   ├── Controllers/       # API endpoints
│   ├── Data/              # EF Core database context
│   ├── DTOs/              # Request/response data-transfer objects
│   ├── Models/            # Entity models
│   ├── Migrations/        # EF Core migrations
│   ├── Authentication/    # Session and antiforgery middleware
│   ├── Program.cs         # Service and middleware configuration
│   └── appsettings.json   # Configuration, including connection string
└── RaceDay.Tests/         # Automated tests
Requirements
1. Visual Studio 2022 or a compatible .NET IDE.
2. The .NET SDK required by the project target (net10.0).
3. Microsoft SQL Server or SQL Server LocalDB.
4. Entity Framework Core command-line tools or the Package Manager Console in Visual Studio.
Note: The project targets .NET 10. Ensure the .NET 10 SDK is installed and supported by your Visual Studio version.

Database Setup
The default connection string in RaceDay.API/appsettings.json uses SQL Server LocalDB:
"RaceDayConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDev;Trusted_Connection=True;TrustServerCertificate=True;"
If you use a different SQL Server instance, update ConnectionStrings:RaceDayConnection in appsettings.json or your local development configuration.
From Visual Studio, open the solution and set RaceDay.API as the startup project. Open Tools → NuGet Package Manager → Package Manager Console, select RaceDay.API as the default project, then run:
Update-Database
The repository includes an initial EF Core migration. If the database has not been created, Update-Database applies the existing migration and creates the schema. On startup, the application seeds the Organiser and Participant roles if they do not already exist.
If EF tooling reports that no migration can be found, check the selected default project and startup project before creating a new migration.
Run the API
1. Open RaceDay.API.slnx in Visual Studio.
2. Confirm the database connection string is correct.
3. Apply the database migration using the instructions above.
4. Set RaceDay.API as the startup project.
5. Run using HTTPS (F5 or Ctrl+F5).
In Development, Swagger UI is enabled for exploring and trying endpoints. The application redirects HTTP to HTTPS and configures secure cookies, so use the HTTPS URL.
Alternatively, from a terminal in the solution folder:
dotnet restore
dotnet build
dotnet run --project RaceDay.API/RaceDay.API.csproj
Authentication
The authentication controller is routed under /api/auth.
Method	Endpoint	Purpose
POST	/api/auth/register	Register an Organiser or Participant
POST	/api/auth/login	Validate credentials and sign in
POST	/api/auth/logout	Sign out and clear the session


Registration accepts a role of Organiser or Participant. Passwords are hashed before storage. Login establishes a session and authentication cookie. Protected endpoints require authentication, and role-restricted endpoints may require the appropriate role.
Use the DTOs in RaceDay.API/DTOs/ to confirm the exact request fields and validation rules for each endpoint. Since authentication uses cookies, API clients such as Swagger or Postman must retain and send the authentication cookie for subsequent protected requests.
Example registration request
Adjust the fields to match RegisterDto:
{
  "firstName": "Alex",
  "lastName": "Example",
  "email": "alex@example.com",
  "password": "Use-A-Strong-Password",
  "role": "Participant"
}
Example login request
{
  "email": "alex@example.com",
  "password": "Use-A-Strong-Password"
}
Do not use real passwords in examples, source control, or test data.
API Controllers
Controllers currently included in the project:
- AuthController — registration, login, and logout.
- CategoriesController — category endpoints.
- EnrolmentsController — enrolment endpoints.
- EventsController — race event endpoints.
- ProfileController — profile-related endpoints.
- ResultsController — race result endpoints.
- SecurityController — security / antiforgery-related endpoint(s).
- WeatherForecastController — ASP.NET Core template sample endpoint.
Check each controller's attributes and DTOs for the precise route paths, HTTP methods, and role requirements.
Run Tests
The automated test project is RaceDay.Tests.
From the solution folder, run:
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
Or in Visual Studio, right-click the test project and select Run Tests / use Test Explorer.
Test files include authentication integration tests, database isolation tests, and registration validation tests. A successful build does not itself guarantee every test passes; review the test output for failures.
Security Notes
- Use HTTPS when testing authentication.
- Keep connection strings and credentials appropriate for your environment; do not commit production secrets.
- The configured cookies are HTTP-only and use secure settings. For local HTTP-only development, adjust settings carefully and do not carry insecure settings into production.
- Cookie-authenticated state-changing requests may require the configured antiforgery token/header (X-CSRF-TOKEN). Follow the security endpoint/controller implementation to obtain and send the token.
- Do not expose this development database or test credentials publicly.
Troubleshooting
- Cannot connect to SQL Server: Verify SQL Server LocalDB/SQL Server is installed and running, and confirm the connection string.
- Database tables are missing: Run Update-Database from the correct startup/default project.
- 401 Unauthorized: Log in and ensure the client sends the authentication cookie.
- 403 Forbidden: The authenticated account may not have the role required by the endpoint.
- Antiforgery error: Obtain and send the token as required by the security endpoint and the X-CSRF-TOKEN header.
- Swagger is unavailable: It is enabled only in the Development environment; run with the Development environment selected.
License
No license information is specified in this repository. Add a license file if you intend to distribute the project publicly.

The RaceDay solution uses the ERD as the database blueprint, the
endpoint plan as the API blueprint, and the SQL script as the database
implementation blueprint. Part 2 should implement these approved designs
without unexplained deviations.
<img width="1024" height="576" alt="image" src="https://github.com/user-attachments/assets/1289131c-a866-45eb-90dc-0c9e2fbb725f" />
