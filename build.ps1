param([switch]$pack, [switch]$install)

$project_name = "BinProfile"
$project_version = "0.0.1"


dotnet build --configuration Release "/p:VersionPrefix=$project_version"
if (-not $?) { exit 1 }


$dll = ".\bin\Release\net8.0\$project_name.dll"
if (-not (Test-Path $dll)) {
    Write-Output "Could not find $project_name.dll"
    exit 1
}


if (-not ($pack -or $install)) { exit 0 }


$manifest = ".\$project_name.psd1"
if (-not (Test-Path $manifest)) {
    Write-Output "Could not find $project_name.psd1"
    exit 1
}

$pack_dirs = "$project_name\$project_version"  

mkdir $pack_dirs -ErrorAction Ignore > $null
copy $manifest $pack_dirs
copy $dll $pack_dirs

# Panic if psd1 version is different from above version
Update-ModuleManifest -Path "$pack_dirs\$project_name.psd1" -ModuleVersion $project_version -WhatIf
if (-not $?) { exit 1 }


if (-not $install) { exit 0 }


$install_path = $env:PSModulePath.Split(';')[0]
copy $project_name $install_path -Recurse -Force
