# Real-Time Chat Application

A modern, cross-platform real-time chat application built with Avalonia UI, Azure Functions, and SignalR.

## Table of Contents
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Development Guide](#development-guide)
- [API Documentation](#api-documentation)
- [Architecture Decisions](#architecture-decisions)
- [Project Structure](#project-structure)

## Features

- Real-time messaging
- Cross-platform support (Windows, macOS, Linux, iOS, Android, Web)
- User authentication and authorization
- Message history
- Multiple chat room support
- Presence indicators
- Message persistence

## Architecture

### System Components

#### 1. Frontend (ChatAppFrontEndAvalonia)
The frontend is built using Avalonia UI, providing a cross-platform user interface that works seamlessly across desktop, mobile, and web platforms.

**Key Components:**
- **UI Layer**: MVVM architecture with views and view models
- **Service Layer**: Handles communication with backend services
- **State Management**: Manages application state and real-time updates
- **Authentication**: Handles user authentication and session management

#### 2. Backend Services

##### 2.1 Azure Functions (ChatAppDatabaseFunctions)
Serverless functions handling data persistence and business logic.

**Key Responsibilities:**
- User management (registration, profile updates)
- Message persistence
- Chat room management
- Data validation and business rules

##### 2.2 SignalR Server
Real-time communication hub managing WebSocket connections.

**Key Responsibilities:**
- Message broadcasting
- Presence management
- Connection state handling
- Real-time notifications

#### 3. Data Layer (Azure Cosmos DB)

**Data Models:**
- Users
- Messages
- ChatRooms
- UserConnections

**Key Features:**
- Global distribution
- Automatic scaling
- Multi-region writes
- Low latency access

### Communication Flow

1. **User Authentication:**
   ```
   Client -> Azure Functions -> Azure AD B2C -> Client
   ```

2. **Real-time Messaging:**
   ```
   Client -> SignalR Server -> Other Clients
           -> Azure Functions -> Cosmos DB (persistence)
   ```

3. **Data Operations:**
   ```
   Client -> Azure Functions -> Cosmos DB
   ```

### Security

#### Authentication
- Azure AD B2C integration
- JWT token-based authentication
- Secure token storage

#### Authorization
- Role-based access control
- Chat room-specific permissions
- Message encryption

### Scalability

#### Frontend
- Stateless design
- Caching strategies
- Offline support

#### Backend
- Auto-scaling Azure Functions
- SignalR Service scaling
- Cosmos DB throughput scaling

### Monitoring and Logging

#### Application Insights
- Performance monitoring
- Error tracking
- Usage analytics

#### Custom Logging
- User actions
- System events
- Error handling

## Getting Started

### Prerequisites

#### Required Software
- Visual Studio 2022 or later (with .NET desktop development workload)
- .NET 6.0 SDK or later
- Azure Functions Core Tools
- Azure CLI
- Git

#### Azure Resources
- Azure subscription
- Azure Cosmos DB account
- Azure SignalR Service
- Azure Functions App

### Setup

1. Clone the repository:
```bash
git clone https://github.com/ErrolMc/RealTimeChatApp.git
cd RealTimeChatApp
```

2. Configure Azure Resources:
   - Create a local.settings.json in the ChatAppDatabaseFunctions directory:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet",
       "CosmosDBConnection": "your_cosmos_db_connection_string",
       "SignalRConnection": "your_signalr_connection_string"
     }
   }
   ```

3. Set up Azure Cosmos DB Emulator (for local development)
   - Download and install the emulator
   - Update connection strings accordingly

4. Configure Frontend:
   - Navigate to ChatAppFrontEndAvalonia
   - Create appsettings.Development.json:
   ```json
   {
     "ApiBaseUrl": "http://localhost:7071",
     "SignalR": {
       "Endpoint": "http://localhost:5000/chat"
     }
   }
   ```

## Development Guide

### Running the Application

#### Start Backend Services
1. Run Azure Functions:
```bash
cd ChatAppDatabaseFunctions
func start
```

2. Run SignalR Server:
```bash
cd SignalRServer
dotnet run
```

#### Start Frontend
```bash
cd ChatAppFrontEndAvalonia/ChatAppFrontEnd.Desktop
dotnet run
```

### Development Workflow

#### 1. Branch Management
- `main`: Production-ready code
- `develop`: Integration branch
- Feature branches: `feature/feature-name`
- Bug fixes: `fix/bug-name`

#### 2. Code Style
- Follow C# coding conventions
- Use meaningful variable and function names
- Add XML documentation comments for public APIs
- Keep methods focused and small

#### 3. Testing
- Write unit tests for business logic
- Integration tests for API endpoints
- UI tests for critical user flows

#### 4. Debugging

##### Backend Debugging
1. Set breakpoints in Visual Studio
2. Attach debugger to Azure Functions host
3. Use Application Insights for monitoring

##### Frontend Debugging
1. Use Avalonia Preview Window
2. Debug XAML layout issues
3. Monitor SignalR connections

### Common Tasks

#### Adding a New Feature
1. Create feature branch
2. Implement backend endpoints
3. Add frontend components
4. Write tests
5. Create pull request

#### Database Changes
1. Update data models
2. Modify Azure Functions
3. Update frontend services
4. Test data migration

#### UI Changes
1. Design in XAML
2. Implement view models
3. Add localization
4. Test responsiveness

### Deployment

#### 1. Staging Deployment
```bash
# Deploy Azure Functions
func azure functionapp publish YourFunctionApp-Staging

# Deploy SignalR Server
dotnet publish SignalRServer -c Release
# Deploy to Azure App Service

# Build Frontend
dotnet publish -c Release
```

#### 2. Production Deployment
- Use Azure DevOps or GitHub Actions
- Follow the CI/CD pipeline
- Run automated tests
- Monitor deployment health

### Troubleshooting

#### Common Issues

1. SignalR Connection Issues
   - Check connection strings
   - Verify CORS settings
   - Check client connection status

2. Database Connection Problems
   - Verify connection strings
   - Check firewall rules
   - Monitor Cosmos DB metrics

3. Build Errors
   - Clean solution
   - Restore NuGet packages
   - Check SDK versions

#### Logging

##### Enable Debug Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

##### View Logs
- Check Application Insights
- Review Azure Function logs
- Monitor SignalR diagnostic logs

### Performance Optimization

#### Frontend
- Implement caching
- Optimize XAML rendering
- Lazy load components

#### Backend
- Use async/await properly
- Optimize database queries
- Configure auto-scaling

### Security Best Practices

1. Authentication
   - Use secure token storage
   - Implement refresh tokens
   - Enable MFA where possible

2. Data Protection
   - Encrypt sensitive data
   - Use HTTPS everywhere
   - Implement rate limiting

3. Code Security
   - Regular dependency updates
   - Security scanning
   - Code reviews

## API Documentation

### Base URLs
- **Azure Functions API**: `https://{your-function-app}.azurewebsites.net/api`
- **SignalR Hub**: `https://{your-signalr-service}.service.signalr.net`

### Authentication
All API endpoints require authentication using JWT tokens. Include the token in the Authorization header:
```
Authorization: Bearer {your-jwt-token}
```

### REST API Endpoints

#### User Management

##### Register User
```http
POST /users/register
Content-Type: application/json

{
    "username": "string",
    "email": "string",
    "password": "string"
}
```

##### Login
```http
POST /users/login
Content-Type: application/json

{
    "email": "string",
    "password": "string"
}
```

##### Get User Profile
```http
GET /users/{userId}
```

##### Update User Profile
```http
PUT /users/{userId}
Content-Type: application/json

{
    "username": "string",
    "avatar": "string",
    "status": "string"
}
```

#### Chat Rooms

##### Create Chat Room
```http
POST /chatrooms
Content-Type: application/json

{
    "name": "string",
    "description": "string",
    "isPrivate": boolean
}
```

##### Get Chat Rooms
```http
GET /chatrooms
```

##### Get Chat Room Details
```http
GET /chatrooms/{roomId}
```

##### Join Chat Room
```http
POST /chatrooms/{roomId}/join
```

##### Leave Chat Room
```http
POST /chatrooms/{roomId}/leave
```

#### Messages

##### Send Message
```http
POST /messages
Content-Type: application/json

{
    "roomId": "string",
    "content": "string",
    "type": "text|image|file"
}
```

##### Get Messages
```http
GET /messages/{roomId}?before={timestamp}&limit={number}
```

##### Delete Message
```http
DELETE /messages/{messageId}
```

### SignalR Hub Methods

#### Client Methods (to call from client)

##### Connect to Chat Room
```csharp
await hubConnection.InvokeAsync("JoinRoom", roomId);
```

##### Send Message
```csharp
await hubConnection.InvokeAsync("SendMessage", new
{
    RoomId = roomId,
    Content = content,
    Type = type
});
```

##### Update Presence
```csharp
await hubConnection.InvokeAsync("UpdatePresence", status);
```

#### Server Methods (to handle in client)

##### Receive Message
```csharp
hubConnection.On<ChatMessage>("ReceiveMessage", (message) =>
{
    // Handle new message
});

public class ChatMessage
{
    public string Id { get; set; }
    public string RoomId { get; set; }
    public string SenderId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
```

##### User Presence Update
```csharp
hubConnection.On<PresenceUpdate>("UserPresenceChanged", (update) =>
{
    // Handle presence update
});

public class PresenceUpdate
{
    public string UserId { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen { get; set; }
}
```

##### Room Update
```csharp
hubConnection.On<ChatRoom>("RoomUpdated", (room) =>
{
    // Handle room update
});

public class ChatRoom
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> Participants { get; set; }
}
```

### Response Formats

#### Success Response
```json
{
    "success": true,
    "data": {
        // Response data
    }
}
```

#### Error Response
```json
{
    "success": false,
    "error": {
        "code": "string",
        "message": "string"
    }
}
```

### Rate Limiting
- API requests are limited to 100 requests per minute per user
- WebSocket connections are limited to 5 concurrent connections per user

### Error Codes
- `401`: Unauthorized
- `403`: Forbidden
- `404`: Resource not found
- `429`: Too many requests
- `500`: Internal server error

## Architecture Decisions

### Context
We needed to design a real-time chat application that would:
- Support multiple platforms (desktop, mobile, web)
- Handle real-time messaging efficiently
- Scale well with increasing users
- Maintain message persistence
- Be cost-effective to operate

### Decisions

#### 1. Frontend Framework: Avalonia UI
We chose Avalonia UI for the frontend because:
- True cross-platform support (Windows, macOS, Linux, iOS, Android, WebAssembly)
- Native performance on all platforms
- Modern MVVM architecture support
- Rich UI component library
- Active community and good documentation

#### 2. Backend Architecture: Microservices with Azure
We decided to split the backend into two main components:

##### a. Azure Functions
- Serverless architecture for cost optimization
- Automatic scaling
- Pay-per-use pricing model
- Easy integration with other Azure services
- Handles data persistence and business logic

##### b. SignalR Service
- Managed service for real-time communication
- WebSocket support with automatic fallback
- Built-in scaling and load balancing
- Simplified client-server communication

#### 3. Database: Azure Cosmos DB
Selected for:
- Global distribution capabilities
- Schema-less design for flexibility
- Automatic scaling
- Multi-region writes
- Low latency access
- Built-in backup and restore

### Consequences

#### Positive
1. Development Efficiency
   - Shared code between platforms
   - Rapid development with Azure services
   - Simplified deployment process

2. Performance
   - Low latency real-time communication
   - Efficient data access patterns
   - Good user experience across platforms

3. Scalability
   - Automatic scaling at all layers
   - Pay-as-you-go cost model
   - Easy to add new features

#### Negative
1. Azure Lock-in
   - Dependent on Azure services
   - Migration to other clouds would be complex

2. Cost Management
   - Need to monitor Azure consumption
   - Cosmos DB can be expensive at scale

3. Complexity
   - Multiple services to manage
   - More complex deployment pipeline
   - Learning curve for team members

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
