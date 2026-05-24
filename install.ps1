$ErrorActionPreference = 'Stop'
$Repo = 'thomaslazar/dnb-cli'
$InstallDir = if ($env:DNB_CLI_INSTALL_DIR) { $env:DNB_CLI_INSTALL_DIR } else { Join-Path $env:LOCALAPPDATA 'dnb-cli' }
$Version = $env:DNB_CLI_VERSION

$Arch = switch ($env:PROCESSOR_ARCHITECTURE) {
  'AMD64' { 'x64' }
  'ARM64' { 'arm64' }
  default { throw "Unsupported architecture: $env:PROCESSOR_ARCHITECTURE" }
}
$Rid = "win-$Arch"

if (-not $Version) {
  $latest = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest"
  $Version = $latest.tag_name
  if (-not $Version) { throw 'Could not determine latest version' }
}

Write-Host "Installing dnb-cli $Version ($Rid)..."

$DownloadUrl = "https://github.com/$Repo/releases/download/$Version/dnb-cli-$Rid.exe"
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
$Target = Join-Path $InstallDir 'dnb-cli.exe'
Invoke-WebRequest -Uri $DownloadUrl -OutFile $Target

$UserPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if (-not ($UserPath -split ';' | Where-Object { $_ -eq $InstallDir })) {
  Write-Warning "$InstallDir is not on your user PATH. Add it via System Properties → Environment Variables."
}

& $Target --version
Write-Host "dnb-cli installed to $Target"
