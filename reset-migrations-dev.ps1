Write-Host "DANGER: Reset migrations + database DEV only" -ForegroundColor Yellow

$confirmation = Read-Host "This will delete the database volume and migrations. Type YES to continue"

if ($confirmation -ne "YES") {
    Write-Host "Cancelled."
    exit 0
}

Write-Host "Stopping and deleting Docker database volume..."
docker compose down -v --remove-orphans

Write-Host "Deleting EF migrations..."
Remove-Item -Recurse -Force .\backend\Crystal.Infrastructure\Migrations -ErrorAction SilentlyContinue

Write-Host "Starting PostgreSQL container..."
docker compose up -d db

Write-Host "Restoring backend..."
dotnet restore .\backend\Crystal.sln

Write-Host "Creating clean migration..."
dotnet ef migrations add InitialCreate `
    -p .\backend\Crystal.Infrastructure `
    -s .\backend\Crystal.API

Write-Host "Applying migration to Docker PostgreSQL..."
dotnet ef database update `
    -p .\backend\Crystal.Infrastructure `
    -s .\backend\Crystal.API

Write-Host "Running tests..."
dotnet test .\backend\Crystal.sln

Write-Host "Done. Check git status and commit the new clean migration." -ForegroundColor Green
git status