$ErrorActionPreference = 'Stop'
$researchRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$managed = Join-Path $researchRoot 'Analysis\Inputs\GameCopy\NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame\Managed'
$resolve = [System.ResolveEventHandler] {
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]::new($eventArgs.Name)).Name
    $dependencyPath = Join-Path $managed ($name + '.dll')
    if (Test-Path -LiteralPath $dependencyPath) {
        return [Reflection.Assembly]::LoadFrom($dependencyPath)
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolve)
$asm = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'YarnSpinner.dll'))
$yarnPath = Join-Path $researchRoot 'RecoveredUnityProject\ExportedProject\Assets\MonoBehaviour\_NoImNotAHuman.asset'
$line = (Get-Content -LiteralPath $yarnPath)[14]
if (-not $line.StartsWith('  compiledYarnProgram:')) { throw 'Expected compiledYarnProgram at YAML line 15.' }
$bytes = [Convert]::FromHexString($line.Substring($line.IndexOf(':') + 1).Trim())
$parser = $asm.GetType('Yarn.Program').GetProperty('Parser').GetValue($null)
$program = $parser.ParseFrom([byte[]]$bytes)
$output = Join-Path $researchRoot 'Analysis\Gameplay'
[IO.File]::WriteAllBytes((Join-Path $output 'yarn-program.bytes'), $bytes)
[IO.File]::WriteAllText((Join-Path $output 'yarn-program.json'), $program.ToString(), [Text.UTF8Encoding]::new($false))
[PSCustomObject]@{
    Bytes = $bytes.Length
    Nodes = $program.Nodes.Count
    InitialValues = $program.InitialValues.Count
    DefaultOpcode = [Enum]::GetName($asm.GetType('Yarn.Instruction+Types+OpCode'), 0)
    SHA256 = (Get-FileHash -LiteralPath (Join-Path $output 'yarn-program.bytes') -Algorithm SHA256).Hash
} | ConvertTo-Json

