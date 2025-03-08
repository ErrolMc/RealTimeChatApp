# Development Guide

## Prerequisites

### Required Software
- Visual Studio 2022 or later (with .NET desktop development workload)
- .NET 6.0 SDK or later
- Azure Functions Core Tools
- Azure CLI
- Git

### Azure Resources
- Azure subscription
- Azure Cosmos DB account
- Azure SignalR Service
- Azure Functions App

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/ErrolMc/RealTimeChatApp.git
cd ChatAppGithub
```

### 2. Local Development Setup

#### Configure Azure Resources
1. Create a local.settings.json in the ChatAppDatabaseFunctions directory:
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

2. Set up Azure Cosmos DB Emulator (for local development)
   - Download and install the emulator
   - Update connection strings accordingly

#### Frontend Setup
1. Navigate to ChatAppFrontEndAvalonia:
```bash
cd ChatAppFrontEndAvalonia
```

2. Create appsettings.Development.json:
```json
{
  "ApiBaseUrl": "http://localhost:7071",
  "SignalR": {
    "Endpoint": "http://localhost:5000/chat"
  }
}
```

### 3. Running the Application

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

## Development Workflow

### 1. Branch Management
- `main`: Production-ready code
- `develop`: Integration branch
- Feature branches: `feature/feature-name`
- Bug fixes: `fix/bug-name`

### 2. Code Style
- Follow C# coding conventions
- Use meaningful variable and function names
- Add XML documentation comments for public APIs
- Keep methods focused and small

### 3. Testing
- Write unit tests for business logic
- Integration tests for API endpoints
- UI tests for critical user flows

### 4. Debugging

#### Backend Debugging
1. Set breakpoints in Visual Studio
2. Attach debugger to Azure Functions host
3. Use Application Insights for monitoring

#### Frontend Debugging
1. Use Avalonia Preview Window
2. Debug XAML layout issues
3. Monitor SignalR connections

### 5. Common Tasks

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

## Deployment

### 1. Staging Deployment
```bash
# Deploy Azure Functions
func azure functionapp publish YourFunctionApp-Staging

# Deploy SignalR Server
dotnet publish SignalRServer -c Release
# Deploy to Azure App Service

# Build Frontend
dotnet publish -c Release
```

### 2. Production Deployment
- Use Azure DevOps or GitHub Actions
- Follow the CI/CD pipeline
- Run automated tests
- Monitor deployment health

## Troubleshooting

### Common Issues

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

### Logging

#### Enable Debug Logging
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

#### View Logs
- Check Application Insights
- Review Azure Function logs
- Monitor SignalR diagnostic logs

## Performance Optimization

### Frontend
- Implement caching
- Optimize XAML rendering
- Lazy load components

### Backend
- Use async/await properly
- Optimize database queries
- Configure auto-scaling

## Security Best Practices

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