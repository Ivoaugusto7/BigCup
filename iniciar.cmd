@echo off
REM Inicia o BigCup: backend (http://localhost:5000) e frontend (http://localhost:3000)
REM Uso: clique duas vezes neste arquivo ou rode "iniciar.cmd" no terminal.
REM Para parar: feche as janelas "BigCup Backend" e "BigCup Frontend".

cd /d "%~dp0"

where dotnet >nul 2>nul || (echo [ERRO] .NET SDK nao encontrado. Instale o .NET 10 SDK. & pause & exit /b 1)
where npm >nul 2>nul || (echo [ERRO] Node.js/npm nao encontrado. Instale o Node.js. & pause & exit /b 1)

if not exist "frontend\node_modules\vite" (
    echo Instalando dependencias do frontend...
    pushd frontend
    call npm install || (popd & echo [ERRO] Falha no npm install. & pause & exit /b 1)
    popd
)

echo Iniciando backend em http://localhost:5000 ...
start "BigCup Backend" cmd /k "cd /d "%~dp0backend" && set "ASPNETCORE_ENVIRONMENT=Development" && dotnet run --project backend.csproj --urls http://localhost:5000"

echo Iniciando frontend em http://localhost:3000 ...
start "BigCup Frontend" cmd /k "cd /d "%~dp0frontend" && npm run dev"

echo Aguardando o frontend responder...
powershell -NoProfile -Command "for($i=0;$i -lt 90;$i++){try{Invoke-WebRequest http://localhost:3000 -UseBasicParsing -TimeoutSec 2 | Out-Null; exit 0}catch{Start-Sleep 1}}; exit 1"
if errorlevel 1 (
    echo [AVISO] O frontend demorou para responder. Abra http://localhost:3000 manualmente.
) else (
    start "" http://localhost:3000
)

echo.
echo BigCup rodando:
echo   App:     http://localhost:3000
echo   API:     http://localhost:5000
echo   Swagger: http://localhost:5000/swagger
echo.
echo Para parar, feche as janelas "BigCup Backend" e "BigCup Frontend".
