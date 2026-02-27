# spec-pdf-downloader

MyApp
 ├── Api              → Controllers, JWT, Middleware
 ├── Application      → Use cases, interfaces
 ├── Domain           → Entities, business rules
 └── Infrastructure   → EF Core, DB, external services

 pdf-downloader
│
├── pdf-downloader.Api
│   ├── Controllers
│   │   └── AuthController.cs
│   ├── Extensions
│   │   └── ServiceCollectionExtensions.cs
│   ├── Middlewares
│   ├── appsettings.json
│   └── Program.cs
│
├── pdf-downloader.Application
│   ├── Interfaces
│   │   ├── IJwtTokenGenerator.cs
│   │   └── IUserRepository.cs
│   ├── DTOs
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   └── Services
│       └── AuthService.cs
│
├── pdf-downloader.Domain
│   └── Entities
│       └── User.cs
│
└── pdf-downloader.Infrastructure
    ├── Persistence
    │   ├── AppDbContext.cs
    │   └── UserRepository.cs
    ├── Security
    │   └── JwtTokenGenerator.cs
    └── DependencyInjection.cs

pdf-downloader
│
├── Api
│   ├── Controllers
│   │   ├── AuthController.cs
│   │   ├── ExcelController.cs
│   │   └── ReportsController.cs
│   └── Background
│       └── PdfDownloadWorker.cs
│
├── Application
│   ├── Interfaces
│   │   ├── IPdfDownloader.cs
│   │   ├── IExcelParser.cs
│   │   └── IReportService.cs
│   ├── DTOs
│   └── Services
│
├── Domain
│   ├── Entities
│   │   ├── User.cs
│   │   ├── ExcelUpload.cs
│   │   └── PdfDownload.cs
│
└── Infrastructure
    ├── Persistence
    │   └── AppDbContext.cs
    ├── Services
    │   ├── ExcelParser.cs
    │   └── PdfDownloader.cs