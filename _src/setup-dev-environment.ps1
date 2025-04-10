# Setup Development Environment Script

Write-Host "Checking development environment requirements..."

# Check for .NET SDK
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host ".NET SDK not found. Please install .NET 8 SDK from:"
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0"
    Write-Host "After installation, restart your terminal and run this script again."
    exit 1
}

Write-Host ".NET SDK version: $dotnetVersion"

# Create solution and projects
Write-Host "Creating solution and projects..."

# Create solution
dotnet new sln -n FloByte

# Create projects
dotnet new classlib -n FloByte.Domain -f net8.0
dotnet new classlib -n FloByte.Application -f net8.0
dotnet new classlib -n FloByte.Infrastructure -f net8.0
dotnet new webapi -n FloByte.API -f net8.0

# Add projects to solution
dotnet sln add FloByte.Domain/FloByte.Domain.csproj
dotnet sln add FloByte.Application/FloByte.Application.csproj
dotnet sln add FloByte.Infrastructure/FloByte.Infrastructure.csproj
dotnet sln add FloByte.API/FloByte.API.csproj

# Add project references
dotnet add FloByte.Application/FloByte.Application.csproj reference FloByte.Domain/FloByte.Domain.csproj
dotnet add FloByte.Infrastructure/FloByte.Infrastructure.csproj reference FloByte.Application/FloByte.Application.csproj
dotnet add FloByte.API/FloByte.API.csproj reference FloByte.Application/FloByte.Application.csproj
dotnet add FloByte.API/FloByte.API.csproj reference FloByte.Infrastructure/FloByte.Infrastructure.csproj

Write-Host "Development environment setup complete!"
