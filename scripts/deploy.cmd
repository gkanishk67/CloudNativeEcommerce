@echo off
setlocal

echo ==========================================
echo Deploying CloudNativeEcommerce
echo ==========================================

echo.
echo Getting AKS credentials...

az aks get-credentials ^
--resource-group CloudNativeEcommerce-RG ^
--name cloudnativeecommerce-aks ^
--overwrite-existing

if errorlevel 1 (
echo ERROR: Failed to get AKS credentials.
exit /b 1
)

echo.
echo Checking AKS...

kubectl get nodes

if errorlevel 1 (
echo ERROR: kubectl cannot access AKS.
exit /b 1
)

echo.
echo Creating namespace if it does not exist...

kubectl get namespace cloudnativeecommerce >nul 2>&1

if errorlevel 1 (
kubectl create namespace cloudnativeecommerce
)

echo.
echo Deploying Helm chart...

helm upgrade --install cloudnativeecommerce ^
helm\cloudnativeecommerce ^
--namespace cloudnativeecommerce ^
--create-namespace

if errorlevel 1 (
echo ERROR: Helm deployment failed.
exit /b 1
)

echo.
echo ==========================================
echo Deployment complete.
echo ==========================================

echo.
echo Helm status:

helm status cloudnativeecommerce -n cloudnativeecommerce

echo.
echo Kubernetes deployments:

kubectl get deployments -n cloudnativeecommerce

echo.
echo Kubernetes pods:

kubectl get pods -n cloudnativeecommerce

endlocal
