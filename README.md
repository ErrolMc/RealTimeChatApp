# Real-Time Chat Application

A modern, cross-platform real-time chat application built with .NET Aspire, Avalonia UI, ASP.NET Core, and Azure SignalR.

**Live Demo:** https://browser-frontend.proudforest-826495aa.australiaeast.azurecontainerapps.io/

## Features

- Real-time messaging with SignalR (MessagePack binary protocol)
- Cross-platform support (Windows, macOS, Linux, Web via WebAssembly)
- User authentication with JWT
- Friend system and group chats
- Message history and threading
- Offline-first caching with LiteDB (desktop)
- .NET Aspire orchestration for local development

## Architecture

### System Overview

The application follows a microservices architecture orchestrated by .NET Aspire, with three main components communicating via REST and WebSockets.

```
                    .NET Aspire AppHost
                   (Service Orchestration)
                            |
          ┌─────────────────┼─────────────────┐
          |                 |                  |
   ┌──────┴──────┐  ┌──────┴──────┐  ┌───────┴───────┐
   |   Backend   |  |   SignalR   |  |   Frontend    |
   |   ASP.NET   |  |   Server    |  |  Avalonia UI  |
   |   Core API  |  |   (Hub)     |  | Desktop / Web |
   └──────┬──────┘  └──────┬──────┘  └───────────────┘
          |                |
   ┌──────┴──────┐  ┌──────┴──────┐
   | Azure       |  | Azure       |
   | Cosmos DB   |  | SignalR     |
   | (Emulator)  |  | (Emulator)  |
   └─────────────┘  └─────────────┘
```

### Components

#### 1. Frontend (Avalonia UI)
Cross-platform UI targeting desktop (Windows, macOS, Linux) and web (WebAssembly).

- **MVVM architecture** using CommunityToolkit.Mvvm and ReactiveUI
- **Service layer** with dependency injection for auth, chat, friends, groups, SignalR, caching, and navigation
- **Offline-first caching** via LiteDB on desktop
- **Real-time updates** via SignalR client with MessagePack protocol

#### 2. Backend API (ASP.NET Core Web API)
REST API handling data persistence and business logic.

- User management (registration, login, profiles)
- Message and thread persistence
- Friend and group management
- JWT token generation and validation
- Azure Cosmos DB integration (users, messages, threads containers)

#### 3. SignalR Server
Dedicated real-time communication hub.

- Notification broadcasting
- JWT-authenticated WebSocket connections
- MessagePack binary serialization for performance
- Custom user ID provider for targeted messaging

#### 4. .NET Aspire AppHost
Orchestrates all services and infrastructure for local development.

- **Cosmos DB emulator** with persistent data volume
- **Azure SignalR emulator** with persistent lifetime
- **Service discovery** via environment variables (no hardcoded URLs needed)
- **nginx container** serving the WebAssembly frontend
- Dependency graph ensuring services start in the correct order

### Communication Flow

1. **Authentication:**
   ```
   Client -> Backend API -> Cosmos DB -> JWT Token -> Client
   ```

2. **Real-time Messaging:**
   ```
   Client -> SignalR Server -> Other Clients
   Client -> Backend API -> Cosmos DB (persistence)
   ```

3. **Data Operations:**
   ```
   Client -> Backend API -> Cosmos DB
   ```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Orchestration | .NET Aspire 9.5 |
| Frontend | Avalonia UI 11.3 |
| Backend API | ASP.NET Core (.NET 10) |
| Real-time | Azure SignalR + MessagePack |
| Database | Azure Cosmos DB |
| Auth | JWT Bearer tokens + BCrypt |
| Caching | LiteDB (desktop) |
| Telemetry | OpenTelemetry (OTLP) |

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for Aspire emulators)
- Visual Studio 2022+ or VS Code
- Git

### Setup

1. Clone the repository:
```bash
git clone https://github.com/ErrolMc/RealTimeChatApp.git
cd RealTimeChatApp
```

2. Run the Aspire AppHost (starts all services, emulators, and the desktop frontend):
```bash
cd ChatApp
dotnet run --project ChatApp.AppHost
```

This will automatically:
- Start the Cosmos DB emulator (persistent container with data volume)
- Start the Azure SignalR emulator
- Launch the Backend API (connected to Cosmos DB and SignalR)
- Launch the SignalR Server
- Launch the Desktop frontend
- Serve the Browser frontend via nginx on port 80

3. Open the Aspire dashboard (URL shown in terminal output) to monitor all services.

### Running Individual Projects

If you need to run services independently without Aspire:

```bash
# Backend API
cd ChatApp.Backend
dotnet run

# SignalR Server
cd SignalRServer/ChatAppSignalRServer
dotnet run

# Desktop Frontend
cd ChatAppFrontEndAvalonia/ChatAppFrontEnd.Desktop
dotnet run
```

> **Note:** When running without Aspire, services fall back to localhost URLs defined in `NetworkConstants.cs`.

### Deploying to Azure

The application can be deployed to Azure Container Apps using the Azure Developer CLI (`azd`), which provisions all infrastructure directly from the Aspire AppHost configuration.

**Prerequisites:**
- [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd) (`azd`)
- An Azure subscription
- Docker Desktop (for building container images)

**Deploy:**
```bash
cd ChatApp/ChatApp.AppHost
azd init
azd up
```

`azd up` will:
- Provision an Azure resource group with all required infrastructure
- Create an **Azure Cosmos DB** account and an **Azure SignalR Service** instance
- Deploy the **Backend API** and **SignalR Server** as Azure Container Apps
- Build the **Browser frontend** Docker image (Avalonia WASM served via nginx) and deploy it as a Container App
- Configure service discovery, environment variables, and external ingress automatically

**Post-deployment: Create Cosmos DB database and containers**

Azure Cosmos DB with managed identity (AAD) authentication does not allow creating databases or containers via data plane tokens. You need to create them manually after `azd up` provisions the Cosmos DB account:

```bash
# Find your Cosmos DB account name
az cosmosdb list --query "[].name" -o tsv

# Create the database and containers
az cosmosdb sql database create --account-name <account-name> --resource-group <resource-group> --name chatappdb

az cosmosdb sql container create --account-name <account-name> --resource-group <resource-group> \
    --database-name chatappdb --name users --partition-key-path /userid

az cosmosdb sql container create --account-name <account-name> --resource-group <resource-group> \
    --database-name chatappdb --name messages --partition-key-path /threadid

az cosmosdb sql container create --account-name <account-name> --resource-group <resource-group> \
    --database-name chatappdb --name threads --partition-key-path /id
```

**How the WASM frontend works in Azure:**

Since the Avalonia WASM app runs entirely in the browser, it can't read server-side environment variables. Instead, the Docker container's `entrypoint.sh` generates a `config.js` file at startup that intercepts `fetch()` and `WebSocket` calls, rewriting localhost URLs to the actual Azure service URLs. This allows the compiled WASM app to reach the correct backend and SignalR endpoints without any code changes.

**Tearing down:**
```bash
azd down
```

## Project Structure

```
├── ChatApp/
│   ├── ChatApp.AppHost/           # .NET Aspire orchestrator
│   └── ChatApp.ServiceDefaults/   # Shared service configuration (OpenTelemetry, resilience)
│
├── ChatApp.Backend/               # ASP.NET Core Web API
│   └── Code/
│       ├── Repositories/          # Data access (Login, Friends, Groups, Messages)
│       └── Services/              # Database and query services
│
├── ChatAppFrontEndAvalonia/       # Avalonia UI frontend
│   ├── ChatAppFrontEnd/           # Core UI library (views, view models, services)
│   ├── ChatAppFrontEnd.Desktop/   # Desktop app (Windows, macOS, Linux)
│   └── ChatAppFrontEnd.Browser/   # WebAssembly app
│
├── ChatAppShared/                 # Shared models, constants, and DTOs
│
└── SignalRServer/
    └── ChatAppSignalRServer/      # SignalR notification hub
```

## Key Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Frontend | Avalonia UI | True cross-platform (desktop + WASM) with native performance and MVVM support |
| Backend | ASP.NET Core Web API | Full-featured, high-performance web framework with built-in DI and middleware |
| Real-time | Azure SignalR | Managed WebSocket service with automatic scaling and fallback |
| Database | Azure Cosmos DB | Globally distributed NoSQL with automatic scaling and low latency |
| Orchestration | .NET Aspire | Simplified local dev with emulators, service discovery, and observability |
| Serialization | MessagePack | Binary protocol for efficient SignalR communication |
| Local caching | LiteDB | Lightweight embedded NoSQL for offline-first desktop experience |
