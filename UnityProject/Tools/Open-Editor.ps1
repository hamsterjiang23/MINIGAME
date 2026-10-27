[CmdletBinding()]
param([string]$EditorPath, [switch]$CheckOnly)

$ErrorActionPreference = 'Stop'
$projectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$versionFile = Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt'
$expectedVersion = ((Get-Content -LiteralPath $versionFile)[0] -split ':', 2)[1].Trim()
if ([string]::IsNullOrWhiteSpace($EditorPath)) {
    $editorRoots = @(
        (Join-Path $env:ProgramFiles 'Unity/Hub/Editor'),
        (Join-Path $env:LOCALAPPDATA 'Programs/Unity/Hub/Editor'),
        'E:/Unity/Editors'
    )
    foreach ($root in $editorRoots) {
        $candidate = Join-Path $root "$expectedVersion/Editor/Unity.exe"
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { $EditorPath = $candidate; break }
    }
}
if ([string]::IsNullOrWhiteSpace($EditorPath) -or -not (Test-Path -LiteralPath $EditorPath -PathType Leaf)) {
    throw "Unity $expectedVersion is not installed. Install it with Unity Hub or pass -EditorPath."
}
$actualVersion = (Get-Item -LiteralPath $EditorPath).VersionInfo.ProductVersion.Split('_')[0]
if ($actualVersion -ne $expectedVersion) { throw "Editor version mismatch: expected $expectedVersion, found $actualVersion" }
if ($CheckOnly) {
    [pscustomobject]@{ Project = $projectPath; Editor = $EditorPath; Version = $actualVersion }
    return
}
$openEditor = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.Replace('\', '/').Contains($projectPath.Replace('\', '/'))
} | Select-Object -First 1
if ($openEditor) {
    $window = New-Object -ComObject WScript.Shell
    [void]$window.AppActivate([int]$openEditor.ProcessId)
    return
}
$artifactPath = [IO.Path]::GetFullPath((Join-Path $projectPath '../Artifacts/Unity'))
New-Item -ItemType Directory -Force -Path $artifactPath | Out-Null
$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = [IO.Path]::GetFullPath($EditorPath)
$startInfo.UseShellExecute = $false
$startInfo.WorkingDirectory = $projectPath
$startInfo.Arguments = '-projectPath "{0}" -logFile "{1}"' -f $projectPath, (Join-Path $artifactPath 'editor.log')
if ([string]::IsNullOrWhiteSpace($startInfo.EnvironmentVariables['ALLUSERSPROFILE'])) {
    $startInfo.EnvironmentVariables['ALLUSERSPROFILE'] = [Environment]::GetFolderPath([Environment+SpecialFolder]::CommonApplicationData)
}
[void][Diagnostics.Process]::Start($startInfo)
