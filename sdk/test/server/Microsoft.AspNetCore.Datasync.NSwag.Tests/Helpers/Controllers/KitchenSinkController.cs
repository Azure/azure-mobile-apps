using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Controllers;

[Route("tables/kitchensink")]
[ExcludeFromCodeCoverage]
internal class KitchenSinkController : TableController<KitchenSink>
{
    public KitchenSinkController(IRepository<KitchenSink> repository) : base(repository)
    {
    }
}