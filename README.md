# 🏥 OCR Medical Document API

.NET 8 Web API for uploading, OCR-processing, and managing medical documents.  
Uses **Azure Computer Vision** for OCR and **Azure Text Analytics** to extract structured medical data.

## Architecture

```
OCR/                    # Host — ASP.NET Core Web API
OCR.Application/        # CQRS commands & queries (MediatR)
OCR.Domain/             # Entities, enums, value objects
OCR.Infrastructure/     # EF Core, Azure services, repositories
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server / LocalDB
- Azure Computer Vision resource
- Azure Blob Storage account

## Configuration

Fill in `OCR/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OCRAuthConnectionString": "Server=(localdb)\\mssqllocaldb;Database=OCRAuthDb;Trusted_Connection=True;",
    "OCRConnectionString": "Server=(localdb)\\mssqllocaldb;Database=OCRDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "<secret-key-min-32-chars>",
    "Issuer": "https://localhost:7241",
    "Audience": "https://localhost:7241"
  },
  "AzureComputerVision": {
    "Endpoint": "https://<resource>.cognitiveservices.azure.com/",
    "Key": "<computer-vision-key>"
  },
  "AzureBlobStorage": {
    "ConnectionString": "<blob-storage-connection-string>",
    "ContainerName": "documents"
  },
  "AllowedOrigins": [ "http://localhost:3000" ]
}
```

## Getting Started

```bash
# 1. Restore packages
dotnet restore

# 2. Apply migrations
dotnet ef database update --project OCR.Infrastructure --startup-project OCR --context OCRAuthDbContext
dotnet ef database update --project OCR.Infrastructure --startup-project OCR --context OCRDbContext

# 3. Run
dotnet run --project OCR
```

Swagger UI: **`https://localhost:7241/swagger`**

## API Overview

All endpoints except `/api/Auth/*` require `Authorization: Bearer <token>`.

### 🔐 Auth — `/api/Auth`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/Register` | Register — returns JWT |
| POST | `/Login` | Login — returns JWT |

**Body:** `{ "username": "...", "password": "..." }`

---

### 📄 OCR — `/api/Ocr`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/UploadAndRecognize` | Upload document image, run OCR & extract medical data |
| POST | `/SaveMedicalRecord` | Save extracted record to a patient (new or existing) |
| GET | `/GetRecognizeResultById/{id}` | Fetch a previous OCR result |

**`UploadAndRecognize`** — `multipart/form-data`: `File`, `FileName`, `FileDescription?`  
Returns recognized data + similar existing patients. If `requiresConfirmation: true`, call `SaveMedicalRecord` to confirm.

**`SaveMedicalRecord`** — body:
```json
{
  "existingPatientId": null,
  "firstName": "Jane", "lastName": "Smith", "birthDate": "1990-03-22",
  "recognizedId": "<guid>",
  "recognizedData": { ... }
}
```

---

### 📁 Document — `/api/Document`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/{id}/file` | Download original document file |
| DELETE | `/{id}` | Delete document |

---

### 🧑‍⚕️ Patient — `/api/Patient`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/search` | Search by name / birth date (paginated) |
| GET | `/{id}` | Get patient by ID |
| GET | `/{id}/history` | Get patient's full medical record history |

**Search body:** `{ "firstName": "...", "lastName": "...", "birthDate": "...", "page": 1, "pageSize": 10 }`

## Typical Flow

```
Register / Login → get JWT
→ POST /api/Ocr/UploadAndRecognize   (upload document)
→ POST /api/Ocr/SaveMedicalRecord    (confirm patient, if needed)
→ GET  /api/Patient/{id}/history     (view records)
→ GET  /api/Document/{id}/file       (download file)
```

## Tech Stack

.NET 8 · MediatR · EF Core 8 · ASP.NET Identity · JWT · Azure Computer Vision · Azure Text Analytics · Azure Blob Storage · Serilog · Swagger
