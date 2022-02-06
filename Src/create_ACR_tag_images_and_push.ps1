# Step 1 - Create an Azure Container Registry
# there are Basic, Standard, Premium pricing tiers (skus) for ACR
$ACR_NAME='shoppingportalacr'  #<---- THIS NEEDS TO BE GLOBALLY unique inside of Azure
az acr create `
	--resource-group shopping-portal `
	--name $ACRName `
	--sku Standard


# step 2 - log in to the deployed ACR. This will log you with your current session login credentials
az acr login --name $ACR_NAME

# login server for ACR which is public dns name specifing network location of our ACR. 
# We can find it in Azure Portal by looking at the properties for our ACR
# by specifying query parameter we get the value of loginServer property. Example of login server name: psdemoacr.azurecr.io
$ACR_LOGINSERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)

# step 3 - Create tagged images locally for your services that will have ACR dns name preffix so we can push them to the ACR
docker tag shoppingrazor:latest $ACR_LOGINSERVER/web.razor:latest
docker tag shoppingidp:latest $ACR_LOGINSERVER/identityprovider:latest
docker tag ocelotapigateway:latest $ACR_LOGINSERVER/ocelotapigateway:latest
docker tag catalogapi:latest $ACR_LOGINSERVER/catalog.api:latest
docker tag mcr.microsoft.com/mssql/server:2017-latest $ACR_LOGINSERVER/identitydb
docker tag mongo:latest $ACR_LOGINSERVER/catalogdb

# step 4 - we now push/upload our tagged image to ACR where it can be pulled by the authenticated users or services. 
# Prefix $ACR_LOGINSERVER/ is the name of the repository/container registry in Azure where we want to push our local image
# that's how docker knows where to push an image: [loginUrl]/[repository:][tag]
docker push $ACR_LOGINSERVER/web.razor:latest
docker push $ACR_LOGINSERVER/identityprovider:latest
docker push $ACR_LOGINSERVER/ocelotapigateway:latest
docker push $ACR_LOGINSERVER/catalog.api:latest
docker push $ACR_LOGINSERVER/identitydb
docker push $ACR_LOGINSERVER/catalogdb

#Step 5 - Get a listing of the repositories and images/tags in our Azure Container Registry
az acr repository list --name $ACR_NAME --output table
az acr repository show-tags --name $ACR_NAME --repository web.razor --output table

####
#We don't have to build locally then push, we can build in ACR with Tasks.
####

# use ACR build to build our image in azure and then push that into ACR
az acr build --image "web.razor:v1-acr-task" --registry $ACR_NAME .










