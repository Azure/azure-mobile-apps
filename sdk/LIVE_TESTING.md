# Running Live Tests

There are a set of live tests that are disabled by default, but can be enabled by provisioning the appropriate environment variables.  

Note that file-based databases (in-memory, LiteDb, and Sqlite) are always run "live".  

Follow these instructions to enable live testing of the service based databases:

## 1. Deploy the databases

Use the following Azure CLI commands to deploy the database services in the Azure cloud:

```bash
az group create -l westus3 -n zumo-testing
az deployment group create -n zumo-testing-deployment -g zumo-testing -f ./infra/main.bicep
```

This will take approximately 15-20 minutes to fully deploy, and provides the following resources:

* Azure SQL Database (Basic SKU)
* Azure Cosmos Database (Standard SKU)
* Azure DB for PostgreSQL flexible server (Burstable B1ms SKU)

## 2. Create a .runsettings file

First, get the outputs:

```bash
az deployment group show -n zumo-testing-deployment -g zumo-testing --query properties.outputs
```

Create the `.runsettings` file in the `sdk` directory with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <ZUMO_AZSQL_CONNECTIONSTRING>{{connection string}}</ZUMO_AZSQL_CONNECTIONSTRING>
      <ZUMO_COSMOS_CONNECTIONSTRING>{{connection string}}</ZUMO_COSMOS_CONNECTIONSTRING>
      <ZUMO_PGSQL_CONNECTIONSTRING>{{connection string}}</ZUMO_PGSQL_CONNECTIONSTRING>
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
```

Replace the connection strings (`{{connection string}}`) with the appropriate value from the deployment outputs.  Additionally, you can add the following environment variable if you want the output to include verbose SQL logging:

```xml
<ENABLE_SQL_LOGGING>true</ENABLE_SQL_LOGGING>
```

## 3. Run the tests

You can either go into Visual Studio and run the tests from the **Test Explorer**, or just run `dotnet test` to run the tests on the command line.  The `.runsettings` file will get picked up automatically in both cases.
