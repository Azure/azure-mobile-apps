// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class RoundTripTableController : TableController<RoundTripTableItem>
    {
        public RoundTripTableController(AppDbContext context)
        {
            TableRepository = new EntityTableRepository<RoundTripTableItem>(context);
        }

        public override async Task<IActionResult> PatchItemAsync(string id, [FromBody] Delta<RoundTripTableItem> patchDoc)
        {
            const int numAttempts = 5;
            IActionResult response = null;

            for (int i = 0; i < numAttempts; i++)
            {
                response = await base.PatchItemAsync(id, patchDoc);
                var statusCode = GetStatusCode(response);
                if (statusCode == 412)
                {
                    var serverItem = (response as ObjectResult).Value as RoundTripTableItem;
                    if (Request.Query.ContainsKey("conflictPolicy"))
                    {
                        var conflictPolicy = Request.Query.FirstOrDefault(p => p.Key == "conflictPolicy").Value.ToString().ToLowerInvariant();
                        switch (conflictPolicy)
                        {
                            case "clientWins":
                                Request.Headers.Remove("If-Match");
                                Request.Headers.Add("If-Match", $"\"{Convert.ToBase64String(serverItem.Version)}\"");
                                continue;
                            case "serverWins":
                                return Ok(serverItem);
                        }
                    }
                }
                return response;
            }
            return response;
        }

        private int GetStatusCode(IActionResult response)
        {
            if (response is ObjectResult)
            {
                return (response as ObjectResult).StatusCode ?? -1;
            }

            if (response is StatusCodeResult)
            {
                return (response as StatusCodeResult).StatusCode;
            }

            return -1;
        }
    }
}
