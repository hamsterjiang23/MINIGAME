[CmdletBinding()]
param(
    [string]$EditorPath = 'E:/Unity/Editors/2022.3.23f1/Editor/Unity.exe',
    [switch]$SkipPrepare,
    [switch]$BuildPlayer
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifactRoot = Join-Path $repositoryRoot 'Artifacts/Unity/TerminalDemo'
$offlineProject = Join-Path $repositoryRoot 'Artifacts/Unity/OfflineProject'
New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
if (-not $SkipPrepare) { & (Join-Path $PSScriptRoot 'Invoke-OfflineValidation.ps1') -Action Prepare -EditorPath $EditorPath }
& (Join-Path $PSScriptRoot 'Invoke-OfflineValidation.ps1') -Action Create -SkipPrepare -EditorPath $EditorPath

function Invoke-TerminalEditor {
    param([string]$Name, [string]$Platform, [string]$Filter = '', [switch]$Graphics)
    $resultPath = Join-Path $artifactRoot ($Name + '-results.xml')
    $logPath = Join-Path $artifactRoot ($Name + '.log')
    if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
    $arguments = @('-batchmode','-noUpm','-projectPath',('"' + $offlineProject + '"'),
        '-runTests','-testPlatform',$Platform,'-testResults',('"' + $resultPath + '"'),'-logFile',('"' + $logPath + '"'))
    if ($Filter) { $arguments += @('-testFilter', $Filter) }
    if (-not $Graphics) { $arguments += '-nographics' }
    $priorRoot = $env:MINIGAME_REPOSITORY_ROOT
    $priorCapture = $env:MINIGAME_CAPTURE_DIR
    try {
        $env:MINIGAME_REPOSITORY_ROOT = $repositoryRoot
        if ($Graphics) { $env:MINIGAME_CAPTURE_DIR = Join-Path $artifactRoot 'Visuals' }
        $process = Start-Process -FilePath $EditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $started = Get-Date
        while (-not $process.WaitForExit(30000)) {
            $process.Refresh()
            Write-Host "$Name running PID=$($process.Id), memory=$([math]::Round($process.WorkingSet64/1MB))MiB"
            if (((Get-Date) - $started).TotalSeconds -gt 300) {
                Stop-Process -Id $process.Id
                throw "$Name exceeded its five-minute validation bound; only the launched isolated Editor was stopped."
            }
        }
        if (-not (Test-Path -LiteralPath $resultPath)) { throw "$Name produced no result XML; see $logPath" }
        [xml]$result = Get-Content -LiteralPath $resultPath
        if ($process.ExitCode -ne 0 -or $result.'test-run'.result -ne 'Passed' -or [int]$result.'test-run'.passed -lt 1) {
            throw "$Name failed; see $resultPath and $logPath"
        }
        return [pscustomobject]@{ name=$Name; passed=[int]$result.'test-run'.passed; failed=[int]$result.'test-run'.failed; xml=$resultPath; log=$logPath; graphics=[bool]$Graphics }
    }
    finally { $env:MINIGAME_REPOSITORY_ROOT = $priorRoot; $env:MINIGAME_CAPTURE_DIR = $priorCapture }
}

$checks = @()
$checks += Invoke-TerminalEditor 'terminal-editmode' 'EditMode' 'MiniGame.UnityHost.Tests.DialoguePackageImporterTests'
$checks += Invoke-TerminalEditor 'terminal-playmode' 'PlayMode' -Graphics
if ($BuildPlayer) { & (Join-Path $PSScriptRoot 'Invoke-OfflineValidation.ps1') -Action Build -SkipPrepare -EditorPath $EditorPath }
$report = [pscustomobject]@{
    checkedAtUtc = [DateTime]::UtcNow.ToString('o')
    editor = $EditorPath
    project = $offlineProject
    checks = $checks
    playerBuildRequested = [bool]$BuildPlayer
    scope = 'Real isolated Unity Editor import, native Yarn and UI tests; HTML browser visual testing is separate.'
}
$report | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $artifactRoot 'verification.json') -Encoding utf8
$report | ConvertTo-Json -Depth 7
