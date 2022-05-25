// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Web.Http.Tracing;

namespace Microsoft.Azure.Mobile.Diagnostics
{
    internal class TraceLevelHelper : EnumHelper<TraceLevel>
    {
        private static readonly TraceLevelHelper HelperInstance = new TraceLevelHelper();

        public static TraceLevelHelper Instance
        {
            get
            {
                return HelperInstance;
            }
        }

        public override bool IsDefined(TraceLevel level)
        {
            return TraceLevel.Off <= level && level <= TraceLevel.Fatal;
        }

        /// <summary>
        /// Parses a trace level app setting string value (as set by the RP)
        /// into a TraceLevel.
        /// </summary>
        /// <remarks>
        /// This special method is needed because the values the RP sets don't
        /// map one to one with the TraceLevel enumeration values.
        /// </remarks>
        internal static TraceLevel ParseLogLevelSetting(string logLevelSettingValue)
        {
            if (string.IsNullOrEmpty(logLevelSettingValue))
            {
                return TraceLevel.Error;
            }

            switch (logLevelSettingValue.ToLowerInvariant())
            {
                case "verbose":
                    return TraceLevel.Debug;
                case "information":
                case "info":
                    return TraceLevel.Info;
                case "warning":
                case "warn":
                    return TraceLevel.Warn;
                case "error":
                    return TraceLevel.Error;
                default:
                    return TraceLevel.Error;
            }
        }
    }
}
