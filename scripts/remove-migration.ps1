# Set working directory to solution root
$solutionRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Removing last migration..."
dotnet ef migrations remove `
    --project "$solutionRoot/_src/FloByte.Infrastructure" `
    --startup-project "$solutionRoot/_src/FloByte.API" `
    --force

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to remove migration"
    exit 1
}

Write-Host "Migration removed successfully"
