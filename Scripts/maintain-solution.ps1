<# Unified one-shot maintainer (PowerShell 5 compatible, hardened)
- Task A: Update .filenesting.json (I*.cs → Thing*.cs) per project
- Task B: Link per-view CSS/JS from wwwroot to Views (+ clean dangling symlinks)
- Runs once, no watchers or parameter sets
#>

param(
  [string]$SolutionRoot,
  [string[]]$ExcludeDirs = @('bin','obj','.git','.vs','node_modules','Migrations')
)

$ErrorActionPreference = 'Stop'

# ---------- Resolve root ----------
if (-not $SolutionRoot -or [string]::IsNullOrWhiteSpace($SolutionRoot)) {
  if ($PSScriptRoot) {
    try {
      $SolutionRoot = (Get-Item -LiteralPath $PSScriptRoot).Parent.FullName
    } catch {
      $SolutionRoot = (Get-Location).Path
    }
  } else {
    $SolutionRoot = (Get-Location).Path
  }
}
Write-Host "Using SolutionRoot: $SolutionRoot" -ForegroundColor DarkCyan

# ---------- Helpers ----------
function Is-ExcludedPath([string]$path) {
  foreach ($e in $ExcludeDirs) {
    if ($path -match "\\$([Regex]::Escape($e))(\\|$)") { return $true }
  }
  return $false
}
function Is-GeneratedName([string]$name) { $name -match '\.(g|generated|Designer)\.cs$' }

# Convert a PSCustomObject (or null) to Hashtable shallowly
function As-Hashtable($obj) {
  if ($null -eq $obj) { return @{} }
  if ($obj -is [hashtable]) { return $obj }
  $ht = @{}
  if ($obj.PSObject -and $obj.PSObject.Properties) {
    foreach ($p in $obj.PSObject.Properties) {
      $ht[$p.Name] = $p.Value
    }
  }
  return $ht
}

function Read-Json([string]$path) {
  if (Test-Path -LiteralPath $path) {
    $raw = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    try   { return ($raw | ConvertFrom-Json) }
    catch { throw "Invalid JSON: $path`n$($_.Exception.Message)" }
  }
  return (ConvertFrom-Json @'
{
  "help": "https://go.microsoft.com/fwlink/?linkid=866610",
  "dependentFileProviders": { "add": {} }
}
'@)
}

function Ensure-Paths($obj) {
  if (-not $obj.PSObject.Properties.Match('dependentFileProviders')) {
    $obj | Add-Member -NotePropertyName dependentFileProviders -NotePropertyValue (@{ add = @{ } })
  }
  if (-not $obj.dependentFileProviders.PSObject.Properties.Match('add')) {
    $obj.dependentFileProviders | Add-Member -NotePropertyName add -NotePropertyValue (@{ })
  }
  if (-not $obj.dependentFileProviders.add.PSObject.Properties.Match('fileToFile')) {
    $obj.dependentFileProviders.add | Add-Member -NotePropertyName fileToFile -NotePropertyValue (@{ add = @{} })
  } elseif (-not $obj.dependentFileProviders.add.fileToFile.PSObject.Properties.Match('add')) {
    $obj.dependentFileProviders.add.fileToFile | Add-Member -NotePropertyName add -NotePropertyValue (@{})
  }
}

function Normalize-MapToStringArrays($map) {
  if ($null -eq $map) { return }
  foreach ($k in @($map.Keys)) {
    $v = $map[$k]
    if ($null -eq $v) { $map[$k] = @(); continue }
    if ($v -is [string]) { $map[$k] = @($v); continue }
    if ($v -isnot [System.Collections.IEnumerable]) { $map[$k] = @("$v"); continue }
    $out = @(); foreach ($it in $v) { $out += ,("$it") }; $map[$k] = $out
  }
}

function Write-JsonIfChanged([string]$path, $obj) {
  # ensure fileToFile.add is a Hashtable before normalization
  $obj.dependentFileProviders.add.fileToFile = As-Hashtable $obj.dependentFileProviders.add.fileToFile
  $obj.dependentFileProviders.add.fileToFile.add = As-Hashtable $obj.dependentFileProviders.add.fileToFile.add

  Normalize-MapToStringArrays $obj.dependentFileProviders.add.fileToFile.add
  $json = $obj | ConvertTo-Json -Depth 64
  if (Test-Path -LiteralPath $path) {
    $old = Get-Content -LiteralPath $path -Raw -Encoding UTF8
  } else {
    $old = ""
  }
  if ($json -ne $old) { Set-Content -LiteralPath $path -Value $json -Encoding UTF8 }
}

# ---------- Bootstrap content for .filenesting.json when missing ----------
# (Your exact JSON)
$BootstrapFilenestingJson = @'
{
	"dependentFileProviders": {
		"add": {
			"extensionToExtension": {
				"add": {
					".css": [
						".cshtml"
					],
					".js": [
						".cshtml"
					]
				}
			},
			"fileToFile": {
				"add": {
				}
			}
		}
	}
}
'@

# ---------- Task A: Filenesting updater ----------
function Update-ProjectFilenesting([string]$projDir) {
  if (Is-ExcludedPath $projDir) { return }
  $nestPath = Join-Path $projDir ".filenesting.json"

  # NEW: If missing, create with your exact JSON and continue
  if (-not (Test-Path -LiteralPath $nestPath -PathType Leaf)) {
    try {
      Set-Content -LiteralPath $nestPath -Value $BootstrapFilenestingJson -Encoding UTF8 -Force
      Write-Host "  [Created] $nestPath" -ForegroundColor Green
    } catch {
      Write-Host "  [Error ] Failed to create $nestPath : $($_.Exception.Message)" -ForegroundColor Yellow
      return
    }
  }

  try { $json = Read-Json $nestPath } catch { Write-Host "  [!] $_" -ForegroundColor Yellow; $json = Read-Json $null }
  Ensure-Paths $json

  # Force Hashtable for fileToFile.add so we can safely .Keys/.Remove/.ContainsKey
  $json.dependentFileProviders.add.fileToFile = As-Hashtable $json.dependentFileProviders.add.fileToFile
  $json.dependentFileProviders.add.fileToFile.add = As-Hashtable $json.dependentFileProviders.add.fileToFile.add
  $fileMap = $json.dependentFileProviders.add.fileToFile.add

  # Build fresh interface -> implementations map (same folder as interface)
  $pairs = @{}
  $interfaces = Get-ChildItem -Path $projDir -Recurse -Filter "I*.cs" -File |
                Where-Object { -not (Is-ExcludedPath $_.DirectoryName) }

  foreach ($iface in $interfaces) {
    if ($iface.BaseName.Length -lt 2) { continue }
    $dir = $iface.DirectoryName
    if (Is-ExcludedPath $dir) { continue }

    $base = $iface.BaseName.Substring(1)  # drop leading 'I'
    $impls = Get-ChildItem -Path $dir -Filter ($base + "*.cs") -File |
             Where-Object { $_.Name -ne $iface.Name -and -not (Is-GeneratedName $_.Name) } |
             Select-Object -ExpandProperty Name

    if ($impls -and $impls.Count -gt 0) {
      $pairs[$iface.Name] = ($impls | Sort-Object -Unique)
    }
  }

  # Remove stale entries (skip null/empty keys defensively)
  $keys = @($fileMap.Keys | Where-Object { $_ -is [string] -and $_.Length -gt 0 })
  foreach ($k in $keys) {
    $exists = Get-ChildItem -Path $projDir -Recurse -Filter $k -File |
              Where-Object { -not (Is-ExcludedPath $_.DirectoryName) } |
              Select-Object -First 1
    if (-not $exists -or -not $pairs.ContainsKey($k)) {
      [void]$fileMap.Remove($k)
    }
  }

  # Add/merge current pairs
  foreach ($k in $pairs.Keys) {
    $vals = @($pairs[$k] | ForEach-Object { "$_" })
    if ($fileMap.ContainsKey($k)) {
      $merged = (@($fileMap[$k]) + $vals) | Sort-Object -Unique
      $fileMap[$k] = $merged
    } else {
      $fileMap[$k] = $vals
    }
  }

  Write-JsonIfChanged $nestPath $json
}

function Update-AllProjects([string]$root) {
  $projects = Get-ChildItem -Path $root -Recurse -Filter *.csproj -File |
              Where-Object { -not (Is-ExcludedPath $_.DirectoryName) }
  foreach ($p in $projects) { Update-ProjectFilenesting $p.DirectoryName }
}

# ---------- Task B: Per-view CSS/JS symlink + cleanup ----------
function Ensure-Symlink {
  param([string]$TargetAbsolute,[string]$LinkPath)

  if (Test-Path -LiteralPath $LinkPath) { return "skip" }

  $linkDir = Split-Path -Path $LinkPath -Parent
  if (-not (Test-Path -LiteralPath $linkDir -PathType Container)) {
    New-Item -ItemType Directory -Path $linkDir | Out-Null
  }

  cmd.exe /c ('mklink "{0}" "{1}"' -f $LinkPath, $TargetAbsolute) | Out-Null
  if (-not (Test-Path -LiteralPath $LinkPath -PathType Leaf)) {
    throw "mklink failed for '$LinkPath'."
  }
  return "linked"
}

function Is-Symlink([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
  try { return ((Get-Item -LiteralPath $Path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 }
  catch { return $false }
}

function Link-Assets-ForWebProject([string]$projectRoot) {
  $viewsRoot = Join-Path $projectRoot 'Views'
  $cssRoot   = Join-Path $projectRoot 'wwwroot\css\views'
  $jsRoot    = Join-Path $projectRoot 'wwwroot\js\views'
  if (-not (Test-Path $viewsRoot -PathType Container)) { return }
  if (-not (Test-Path (Split-Path $cssRoot -Parent) -PathType Container)) { return }
  if (-not (Test-Path (Split-Path $jsRoot  -Parent) -PathType Container)) { return }

  Write-Host ""
  Write-Host "—— Linking view assets in: $projectRoot" -ForegroundColor Cyan

  $linked=@(); $skipped=@(); $removed=@(); $errors=@()

  $views = Get-ChildItem -Path $viewsRoot -Filter '*.cshtml' -Recurse |
           Where-Object { -not ($_.Name -like '_*.cshtml') }

  foreach ($v in $views) {
    $relFromViews = $v.FullName.Substring($viewsRoot.Length).TrimStart('\','/')
    $viewDirRel   = Split-Path $relFromViews -Parent
    if ($null -eq $viewDirRel) { $viewDirRel = '' }
    $baseName     = [IO.Path]::GetFileNameWithoutExtension($v.Name)
    $viewFolder   = Split-Path $v.FullName -Parent

    $targets = @(
      @{ ext = '.css'; path = Join-Path (Join-Path $cssRoot $viewDirRel) "$baseName.css" },
      @{ ext = '.js' ; path = Join-Path (Join-Path $jsRoot  $viewDirRel) "$baseName.js"  }
    )

    foreach ($t in $targets) {
      $targetFile = $t.path
      $linkPath   = Join-Path $viewFolder ($baseName + $t.ext)

      if (Test-Path -LiteralPath $targetFile -PathType Leaf) {
        try {
          $r = Ensure-Symlink -TargetAbsolute $targetFile -LinkPath $linkPath
          if ($r -eq 'linked') { $linked += "$relFromViews -> $($t.ext)" } else { $skipped += "$relFromViews -> $($t.ext)" }
        } catch { $errors += $_.Exception.Message }
      } else {
        if (Is-Symlink $linkPath) {
          try { Remove-Item -LiteralPath $linkPath -Force; $removed += "$relFromViews -> $($t.ext)" }
          catch { $errors += "Failed to remove dangling symlink '$linkPath': $($_.Exception.Message)" }
        }
      }
    }
  }

  if ($linked.Count)  { Write-Host "Linked:";                 $linked  | ForEach-Object { Write-Host "  [OK] $_" } }
  if ($skipped.Count) { Write-Host "Already present:";        $skipped | ForEach-Object { Write-Host "  [Skip] $_" } }
  if ($removed.Count) { Write-Host "Removed dangling links:"; $removed | ForEach-Object { Write-Host "  [Del] $_" } }
  if ($errors.Count)  { Write-Host "Errors:";                 $errors  | ForEach-Object { Write-Host "  [Err] $_" } }
  if (-not ($linked.Count + $skipped.Count + $removed.Count) -and -not $errors.Count) {
    Write-Host "No matching per-view CSS/JS and no dangling symlinks."
  }
}

function Find-WebProjectRoots([string]$root) {
  Get-ChildItem -Path $root -Directory -Recurse |
  Where-Object {
    -not (Is-ExcludedPath $_.FullName) -and
    (Test-Path (Join-Path $_.FullName 'Views')   -PathType Container) -and
    (Test-Path (Join-Path $_.FullName 'wwwroot') -PathType Container)
  } | Select-Object -ExpandProperty FullName -Unique
}

function Link-Assets-AllWebProjects([string]$root) {
  foreach ($wr in (Find-WebProjectRoots $root)) { Link-Assets-ForWebProject $wr }
}

# ---------- Run both tasks ----------
Write-Host "=== Task A: Updating filenesting across projects ===" -ForegroundColor Green
Update-AllProjects $SolutionRoot

Write-Host ""
Write-Host "=== Task B: Linking per-view CSS/JS & cleaning symlinks ===" -ForegroundColor Green
Link-Assets-AllWebProjects $SolutionRoot

Write-Host ""
Write-Host "✅ Done. Filenesting + view asset links processed under: $SolutionRoot" -ForegroundColor Green
