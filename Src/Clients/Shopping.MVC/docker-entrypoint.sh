#!/usr/bin/env bash

# exit when any command fails
set -e

# trust dev root CA
update-ca-certificates

# start the app
dotnet Shopping.MVC.dll