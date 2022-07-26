# Create and manage Kubernetes cluster on Azure

# Step 1: Login to Azure Account
az login

# step 2- install kubectl locally with azure-cli command if not already installed
az aks install-cli

# or by installing .exe file https://storage.googleapis.com/kubernetes-release/release/v1.12.0/bin/windows/amd64/kubectl.exe
# Then create a new folder in C drive, name it ‘kube’ or whatever you want and copy kubectl.exe inside
# Go to Advance System settings>Environment Variables and edit path by adding “C:\kube” to local user or system variables
# Now open command prompt from “C:\kube” and run kubectl

# step 3: To enable a Kubernetes cluster to interact with other Azure resource, an Azure Active Directory service principal is required.
# 3a - Create a service principal by using the az ad sp create-for-rbac command
# The output of the command provides the appId which is the service principal and password which is the client-secret for creating the Kubernetes cluster
az ad sp create-for-rbac --skip-assignment # simple one

# 3b - complex service principal
$MAIN_RES_GROUP='shopping-portal'
$ACR_NAME='shoppingportalacr' # name of our container registry
$ACR_REGISTRY_ID=$(az acr show --name $ACR_NAME --query id --output tsv) # resource id of our container registry

$SP_NAME="$ACR_NAME-push-sp" # variable for our service principal name

  # Create a Key Vault to store service principal username and password
  $AKV_NAME="$ACR_NAME-vault"
  az keyvault create --resource-group $MAIN_RES_GROUP --name $AKV_NAME

# create service principal and store credentials in azure key vault
# role based access control role (RBAC) that allows orchestrators like ACI or Kubernetes to pull images from ACR
# use query to retreive password field
# Create service principal, store its password in AKV (the registry *password*)
# To grant both push and pull access, change the --role argument to acrpush
az ad sp create-for-rbac `
                --name $SP_NAME `
                --scopes $ACR_REGISTRY_ID `
                --role acrpush
                #--query password `
                #--output tsv)

# Copy entire JSON output from above command and use it in GitHub workflow action.
# Place entire JSON in AZURE_ACR_SP_CREDENTIALS secret in GitHub and use it for Azure CLI Login
# Store also in GitHub secret REGISTRY_PASSWORD because it will be used to login to ACR to build and push container image
$SP_PASSWD='' # copy client_secret from JSON which we use as a password
az keyvault secret set `
    --vault-name $AKV_NAME `
    --name $ACR_NAME-push-pwd `
    --value $SP_PASSWD

     # get registry password from key vault
     az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-push-pwd --query value -o tsv

# Next, store the service principal's appId in the vault, which is the username you pass to Azure Container Registry for authentication
# Store service principal ID in AKV (the registry *username*)
# app id will be used as our username when authenticating to ACR
# Store also in GitHub secret REGISTRY_USERNAME because it will be used to login to ACR to build and push container image
$ACR_SP_APP_ID=$(az ad sp list `
                    --display-name $SP_NAME `
                    --query "[].appId" `
                    --output tsv)

az keyvault secret set `
    --vault-name $AKV_NAME `
    --name $ACR_NAME-push-usr `
    --value $ACR_SP_APP_ID

      # get username from key vault
      az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-push-usr --query value -o tsv

    # To delegate permissions, create a role assignment using the az role assignment create command. 
    # Assign the appId to a particular scope, such as a resource group or virtual network resource. 
    # A role then defines what permissions the service principal has on the resource, as shown in the following example:
    az role assignment create --assignee $ACR_SP_APP_ID --scope $ACR_REGISTRY_ID --role AcrPush

    # DISPLAY SP
    # objectId
    az ad sp list --display-name $SP_NAME --query [].objectId -o tsv

    az ad sp list --display-name $SP_NAME --query [].appOwnerTenantId -o tsv

    # appId
    az ad sp list --display-name $SP_NAME --query [].appId -o tsv

    az ad sp show --id 7f46f2fa-95c0-4d1d-815e-9472aa6e06e3

    az ad sp list --show-mine --query "[].{id:appId, tenant:appOwnerTenantId}"

# create our ACR Task. This task will setup an automated trigger on code commit which will run a docker build and push the container to your registry
$GIT_USER='Ninchuga'
az keyvault secret set `
    --vault-name $AKV_NAME `
    --name $ACR_NAME-git-acr-task-token `
    --value '<git-personal-access-token>'

az acr task create `
    --registry $ACR_NAME `
    --name webrazor-build-task `
    --context https://github.com/$GIT_USER/AspNetMicroservicesShop.git#main `
    --file Src/acrmultitask.yaml `
    --git-access-token $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-git-acr-task-token --query value -o tsv)

    # trigger ACR Task manually to check if it's working
    az acr task run --registry $ACR_NAME --name webrazor-build-task

    # To verify that your tasks have run successfully use the below command
    az acr task list-runs --registry $ACR_NAME --output table


# 4 - Create the Kubernetes cluster with repository group and service principal created earlier
$AKS_NAME='shoppingPortalAKSCluster'
az aks create `
    --resource-group shopping-portal `
    --name $AKS_NAME `
    --service-principal $ACR_SP_APP_ID `
    --client-secret $SP_PASSWD `
    --node-count 1 `
    --generate-ssh-keys

# 4a - Create an AKS Cluster with managed service principal created by Azure CLI
# service principal is not specified
# When you create an AKS cluster, a second resource group is automatically created to store the AKS resources.
az aks create --resource-group shopping-portal --name $AKS_NAME --node-count 1 --generate-ssh-keys

# 5 - Configure kubectl to connect your Kubernetes cluster by using the az aks get-credentials command
az aks get-credentials --resource-group shopping-portal --name $AKS_NAME

# 6 - Verify the connection to your Kubernetes cluster by using the kubectl get nodes command
kubectl get nodes

# 7 - Create Image Pull Secret
# Template
kubectl create secret docker-registry <secret-name> `
    --namespace <namespace> `
    --docker-server=<container-registry-name>.azurecr.io `
    --docker-username=<service-principal-ID> `
    --docker-password=<service-principal-password>

kubectl create secret docker-registry acrandaksshopping-secret `
    --namespace default `
    --docker-server=shoppingportalacr.azurecr.io `
    --docker-username=$SP_APPID `
    --docker-password=$SP_PASSWD

# List Secrets
kubectl get secrets

# 8 - Create a .yaml file with deployment, service and ingress-controller and Deploy to AKS
# Update Deployment Manifest with Image Name, ImagePullSecrets
# check acr-shoppingportal.yaml file inside of app Src folder

# 9 - Now use kubectl to apply your yaml
# navigate to a folder where your .yaml file is and run
kubectl apply -f acr-shoppingportal.yaml

# 10 - list Pods
kubectl get pods

# Describe Pod
kubectl describe pod POD_NAME

# Get logs
kubectl logs POD_NAME

# Get Load Balancer IP | Get Services
kubectl get services

# Access Application
http://<External-IP-from-get-service-output>

# --------------- Clean up ---------------------

# delete applications
kubectl delete -f acr-shoppingportal.yaml

# delete Pod
kubectl delete pod POD-NAME

# It can happen that after deletion pod will be recreated again and started
# you need to delete deployment to avoid this from happening

# list all deployments
kubectl get deployments --all-namespaces

# delete deployment
kubectl delete -n NAMESPACE deployment DEPLOYMENT-NAME

# Detach ACR from AKS Cluster
az aks update -n shoppingPortalAKSCluster -g shopping-portal --detach-acr $ACR_NAME

# Delete ACR Repository
Go To Services -> Container Registries -> shoppingportalacr -> Delete it

az ad sp list


az ad sp delete --id edfa2c36-84ef-43c4-9d83-d93a7b138797

az ad sp show --id a497d64c-6ad7-4f16-b9c5-d1f36ac27a84

# ----------------------------------------

# create a container in ACI (ACI resource will be created as well if doesn't exist)
# we use created service principal to log in to ACR and pull the image that we specified and create the container from it
$ACR_LOGINSERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
az container create `
		--resource-group $MAIN_RES_GROUP `
		--name webrazor-aci `
		--dns-name-label webrazor-aci-dns-name `
		--ports 443 `
		--image $ACR_LOGINSERVER/web.razor:a3c16ee728f205dcc8650e65a42d240e5c4a833b `
		--registry-login-server $ACR_LOGINSERVER `
		--registry-username $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-push-usr --query value -o tsv) `
		--registry-password $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-push-pwd --query value -o tsv)