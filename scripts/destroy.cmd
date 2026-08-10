@echo off
setlocal

echo ==========================================
echo WARNING: DESTROYING AZURE INFRASTRUCTURE
echo ==========================================

echo.
echo This will destroy the Terraform-managed
echo Azure infrastructure, including:
echo.
echo   - AKS
echo   - ACR
echo   - Resource Group
echo   - AKS-related resources
echo   - ACR role assignment
echo.
echo Kubernetes workloads inside AKS will also
echo disappear when AKS is destroyed.
echo.

set /p CONFIRM="Type DESTROY to continue: "

if /I not "%CONFIRM%"=="DESTROY" (
echo.
echo Cancelled.
exit /b 0
)

echo.
echo Running Terraform destroy...

cd /d "%~dp0..\terraform"

terraform destroy

if errorlevel 1 (
echo.
echo ERROR: Terraform destroy failed.
exit /b 1
)

echo.
echo ==========================================
echo Azure infrastructure destroyed.
echo ==========================================

endlocal
