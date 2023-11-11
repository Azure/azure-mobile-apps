using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Datasync.Converters;
using Newtonsoft.Json.Converters;

namespace Microsoft.AspNetCore.Datasync.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CamelCasePropertyNamesContractResolverAttribute : ResultFilterAttribute
    {
        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                IContractResolver contractResolver = new CamelCasePropertyNamesContractResolver();

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ContractResolver = contractResolver
                };

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ContractResolver = contractResolver
                };

                jsonSerializerSettings.Converters.Add(new JSelectExpandWrapperConverter());
                jsonSerializerSettings.Converters.Add(new JDynamicTypeWrapperConverter());
                jsonSerializerSettings.Converters.Add(new DateTimeOffsetJsonConverter());
                jsonSerializerSettings.Converters.Add(new StringEnumConverter());

                objectResult.Formatters.Add(new NewtonsoftJsonOutputFormatter(jsonSerializerSettings,
                    context.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
                    context.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value,
                    new MvcNewtonsoftJsonOptions()));
            }
            return base.OnResultExecutionAsync(context, next);
        }
    }
}
