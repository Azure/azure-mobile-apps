using Microsoft.AspNetCore.Datasync.InMemory;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Models;

[ExcludeFromCodeCoverage]
public class TodoItem : InMemoryTableData
{
    public string Title { get; set; }
}
