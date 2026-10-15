[CmdletBinding()]
param(
    [string]$EditorPath = 'E:/Unity/Editors/2022.3.23f1/Editor/Unity.exe'
)

$ErrorActionPreference = 'Stop'
$projectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$scenePath = Join-Path $projectPath 'Assets/MiniGame/Scenes/InspectionApartment.unity'
$artifactPath = [IO.Path]::GetFullPath((Join-Path $projectPath '../Artifacts/Unity'))
foreach ($requiredPath in @($EditorPath, $scenePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required file not found: $requiredPath"
    }
}
$expectedVersion = (Get-Content -LiteralPath (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt'))[0].Split(':')[1].Trim()
$actualVersion = (Get-Item -LiteralPath $EditorPath).VersionInfo.ProductVersion.Split('_')[0]
if ($actualVersion -ne $expectedVersion) {
    throw "Editor version mismatch: expected $expectedVersion, found $actualVersion"
}
$busyEditor = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.Replace('\', '/').Contains($projectPath.Replace('\', '/'))
}
if ($busyEditor) {
    throw 'This project already has a running Editor. Close it before using this launcher.'
}
New-Item -ItemType Directory -Force -Path $artifactPath | Out-Null
$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = [IO.Path]::GetFullPath($EditorPath)
$startInfo.UseShellExecute = $false
$startInfo.WorkingDirectory = $projectPath
$startInfo.Arguments = '-projectPath "{0}" -executeMethod MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.OpenPrototype -logFile "{1}"' -f $projectPath, (Join-Path $artifactPath 'editor.log')
if ([string]::IsNullOrWhiteSpace($startInfo.EnvironmentVariables['ALLUSERSPROFILE'])) {
    $commonDataPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::CommonApplicationData)
    if ([string]::IsNullOrWhiteSpace($commonDataPath) -or -not (Test-Path -LiteralPath $commonDataPath -PathType Container)) {
        throw 'Cannot locate CommonApplicationData for Unity Package Manager.'
    }
    $startInfo.EnvironmentVariables['ALLUSERSPROFILE'] = $commonDataPath
}
$editorProcess = [Diagnostics.Process]::Start($startInfo)
[pscustomobject]@{
    ProcessId = $editorProcess.Id
    Project = $projectPath
    Scene = $scenePath
    Log = Join-Path $artifactPath 'editor.log'
    AllUsersProfile = $startInfo.EnvironmentVariables['ALLUSERSPROFILE']
}
