param(
    [switch]$Publish
)

$ErrorActionPreference = 'Stop'
dotnet restore OutlookOtpClipboard.sln
if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet build OutlookOtpClipboard.sln -c Release --no-restore
if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet test OutlookOtpClipboard.sln -c Release --no-build
if ($LASTEXITCODE) { exit $LASTEXITCODE }

if ($Publish) {
    dotnet publish src/OutlookOtpClipboard/OutlookOtpClipboard.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}
