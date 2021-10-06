# Source: https://stackoverflow.com/a/62060315
# Generate self-signed certificate to be used by IdentityServer.
# When using localhost - API cannot see the IdentityServer from within the docker-compose'd network.
# You have to run this script as Administrator (open Powershell by right click -> Run as Administrator).

$ErrorActionPreference = "Stop"

$rootCN = "IdentityServerDockerRootCert"
$identityServerCNs = "shopping.idp", "localhost"
$shoppingWebClientCNs = "shopping.web" , "localhost"
$catalogApiCNs = "catalog.api", "localhost"
$basketApiCNs = "basket.api", "localhost"
$discountGrpcCNs = "discount.grpc", "localhost"
$orderApiCNs = "order.api", "localhost"
$ocelotGatewayCNs = "ocelotapigateway", "localhost"
$shoppingAggregatorCNs = "shopping.aggregator", "localhost"

$alreadyExistingCertsRoot = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq "CN=$rootCN"}
$alreadyExistingCertsIdentityServer = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $identityServerCNs[0])}
$alreadyExistingCertsShoppingWebClient = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingWebClientCNs[0])}
$alreadyExistingCertsCatalogApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $catalogApiCNs[0])}
$alreadyExistingCertsBasketApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $basketApiCNs[0])}
$alreadyExistingCertsDiscountGrpc = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $discountGrpcCNs[0])}
$alreadyExistingCertsOrderApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $orderApiCNs[0])}
$alreadyExistingCertsOcelotGatewayApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $ocelotGatewayCNs[0])}
$alreadyExistingCertsShoppingAggreagtorApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingAggregatorCNs[0])}

if ($alreadyExistingCertsRoot.Count -eq 1) {
    Write-Output "Skipping creating Root CA certificate as it already exists."
    $shoppingRootCA = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsRoot[0]
} else {
    $shoppingRootCA = New-SelfSignedCertificate -Subject $rootCN -KeyUsageProperty Sign -KeyUsage CertSign -CertStoreLocation Cert:\LocalMachine\My
}

if ($alreadyExistingCertsIdentityServer.Count -eq 1) {
    Write-Output "Skipping creating Identity Server certificate as it already exists."	
    $identityServerCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsIdentityServer[0]
} else {
    # Create a SAN cert for both identity-server and localhost.
    $identityServerCert = New-SelfSignedCertificate -DnsName $identityServerCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Web Client
if ($alreadyExistingCertsShoppingWebClient.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $shoppingWebClientCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingWebClient[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingWebClientCert = New-SelfSignedCertificate -DnsName $shoppingWebClientCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Catalog API
if ($alreadyExistingCertsCatalogApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $catalogApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsCatalogApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $catalogApiCert = New-SelfSignedCertificate -DnsName $catalogApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Basket API
if ($alreadyExistingCertsBasketApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $basketApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsBasketApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $basketApiCert = New-SelfSignedCertificate -DnsName $basketApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Discount Grpc
if ($alreadyExistingCertsDiscountGrpc.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $discountGrpcCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsDiscountGrpc[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $discountGrpcCert = New-SelfSignedCertificate -DnsName $discountGrpcCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Order API
if ($alreadyExistingCertsOrderApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $orderApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsOrderApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $orderApiCert = New-SelfSignedCertificate -DnsName $orderApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Ocelot Gateway
if ($alreadyExistingCertsOcelotGatewayApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $ocelotGatewayCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsOcelotGatewayApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $ocelotGatewayCert = New-SelfSignedCertificate -DnsName $ocelotGatewayCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Aggregator API
if ($alreadyExistingCertsShoppingAggreagtorApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $shoppingAggregatorCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingAggreagtorApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingAggregatorCert = New-SelfSignedCertificate -DnsName $shoppingAggregatorCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Export it for docker container to pick up later.
$password = ConvertTo-SecureString -String "password" -Force -AsPlainText

$rootCertPathPfx = "certs"
$identityServerCertPath = "Src/Identity/Shopping.IDP/certs"
$shoppingWebClientCertPath = "Src/Clients/Shopping.MVC/certs"
$catalogApiCertPath = "Src/Services/Catalog/Catalog.API/certs"
$basketApiCertPath = "Src/Services/Basket/Basket.API/certs"
$discountGrpcCertPath = "Src/Services/Discount/Discount.Grpc/certs"
$orderApiCertPath = "Src/Services/Ordering/Ordering.API/certs"
$ocelotGatewayCertPath = "Src/ApiGateways/OcelotApiGateway/certs"
$shoppingAggregatorCertPath = "Src/ApiGateways/Shopping.Aggregator/certs"

[System.IO.Directory]::CreateDirectory($rootCertPathPfx) | Out-Null
[System.IO.Directory]::CreateDirectory($identityServerCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($shoppingWebClientCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($catalogApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($basketApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($discountGrpcCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($orderApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($ocelotGatewayCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($shoppingAggregatorCertPath) | Out-Null

Export-PfxCertificate -Cert $shoppingRootCA -FilePath "$rootCertPathPfx/shopping-root-cert.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $identityServerCert -FilePath "$identityServerCertPath/Shopping.IDP.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingWebClientCert -FilePath "$shoppingWebClientCertPath/Shopping.MVC.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $catalogApiCert -FilePath "$catalogApiCertPath/Catalog.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $basketApiCert -FilePath "$basketApiCertPath/Basket.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $discountGrpcCert -FilePath "$discountGrpcCertPath/Discount.Grpc.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $orderApiCert -FilePath "$orderApiCertPath/Ordering.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $ocelotGatewayCert -FilePath "$ocelotGatewayCertPath/OcelotApiGateway.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingAggregatorCert -FilePath "$shoppingAggregatorCertPath/Shopping.Aggregator.pfx" -Password $password | Out-Null

# Export .cer to be converted to .crt to be trusted within the Docker container.
$rootCertPathCer = "$rootCertPathPfx/shopping-root-cert.cer"
Export-Certificate -Cert $shoppingRootCA -FilePath $rootCertPathCer -Type CERT | Out-Null

# Trust it on your host machine.
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
$store.Open("ReadWrite")

$rootCertAlreadyTrusted = ($store.Certificates | Where-Object {$_.Subject -eq "CN=$rootCN"} | Measure-Object).Count -eq 1

if ($rootCertAlreadyTrusted -eq $false) {
	Write-Output "Adding the root CA certificate to the trust store."
    $store.Add($testRootCA)
}

$store.Close()