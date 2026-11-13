RaceDay – Event Management System
Project overview
RaceDay is an ASP.NET Core solution made up of an API project, an MVC web application, and an automated test project. The API is responsible for application data, authentication, and communication with Azure Blob Storage. The MVC application communicates with the API over HTTP; it must not connect to Azure Blob Storage directly or contain Azure storage credentials.
Projects
Project	Purpose
RaceDay.API	REST API, SQL Server/Entity Framework Core data access, cookie authentication, role authorisation, and Azure Blob Storage integration.
RaceDay.Mvc	ASP.NET Core MVC front end. It uses an HttpClient to call the API.
RaceDay.Tests	Automated tests for the application.


Technologies
- C# and ASP.NET Core MVC / Web API
- .NET 10 (the project files target net10.0)
- SQL Server and Entity Framework Core
- Azure Blob Storage (Azure.Storage.Blobs)
- Swagger / OpenAPI
- xUnit test project
Prerequisites
Install the following before running the project:
1. Visual Studio 2022 with the ASP.NET and web development workload, with a .NET 10 SDK available.
2. SQL Server LocalDB (or a SQL Server instance you configure).
3. Azure Storage Account access if you want to test event banner uploads.
4. Internet access to restore NuGet packages.
Check that your installed Visual Studio version supports the .NET 10 SDK and net10.0 projects. If it does not, install a compatible Visual Studio update/SDK before building.

Open the project
1. Extract the ZIP file to a working folder.
2. Open the RaceDay.API folder.
3. Open the API and MVC .csproj files in Visual Studio, or create/open a solution and add these projects:
   - RaceDay.API/RaceDay.API.csproj
   - RaceDay.Mvc/RaceDay.Mvc.csproj
   - RaceDay.Tests/RaceDay.Tests.csproj
4. Restore NuGet packages and build the projects.
Database configuration
The API currently uses this development connection string in RaceDay.API/appsettings.json:
"ConnectionStrings": {
  "RaceDayConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDev;Trusted_Connection=True;TrustServerCertificate=True;"
}
Change the connection string if you use a different SQL Server instance. Keep production credentials and secrets out of source control.

Apply the Entity Framework Core migrations from the API project directory:
dotnet tool install --global dotnet-ef
dotnet ef database update
If dotnet-ef is already installed, the first command is not required. In Visual Studio's Package Manager Console, select RaceDay.API as the Default project and run:
Update-Database
The API initialises the Organiser and Participant roles when it starts outside the Testing environment.
Azure Blob Storage – event banners
Architecture
The MVC application sends the selected image to the RaceDay API. The API validates the image, uploads it to the Azure Storage container, and saves the returned BannerImageUrl on the event record. The event detail API response includes that URL so the front end can display the banner.

Do not add Azure connection strings or Azure Storage SDK calls to the MVC project. All file operations must go through the API.
Configure Azure credentials
1. Create an Azure Storage Account in the Azure portal.
2. Copy its connection string from the storage account's Access keys page.
3. Open a terminal in the RaceDay.API project directory and run:
dotnet user-secrets init
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "YOUR_AZURE_STORAGE_CONNECTION_STRING"
dotnet user-secrets set "AzureBlobStorage:ContainerName" "event-banners"
Replace the placeholder with your real connection string. Do not commit it to Git or paste it into this README. The blank connection string in appsettings.json is intentional.

For deployment, configure the secret in the hosting platform using the environment variable:
AzureBlobStorage__ConnectionString
The configured container name is event-banners. The API creates the container when an image is first uploaded. The current implementation is designed to use publicly readable blob URLs for displaying event banners. If your storage account policy blocks anonymous blob access, configure private access and implement short-lived, read-only SAS URLs instead.
Banner API endpoints
HTTP method	Endpoint	Purpose
POST	/api/events	Create an event using the event DTO.
POST	/api/events/{id}/banner	Upload an event banner as multipart/form-data, using the form field name file.
PUT	/api/events/{id}	Update event details.
GET	/api/events/{id}	Retrieve event details, including BannerImageUrl.


Banner uploads require the existing Organiser authentication and event ownership checks. The API accepts JPEG, PNG, WEBP, and GIF images up to 5 MB.
MVC upload sequence
For a new event:
1. The MVC application posts the event details to POST /api/events.
2. It reads the new event ID from the API response.
3. If a banner was selected, it posts the image to POST /api/events/{id}/banner as multipart form data. The form field must be named file.
For an existing event:
1. The MVC application updates event details through PUT /api/events/{id}.
2. If the organiser selected a replacement banner, it posts that image to POST /api/events/{id}/banner.
The event detail page should display the BannerImageUrl returned by the API, for example:
@if (!string.IsNullOrWhiteSpace(Model.BannerImageUrl))
{
    <img src="@Model.BannerImageUrl" alt="@Model.EventName banner" />
}
The MVC project's API base address is currently configured in RaceDay.Mvc/Program.cs as https://localhost:44332/. Update this URL to match the API's actual launch URL in your local environment. Ensure the API and MVC applications are both running when testing the complete website workflow.
Run the applications
1. Configure the SQL Server connection string.
2. Apply the database migrations.
3. Configure Azure Blob Storage secrets if testing banner uploads.
4. Set RaceDay.API as a startup project and run it. In Development, Swagger is enabled for API testing.
5. Confirm the API URL and update the RaceDayApi HttpClient base address in RaceDay.Mvc/Program.cs if necessary.
6. Run RaceDay.Mvc and use the website to access the API-backed features.
Run the API and MVC on HTTPS during local development so cookie authentication behaves as expected.
Run automated tests
From the extracted project root, run:
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
Or use Test > Run All Tests in Visual Studio's Test Explorer.
Security and configuration notes
- Never commit Azure Storage connection strings, passwords, or production secrets.
- Use User Secrets for local development and the hosting provider's secret store for deployment.
- Keep Azure Blob Storage access inside the API; the MVC project should only call API endpoints.
- Use HTTPS for deployed applications.
- Review the storage account's public-access policy. Public blob access may be restricted by organisational or cloud security policies.
- Change the development database connection string and API base URL to match your own environment.
Troubleshooting
Problem	What to check
SQL Server connection fails	Confirm LocalDB/SQL Server is installed and the connection string points to the correct instance.
Database tables are missing	Run dotnet ef database update from the API project directory.
Banner upload fails	Check the Azure connection string, container configuration, Organiser login, event ownership, image type, and 5 MB limit.
MVC cannot reach the API	Check that the API is running and that the RaceDayApi base address matches its HTTPS URL.
Swagger is unavailable	Run the API using the Development environment.
NuGet restore/build fails	Confirm the .NET 10 SDK is installed and restore packages again.


Additional Azure setup guide
See [`RaceDay.API/AZURE-BLOB-STORAGE-SETUP.md`](RaceDay.API/AZURE-BLOB-STORAGE-SETUP.md) for the focused Azure Blob Storage configuration notes and API upload example.
<img width="1024" height="576" alt="image" src="https://github.com/user-attachments/assets/dd011b6b-d953-46bf-8279-b29e14c340db" />
