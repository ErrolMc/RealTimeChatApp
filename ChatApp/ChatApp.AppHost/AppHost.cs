var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator(emulator => emulator
        .WithLifetime(ContainerLifetime.Persistent)   
        .WithDataVolume("chatapp-cosmos-data")
        .WithHttpsEndpoint(targetPort: 8081, name: "explorer", isProxied: false)
        .WithUrlForEndpoint("explorer", url =>
        {
            url.Url = "/_explorer/index.html";
            url.DisplayText = "Cosmos Explorer";
        }));

var signalr = builder.AddAzureSignalR("signalr")
    .RunAsEmulator(emulator =>
        emulator.WithLifetime(ContainerLifetime.Persistent));

// Backend API
var backend = builder.AddProject<Projects.ChatApp_Backend>("backend")
    .WithReference(cosmos)
    .WithReference(signalr)
    .WaitFor(cosmos)
    .WaitFor(signalr);

// SignalR Server
var signalrServer = builder.AddProject<Projects.ChatAppSignalRServer>("signalr-server")
    .WithReference(signalr)
    .WaitFor(signalr);

// Desktop Frontend
builder.AddProject<Projects.ChatAppFrontEnd_Desktop>("desktop")
    .WithReference(backend)
    .WithReference(signalrServer)
    .WaitFor(backend);

// Browser Frontend (served via nginx)
builder.AddDockerfile("browser-frontend", contextPath: "../..", dockerfilePath: "ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/Dockerfile")
       .WithHttpEndpoint(port: 5200, targetPort: 80);

builder.Build().Run();
