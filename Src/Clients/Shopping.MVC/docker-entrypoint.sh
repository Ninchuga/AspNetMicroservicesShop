#!/usr/bin/env bash

# exit when any command fails
set -e

# trust dev root CA
openssl x509 -inform DER -in /https-root/shopping-root-cert.cer -out /https-root/shopping-root-cert.crt
cp /https-root/shopping-root-cert.crt /usr/local/share/ca-certificates/
cp /https-root/shopping-root-cert.crt /etc/ssl/certs/shopping-root-cert.crt
update-ca-certificates

# start the app
cd /app
dotnet Shopping.MVC.dll