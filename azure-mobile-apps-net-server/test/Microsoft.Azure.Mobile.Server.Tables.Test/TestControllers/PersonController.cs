// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Mobile.Server.TestModels;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    public class PersonController : TableController<Person>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            string tableName = controllerContext.ControllerDescriptor.ControllerName;
            this.DomainManager = new StorageDomainManager<Person>("storage", tableName, this.Request, enableSoftDelete: true);
        }

        public Task<IEnumerable<Person>> GetAllPersons(ODataQueryOptions query)
        {
            return this.QueryAsync(query);
        }

        public Task<SingleResult<Person>> GetPerson(string id)
        {
            return this.LookupAsync(id);
        }

        public Task<Person> PatchPerson(string id, Delta<Person> patch)
        {
            return this.UpdateAsync(id, patch);
        }

        public async Task<IHttpActionResult> PostPerson(Person person)
        {
            Person current = await this.InsertAsync(person);
            return this.CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task<Person> PostUndeletePerson(string id)
        {
            return this.UndeleteAsync(id);
        }

        public Task Delete(string id)
        {
            return this.DeleteAsync(id);
        }
    }
}