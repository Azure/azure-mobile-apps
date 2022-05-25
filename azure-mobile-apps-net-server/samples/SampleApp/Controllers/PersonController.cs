// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Local.Models;
using Microsoft.Azure.Mobile.Server;

namespace Local.Controllers
{
    public class PersonController : TableController<Person>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            string tableName = controllerContext.ControllerDescriptor.ControllerName;
            DomainManager = new StorageDomainManager<Person>("StorageConnectionString", tableName, Request, enableSoftDelete: true);
        }

        // GET api/Default1
        public Task<IEnumerable<Person>> GetAllPersons(ODataQueryOptions query)
        {
            return QueryAsync(query);
        }

        // GET api/Default1/5
        public Task<SingleResult<Person>> GetPerson(string id)
        {
            return LookupAsync(id);
        }

        // PATCH api/Default1/5
        public Task<Person> PatchPerson(string id, Delta<Person> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST api/Default1
        public async Task<IHttpActionResult> PostPerson(Person person)
        {
            Person current = await InsertAsync(person);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task<Person> PostUndeletePerson(string id)
        {
            return UndeleteAsync(id);
        }

        // DELETE api/Default1/5
        public Task Delete(string id)
        {
            return DeleteAsync(id);
        }
    }
}