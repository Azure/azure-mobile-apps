using System;

namespace Azure.Mobile.Client.Table
{
    /// <summary>
    /// Thrown exception indicating a conflict - either a precondition failed or a conflict
    /// operation.
    /// </summary>
    /// <typeparam name="T">The entity type causing the conflict</typeparam>
    public class ConflictException<T> : RequestFailedException where T : TableData
    {
        /// <summary>
        /// Creates a new <see cref="ConflictException{T}"/> based on a <see cref="Response"/>
        /// </summary>
        /// <param name="response">The <see cref="Response"/> to use in generating the properties</param>
        public ConflictException(Response<T> response): base(response.GetRawResponse().Status, response.GetRawResponse().ReasonPhrase)
        {
            Value = response.Value;
            var headers = response.GetRawResponse().Headers;

            if (headers.ETag.HasValue)
            {
                ETag = headers.ETag.Value;
            }

            string lastModifiedHeader;
            if (headers.TryGetValue("Last-Modified", out lastModifiedHeader))
            {
                LastModified = DateTimeOffset.Parse(lastModifiedHeader);
            }
        }

        /// <summary>
        /// If the response includes the value of the item, then this is set to the value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// If the response includes an ETag, then it is decoded and placed in this property.
        /// </summary>
        public ETag ETag { get; }

        /// <summary>
        /// If the response includes a Last-Modified header, then it is decoded and placed in this property.
        /// </summary>
        public DateTimeOffset LastModified { get; }
    }
}
