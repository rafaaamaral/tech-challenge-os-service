[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [Parameter(Mandatory)][string]$Suite
)

$ErrorActionPreference = 'Stop'
$reports = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -Recurse)
if ($reports.Count -eq 0) { throw "Nenhum relatório TRX foi gerado para a suíte $Suite." }

$total = 0
$executed = 0
$passed = 0
$failed = 0
foreach ($report in $reports) {
    [xml]$result = Get-Content -LiteralPath $report.FullName -Raw
    $counters = $result.TestRun.ResultSummary.Counters
    $total += [int]$counters.total
    $executed += [int]$counters.executed
    $passed += [int]$counters.passed
    $failed += [int]$counters.failed
}

$coverageFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter 'coverage.cobertura.xml' -Recurse)
$summary = @(
    "## Testes: $Suite"
    ''
    '| Total | Executados | Aprovados | Falhas |'
    '| --- | --- | --- | --- |'
    "| $total | $executed | $passed | $failed |"
)
$seenCoverage = @{}
$validLines = 0
$coveredLines = 0
foreach ($coverageFile in $coverageFiles) {
    # O VSTest pode copiar o mesmo anexo para dois diretórios de resultados.
    $hash = (Get-FileHash -LiteralPath $coverageFile.FullName -Algorithm SHA256).Hash
    if ($seenCoverage.ContainsKey($hash)) { continue }
    $seenCoverage[$hash] = $true
    [xml]$coverage = Get-Content -LiteralPath $coverageFile.FullName -Raw
    $validLines += [int]$coverage.coverage.'lines-valid'
    $coveredLines += [int]$coverage.coverage.'lines-covered'
    $rate = [double]::Parse($coverage.coverage.'line-rate', [System.Globalization.CultureInfo]::InvariantCulture)
    $percentage = $rate.ToString('P2', [System.Globalization.CultureInfo]::InvariantCulture)
    $summary += ''
    $summary += "Cobertura de linhas nesta suíte: $percentage."
}

$summary | Write-Output
if ($env:GITHUB_STEP_SUMMARY) { $summary | Out-File -LiteralPath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8 }
if ($total -eq 0 -or $executed -ne $total -or $passed -ne $total) {
    throw "A suíte $Suite deve executar e aprovar todos os testes selecionados, sem testes ignorados."
}
if ($coverageFiles.Count -eq 0) { throw "Relatório de cobertura ausente na suíte $Suite." }
if ($validLines -eq 0 -or $coveredLines -eq 0) { throw "A suíte $Suite não gerou cobertura válida do OS Service." }
