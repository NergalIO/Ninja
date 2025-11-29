param()
$root = "d:\Projects\Unity\Ninja\Assets\Scripts"
if (-not (Test-Path $root)) { Write-Error "Root not found: $root"; exit 1 }

# Backup
$backup = Join-Path $root "Backup_$(Get-Date -Format yyyyMMdd_HHmmss)"
New-Item -ItemType Directory -Path $backup | Out-Null
Get-ChildItem -Path $root -Recurse -Include *.cs -File | ForEach-Object {
    $dest = Join-Path $backup ($_.FullName.Substring($root.Length).TrimStart('\'))
    $d = Split-Path $dest -Parent
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d | Out-Null }
    Copy-Item -Path $_.FullName -Destination $dest -Force
}
Write-Output "Бэкап создан: $backup"

# Create new folder structure
$dirs = @(
    "$root\Core",
    "$root\Systems\Loader",
    "$root\Gameplay\Player",
    "$root\Gameplay\Enemy",
    "$root\Gameplay\Levels",
    "$root\Input",
    "$root\UI\Menu",
    "$root\UI\InGame",
    "$root\Audio",
    "$root\Settings",
    "$root\Utils",
    "$root\Tests"
)
foreach ($d in $dirs) { 
    if (-not (Test-Path $d)) { 
        New-Item -ItemType Directory -Path $d | Out-Null
        Write-Output "Создана папка: $d"
    }
}

# Move rules
function MoveIfExists($src, $dst) {
    if (Test-Path $src) {
        Write-Output "Перемещение: $src -> $dst"
        if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst | Out-Null }
        Get-ChildItem -Path $src -Force | ForEach-Object {
            Move-Item -Path $_.FullName -Destination $dst -Force
        }
        try { Remove-Item -Path $src -Recurse -Force -ErrorAction SilentlyContinue } catch {}
    }
}

# Move in order
MoveIfExists "$root\InGame\Player" "$root\Gameplay\Player"
MoveIfExists "$root\InGame\Enemy" "$root\Gameplay\Enemy"
MoveIfExists "$root\InGame\InputSystem" "$root\Input"
MoveIfExists "$root\InGame\UI" "$root\UI\InGame"
MoveIfExists "$root\InGame" "$root\Gameplay\InGame"

MoveIfExists "$root\Managers\AsyncSceneLoader" "$root\Systems\Loader"
MoveIfExists "$root\Managers\Sound" "$root\Audio"
MoveIfExists "$root\Managers" "$root\Systems"

MoveIfExists "$root\Levels" "$root\Gameplay\Levels"
MoveIfExists "$root\Menu" "$root\UI\Menu"
MoveIfExists "$root\Settings" "$root\Settings"
MoveIfExists "$root\UI" "$root\UI"
MoveIfExists "$root\Utils" "$root\Utils"

# Fix file name typos
Get-ChildItem -Path $root -Recurse -Filter "ComponentExtansion.cs" | ForEach-Object {
    $new = Join-Path $_.DirectoryName "ComponentExtensions.cs"
    Write-Output "Переименование: $($_.Name) -> ComponentExtensions.cs"
    Rename-Item -Path $_.FullName -NewName $new -Force
}

Get-ChildItem -Path $root -Recurse -Filter "VolumeSpitchController.cs" | ForEach-Object {
    $new = Join-Path $_.DirectoryName "VolumePitchController.cs"
    Write-Output "Переименование: $($_.Name) -> VolumePitchController.cs"
    Rename-Item -Path $_.FullName -NewName $new -Force
}

# Update namespaces for all .cs files
$csFiles = Get-ChildItem -Path $root -Recurse -Filter *.cs | Where-Object { -not $_.FullName.Contains("\Backup_") }

Write-Output "Обновление пространств имён для $($csFiles.Count) файлов..."

foreach ($file in $csFiles) {
    $relDir = $file.DirectoryName.Substring($root.Length).TrimStart('\')
    
    if ([string]::IsNullOrEmpty($relDir)) { 
        $ns = "Ninja" 
    }
    else {
        $ns = "Ninja." + ($relDir -replace "\\", ".")
    }

    $text = Get-Content -Raw -LiteralPath $file.FullName

    $text = $text -replace "\bComponentExtansion\b", "ComponentExtensions"
    $text = $text -replace "\bVolumeSpitchController\b", "VolumePitchController"

    if ($text -match '(?m)^\s*namespace\s+[A-Za-z0-9_.]+') {
        $text = [regex]::Replace($text, '(?m)^\s*namespace\s+[A-Za-z0-9_.]+', "namespace $ns")
    }
    else {
        $lines = $text -split "`n"
        $usingLines = @()
        $bodyStart = 0
        
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match '^\s*using\s+') {
                $usingLines += $lines[$i]
            }
            elseif ($lines[$i].Trim() -ne "" -and $usingLines.Count -gt 0) {
                $bodyStart = $i
                break
            }
        }
        
        if ($bodyStart -eq 0) {
            $bodyStart = $usingLines.Count
        }
        
        $usings = ($usingLines -join "`n")
        $body = ($lines[$bodyStart..($lines.Length - 1)] -join "`n").Trim()
        
        if ($usings) {
            $text = "$usings`n`nnamespace $ns`n{`n$body`n}`n"
        }
        else {
            $text = "namespace $ns`n{`n$body`n}`n"
        }
    }

    Set-Content -LiteralPath $file.FullName -Value $text -Encoding UTF8
    Write-Output "✓ $($file.Name) -> namespace $ns"
}

Write-Output "`nМиграция завершена успешно!`nБэкап: $backup"