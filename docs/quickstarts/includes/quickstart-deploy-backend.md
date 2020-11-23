## Deploy the backend service

To deploy the quickstart service, first login to Azure with the Azure CLI:

```bash
> az login
```

A web browser will be opened to complete the authorization.

If necessary, [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli).

### Create a resource group

Type the following to create a resource group:

```bash
> az group create -l westus -n zumo-quickstart
```

This will create a resource group called _zumo-quickstart_ to hold all the resources we create.  

### Deploy the backend to Azure

The service is comprised of the following resources:

* An App Service Hosting Plan on the Free plan.
* A web-site hosted within the App Service Hosting plan.
* An Azure SQL server
* An Azure SQL database in the Basic tier (incurs cost)

The only item that incurs cost if the Azure SQL database.  For details, see [Pricing](https://azure.microsoft.com/en-us/pricing/details/sql-database/single/).

To deploy the resources, type the following:

```bash
> cd samples/nodejs
> az deployment group create -n ZumoQuickstart -g zumo-quickstart --template-file ./azuredeploy.json
```

> **Deployment Failed**
>
> If the deployment failed, but all the resources seem to be created, it is likely that the deployment of the backend code failed.  To check this, log onto the Azure portal, then locate your App Service (in the _zumo-quickstart_ resource group).  Select **Deployment Center**.  If the deployment is listed as failed, press the **Sync** button to re-try the deployment.  You can then re-run the deployment above.

Once complete, run the following to see the outputs:

```bash
az deployment group show -n ZumoQuickstart -g zumo-quickstart --query properties.outputs
```

This will show the password for your database and the URI of the backend are printed.  You will need the URI when configuring your mobile app.  You do not require the password for your database.  However, it is useful if you wish to inspect the database through the Azure portal.

> **Deleting the resources**
>
> Once you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`.
> 
> The tutorial is comprised of three parts (including this section).  Do not delete the resources before completing the tutorial.
