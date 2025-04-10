# Get the migration name from command line argument or use "Update"
param(
    [string]$MigrationName = "Update",
    [string]$Environment = "Development"
)

# Set working directory to solution root
$solutionRoot = Split-Path -Parent $PSScriptRoot

# Add migration if name is not "Update"
if ($MigrationName -ne "Update") {
    Write-Host "Creating migration '$MigrationName'..."
    dotnet ef migrations add $MigrationName `
        --project "$solutionRoot/_src/FloByte.Infrastructure" `
        --startup-project "$solutionRoot/_src/FloByte.API" `
        --output-dir Persistence/Migrations `
        --configuration $Environment
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create migration"
        exit 1
    }
}

# Update database
Write-Host "Updating database..."
dotnet ef database update `
    --project "$solutionRoot/_src/FloByte.Infrastructure" `
    --startup-project "$solutionRoot/_src/FloByte.API" `
    --configuration $Environment

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to update database"
    exit 1
}

Write-Host "Database update completed successfully"
