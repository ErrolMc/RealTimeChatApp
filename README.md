# Real-Time Chat Application

A modern, cross-platform real-time chat application built with Avalonia UI, Azure Functions, and SignalR.

## Architecture

The application is built using a microservices architecture with the following components:

### Frontend (ChatAppFrontEndAvalonia)
- Built with Avalonia UI for cross-platform support
- Supports multiple platforms:
  - Desktop (Windows, macOS, Linux)
  - Mobile (iOS, Android)
  - Browser (WebAssembly)
- Provides a modern, responsive user interface

### Backend Services

#### Azure Functions (ChatAppDatabaseFunctions)
- Handles data persistence and business logic
- Integrates with Azure Cosmos DB for data storage
- RESTful API endpoints for:
  - User management
  - Message handling
  - Chat room operations

#### SignalR Server
- Enables real-time communication
- Manages WebSocket connections
- Handles message broadcasting
- Provides presence awareness

### Data Storage
- Azure Cosmos DB
- Scalable NoSQL database
- Stores:
  - User profiles
  - Chat messages
  - Chat room information

## Features

- Real-time messaging
- Cross-platform support
- User authentication and authorization
- Message history
- Multiple chat room support
- Presence indicators
- Message persistence

## Prerequisites

- .NET 6.0 SDK or later
- Azure subscription
- Azure Cosmos DB instance
- Azure SignalR Service

## Setup Instructions

1. Clone the repository:
```bash
git clone https://github.com/ErrolMc/RealTimeChatApp.git
```

2. Configure Azure Services:
   - Create an Azure Cosmos DB instance
   - Set up Azure Functions
   - Configure Azure SignalR Service

3. Configure connection strings:
   - Update the connection strings in the Azure Functions configuration
   - Update the SignalR connection settings
   - Configure the frontend application settings

4. Running the Frontend:
```bash
cd ChatAppFrontEndAvalonia/ChatAppFrontEnd.Desktop
dotnet run
```

5. Running the Backend:
```bash
cd ChatAppDatabaseFunctions
func start
```

6. Running the SignalR Server:
```bash
cd SignalRServer
dotnet run
```

## Project Structure

```
├── ChatAppFrontEndAvalonia/    # Frontend application
│   ├── ChatAppFrontEnd/        # Core UI components
│   ├── ChatAppFrontEnd.Desktop # Desktop-specific code
│   ├── ChatAppFrontEnd.Android # Android-specific code
│   ├── ChatAppFrontEnd.iOS     # iOS-specific code
│   └── ChatAppFrontEnd.Browser # Browser-specific code
│
├── ChatAppDatabaseFunctions/   # Azure Functions backend
│   └── Code/                  # Backend business logic
│
├── ChatAppShared/             # Shared models and utilities
│
└── SignalRServer/             # Real-time communication server
```

## Development

### Building the Frontend
```bash
cd ChatAppFrontEndAvalonia
dotnet build
```

### Building the Backend
```bash
cd ChatAppDatabaseFunctions
dotnet build
```

## Deployment

### Frontend
- Desktop: Build and distribute the executable
- Mobile: Deploy through respective app stores
- Web: Deploy to a web server

### Backend
1. Deploy Azure Functions using Azure CLI:
```bash
func azure functionapp publish <FunctionAppName>
```

2. Deploy SignalR Server to Azure App Service
