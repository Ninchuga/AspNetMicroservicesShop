# Source: https://stackoverflow.com/a/62060315
# Generate self-signed certificate to be used by IdentityServer.
# When using localhost - API cannot see the IdentityServer from within the docker-compose'd network.
# You have to run this script as Administrator (open Powershell by right click -> Run as Administrator).

$ErrorActionPreference = "Stop"

$rootCN = "IdentityServerDockerRootCert"
#$hostDockerCNs = 'host.docker.internal'
$identityServerCNs = "shopping.identity", "localhost"
$shoppingRazorWebClientCNs = "shopping.razor" , "localhost"
$catalogApiCNs = "catalog.api", "localhost"
$basketApiCNs = "basket.api", "localhost"
$discountGrpcCNs = "discount.grpc", "localhost"
$orderingApiCNs = "ordering.api", "localhost"
$ocelotGatewayCNs = "ocelotapigateway", "localhost"
$shoppingAggregatorCNs = "shopping.aggregator", "localhost"
$shoppingWebStatusCNs = "shopping.webstatus", "localhost"
$shoppingOrderSagaCNs = "shopping.ordersagaorchestrator", "localhost"
$paymentApiCNs = "payment.api", "localhost"
$deliveryApiCNs = "delivery.api", "localhost"

$alreadyExistingCertsRoot = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq "CN=$rootCN"}
$alreadyExistingHostDockerInternal = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq "CN=$hostDockerCNs"}
$alreadyExistingCertsIdentityServer = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $identityServerCNs[0])}
$alreadyExistingCertsShoppingWebClient = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingRazorWebClientCNs[0])}
$alreadyExistingCertsCatalogApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $catalogApiCNs[0])}
$alreadyExistingCertsBasketApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $basketApiCNs[0])}
$alreadyExistingCertsDiscountGrpc = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $discountGrpcCNs[0])}
$alreadyExistingCertsOrderApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $orderingApiCNs[0])}
$alreadyExistingCertsOcelotGatewayApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $ocelotGatewayCNs[0])}
$alreadyExistingCertsShoppingAggreagtorApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingAggregatorCNs[0])}
$alreadyExistingCertsShoppingWebStatus = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingWebStatusCNs[0])}
$alreadyExistingCertsShoppingOrderSaga = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $shoppingOrderSagaCNs[0])}
$alreadyExistingCertsPaymentApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $paymentApiCNs[0])}
$alreadyExistingCertsDeliveryApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $deliveryApiCNs[0])}

# Root cert
if ($alreadyExistingCertsRoot.Count -eq 1) {
    Write-Output "Skipping creating Root CA certificate as it already exists."
    $shoppingRootCA = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsRoot[0]
} else {
    $shoppingRootCA = New-SelfSignedCertificate -Subject $rootCN -KeyUsageProperty Sign -KeyUsage CertSign -CertStoreLocation Cert:\LocalMachine\My
}

# host.docker.internal
# if ($alreadyExistingHostDockerInternal.Count -eq 1) {
    # Write-Output "Skipping creating host.docker.internal certificate as it already exists."
    # $hostDockerInternalCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingHostDockerInternal[0]
# } else {
    # $hostDockerInternalCert = New-SelfSignedCertificate -DnsName $hostDockerCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
# }

# Identity Provider
if ($alreadyExistingCertsIdentityServer.Count -eq 1) {
    Write-Output "Skipping creating Identity Server certificate as it already exists."	
    $identityServerCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsIdentityServer[0]
} else {
    # Create a SAN cert for both identity-server and localhost.
    $identityServerCert = New-SelfSignedCertificate -DnsName $identityServerCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Razor Web Client
if ($alreadyExistingCertsShoppingWebClient.Count -eq 1) {
    Write-Output "Skipping creating Web client certificate as it already exists."
    $shoppingWebClientCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingWebClient[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingWebClientCert = New-SelfSignedCertificate -DnsName $shoppingRazorWebClientCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Catalog API
if ($alreadyExistingCertsCatalogApi.Count -eq 1) {
    Write-Output "Skipping creating Catalog API certificate as it already exists."
    $catalogApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsCatalogApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $catalogApiCert = New-SelfSignedCertificate -DnsName $catalogApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Basket API
if ($alreadyExistingCertsBasketApi.Count -eq 1) {
    Write-Output "Skipping creating Basket API certificate as it already exists."
    $basketApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsBasketApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $basketApiCert = New-SelfSignedCertificate -DnsName $basketApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Discount Grpc
if ($alreadyExistingCertsDiscountGrpc.Count -eq 1) {
    Write-Output "Skipping creating Discount Grpc certificate as it already exists."
    $discountGrpcCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsDiscountGrpc[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $discountGrpcCert = New-SelfSignedCertificate -DnsName $discountGrpcCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Ordering API
if ($alreadyExistingCertsOrderApi.Count -eq 1) {
    Write-Output "Skipping creating Ordering API certificate as it already exists."
    $orderApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsOrderApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $orderApiCert = New-SelfSignedCertificate -DnsName $orderingApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Ocelot Gateway
if ($alreadyExistingCertsOcelotGatewayApi.Count -eq 1) {
    Write-Output "Skipping creating Ocelot Gateway certificate as it already exists."
    $ocelotGatewayCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsOcelotGatewayApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $ocelotGatewayCert = New-SelfSignedCertificate -DnsName $ocelotGatewayCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Aggregator API
if ($alreadyExistingCertsShoppingAggreagtorApi.Count -eq 1) {
    Write-Output "Skipping creating Shopping Aggreagtor certificate as it already exists."
    $shoppingAggregatorCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingAggreagtorApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingAggregatorCert = New-SelfSignedCertificate -DnsName $shoppingAggregatorCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Web Status
if ($alreadyExistingCertsShoppingWebStatus.Count -eq 1) {
    Write-Output "Skipping creating Shopping Web Status certificate as it already exists."
    $shoppingWebStatusCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingWebStatus[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingWebStatusCert = New-SelfSignedCertificate -DnsName $shoppingWebStatusCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Order Saga Orchestrator
if ($alreadyExistingCertsShoppingOrderSaga.Count -eq 1) {
    Write-Output "Skipping creating Order Saga Orchestrator certificate as it already exists."
    $shoppingOrderSagaOrchestratorCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsShoppingOrderSaga[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $shoppingOrderSagaOrchestratorCert = New-SelfSignedCertificate -DnsName $shoppingOrderSagaCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Payment API
if ($alreadyExistingCertsPaymentApi.Count -eq 1) {
    Write-Output "Skipping creating Payment API certificate as it already exists."
    $paymentApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsPaymentApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $paymentApiCert = New-SelfSignedCertificate -DnsName $paymentApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Shopping Delivery API
if ($alreadyExistingCertsDeliveryApi.Count -eq 1) {
    Write-Output "Skipping creating Delivery API certificate as it already exists."
    $deliveryApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsDeliveryApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $deliveryApiCert = New-SelfSignedCertificate -DnsName $deliveryApiCNs -Signer $shoppingRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Export it for docker container to pick up later.
$password = ConvertTo-SecureString -String "password" -Force -AsPlainText

$rootCertPathPfx = "D:/Practice/AspNetMicroservicesShop/Src/certs"
$hostDockerInternalCertPath = "D:/Practice/AspNetMicroservicesShop/Src/certs"
$identityServerCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Identity/Shopping.IDP/certs"
$shoppingRazorWebClientCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Clients/Shopping.Razor/certs"
$catalogApiCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Catalog/Catalog.API/certs"
$basketApiCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Basket/Basket.API/certs"
$discountGrpcCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Discount/Discount.Grpc/certs"
$orderApiCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Ordering/Ordering.API/certs"
$ocelotGatewayCertPath = "D:/Practice/AspNetMicroservicesShop/Src/ApiGateways/OcelotApiGateway/certs"
$shoppingAggregatorCertPath = "D:/Practice/AspNetMicroservicesShop/Src/ApiGateways/Shopping.Aggregator/certs"
$shoppingWebStatusCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Common/Shopping.WebStatus/certs"
$orderSagaOrchestratorCertPath = "D:/ractice/AspNetMicroservicesShop/Src/Orchestrators/Shopping.CheckoutOrchestrator/certs"
$paymentApiCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Payment/Payment.API/certs"
$deliveryApiCertPath = "D:/Practice/AspNetMicroservicesShop/Src/Services/Delivery/Delivery.API/certs"

[System.IO.Directory]::CreateDirectory($rootCertPathPfx) | Out-Null
[System.IO.Directory]::CreateDirectory($hostDockerInternalCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($identityServerCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($shoppingRazorWebClientCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($catalogApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($basketApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($discountGrpcCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($orderApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($ocelotGatewayCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($shoppingAggregatorCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($shoppingWebStatusCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($orderSagaOrchestratorCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($paymentApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($deliveryApiCertPath) | Out-Null

Export-PfxCertificate -Cert $shoppingRootCA -FilePath "$rootCertPathPfx/shopping-root-cert.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $hostDockerInternalCert -FilePath "$rootCertPathPfx/host-docker-internal.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $identityServerCert -FilePath "$identityServerCertPath/Shopping.IDP.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingWebClientCert -FilePath "$shoppingRazorWebClientCertPath/Shopping.Razor.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $catalogApiCert -FilePath "$catalogApiCertPath/Catalog.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $basketApiCert -FilePath "$basketApiCertPath/Basket.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $discountGrpcCert -FilePath "$discountGrpcCertPath/Discount.Grpc.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $orderApiCert -FilePath "$orderApiCertPath/Ordering.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $ocelotGatewayCert -FilePath "$ocelotGatewayCertPath/OcelotApiGateway.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingAggregatorCert -FilePath "$shoppingAggregatorCertPath/Shopping.Aggregator.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingWebStatusCert -FilePath "$shoppingWebStatusCertPath/Shopping.WebStatus.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $shoppingOrderSagaOrchestratorCert -FilePath "$orderSagaOrchestratorCertPath/Shopping.CheckoutOrchestrator.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $paymentApiCert -FilePath "$paymentApiCertPath/Payment.API.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $deliveryApiCert -FilePath "$deliveryApiCertPath/Delivery.API.pfx" -Password $password | Out-Null

# Export .cer to be converted to .crt to be trusted within the Docker container.
$rootCertPathCer = "$rootCertPathPfx/shopping-root-cert.cer"
Export-Certificate -Cert $shoppingRootCA -FilePath $rootCertPathCer -Type CERT | Out-Null

# Import root cert
$absoluteRootCertPfxFilePath = "$rootCertPathPfx/shopping-root-cert.pfx"
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
$cert.Import($absoluteRootCertPfxFilePath, $Password, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]"PersistKeySet")

# Trust it on your host machine.
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
$store.Open("ReadWrite")

$rootCertAlreadyTrusted = ($store.Certificates | Where-Object {$_.Subject -eq "CN=$rootCN"} | Measure-Object).Count -eq 1

if ($rootCertAlreadyTrusted -eq $false) {
	Write-Output "Adding the root CA certificate to the trust store."
	$store.Add($cert)
}

$store.Close()

# Path where my local trusted certificates are stored
# Cert:\LocalMachine\My

#Delete by thumbprint
#Get-ChildItem Cert:\LocalMachine\My\D20159B7772E33A6A33E436C938C6FE764367396 | Remove-Item

#Delete by subject/serialnumber/issuer/whatever
#Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -match 'CN=shopping.identity' } | Remove-Item