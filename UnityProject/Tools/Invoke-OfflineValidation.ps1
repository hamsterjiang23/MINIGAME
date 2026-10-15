[CmdletBinding()]
param(
    [string]$EditorPath = 'E:/Unity/Editors/2022.3.23f1/Editor/Unity.exe',
    [string]$CachedPackagesRoot = (Join-Path $env:LOCALAPPDATA 'Unity/cache/packages/packages.unity.com'),
    [ValidateSet('Prepare','Create','PlayMode','Build','All')][string]$Action = 'All',
    [switch]$SkipPrepare,
    [switch]$CaptureVisuals
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$mainProject = Join-Path $repositoryRoot 'UnityProject'
$artifactRoot = Join-Path $repositoryRoot 'Artifacts/Unity'
$offlineProject = [IO.Path]::GetFullPath((Join-Path $artifactRoot 'OfflineProject'))
if (-not $offlineProject.StartsWith([IO.Path]::GetFullPath($artifactRoot) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Offline project must remain inside this repository Artifacts/Unity directory.'
}
if (-not (Test-Path -LiteralPath $EditorPath)) { throw "Editor not found: $EditorPath" }
$busyEditor = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq 'Unity.exe' -and $_.CommandLine -and $_.CommandLine.Replace('\','/').Contains($offlineProject.Replace('\','/'))
}
if ($busyEditor) { throw 'The isolated offline project already has a running Editor. Wait for it to finish.' }
New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
$manifest = Get-Content -LiteralPath (Join-Path $mainProject 'Packages/manifest.json') -Raw | ConvertFrom-Json

function Get-PackageCopy {
    param([string]$Id, [string]$Source)
    $metadata = Get-Content -LiteralPath (Join-Path $Source 'package.json') -Raw | ConvertFrom-Json
    $expected = $manifest.dependencies.$Id
    if ($metadata.name -ne $Id) { throw "Package name mismatch at $Source" }
    if ($expected -and -not $expected.StartsWith('file:') -and $metadata.version -ne $expected) {
        throw "Version mismatch: $Id cache=$($metadata.version), manifest=$expected"
    }
    return [pscustomobject]@{ id=$Id; version=$metadata.version; source=[IO.Path]::GetFullPath($Source) }
}

function Copy-PackageSources {
    param([string]$Source, [string]$Destination)
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | Where-Object {
        $_.Name -notin @('.git','.github','Tests','Tests.meta','Samples~','Documentation~','bin','obj')
    } | Copy-Item -Destination $Destination -Recurse -Force
}

function Prepare-OfflineProject {
    $editorData = Join-Path (Split-Path $EditorPath -Parent) 'Data'
    $packages = [ordered]@{
        Core = Get-PackageCopy 'com.hamster.minigame.core' (Join-Path $repositoryRoot 'src/MiniGame.Core')
        Presentation = Get-PackageCopy 'com.hamster.minigame.presentation' (Join-Path $repositoryRoot 'src/MiniGame.Presentation')
        YarnSpinner = Get-PackageCopy 'dev.yarnspinner.unity' (Join-Path $repositoryRoot 'references/YarnSpinner-Unity')
        uGUI = Get-PackageCopy 'com.unity.ugui' (Join-Path $editorData 'Resources/PackageManager/BuiltInPackages/com.unity.ugui')
        NewtonsoftJson = Get-PackageCopy 'com.unity.nuget.newtonsoft-json' (Join-Path $CachedPackagesRoot ('com.unity.nuget.newtonsoft-json@' + $manifest.dependencies.'com.unity.nuget.newtonsoft-json'))
        TextMeshPro = Get-PackageCopy 'com.unity.textmeshpro' (Join-Path $CachedPackagesRoot ('com.unity.textmeshpro@' + $manifest.dependencies.'com.unity.textmeshpro'))
        TestFramework = Get-PackageCopy 'com.unity.test-framework' (Join-Path $CachedPackagesRoot ('com.unity.test-framework@' + $manifest.dependencies.'com.unity.test-framework'))
    }
    $testMetadata = Get-Content -LiteralPath (Join-Path $packages.TestFramework.source 'package.json') -Raw | ConvertFrom-Json
    $nunitVersion = $testMetadata.dependencies.'com.unity.ext.nunit'
    $packages.NUnit = Get-PackageCopy 'com.unity.ext.nunit' (Join-Path $CachedPackagesRoot ('com.unity.ext.nunit@' + $nunitVersion))
    if ($packages.YarnSpinner.version -ne '3.2.8') { throw 'The pinned Yarn source must remain version 3.2.8.' }
    foreach ($child in @('Assets','ProjectSettings','Packages')) {
        $target = [IO.Path]::GetFullPath((Join-Path $offlineProject $child))
        if (-not $target.StartsWith($offlineProject + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe offline cleanup target.' }
        if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
    }
    New-Item -ItemType Directory -Force -Path $offlineProject,(Join-Path $offlineProject 'Packages') | Out-Null
    Copy-Item -LiteralPath (Join-Path $mainProject 'Assets'),(Join-Path $mainProject 'ProjectSettings') -Destination $offlineProject -Recurse -Force
    foreach ($entry in $packages.GetEnumerator()) {
        Copy-PackageSources $entry.Value.source (Join-Path $offlineProject ('Assets/_OfflinePackages/' + $entry.Key))
    }
    foreach ($relative in @('Runtime/YarnSpinner.Unity.asmdef','Editor/YarnSpinner.Unity.Editor.asmdef')) {
        $path = Join-Path $offlineProject ('Assets/_OfflinePackages/YarnSpinner/' + $relative)
        $assembly = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
        $assembly.references = @($assembly.references | Where-Object {
            $_ -notin @('Unity.Addressables','Unity.ResourceManager','Unity.InputSystem','Unity.Localization','UniTask','UniTask.Addressables','Unity.Addressables.Editor','Unity.Localization.Editor')
        })
        # No UPM package-version symbols are available. Preserve real TMP code via the Unity-version symbol.
        $assembly.versionDefines = @($assembly.versionDefines | Where-Object { $_.define -ne 'USE_TMP' }) +
            [pscustomobject]@{ name='Unity'; expression='2022.3'; define='USE_TMP' }
        $assembly | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path -Encoding utf8
    }
    '{"dependencies":{}}' | Set-Content -LiteralPath (Join-Path $offlineProject 'Packages/manifest.json') -Encoding utf8
    [pscustomobject]@{
        project=$offlineProject; packages=$packages; flags='-noUpm';
        copiedAsmdefChanges=@('Yarn runtime/editor optional references removed','USE_TMP enabled using Unity version');
        scope='Isolated Assets copies with original licenses and metas; not normal UPM resolution success.'
    } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifactRoot 'offline-project-provenance.json') -Encoding utf8
}

function Publish-GeneratedAssets {
    $offlineAssets = Join-Path $offlineProject 'Assets/MiniGame'
    $mainAssets = Join-Path $mainProject 'Assets/MiniGame'
    foreach ($folder in @('Scenes','Generated','Content','ImportedDialogue')) {
        $source = Join-Path $offlineAssets $folder
        if (Test-Path -LiteralPath $source) { Copy-Item -LiteralPath $source -Destination $mainAssets -Recurse -Force }
    }
    $offlineResources = Join-Path $offlineProject 'Assets/Resources'
    $mainResources = Join-Path $mainProject 'Assets/Resources'
    if (Test-Path -LiteralPath (Join-Path $offlineResources 'MiniGame')) {
        New-Item -ItemType Directory -Force -Path $mainResources | Out-Null
        Copy-Item -LiteralPath (Join-Path $offlineResources 'MiniGame') -Destination $mainResources -Recurse -Force
        foreach ($folderMeta in @('Assets/Resources.meta','Assets/Resources/MiniGame.meta')) {
            $sourceMeta = Join-Path $offlineProject $folderMeta
            $targetMeta = Join-Path $mainProject $folderMeta
            if ((Test-Path -LiteralPath $sourceMeta) -and -not (Test-Path -LiteralPath $targetMeta)) {
                Copy-Item -LiteralPath $sourceMeta -Destination $targetMeta
            }
        }
    }
    # MonoBehaviour scene references must retain the same GUIDs in the normal project.
    Get-ChildItem -LiteralPath $offlineAssets -Recurse -File -Filter '*.meta' | ForEach-Object {
        $relative = $_.FullName.Substring($offlineAssets.Length + 1)
        $destination = Join-Path $mainAssets $relative
        New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
    }
    Copy-Item -LiteralPath (Join-Path $offlineProject 'ProjectSettings/EditorBuildSettings.asset') -Destination (Join-Path $mainProject 'ProjectSettings/EditorBuildSettings.asset') -Force
}

function Invoke-OfflineEditor {
    param([string]$Name, [string[]]$ExtraArguments)
    $log = Join-Path $artifactRoot ($Name + '.log')
    $arguments = @('-batchmode','-noUpm','-projectPath',('"' + $offlineProject + '"'),'-logFile',('"' + $log + '"')) + $ExtraArguments
    if (-not ($CaptureVisuals -and $Name -eq 'offline-playmode')) { $arguments += '-nographics' }
    $priorRootSetting = $env:MINIGAME_REPOSITORY_ROOT
    $priorCaptureSetting = $env:MINIGAME_CAPTURE_DIR
    try {
        $env:MINIGAME_REPOSITORY_ROOT = $repositoryRoot
        if ($CaptureVisuals -and $Name -eq 'offline-playmode') { $env:MINIGAME_CAPTURE_DIR = Join-Path $artifactRoot 'Visuals' }
        $process = Start-Process -FilePath $EditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        while (-not $process.WaitForExit(30000)) {
            $process.Refresh()
            Write-Host "$Name running (PID $($process.Id), memory $([math]::Round($process.WorkingSet64/1MB)) MiB)."
        }
        if ($process.ExitCode -ne 0) { throw "$Name failed with Editor exit $($process.ExitCode). Inspect $log" }
        Write-Host "$Name passed. Log: $log"
    }
    finally { $env:MINIGAME_REPOSITORY_ROOT = $priorRootSetting; $env:MINIGAME_CAPTURE_DIR = $priorCaptureSetting }
}

if (-not $SkipPrepare) { Prepare-OfflineProject }
if ($Action -eq 'Prepare') { Write-Host "Prepared isolated project: $offlineProject"; exit 0 }
if ($Action -in @('Create','All')) {
    Invoke-OfflineEditor 'offline-create' @('-quit','-executeMethod','MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.CreateAndValidate')
    Publish-GeneratedAssets
}
if ($Action -in @('PlayMode','All')) {
    if ($Action -eq 'PlayMode') {
        Invoke-OfflineEditor 'offline-create' @('-quit','-executeMethod','MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.CreateAndValidate')
        Publish-GeneratedAssets
    }
    Invoke-OfflineEditor 'offline-playmode' @('-runTests','-testPlatform','PlayMode','-testResults',('"' + (Join-Path $artifactRoot 'offline-playmode-results.xml') + '"'))
    [xml]$tests = Get-Content -LiteralPath (Join-Path $artifactRoot 'offline-playmode-results.xml')
    if ($tests.'test-run'.result -ne 'Passed' -or [int]$tests.'test-run'.passed -lt 4) { throw 'PlayMode result must report at least four passed tests and no failure.' }
}
if ($Action -in @('Build','All')) {
    Invoke-OfflineEditor 'offline-build' @('-quit','-executeMethod','MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.BuildWindows')
    Publish-GeneratedAssets
    $build = Get-Content -LiteralPath (Join-Path $artifactRoot 'unity-windows-build.json') -Raw | ConvertFrom-Json
    if ($build.result -ne 'Succeeded' -or -not (Test-Path -LiteralPath (Join-Path $repositoryRoot 'Builds/MiniGame/MiniGame.exe'))) { throw 'Windows build artifact is missing or did not succeed.' }
}
