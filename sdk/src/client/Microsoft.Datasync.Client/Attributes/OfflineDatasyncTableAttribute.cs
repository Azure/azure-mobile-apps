namespace Microsoft.Datasync.Client;

/// <summary>
/// The attribute that should be applied to a class or struct that is being used
/// as a DTO for a remote datasync table.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class OfflineDatasyncTableAttribute : Attribute
{
    public OfflineDatasyncTableAttribute(string? Table = null, string? Endpoint = null)
    {
        if (!string.IsNullOrWhiteSpace(Table) && !string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new ArgumentException("Cannot specify both Table and Endpoint");
        }
        if (string.IsNullOrWhiteSpace(Table) && string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new ArgumentException("Must specify either Table or Endpoint");
        }
        this.Table = Table;

        if (Endpoint != null)
        {
            Ensure.That(new Uri(Endpoint), nameof(Endpoint)).IsValidEndpoint();
            this.Endpoint = Endpoint;
        }
    }

    /// <summary>
    /// The name of the table - will be passed through the table resolver to get a relative path.
    /// </summary>
    public string? Table { get; }

    /// <summary>
    /// The relative path of the table - will be combined with the base address of the client.
    /// </summary>
    public string? Endpoint { get; }
}
