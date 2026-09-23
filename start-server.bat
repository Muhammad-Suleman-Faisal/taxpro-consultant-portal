@echo off
title TaxPro Consultant Portal Server
echo ========================================================
echo Starting TaxPro Consultant Portal Server on Port 3000...
echo ========================================================
powershell -ExecutionPolicy Bypass -File "%~dp0server.ps1" -Port 3000
pause
