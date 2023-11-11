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
    public class CamelCasePropertyNamesContractResolverAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (ctx.Result is ObjectResult objectResult)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                jsonSerializerSettings.Converters.Add(new JSelectExpandWrapperConverter());
                jsonSerializerSettings.Converters.Add(new JDynamicTypeWrapperConverter());
                jsonSerializerSettings.Converters.Add(new DateTimeOffsetJsonConverter());
                jsonSerializerSettings.Converters.Add(new StringEnumConverter());

                objectResult.Formatters.Add(new NewtonsoftJsonOutputFormatter(jsonSerializerSettings,
                    ctx.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
                    ctx.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value));
            }
        }
    }
}
