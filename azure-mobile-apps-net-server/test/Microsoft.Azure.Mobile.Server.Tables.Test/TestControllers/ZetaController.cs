// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    /// <summary>
    /// This controller is here to force a bug where we were not creating new formatters so Controller
    /// configurations were resetting the TableContractResolver globally. Making this start
    /// with 'Z' means that it will be called last and covers the bug that has already been fixed.
    /// </summary>
    public class ZetaController : ApiController
    {
        public string Get()
        {
            return "hello";
        }
    }
}
