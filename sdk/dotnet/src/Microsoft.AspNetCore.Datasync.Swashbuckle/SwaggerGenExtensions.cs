using Microsoft.Extensions.DependencyInjection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Extensions to enable the developer to wire in the Datasync Controllers Swashbuckle
    /// <see cref="IDocumentFilter"/> into the Swagger pipeline.
    /// </summary>
    public static class DatasyncSwashbuckleExtensions
    {
        public static void AddDatasyncControllers(this SwaggerGenOptions options)
        {
            options.DocumentFilter<DatasyncDocumentFilter>();
        }
    }
}
