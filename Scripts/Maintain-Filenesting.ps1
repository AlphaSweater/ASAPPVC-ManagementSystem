param(
  [string]$SolutionRoot,
  [switch]$Watch,
  [string[]]$ExcludeDirs = @('bin','obj','.git','.vs','node_modules','Migrations')
)

if (-not $SolutionRoot -or [string]::IsNullOrWhiteSpace($SolutionRoot)) {
  if ($PSScriptRoot) {
    $SolutionRoot = Split-Path -LiteralPath $PSScriptRoot -Parent
  } else {
    $SolutionRoot = (Get-Location).Path
  }
}

function Is-ExcludedPath([string]$path) {
  foreach ($e in $ExcludeDirs) {
    if ($path -match "\\$([Regex]::Escape($e))(\\|$)") { return $true }
  }
  return $false
}

function Is-GeneratedName([string]$name) {
  return $name -match '\.(g|generated|Designer)\.cs$'
}

function Sanitize-JsonText([string]$text) {
  if (-not $text) { return $text }
  $text = [Regex]::Replace($text, '/\*.*?\*/', '', 'Singleline')
  $text = [Regex]::Replace($text, '(^|\s)//.*$', '$1', 'Multiline')
  $text = [Regex]::Replace($text, ',\s*(\}|\])', '$1', 'Singleline')
  return $text
}

function To-StringArray($val) {
  if ($null -eq $val) { return @() }
  if ($val -is [System.Array]) { return @($val | ForEach-Object { "$_" }) }
  return @("$val")
}

function Read-JsonOrInit([string]$path) {
  if (Test-Path -LiteralPath $path) {
    $raw = Get-Content -LiteralPath $path -Raw
    $clean = Sanitize-JsonText $raw
    try { return $clean | ConvertFrom-Json -Depth 200 } catch {}
    $bak = "$path.bak"
    try { Copy-Item -LiteralPath $path -Destination $bak -Force } catch {}
    Write-Host "  [!] Backed up invalid JSON to $bak; creating a fresh file." -ForegroundColor Yellow
  }
  $obj = [ordered]@{
    help = "https://go.microsoft.com/fwlink/?linkid=866610"
    dependentFileProviders = @{ add = @{} }
  }
  return $obj
}

function Ensure-FileToFileBlock($json) {
  if (-not $json.dependentFileProviders) { $json.dependentFileProviders = @{ add = @{} } }
  if (-not $json.dependentFileProviders.add) { $json.dependentFileProviders.add = @{} }
  if (-not $json.dependentFileProviders.add.fileToFile) { $json.dependentFileProviders.add.fileToFile = @{ add = @{} } }
  if (-not $json.dependentFileProviders.add.fileToFile.add) { $json.dependentFileProviders.add.fileToFile.add = @{} }
}

function Write-JsonIfChanged([string]$path, $obj) {
  $new = ($obj | ConvertTo-Json -Depth 100)
  $old = ""
  if (Test-Path -LiteralPath $path) {
    $old = Get-Content -LiteralPath $path -Raw
  }
  if ($new -ne $old) {
    Set-Content -LiteralPath $path -Value $new -Encoding UTF8
  }
}

function Update-ProjectFilenesting([string]$projDir) {
  if (Is-ExcludedPath $projDir) { return }
  $nestPath = Join-Path $projDir ".filenesting.json"
  $json = Read-JsonOrInit $nestPath
  Ensure-FileToFileBlock $json
  $fileMap = $json.dependentFileProviders.add.fileToFile.add
  $existingKeys = @($fileMap.PSObject.Properties.Name)
  foreach ($ek in $existingKeys) {
    $fileMap[$ek] = To-StringArray $fileMap[$ek]
  }
  $pairs = @{}
  $interfaces = Get-ChildItem -Path $projDir -Recurse -Filter "I*.cs" -File | Where-Object { -not (Is-ExcludedPath $_.DirectoryName) }
  foreach ($iface in $interfaces) {
    if ($iface.BaseName.Length -lt 2) { continue }
    $dir = $iface.DirectoryName
    if (Is-ExcludedPath $dir) { continue }
    $base = $iface.BaseName.Substring(1)
    $impls = Get-ChildItem -Path $dir -Filter ($base + "*.cs") -File | Where-Object { $_.Name -ne $iface.Name -and -not (Is-GeneratedName $_.Name) } | Select-Object -ExpandProperty Name
    if ($impls -and $impls.Count -gt 0) {
      $pairs[$iface.Name] = ($impls | Sort-Object -Unique)
    }
  }
  $existingKeys = @($fileMap.PSObject.Properties.Name)
  foreach ($k in $existingKeys) {
    $existsSomewhere = (Get-ChildItem -Path $projDir -Recurse -Filter $k -File | Where-Object { -not (Is-ExcludedPath $_.DirectoryName) } | Select-Object -First 1)
    if (-not $existsSomewhere -or -not $pairs.ContainsKey($k)) {
      $fileMap.PSObject.Properties.Remove($k) | Out-Null
    }
  }
  foreach ($k in $pairs.Keys) {
    $vals = To-StringArray $pairs[$k]
    if ($fileMap.ContainsKey($k)) {
      $merged = (To-StringArray $fileMap[$k] + $vals) | Sort-Object -Unique
      $fileMap[$k] = @($merged)
    } else {
      $fileMap[$k] = @($vals)
    }
  }
  Write-JsonIfChanged $nestPath $json
}

function Update-AllProjects([string]$root) {
  $projects = Get-ChildItem -Path $root -Recurse -Filter *.csproj -File | Where-Object { -not (Is-ExcludedPath $_.DirectoryName) }
  foreach ($p in $projects) { Update-ProjectFilenesting $p.DirectoryName }
}

if (-not $Watch) {
  Update-AllProjects $SolutionRoot
  return
}

$debounce = @{}
$subs = @()
$projects = Get-ChildItem -Path $SolutionRoot -Recurse -Filter *.csproj -File | Where-Object { -not (Is-ExcludedPath $_.DirectoryName) }

foreach ($p in $projects) {
  $projDir = $p.DirectoryName
  if (Is-ExcludedPath $projDir) { continue }
  $w = New-Object System.IO.FileSystemWatcher
  $w.Path = $projDir
  $w.Filter = "*.cs"
  $w.IncludeSubdirectories = $true
  $w.EnableRaisingEvents = $true
  $action = {
    param($sender, $eventArgs)
    $projDirLocal = $event.MessageData
    if (-not $projDirLocal) { return }
    $key = $projDirLocal.ToLower()
    if ($script:debounce.ContainsKey($key)) {
      $script:debounce[$key].Stop()
      $script:debounce[$key].Dispose()
      $script:debounce.Remove($key) | Out-Null
    }
    $t = New-Object System.Timers.Timer 500
    $t.AutoReset = $false
    $t.Add_Elapsed({
      try { Update-ProjectFilenesting $projDirLocal } catch {}
      $script:debounce.Remove($key) | Out-Null
    })
    $script:debounce[$key] = $t
    $t.Start()
  }
  $subs += Register-ObjectEvent -InputObject $w -EventName Changed -Action $action -MessageData $projDir
  $subs += Register-ObjectEvent -InputObject $w -EventName Created -Action $action -MessageData $projDir
  $subs += Register-ObjectEvent -InputObject $w -EventName Deleted -Action $action -MessageData $projDir
  $subs += Register-ObjectEvent -InputObject $w -EventName Renamed -Action $action -MessageData $projDir
}

while ($true) { Start-Sleep -Seconds 5 }
