FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN apt-get update && apt-get install -y python3 && rm -rf /var/lib/apt/lists/*
RUN dotnet workload install wasm-tools

WORKDIR /src

# Copy project files for restore
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/ChatAppFrontEnd.Browser.csproj ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd/ChatAppFrontEnd.csproj ChatAppFrontEndAvalonia/ChatAppFrontEnd/

RUN dotnet restore ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/ChatAppFrontEnd.Browser.csproj

# Copy source files (including shared files referenced via Compile Include links)
COPY ChatApp/ChatApp.Shared/ ChatApp/ChatApp.Shared/
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd/ ChatAppFrontEndAvalonia/ChatAppFrontEnd/
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/ ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/

RUN dotnet publish ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/ChatAppFrontEnd.Browser.csproj -c Release \
    && cp -r "$(find /src -type d -name AppBundle | head -1)" /app

# Inject config.js script tag into the published index.html (publish overwrites our source edits)
RUN sed -i 's|</head>|<script src="./config.js"></script></head>|' /app/index.html

# Inject env var passthrough into main.js so ServiceConfig reads them via Environment.GetEnvironmentVariable
RUN sed -i '/const dotnetRuntime = await dotnet/i \
const cfg = window.__APP_CONFIG__ || {};' /app/main.js && \
    sed -i 's|const dotnetRuntime = await dotnet|const dotnetRuntime = await dotnet\n    .withEnvironmentVariable("services__backend__https__0", cfg.BackendUri \|\| "")\n    .withEnvironmentVariable("services__signalr-server__https__0", cfg.SignalRUri \|\| "")|' /app/main.js

FROM nginx:alpine
RUN rm /etc/nginx/conf.d/default.conf
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/nginx.conf /etc/nginx/conf.d/default.conf
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
COPY --from=build /app /usr/share/nginx/html
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
