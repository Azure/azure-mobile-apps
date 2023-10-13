using api.Db;
using Microsoft.AspNetCore.Datasync.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace api.Extensions;

public static class AspNetCoreExtensions
{
    public static IServiceCollection AddCosmosDatasync(this IServiceCollection services, string connectionString, string databaseName)
    {
        var client = new CosmosClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var todoItemsContainer = database.GetContainer("TodoItems");

        services.AddSingleton(new CosmosRepository<TodoItem>(todoItemsContainer));

        return services;
    }
}
