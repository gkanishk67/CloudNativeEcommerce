@echo off
setlocal

echo ==========================================
echo Building and pushing Docker images to ACR
echo ==========================================

set ACR=cloudnativeecommerceacr67.azurecr.io

echo.
echo Logging in to Azure Container Registry...
az acr login --name cloudnativeecommerceacr67

if errorlevel 1 (
echo ERROR: ACR login failed.
exit /b 1
)

echo.
echo Building ApiGateway...
docker build -t %ACR%/cloudnativeecommerce-apigateway:latest -f src\ApiGateway\Dockerfile .

if errorlevel 1 (
echo ERROR: ApiGateway build failed.
exit /b 1
)

echo.
echo Building IdentityService...
docker build -t %ACR%/cloudnativeecommerce-identityservice:latest -f src\IdentityService\Dockerfile .

if errorlevel 1 (
echo ERROR: IdentityService build failed.
exit /b 1
)

echo.
echo Building ProductService...
docker build -t %ACR%/cloudnativeecommerce-productservice:latest -f src\ProductService\Dockerfile .

if errorlevel 1 (
echo ERROR: ProductService build failed.
exit /b 1
)

echo.
echo Building OrderService...
docker build -t %ACR%/cloudnativeecommerce-orderservice:latest -f src\OrderService\Dockerfile .

if errorlevel 1 (
echo ERROR: OrderService build failed.
exit /b 1
)

echo.
echo ==========================================
echo Pushing images to ACR
echo ==========================================

docker push %ACR%/cloudnativeecommerce-apigateway:latest

if errorlevel 1 exit /b 1

docker push %ACR%/cloudnativeecommerce-identityservice:latest

if errorlevel 1 exit /b 1

docker push %ACR%/cloudnativeecommerce-productservice:latest

if errorlevel 1 exit /b 1

docker push %ACR%/cloudnativeecommerce-orderservice:latest

if errorlevel 1 exit /b 1

echo.
echo ==========================================
echo All images built and pushed successfully.
echo ==========================================

endlocal
