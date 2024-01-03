// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Controllers;

[Route("tables/kitchenreader")]
[ExcludeFromCodeCoverage]
public class KitchenReaderController : ReadonlyTableController<KitchenSink>
{
    public KitchenReaderController(IRepository<KitchenSink> repository) : base(repository)
    {
    }
}
