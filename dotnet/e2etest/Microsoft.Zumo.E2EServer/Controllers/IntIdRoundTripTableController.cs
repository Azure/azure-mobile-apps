// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.E2EServer.Utils;
using Microsoft.Zumo.Server;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class IntIdRoundTripTableController : TableController<IntIdRoundTripTableItemDto>
    {
        public IntIdRoundTripTableController(AppDbContext context, MapperConfiguration configuration)
        {
            TableRepository = new IntIdMappedEntityTableRepository<IntIdRoundTripTableItemDto, IntIdRoundTripTableItem>(context, configuration);
        }
    }
}
