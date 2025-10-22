<# 
Quietly link per-view CSS/JS from wwwroot to matching Views folders.
Also removes dangling symlinks in Views when no matching base file exists.
Symlink-only; fails if a link can’t be created. Minimal logging.
#>

[CmdletBinding()]
param(
    [string]$StartDir = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

# Normalize StartDir
if ([string]::IsNullOrWhiteSpace($StartDir)) { $StartDir = (Get-Location).Path }
if (-not (Test-Path -LiteralPath $StartDir)) { throw "StartDir '$StartDir' does not exist." }

function Find-ProjectRoot {
    param([string]$Start)

    # Only walk UP from Start; do NOT search down (avoids System32 access issues)
    $cur = (Resolve-Path -LiteralPath $Start).Path
    while ($true) {
        $hasViews = Test-Path -LiteralPath (Join-Path $cur 'Views')   -PathType Container
        $hasWroot = Test-Path -LiteralPath (Join-Path $cur 'wwwroot') -PathType Container
        if ($hasViews -and $hasWroot) { return $cur }

        $parent = Split-Path -Parent $cur
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $cur) { break }
        $cur = $parent
    }

    throw "Could not locate a project root containing BOTH 'Views' and 'wwwroot' by walking up from '$Start'."
}

function Ensure-Symlink {
    param(
        [Parameter(Mandatory)] [string]$TargetAbsolute,
        [Parameter(Mandatory)] [string]$LinkPath
    )

    if (Test-Path -LiteralPath $LinkPath) { return "skip" }

    try {
        New-Item -ItemType SymbolicLink -Path $LinkPath -Target $TargetAbsolute -Force | Out-Null
        return "linked"
    }
    catch {
        throw "Failed to create symlink for '$LinkPath' -> '$TargetAbsolute' : $($_.Exception.Message)"
    }
}

function Is-Symlink {
    param([Parameter(Mandatory)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    try {
        $item = Get-Item -LiteralPath $Path -Force
        return ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0
    } catch { return $false }
}

try {
    Write-Host "===================================================="
    Write-Host " Running link-view-assets.ps1 from $PSScriptRoot"
    Write-Host "====================================================`n"

    $ProjectRoot = Find-ProjectRoot -Start $StartDir
    $ViewsRoot   = Join-Path $ProjectRoot 'Views'
    $CssRoot     = Join-Path $ProjectRoot 'wwwroot\css\views'
    $JsRoot      = Join-Path $ProjectRoot 'wwwroot\js\views'

    Write-Host "ProjectRoot: $ProjectRoot"
    Write-Host "ViewsRoot  : $ViewsRoot"
    Write-Host "CSS root   : $CssRoot"
    Write-Host "JS root    : $JsRoot"
    Write-Host ""

    $linked   = New-Object System.Collections.Generic.List[string]
    $skipped  = New-Object System.Collections.Generic.List[string]
    $removed  = New-Object System.Collections.Generic.List[string]
    $errors   = New-Object System.Collections.Generic.List[string]

    $views = Get-ChildItem -Path $ViewsRoot -Filter '*.cshtml' -Recurse
    foreach ($v in $views) {
        if ($v.Name -like '_*.cshtml') { continue }  # skip partials/layouts

        $relFromViews = $v.FullName.Substring($ViewsRoot.Length).TrimStart('\','/')
        $viewDirRel   = Split-Path $relFromViews -Parent
        if ($null -eq $viewDirRel) { $viewDirRel = '' } # root-level views
        $baseName     = [System.IO.Path]::GetFileNameWithoutExtension($v.Name)
        $viewFolder   = Split-Path $v.FullName -Parent

        $targets = @(
            @{ ext = '.css'; path = Join-Path (Join-Path $CssRoot $viewDirRel) "$baseName.css" },
            @{ ext = '.js' ; path = Join-Path (Join-Path $JsRoot  $viewDirRel) "$baseName.js"  }
        )

        foreach ($t in $targets) {
            $targetFile = $t.path
            $linkPath   = Join-Path $viewFolder ($baseName + $t.ext)

            if (Test-Path -LiteralPath $targetFile -PathType Leaf) {
                # Base exists — ensure link
                $result = Ensure-Symlink -TargetAbsolute $targetFile -LinkPath $linkPath
                switch ($result) {
                    'linked' { $linked.Add(  "$relFromViews -> $($t.ext)") }
                    'skip'   { $skipped.Add( "$relFromViews -> $($t.ext)") }
                }
            }
            else {
                # Base missing — if a symlink exists in the view folder with same name, remove it
                if (Is-Symlink -Path $linkPath) {
                    try {
                        Remove-Item -LiteralPath $linkPath -Force
                        $removed.Add("$relFromViews -> $($t.ext)")
                    }
                    catch {
                        $errors.Add("Failed to remove dangling symlink '$linkPath': $($_.Exception.Message)")
                    }
                }
                # Quiet if no link or if it's a real file (we only touch symlinks)
            }
        }
    }

    if ($linked.Count -gt 0)  { Write-Host "Linked:";         $linked  | ForEach-Object { Write-Host "  [OK] $_" }; Write-Host "" }
    if ($skipped.Count -gt 0) { Write-Host "Already present:"; $skipped | ForEach-Object { Write-Host "  [Skip] $_" }; Write-Host "" }
    if ($removed.Count -gt 0) { Write-Host "Removed dangling symlinks:"; $removed | ForEach-Object { Write-Host "  [Del] $_" }; Write-Host "" }
    if ($errors.Count -gt 0)  { Write-Host "Errors:"; $errors | ForEach-Object { Write-Host "  [Err] $_" }; Write-Host "" }

    if (($linked.Count + $skipped.Count + $removed.Count) -eq 0) {
        Write-Host "No matching per-view CSS/JS found under wwwroot, and no dangling symlinks to clean."
    }

    Write-Host ""
    Write-Host " Done. Quiet mode: linked/skipped/removed shown."
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)"
    exit 1
}
