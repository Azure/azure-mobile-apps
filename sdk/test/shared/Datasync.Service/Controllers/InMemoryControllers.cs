// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Service.Controllers;

[ExcludeFromCodeCoverage]
[Route("api/in-memory/movies")]
public class InMemoryMovieController : TableController<InMemoryMovie>
{
    public InMemoryMovieController(IRepository<InMemoryMovie> repository) : base(repository)
    {
    }
}

[ExcludeFromCodeCoverage]
[Route("api/in-memory/softmovies")]
public class InMemorySoftDeletedMovieController : TableController<InMemoryMovie>
{
    public InMemorySoftDeletedMovieController(IRepository<InMemoryMovie> repository) : base(repository)
    {
        Options.EnableSoftDelete = true;
    }
}

[ExcludeFromCodeCoverage]
[Route("api/in-memory/kitchensink")]
public class InMemoryKitchenSinkController : TableController<InMemoryKitchenSink>
{
    public InMemoryKitchenSinkController(IRepository<InMemoryKitchenSink> repository) : base(repository)
    {
    }
}
