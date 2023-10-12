using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Datasync.NSwag.Test.Service;

[ExcludeFromCodeCoverage]
public abstract class ReadonlyTableController<TData> : TableController<TData> where TData : class, ITableData
{
    [NonAction]
    public override Task<IActionResult> CreateAsync([FromBody] TData item, CancellationToken token = default)
    {
        return base.CreateAsync(item, token);
    }

    [NonAction]
    public override Task<IActionResult> DeleteAsync([FromRoute] string id, CancellationToken token = default)
    {
        return base.DeleteAsync(id, token);
    }

    [NonAction]
    public override Task<IActionResult> PatchAsync([FromRoute] string id, CancellationToken token = default)
    {
        return base.PatchAsync(id, token);
    }

    [NonAction]
    public override Task<IActionResult> ReplaceAsync([FromRoute] string id, [FromBody] TData item, CancellationToken token = default)
    {
        return base.ReplaceAsync(id, item, token);
    }
}

[Route("tables/kitchenreader")]
[ExcludeFromCodeCoverage]
public class KitchenReaderController : ReadonlyTableController<KitchenSink>
{
    public KitchenReaderController(ServiceDbContext context, ILogger<KitchenSink> logger) : base()
    {
        Repository = new EntityTableRepository<KitchenSink>(context);
        Logger = logger;
    }
}

