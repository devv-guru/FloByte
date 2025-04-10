# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["_src/FloByte.API/FloByte.API.csproj", "_src/FloByte.API/"]
COPY ["_src/FloByte.Application/FloByte.Application.csproj", "_src/FloByte.Application/"]
COPY ["_src/FloByte.Domain/FloByte.Domain.csproj", "_src/FloByte.Domain/"]
COPY ["_src/FloByte.Infrastructure/FloByte.Infrastructure.csproj", "_src/FloByte.Infrastructure/"]
RUN dotnet restore "_src/FloByte.API/FloByte.API.csproj"

# Copy the rest of the code
COPY _src/. .
WORKDIR "/src/_src/FloByte.API"

# Build and publish
RUN dotnet build "FloByte.API.csproj" -c Release -o /app/build
RUN dotnet publish "FloByte.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published files from build stage
COPY --from=build /app/publish .

# Create non-root user
RUN useradd -m -s /bin/bash appuser
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "FloByte.API.dll"]
