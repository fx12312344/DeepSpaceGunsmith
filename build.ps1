param(
    [string]$GameManaged = $env:PROBABLY_STOLEN_MANAGED,
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($GameManaged)) {
    throw 'Supply -GameManaged with your own Probably Stolen_Data/Managed directory, or set PROBABLY_STOLEN_MANAGED.'
}
$managed = (Resolve-Path -LiteralPath $GameManaged).Path
if (!(Test-Path -LiteralPath (Join-Path $managed 'Assembly-CSharp.dll'))) { throw 'GameManaged is not a valid game Managed directory.' }
& dotnet build (Join-Path $root 'Expansion.csproj') -c $Configuration --nologo "-p:GameManaged=$managed"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
[xml]$manifest = Get-Content -LiteralPath (Join-Path $root 'manifest.xml') -Raw -Encoding UTF8
$version = [string]$manifest.Manifest.ModVersion
if ($version -notmatch '^[A-Za-z0-9._-]+$') { throw 'Invalid package version.' }
$out = Join-Path $root "dist/DeepSpaceGunsmith-$version/DeepSpaceGunsmith"
$textures = Join-Path $out 'Textures/Items'
New-Item -ItemType Directory -Force -Path $textures | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'manifest.xml') -Destination $out -Force
Copy-Item -LiteralPath (Join-Path $root "bin/$Configuration/DeepSpaceGunsmith.dll") -Destination $out -Force
Get-ChildItem -LiteralPath (Join-Path $root 'assets') -File -Filter '*.png' | Copy-Item -Destination $textures -Force
foreach ($folder in @('Localization','Books')) {
    $target = Join-Path $out $folder
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $root $folder) -File | Copy-Item -Destination $target -Force
}
$paper = Join-Path $out 'Textures/Paper'
New-Item -ItemType Directory -Force -Path $paper | Out-Null
Get-ChildItem -LiteralPath (Join-Path $root 'Paper') -File -Filter '*.png' | Copy-Item -Destination $paper -Force
Write-Output "Package: $out"
