// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Controllers;

[ExcludeFromCodeCoverage]
public abstract class ReadonlyTableController<TData> : TableController<TData> where TData : class, ITableData
{
    protected ReadonlyTableController(IRepository<TData> repository) : base(repository)
    {
    }

    [NonAction]
    public override Task<IActionResult> CreateAsync(CancellationToken token = default)
    {
        return base.CreateAsync(token);
    }

    [NonAction]
    public override Task<IActionResult> DeleteAsync([FromRoute] string id, CancellationToken token = default)
    {
        return base.DeleteAsync(id, token);
    }

    [NonAction]
    public override Task<IActionResult> ReplaceAsync([FromRoute] string id, CancellationToken token = default)
    {
        return base.ReplaceAsync(id, token);
    }
}
