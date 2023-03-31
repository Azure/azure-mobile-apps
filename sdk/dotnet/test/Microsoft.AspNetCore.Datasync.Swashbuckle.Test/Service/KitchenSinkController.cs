// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Test.Service;

[Route("tables/kitchensink")]
[ExcludeFromCodeCoverage]
public class KitchenSinkController : TableController<KitchenSink>
{
    public KitchenSinkController(ServiceDbContext context, ILogger<KitchenSink> logger) : base()
    {
        Repository = new EntityTableRepository<KitchenSink>(context);
        Logger = logger;
    }
}
