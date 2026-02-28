## Run both FIX server and client end-to-end
## Usage: powershell -ExecutionPolicy Bypass -File run.ps1

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Clean store and log directories
Write-Host "=== Cleaning store and log directories ===" -ForegroundColor Cyan
$dirs = @(
    "$root\FixAcceptorServer\bin\Debug\net9.0\store",
    "$root\FixAcceptorServer\bin\Debug\net9.0\log",
    "$root\FixInitiatorClient\bin\Debug\net9.0\store",
    "$root\FixInitiatorClient\bin\Debug\net9.0\log"
)
foreach ($d in $dirs) {
    if (Test-Path $d) {
        Remove-Item $d -Recurse -Force
        Write-Host "  Removed $d"
    }
}
Write-Host ""

# Build both projects
Write-Host "=== Building FixAcceptorServer ===" -ForegroundColor Cyan
dotnet build "$root\FixAcceptorServer" --nologo
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed!" -ForegroundColor Red; exit 1 }

Write-Host "=== Building FixInitiatorClient ===" -ForegroundColor Cyan
dotnet build "$root\FixInitiatorClient" --nologo
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed!" -ForegroundColor Red; exit 1 }
Write-Host ""

# Start the acceptor as a separate process with its own stdin
Write-Host "=== Starting Acceptor Server ===" -ForegroundColor Green
$serverInfo = New-Object System.Diagnostics.ProcessStartInfo
$serverInfo.FileName = "dotnet"
$serverInfo.Arguments = "run --no-build"
$serverInfo.WorkingDirectory = "$root\FixAcceptorServer"
$serverInfo.UseShellExecute = false
$serverInfo.RedirectStandardInput = $true
$server = [System.Diagnostics.Process]::Start($serverInfo)

# Wait for server to start listening
Start-Sleep -Seconds 3

if ($server.HasExited) {
    Write-Host "Server exited unexpectedly with code $($server.ExitCode)" -ForegroundColor Red
    exit 1
}

# Run the client in the foreground
Write-Host "=== Starting Initiator Client ===" -ForegroundColor Green
Write-Host ""
Push-Location "$root\FixInitiatorClient"
dotnet run --no-build
Pop-Location

# Give the server a moment to process the logout
Start-Sleep -Seconds 1

# Shut down the server by closing its stdin (triggers ReadLine to return)
Write-Host ""
Write-Host "=== Stopping Acceptor Server ===" -ForegroundColor Cyan
try {
    $server.StandardInput.Close()
    $server.WaitForExit(5000) | Out-Null
} catch {}
if (!$server.HasExited) {
    Stop-Process -Id $server.Id -Force
}
Write-Host "Done." -ForegroundColor Green
