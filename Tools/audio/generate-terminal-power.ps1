param([string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)

$ErrorActionPreference = 'Stop'
# Original procedural relay/CRT sounds. PCM16 mono 44.1 kHz, no external samples.
function Write-TerminalSound([string] $Path, [bool] $Starting) {
    $sampleRate = 44100
    $seconds = if ($Starting) { 0.85 } else { 0.72 }
    $sampleCount = [int]($seconds * $sampleRate)
    $stream = [System.IO.File]::Create($Path)
    $writer = [System.IO.BinaryWriter]::new($stream)
    $noise = [System.Random]::new(812)
    $phase = 0.0
    try {
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
        $writer.Write([int](36 + 2 * $sampleCount))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVEfmt '))
        $writer.Write([int]16); $writer.Write([short]1); $writer.Write([short]1)
        $writer.Write([int]$sampleRate); $writer.Write([int](2 * $sampleRate))
        $writer.Write([short]2); $writer.Write([short]16)
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
        $writer.Write([int](2 * $sampleCount))
        for ($index = 0; $index -lt $sampleCount; $index++) {
            $time = $index / [double]$sampleRate
            $progress = $time / $seconds
            $frequency = if ($Starting) { 160 + 730 * (1 - [Math]::Exp(-6 * $progress)) } else { 880 * [Math]::Exp(-4.6 * $progress) + 38 }
            $phase += 2 * [Math]::PI * $frequency / $sampleRate
            $envelope = [Math]::Min(1, $time / 0.018) * [Math]::Pow(1 - $progress, 1.8)
            $relay = ($noise.NextDouble() * 2 - 1) * [Math]::Exp(-$time * 180) * 0.5
            $hiss = ($noise.NextDouble() * 2 - 1) * $envelope * 0.045
            $tone = ([Math]::Sin($phase) * 0.12 + [Math]::Sin($phase * 2.003) * 0.035) * $envelope
            $value = [Math]::Clamp($relay + $hiss + $tone, -1, 1)
            $writer.Write([short][Math]::Round($value * 32767))
        }
    }
    finally { $writer.Dispose() }
}

$soundFolder = Join-Path $RepositoryRoot 'UnityProject/Assets/MiniGame/Audio/SFX'
Write-TerminalSound (Join-Path $soundFolder 'TerminalPowerOn.wav') $true
Write-TerminalSound (Join-Path $soundFolder 'TerminalPowerOff.wav') $false
$sourceFolder = Join-Path $RepositoryRoot 'Art/Audio/SFX'
[System.IO.Directory]::CreateDirectory($sourceFolder) | Out-Null
Copy-Item -LiteralPath (Join-Path $soundFolder 'TerminalPowerOn.wav') -Destination (Join-Path $sourceFolder 'TerminalPowerOn.wav') -Force
Copy-Item -LiteralPath (Join-Path $soundFolder 'TerminalPowerOff.wav') -Destination (Join-Path $sourceFolder 'TerminalPowerOff.wav') -Force
