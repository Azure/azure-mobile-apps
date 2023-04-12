// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Datasync.Common.Test.Service;

[Route("tables/datetime")]
[ExcludeFromCodeCoverage]
public class DateTimeController : TableController<DateTimeModel>
{
    public DateTimeController(IRepository<DateTimeModel> repository, ILogger<DateTimeController> logger) : base(repository)
    {
        Logger = logger;
    }
}
