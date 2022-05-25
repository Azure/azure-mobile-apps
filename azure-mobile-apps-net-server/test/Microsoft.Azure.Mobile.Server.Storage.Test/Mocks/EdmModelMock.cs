// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;

namespace Microsoft.Azure.Mobile.Server.Mocks
{
    public static class EdmModelMock
    {
        public static IEdmModel Create<TModel>(string modelName)
            where TModel : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<TModel>(modelName);
            return builder.GetEdmModel();
        }
    }
}
