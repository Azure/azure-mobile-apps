using System;
using System.Runtime.Serialization;

namespace Todo.NetStandard.Common
{
    public class RepositoryException : ApplicationException
    {
        protected RepositoryException()
        {
        }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
