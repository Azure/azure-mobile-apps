using NSwag;

namespace Samples.NSwag.Processors
{
    /// <summary>
    /// A set of extension methods for working with OpenApi types.
    /// </summary>
    public static class OpenApiExtensions
    {
        /// <summary>
        /// Determines if the operation contains the specified request header.
        /// </summary>
        /// <param name="operation">The operation to check.</param>
        /// <param name="headerName">The name of the header to check for.</param>
        /// <returns></returns>
        public static bool ContainsRequestHeader(this OpenApiOperation operation, string headerName)
            => operation.Parameters.Any(parameter => parameter.Name == headerName && parameter.Kind == OpenApiParameterKind.Header);
    }
}
