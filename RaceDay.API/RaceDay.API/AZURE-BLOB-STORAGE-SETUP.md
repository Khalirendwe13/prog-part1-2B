# RaceDay API – Azure Blob Storage event banners

## Architecture
The MVC application must never connect to Azure Storage. It sends event data and image files to the RaceDay REST API. The API validates the image, uploads it to Azure Blob Storage, and stores the resulting `BannerImageUrl` on the event record. Event detail responses return that URL for the MVC page to display.

## 1. Create the Azure storage container
1. Create an Azure Storage Account.
2. Copy its connection string from **Security + networking → Access keys**.
3. The API creates the `event-banners` container on the first upload.
4. The implementation enables blob-level public read access so event banner URLs can be displayed without exposing the storage account key. If the storage account disallows anonymous blob access, configure a private container and replace public URLs with short-lived read-only SAS URLs.

## 2. Configure the API secret
Do not commit the Azure connection string to Git. From the API project directory run:

```powershell
dotnet user-secrets init
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "YOUR_AZURE_STORAGE_CONNECTION_STRING"
dotnet user-secrets set "AzureBlobStorage:ContainerName" "event-banners"
```

For deployment, set the environment variable `AzureBlobStorage__ConnectionString` in the hosting platform's secret/configuration settings. The blank value in `appsettings.json` is intentional.

## 3. Apply the database migration
Run from the solution/project directory in Package Manager Console:

```powershell
Update-Database
```

Or with the .NET CLI:

```bash
dotnet ef database update
```

This adds nullable `BannerImageUrl` to the `Events` table.

## 4. API usage
All endpoints require the existing Organiser cookie authentication and event ownership check for uploads.

- `POST /api/events` – create the event using the existing JSON `EventDto`.
- `POST /api/events/{id}/banner` – upload the image using `multipart/form-data` with the form field name `file`. Maximum 5 MB; JPEG, PNG, WEBP and GIF accepted.
- `PUT /api/events/{id}` – update event details using the existing JSON `EventDto`.
- `GET /api/events/{id}` – returns `BannerImageUrl` for the event detail page.

For the MVC create flow: create the event through `POST /api/events`, read the returned `eventID`, then send the selected image to `POST /api/events/{eventID}/banner`. For editing, update the event and call the banner endpoint only when a replacement image was selected. Both requests go to the API; the MVC project must not reference `Azure.Storage.Blobs` or contain Azure credentials.

## 5. MVC upload example
```csharp
using var content = new MultipartFormDataContent();
await using var imageStream = banner.OpenReadStream();
using var imageContent = new StreamContent(imageStream);
imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(banner.ContentType);
content.Add(imageContent, "file", banner.FileName);
var response = await httpClient.PostAsync($"api/events/{eventId}/banner", content);
response.EnsureSuccessStatusCode();
```

The MVC event detail Razor view can display the URL returned by the API:

```html
<img src="@Model.BannerImageUrl" alt="@Model.EventName banner" />
```

## Important
The API project was updated in this archive. The MVC front-end project was not included in the uploaded ZIP, so its create/edit forms and detail view still need to call these API endpoints and render `BannerImageUrl`.
