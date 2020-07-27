using System;

namespace Azure.Mobile.Server.Utils
{
    /// <summary>
    /// Parameter validation utility methods.
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the provided param is null
        /// </summary>
        /// <param name="param">the parameter value</param>
        /// <param name="paramName">the parameter name</param>
        public static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
