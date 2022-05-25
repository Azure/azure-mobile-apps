// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Mobile.Server.Serialization
{
    /// <summary>
    /// Represents the default <see cref="IContractResolver"/> used by <see cref="JsonMediaTypeFormatter"/> but with the addition of supporting
    /// camel cased serialization. It uses the formatter's <see cref="IRequiredMemberSelector"/> to select required members and recognizes the 
    /// <see cref="System.SerializableAttribute"/> type annotation.
    /// </summary>
    public class ServiceContractResolver : JsonContractResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContractResolver"/> with a given <paramref name="formatter"/> 
        /// to use for resolving required members.
        /// </summary>
        /// <param name="formatter">The <see cref="MediaTypeFormatter"/> used to resolve required members.</param>
        public ServiceContractResolver(MediaTypeFormatter formatter)
            : base(formatter)
        {
        }

        /// <inheritdoc />
        protected override string ResolvePropertyName(string propertyName)
        {
            return StringUtils.ToCamelCase(propertyName);
        }
    }
}
