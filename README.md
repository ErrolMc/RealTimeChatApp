# Real-Time Chat Application

A modern, cross-platform real-time chat application built with Avalonia UI, Azure Functions, and SignalR.

## Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Documentation](docs/api/API.md)
- [Development Guide](docs/guides/DEVELOPMENT.md)
- [Architecture Decisions](docs/adr/001-architecture-choice.md)

## Features

- Real-time messaging
- Cross-platform support (Windows, macOS, Linux, iOS, Android, Web)
- User authentication and authorization
- Message history
- Multiple chat room support
- Presence indicators
- Message persistence

## Quick Start

### Prerequisites

- .NET 6.0 SDK or later
- Azure subscription
- Azure Cosmos DB instance
- Azure SignalR Service

### Setup

1. Clone the repository:
```bash
git clone https://github.com/ErrolMc/RealTimeChatApp.git
cd RealTimeChatApp
```

2. Follow the [Development Guide](docs/guides/DEVELOPMENT.md) for detailed setup instructions.

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

## License

This project is licensed under the MIT License - see the LICENSE file for details.
