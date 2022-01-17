$certPass = "password"
$certSubj = "host.docker.internal"
$certAltNames = "DNS:localhost,DNS:host.docker.internal,DNS:identity_server" # we can also add individual IP addresses here like so: IP:127.0.0.1
$opensslPath= "C:\Program Files\OpenSSL-Win64\bin" #"path\to\openssl\binaries" #aOpenSSL needs to be present on the host, no installation is necessary though
$workDir="D:\Practice\AspNetMicroservicesShop\Src"  # "path\to\your\project"
$workDirForCerts="D:\Practice\AspNetMicroservicesShop\Src\certs"  # "path\to\your\project"
$dockerDir=Join-Path $workDir "Identity\Shopping.IDP\certs"

#generate a self-signed cert with multiple domains
Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ",
                                          (Join-Path $workDirForCerts aspnetapp.key),
                                          "-out", (Join-Path $dockerDir aspnetapp.crt),
                                          "-subj `"/CN=$certSubj`" -addext `"subjectAltName=$certAltNames`""
										  
# this time round we convert PEM format into PKCS#12 (aka PFX) so .net core app picks it up
Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "pkcs12 -export -in ", 
                                           (Join-Path $dockerDir aspnetapp.crt),
                                           "-inkey ", (Join-Path $workDirForCerts aspnetapp.key),
                                           "-out ", (Join-Path $workDirForCerts aspnetapp.pfx),
                                           "-passout pass:$certPass"
										   
$password = ConvertTo-SecureString -String $certPass -Force -AsPlainText
$cert = Get-PfxCertificate -FilePath (Join-Path $workDirForCerts "aspnetapp.pfx") -Password $password

# and still, trust it on our host machine
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store [System.Security.Cryptography.X509Certificates.StoreName]::Root,"LocalMachine"
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()

# -------------------------

$dockerDirRazor=Join-Path $workDir "Clients\Shopping.Razor\certs"

Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ",
                                          (Join-Path $workDirForCerts aspnetapp.key),
                                          "-out", (Join-Path $dockerDirRazor aspnetapp.crt),
                                          "-subj `"/CN=$certSubj`" -addext `"subjectAltName=$certAltNames`""
										  
										  
# -----------------------

$dockerDirOcelot=Join-Path $workDir "ApiGateways\OcelotApiGateway\certs"

Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ",
                                          (Join-Path $workDirForCerts aspnetapp.key),
                                          "-out", (Join-Path $dockerDirOcelot aspnetapp.crt),
                                          "-subj `"/CN=$certSubj`" -addext `"subjectAltName=$certAltNames`""
										  
# ---------------------------------
										  
$dockerDirCatalogApi=Join-Path $workDir "Services\Catalog\Catalog.API\certs"

Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ",
                                          (Join-Path $workDirForCerts aspnetapp.key),
                                          "-out", (Join-Path $dockerDirCatalogApi aspnetapp.crt),
                                          "-subj `"/CN=$certSubj`" -addext `"subjectAltName=$certAltNames`""
										  
										  
										  
										  
										  
										  
