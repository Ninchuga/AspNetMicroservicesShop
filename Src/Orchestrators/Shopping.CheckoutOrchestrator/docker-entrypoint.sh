#!/usr/bin/env bash

# exit when any command fails
set -e

# trust dev root CA
openssl x509 -inform DER -in /https-root/shopping-root-cert.cer -out /https-root/shopping-root-cert.crt
cp /https-root/shopping-root-cert.crt /usr/local/share/ca-certificates/
chmod 644 /usr/local/share/ca-certificates/
update-ca-certificates

# start the app
dotnet Shopping.OrderSagaOrchestrator.dll