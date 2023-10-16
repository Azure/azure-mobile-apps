// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Cosmos;

namespace Microsoft.AspNetCore.Datasync.CosmosDb;

internal static class CosmosUtils
{
    /// <summary>
    /// Default parser for ID and PartitionKey.
    ///
    /// Expected formats:
    /// - "id" => (id, new PartitionKey(id))
    /// - "id:customerId" => (id, new PartitionKey(customerId))
    /// - "id:customerId+employeeId" => (id, new PartitionKeyBuilder().Add(customerId).Add(employeeId).Build())
    /// </summary>
    /// <param name="idPartitionKeyString">Partition key to parse, expected formats:
    /// - "id" => (id, new PartitionKey(id))
    /// - "id:customerId" => (id, new PartitionKey(customerId))
    /// - "id:customerId+employeeId" => (id, new PartitionKeyBuilder().Add(customerId).Add(employeeId).Build())</param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    internal static (string id, PartitionKey partitionKey) DefaultParseIdAndPartitionKey(string idPartitionKeyString)
    {
        if (string.IsNullOrEmpty(idPartitionKeyString))
        {
            throw new BadRequestException();
        }
        // TODO need to handle bool and double keys
        if (!idPartitionKeyString.Contains(":"))
        {
            return (idPartitionKeyString, new PartitionKey(idPartitionKeyString));
        }

        var id = idPartitionKeyString.Split(':')[0];
        if (string.IsNullOrEmpty(id))
        {
            throw new BadRequestException();
        }

        var keysPart = idPartitionKeyString.Substring(id.Length + 1);
        if (string.IsNullOrEmpty(keysPart))
        {
            throw new BadRequestException();
        }
        
        var keys = keysPart.Split('+');
        if (keys.Length == 1)
        {
            return (id, new PartitionKey(keys[0]));
        }
        else
        {
            var partitionKey = new PartitionKeyBuilder();
            foreach (var key in keys)
            {
                partitionKey.Add(key);
            }
            return (id, partitionKey.Build());
        }
    }
}