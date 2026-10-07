param([string]$Dotnet='dotnet')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$stage=Join-Path $env:TEMP ('AulaMarkdown-build-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
try {
 foreach($item in @('src','tests','Directory.Build.props','NuGet.Config','README.md','GUIDA.md','GUIDA.en.md','THIRD-PARTY-NOTICES.md','licenses')) {
  $sourceItem=Join-Path $projectRoot $item
  if(Test-Path -LiteralPath $sourceItem -PathType Container) {
   Get-ChildItem -LiteralPath $sourceItem -Recurse -File | ForEach-Object {
    $relative=[IO.Path]::GetRelativePath($projectRoot,$_.FullName)
    if(($relative -split '[\\/]') -notcontains 'bin' -and ($relative -split '[\\/]') -notcontains 'obj') {
     $target=Join-Path $stage $relative
     New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
     Copy-Item -LiteralPath $_.FullName -Destination $target
    }
   }
  } else { Copy-Item -LiteralPath $sourceItem -Destination $stage }
 }
 & $Dotnet run --project (Join-Path $stage 'tests\AulaMarkdown.Tests') -c Release
 if($LASTEXITCODE -ne 0){throw 'Verifiche non riuscite'}
 $published=Join-Path $stage 'published'
 & $Dotnet publish (Join-Path $stage 'src\AulaMarkdown\AulaMarkdown.csproj') -c Release -r win-x64 --self-contained true -o $published
 if($LASTEXITCODE -ne 0){throw 'Compilazione non riuscita'}
 Copy-Item -LiteralPath (Join-Path $projectRoot 'licenses') -Destination $published -Recurse
 Copy-Item -LiteralPath (Join-Path $projectRoot 'THIRD-PARTY-NOTICES.md') -Destination $published
 $dist=Join-Path $projectRoot 'dist'
 New-Item -ItemType Directory -Path $dist -Force | Out-Null
 [xml]$project=Get-Content -LiteralPath (Join-Path $projectRoot 'src\AulaMarkdown\AulaMarkdown.csproj')
 $packageName='AulaMarkdown-'+$project.Project.PropertyGroup.Version+'-win-x64'
 $destination=Join-Path $dist $packageName
 New-Item -ItemType Directory -Path $destination -Force | Out-Null
 Get-ChildItem -LiteralPath $published | Copy-Item -Destination $destination -Recurse -Force
 Compress-Archive -Path (Join-Path $published '*') -DestinationPath (Join-Path $dist ($packageName+'.zip')) -Force
 Write-Output ('Programma pronto in '+$destination)
} finally {
 # Only remove this script's freshly created, explicitly checked temporary workspace.
 $resolved=[IO.Path]::GetFullPath($stage)
 $tempPrefix=[IO.Path]::GetFullPath($env:TEMP).TrimEnd('\')+'\'
 if($resolved.StartsWith($tempPrefix,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path $resolved -Leaf) -match '^AulaMarkdown-build-[a-f0-9]{32}$') {
  Remove-Item -LiteralPath $resolved -Recurse -Force
 }
}
