using Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Controllers;

[Route("tables/kitchensink")]
[ExcludeFromCodeCoverage]
public class KitchenSinkController : TableController<KitchenSink>
{
    public KitchenSinkController(IRepository<KitchenSink> repository) : base(repository)
    {
    }
}