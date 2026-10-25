param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$taskRepo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $taskRepo 'Artifacts/Audio/SupervisorSpeechSource' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
Add-Type -AssemblyName System.Speech
$taskSynth = New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $taskSynth.SelectVoice('Microsoft Kangkang')
    $taskSynth.Rate = 0
    $taskSynth.Volume = 100
    $taskData = Get-Content -LiteralPath (Join-Path $taskRepo 'UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor/Dialogue.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $taskLines = @($taskData.conversations | ForEach-Object { $_.lines })
    $taskManifest = @()
    $taskIndices = @(0, 1, 3, 6, 8, 12, 17, 24, 30, 36)
    for ($taskIndex = 0; $taskIndex -lt $taskIndices.Count; $taskIndex++) {
        $taskLine = $taskLines[$taskIndices[$taskIndex]]
        $taskName = 'speech-{0:d2}.wav' -f ($taskIndex + 1)
        $taskOutput = Join-Path $OutputDirectory $taskName
        $taskSynth.SetOutputToWaveFile($taskOutput)
        $taskSynth.Speak($taskLine.text)
        $taskSynth.SetOutputToNull()
        $taskManifest += [pscustomobject]@{ file = $taskName; sourceId = $taskLine.id; text = $taskLine.text }
    }
    [pscustomobject]@{ provider = 'Windows installed offline speech synthesis'; voice = $taskSynth.Voice.Name; lines = $taskManifest } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'speech-manifest.json') -Encoding UTF8
    Write-Output ('Rendered {0} spoken phrases.' -f $taskManifest.Count)
}
finally { $taskSynth.Dispose() }
