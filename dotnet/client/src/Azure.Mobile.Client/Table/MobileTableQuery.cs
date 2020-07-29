namespace Azure.Mobile.Client.Table
{
    /// <summary>
    /// The fields necessary to be sent in an OData Query to the
    /// Azure Mobile Apps backend.
    /// </summary>
    public class MobileTableQuery
    {
        /// <summary>
        /// The OData <c>$filter</c> query param.  If null, does not
        /// send the parameter.
        /// </summary>
        public string Filter { get; set; } = null;

        /// <summary>
        /// The OData <c>$orderBy</c> query param.  If null, does not
        /// send the parameter.
        /// </summary>
        public string OrderBy { get; set; } = null;

        /// <summary>
        /// The OData <c>$skip</c> query param.  If negative, does not send
        /// the parameter.
        /// </summary>
        public int Skip { get; set; } = -1;

        /// <summary>
        /// The OData <c>$top</c> query param.  If negative, does not send
        /// the parameter.
        /// </summary>
        public int Top { get; set; } = -1;

        /// <summary>
        /// The OData <c>$count</c> query param.  Is not sent if false.
        /// </summary>
        public bool IncludeCount { get; set; } = false;

        /// <summary>
        /// Set on a table controller with soft-delete enabled, this returns the
        /// deleted records as well.
        /// 
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }
}
