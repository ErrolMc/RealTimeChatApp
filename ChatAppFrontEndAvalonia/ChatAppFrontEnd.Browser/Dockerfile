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

# Debug: verify AppBundle contents
RUN ls -la /app/ && ls -la /app/_framework/ 2>/dev/null || echo "No _framework dir found"

FROM nginx:alpine
RUN rm /etc/nginx/conf.d/default.conf
COPY ChatAppFrontEndAvalonia/ChatAppFrontEnd.Browser/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app /usr/share/nginx/html
EXPOSE 80
