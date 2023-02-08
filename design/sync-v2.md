# Synchronization Process - version 2

The purpose of this design document is to flesh out the details of a new synchronization protocol that a future release of the Microsoft Datasync Client will implement.

## An overview of the current synchronization protocol

The current synchronization protocol uses an offline operations queue and properties within the data transfer object to implement a fully reliable, albiet slow, synchronization.

Each Data Transfer Object has:

* A globally unique ID that uniquely identifies the entity.
* An opaque "version" that can be used to determine if the entity has changed.
* A date/time stamp when the entity was last updated on the service.

The service is essentially a basic RESTful CRUDL, where the "List" operation uses an OData configuration to provide filtering, projection, and paging.

The offline database contains an identically configured table to hold the DTO, plus the following tables:

* An operations queue, named `__operations`
* An errors table, named `__errors`
* A delta token table, named `__config`

Each table in the remote service contains an entry in the delta token table which indicates when the last record that was read has been updated, allowing for incremental synchronization.  The errors table contains a persistent store for conflict and other service errors.

The operations queue is defined thusly:

```json
{
    "id": "a unique ID for the operation",
    "kind": "delete|insert|update",
    "state": "pending|attempted|completed|failed",
    "tableName": "the name of the table in the Offline database",
    "itemId": "the item that is being changed",
    "item": "JSON serialized content for the item",
    "sequence": "a sequence number",
    "version": "the # times the item has been updated within the sync"
}
```

The client application initiates the synchronization event.  To do this, first operations in the operations queue for the table are pushed to the service, then any changes for the table are pulled from the remote service.

### Push Process

During the push process, the operations to be pushed (which can be "everything" or "just the operations for a single table") are loaded into memory, then operated one at a time.  Technically, up to 8 threads can be used for parallelization.  However, for all intents, each operation is treated separately.  For each element:

1. The state is set to attempted.
1. The appropriate operation is performed on the remote service.
    1. If the operation is successful, state is set to completed, and the replacement entity is written to the data table.
    1. If the operation is failed, an error entry is set in the __errors table and the state is set to failed.

Once the batch is complete:

* Errors are compiled into an aggregate exception.
* Any operation that is "completed" is removed from the operations queue.

### Pull Process

During the pull process, the sync process:

1. Reads the delta token
1. Does a "GET /tables/<table>?$filter=(updatedAt gt '<delta-token>')&__includedeleted=true - this results in a paged response.
1. For each element in the paged response:
    * Write the new or updated record to the data table (or delete the record).
    * Store the delta token in the __config table.
1. Repeat for additional pages of the paged response.

## Problems with the current process

1. It is highly reliable, but slow.
1. The current SQLite implementation does not allow for additional property types (like spatial data, DateOnly, or TimeOnly)
1. Requires full support of OData to SQL to work.
1. Requires a specific form of the DTO to properly support incremental sync, conflict resolution, and optimistic concurrency.

## Proposal

Let's look at the remote service end and the local offline end separately.

### Remote Service

By using the OData service endpoint, we allow filtering (thus allowing just a subset of records to be synchronized) and projection (thus allowing only certain properties to be synchronized).  Ideally, we could use any DTO for synchronization.  For example, this is not available today:

```csharp
class MyDto 
{
    [Key]
    public int localId { get; set; }
}
```

* The localId is not a string (and not globally unique) and not called `Id`.
* There is no `UpdatedAt` or `Version` strings.

I'd propose the following:

* The ID should be identified by a `[Key]` attribute and can be any object.  It is up to the developer to ensure that the ID is globally unique within the system, including offline.
* The last updated date should be identified by a new attribute `[UpdatedAt]` and must be an auto-incrementing comparable.  For example, we can use a `long` or a `DateTimeOffset`.  The only requirement is that it must be every increasing so we can use a `>` comparison.
* The version must either be specified by a `[Version]` filter (and can either be a string or byte array), or the DTO must implement `IDatasyncVersion` which has a `.GetVersion()` (returns a string) to generate the version.  This allows the developer to decide how to compute a version.
* A `Deleted` flag may be provided by using `[Deleted]` on a field.

For a `TodoItem`, for example:

```csharp
class TodoItemDTO : IDatasyncVersion
{
    [Key]
    public long Id { get; set; }

    [UpdatedAt]
    public DateTimeOffset UpdatedAt { get; set; }

    public string IDatasyncVersion.GetVersion()
        => UpdatedAt.UtcTicks.ToString();

    public string Title { get; set; }

    public string Description { get; set; }

    public DateOnly DueDate { get; set; }

    public TimeSpan AlertBeforeDueDate { get; set; }

    public TimeOnly AlertTime { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    public UIColor StatusColor { get; set; }
}
```

You will notice:

* We rely on the database to handle the ID and UpdatedAt.
* The version is computed from the UpdatedAt field - no need to keep something extra.
* There are a variety of types here; we should artificially restrict the types.
* The DTO should be able to be handled on both client and server, potentially mapping using Automapper to get the DatabaseGenerated field for UpdatedAt.

This should also be handled quite easily with non-OData requests; for example, GraphQL, gRPC, and other transports should be easily provided.

## Local Service

On the local side, we should utilize common libraries, like Entity Framework Core, to implement the offline synchronization library.  This is split into two parts - 

1. When the client application accesses the local database
2. When the synchronization process makes changes to the local database.

Ideally, the same DTO should be able to be used on both sides (remote service and offline database).  

When a client application makes a change to the local database:

1. The change is validated.
2. The operation is written to the operations queue.
3. The change is written to the local offline table.

When the synchronization process makes a change to the local database:

1. The change is validated.
2. The operation is removed from the operations queue.
3. The change is written to the local offline table.

In both cases, the operations queue and offline table change needs to be written in a transaction so that it may be rolled back in its entirety. 

In the new world:

### Pull Operation

1. Read delta-token from the offline table.
2. Read new items since delta-token from remote service.
3. Add new items to the offline table.
4. Write delta-token to the offline table.

### CRUD on offline table

1. Add new entity to the offline table, writing an operation table entry as well (as transaction).
    TODO: How to deal with Id field that is not globally unique in the offlien table.

### Push Operation

1. Read (block of) entities in the operations queue
1. For each entity:
    * Do operation on remote service.
    * On success, store new entity + remote operation table entry as a transaction.
    * On conflict, deal with conflicts as per current spec.
    * On error, add error to aggregate exception, and leave operation table entry alone (abort queue).

This relies on:

* The operations queue is a part of the store - not a part of the sync context.
* Errors and delta tokens are a part of the store as well.

## Suggested API

```csharp
/// <remarks>
/// Cancellation tokens are assumed.
/// </remarks>
public interface IOfflineTable<T>
{
    // Operations that are normal for client application to do:
    IQueryable<T> AsQueryable();
    ValueTask<Result<T>> GetAsync(object id);
    ValueTask<IEnumerable<Result<T>>> AddRangeAsync(IEnumerable<T> entities);
    ValueTask<IEnumerable<Result>> RemoveRangeAsync(IEnumerable<object> entityIds);
    ValueTask<IEnumerable<Result<T>>> ReplaceRangeAsync(IEnumerable<T> entities);
}

internal interface ISynchronizableTable<T>
{
    // Operations that are synchronization operations
    IQueryable<Operation<T>> QueryOperations();
    long OperationCount { get; }
    DeltaToken DeltaToken { get; }
    ValueTask<IEnumerable<Result<T>>> SynchronizeRangeAsync(IEnumerable<Operation<T>> syncOperations);
}
```

The `IOfflineTable<T>` will be used by client applications, and will encapsulate both the operation queue update and the operation within a single transaction.  Using range will allow us to do bulk changes to the database (with requisite operation queue changes).  The `ISynchronizableTable<T>` interface contains elements that are only of interest to the synchronization process.  We may "externalize" these elements, but the developer should not care.
