// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
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
