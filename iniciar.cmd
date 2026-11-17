@echo off
REM Atalho para iniciar.ps1: sobe backend e frontend no terminal atual, sem abrir janelas.
REM No PowerShell prefira rodar direto:  .\iniciar.ps1   (Ctrl+C para parar)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0iniciar.ps1" %*
