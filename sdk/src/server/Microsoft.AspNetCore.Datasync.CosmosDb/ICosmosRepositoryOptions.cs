// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.CosmosDb;

public interface ICosmosRepositoryOptions<TEntity> where TEntity : CosmosTableData
{
    /// <summary>
    /// The base <see cref="ItemRequestOptions"/> to attach to every Cosmos operation.
    /// </summary>
    ItemRequestOptions ItemRequestOptions { get; set; }

    /// <summary>
    /// Given an entity, creates an Entity ID that can be used with <see cref="TryGetPartitionKey(string, out PartitionKey)"/>
    /// </summary>
    /// <param name="entity">The entity.  Note that the ID may be <c>null</c>, so this must be taken into account.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the entity ID when complete.</returns>
    ValueTask<string> CreateEntityIdAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts the provided ID in the DTO to an ID/partition key pair.
    /// </summary>
    /// <param name="dtoId">The ID from the DTO.</param>
    /// <param name="partitionKey">The Cosmos Partition Key to use.</param>
    /// <returns>The ID to use with Cosmos DB.</returns>
    string TryGetPartitionKey(string dtoId, out PartitionKey partitionKey);
}

public class CosmosRepositoryOptions<TEntity> : ICosmosRepositoryOptions<TEntity> where TEntity : CosmosTableData
{
    /// <summary>
    /// The default partition key value.  This is used when the entity does not have a partition key defined.
    /// </summary>
    protected const string DefaultPartitionKeyValue = "default";

    /// <summary>
    /// The separator between the ID and the partition key.  This must be a character that is not valid in an ID,
    /// but is valid in a HTTP path request.
    /// </summary>
    protected const char SeparatorCharacter = ':';

    /// <summary>
    /// The separator between multiple partition key values.  This must be a character that is not valid in an ID,
    /// </summary>
    protected const char PartitonKeySeparator = '|';

    /// <inheritdoc />
    public virtual ItemRequestOptions ItemRequestOptions { get; set; } = new ItemRequestOptions()
    {
        ConsistencyLevel = ConsistencyLevel.Session
    };

    /// <inheritdoc />
    public virtual ValueTask<string> CreateEntityIdAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(entity.Id) && !entity.Id.Contains(SeparatorCharacter))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        if (string.IsNullOrEmpty(entity.Id))
        {
            string id = Guid.NewGuid().ToString("N");
            PartitionKey partitionKey = new(DefaultPartitionKeyValue);
            entity.Id = BuildEntityId(id, partitionKey);
        }
        return ValueTask.FromResult(entity.Id);
    }

    /// <inheritdoc />
    public virtual string TryGetPartitionKey(string dtoId, out PartitionKey partitionKey)
    {
        if (string.IsNullOrEmpty(dtoId) || !dtoId.Contains(SeparatorCharacter))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        string[] segments = dtoId.Split(SeparatorCharacter);
        if (segments.Length != 2 || string.IsNullOrWhiteSpace(segments[0]) || string.IsNullOrWhiteSpace(segments[1]))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        partitionKey = new PartitionKey(segments[1]);
        return segments[0];
    }

    /// <summary>
    /// Builds an Entity ID from the provided ID and partition key.  Note that this MUST
    /// match the parsing of an Entity ID in <see cref="TryGetPartitionKey(string, out PartitionKey)"/>
    /// </summary>
    /// <param name="id">The ID portion.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <returns>An entity ID representing both the ID and partition key.</returns>
    protected virtual string BuildEntityId(string id, PartitionKey partitionKey)
    {
        // The partition key is stored as a JSON array, so we need to deserialize it into a list of strings,
        // then join it with | to make a string that can be used in the ID.
        List<string> partitionKeyValues = JsonSerializer.Deserialize<List<string>>(partitionKey.ToString())!;
        return $"{id}{SeparatorCharacter}{string.Join(PartitonKeySeparator, partitionKeyValues)}";
    }
}
