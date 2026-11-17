# Inicia o BigCup no terminal atual, sem abrir janelas:
#   backend  -> http://localhost:5000  (Swagger em /swagger)
#   frontend -> http://localhost:3000
#
# Uso (na raiz do projeto):
#   .\iniciar.ps1                 # sobe tudo e abre o navegador
#   .\iniciar.ps1 -SemNavegador   # sobe tudo sem abrir o navegador
#
# Para parar os dois: Ctrl+C no terminal.

param(
    [switch]$SemNavegador
)

$ErrorActionPreference = 'Stop'
$raiz = $PSScriptRoot
$backendPort = 5000
$frontendPort = 3000

function Write-Info($msg) { Write-Host "[BigCup] $msg" -ForegroundColor Cyan }
function Write-Falha($msg) { Write-Host "[BigCup] ERRO: $msg" -ForegroundColor Red }

function Test-PortaEmUso($porta) {
    [bool](Get-NetTCPConnection -State Listen -LocalPort $porta -ErrorAction SilentlyContinue)
}

function Wait-Url($url, $segundos) {
    for ($i = 0; $i -lt $segundos; $i++) {
        try { Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 10 | Out-Null; return $true } catch { Start-Sleep 1 }
    }
    return $false
}

function Stop-Arvore($proc) {
    if ($proc -and -not $proc.HasExited) {
        & taskkill.exe /PID $proc.Id /T /F 2>$null | Out-Null
    }
}

# --- Pre-requisitos ---
foreach ($cmd in 'dotnet', 'node', 'npm') {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Falha "'$cmd' nao encontrado. Instale o .NET 10 SDK e o Node.js."
        exit 1
    }
}

foreach ($porta in $backendPort, $frontendPort) {
    if (Test-PortaEmUso $porta) {
        Write-Falha "a porta $porta ja esta em uso. Feche a outra instancia do BigCup e tente de novo."
        exit 1
    }
}

$vite = Join-Path $raiz 'frontend\node_modules\vite\bin\vite.js'
if (-not (Test-Path $vite)) {
    Write-Info 'Instalando dependencias do frontend (npm install)...'
    Push-Location (Join-Path $raiz 'frontend')
    try {
        & npm install
        if ($LASTEXITCODE -ne 0) { Write-Falha 'npm install falhou.'; exit 1 }
    } finally { Pop-Location }
}

# --- Sobe os dois servidores no mesmo terminal ---
$backend = $null
$frontend = $null
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'

    Write-Info "Iniciando backend em http://localhost:$backendPort ..."
    $backend = Start-Process dotnet -NoNewWindow -PassThru `
        -WorkingDirectory (Join-Path $raiz 'backend') `
        -ArgumentList 'run', '--project', 'backend.csproj', '--urls', "http://localhost:$backendPort"

    # node direto (sem npm.cmd) para o Ctrl+C nao perguntar "Deseja finalizar o arquivo em lotes?"
    Write-Info "Iniciando frontend em http://localhost:$frontendPort ..."
    $frontend = Start-Process node -NoNewWindow -PassThru `
        -WorkingDirectory (Join-Path $raiz 'frontend') `
        -ArgumentList "`"$vite`""

    # 127.0.0.1: o Vite escuta so em IPv4 e "localhost" tenta IPv6 primeiro (lento no Windows)
    # Guarda os handles para conseguir ler o ExitCode depois que o processo terminar
    $null = $backend.Handle
    $null = $frontend.Handle

    $backendOk = Wait-Url "http://127.0.0.1:$backendPort/" 120
    $frontendOk = Wait-Url "http://127.0.0.1:$frontendPort/" 60

    if (-not $backendOk) { Write-Falha 'o backend nao respondeu. Veja as mensagens acima.' }
    if (-not $frontendOk) { Write-Falha 'o frontend nao respondeu. Veja as mensagens acima.' }

    if ($backendOk -and $frontendOk) {
        Write-Host ''
        Write-Info 'BigCup rodando:'
        Write-Host "    App:     http://localhost:$frontendPort"
        Write-Host "    API:     http://localhost:$backendPort"
        Write-Host "    Swagger: http://localhost:$backendPort/swagger"
        Write-Host ''
        Write-Info 'Pressione Ctrl+C para parar.'
        if (-not $SemNavegador) { Start-Process "http://localhost:$frontendPort" }
    }

    # Mantem o terminal preso enquanto os dois estiverem vivos
    while (-not $backend.HasExited -and -not $frontend.HasExited) {
        Start-Sleep -Milliseconds 500
    }

    if ($backend.HasExited) { Write-Falha "o backend parou (codigo $($backend.ExitCode))." }
    if ($frontend.HasExited) { Write-Falha "o frontend parou (codigo $($frontend.ExitCode))." }
}
finally {
    # Executado tambem no Ctrl+C: encerra o que ainda estiver rodando
    Stop-Arvore $backend
    Stop-Arvore $frontend
    Write-Info 'Servidores encerrados.'
}
