// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Datasync.Common.Test.Service;

[Route("tables/kitchensink")]
[ExcludeFromCodeCoverage]
public class KitchenSinkController : TableController<KitchenSink>
{
    public KitchenSinkController(MovieDbContext context, ILogger<KitchenSink> logger) : base()
    {
        Repository = new EntityTableRepository<KitchenSink>(context);
        Logger = logger;
    }
}
