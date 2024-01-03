using Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Controllers;

[Route("tables/kitchenreader")]
[ExcludeFromCodeCoverage]
public class KitchenReaderController : ReadonlyTableController<KitchenSink>
{
    public KitchenReaderController(IRepository<KitchenSink> repository) : base(repository)
    {
    }
}
