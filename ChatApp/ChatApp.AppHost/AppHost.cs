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

// SignalR Server
var signalrServer = builder.AddProject<Projects.ChatAppSignalRServer>("signalr-server")
    .WithReference(signalr)
    .WaitFor(signalr)
    .WithExternalHttpEndpoints();

// Backend API
var backend = builder.AddProject<Projects.ChatApp_Backend>("backend")
    .WithReference(cosmos)
    .WithReference(signalr)
    .WithReference(signalrServer)
    .WaitFor(cosmos)
    .WaitFor(signalr)
    .WithExternalHttpEndpoints();

// Browser Frontend (served via nginx)
var browserFrontend = builder.AddDockerfile("browser-frontend", contextPath: "../..", dockerfilePath: "ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/Dockerfile")
       .WithHttpEndpoint(targetPort: 80)
       .WithEnvironment("BACKEND_URI", backend.GetEndpoint("https"))
       .WithEnvironment("SIGNALR_URI", signalrServer.GetEndpoint("https"))
       .WithExternalHttpEndpoints();

var browserFrontendUrl = browserFrontend.GetEndpoint("http");

// Pass browser frontend URL to backend and signalr for CORS
backend.WithEnvironment("services__browser-frontend__http__0", browserFrontendUrl);
signalrServer.WithEnvironment("services__browser-frontend__http__0", browserFrontendUrl);

// Custom frontend domain for CORS (set this when using a custom domain in Azure)
var customFrontendOrigin = builder.Configuration["CUSTOM_FRONTEND_ORIGIN"] ?? "";
backend.WithEnvironment("CUSTOM_FRONTEND_ORIGIN", customFrontendOrigin);
signalrServer.WithEnvironment("CUSTOM_FRONTEND_ORIGIN", customFrontendOrigin);

// Each service needs to know its own URL and the other's URL for JWT issuer/audience validation
// .WithReference() only injects URLs of OTHER services, not the service's own URL
backend.WithEnvironment("services__backend__https__0", backend.GetEndpoint("https"));
signalrServer.WithEnvironment("services__backend__https__0", backend.GetEndpoint("https"));
signalrServer.WithEnvironment("services__signalr-server__https__0", signalrServer.GetEndpoint("https"));

// Desktop Frontend
builder.AddProject<Projects.ChatAppFrontEnd_Desktop>("desktop")
    .WithReference(backend)
    .WithReference(signalrServer)
    .WaitFor(backend);

builder.Build().Run();
