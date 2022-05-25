// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    [MobileAppController]
    public abstract class PermissionApiControllerBase : ApiController
    {
        public Task<HttpResponseMessage> Get()
        {
            return CustomSharedApi.handleRequest(this.Request, this.User);
        }

        public Task<HttpResponseMessage> Post()
        {
            return CustomSharedApi.handleRequest(this.Request, this.User);
        }

        public Task<HttpResponseMessage> Put()
        {
            return CustomSharedApi.handleRequest(this.Request, this.User);
        }

        public Task<HttpResponseMessage> Delete()
        {
            return CustomSharedApi.handleRequest(this.Request, this.User);
        }

        public Task<HttpResponseMessage> Patch()
        {
            return CustomSharedApi.handleRequest(this.Request, this.User);
        }
    }

    public class PublicPermissionController : PermissionApiControllerBase { }

    public class ApplicationPermissionController : PermissionApiControllerBase { }

    [Authorize]
    public class UserPermissionController : PermissionApiControllerBase { }

    public class AdminPermissionController : PermissionApiControllerBase { }
}