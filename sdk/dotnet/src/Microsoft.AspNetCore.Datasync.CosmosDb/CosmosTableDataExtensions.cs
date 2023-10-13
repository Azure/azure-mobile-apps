using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.CosmosDb
{
    public static class CosmosTableDataExtensions
    {
        /// <summary>
        /// Based on the <see cref="CosmosTableData"/> <paramref name="entity"/> try and 
        /// build a partition key based on the <paramref name="partitionKeyPropertyNames"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="partitionKeyPropertyNames"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static PartitionKey BuildPartitionKey(this CosmosTableData entity, List<string> partitionKeyPropertyNames)
        {
            ArgumentNullException.ThrowIfNull(partitionKeyPropertyNames, nameof(partitionKeyPropertyNames));
            if (!partitionKeyPropertyNames.Any()) 
            {
                throw new ArgumentException("partitionKeyPropertyNames is empty");
            }
            var partitionKeyBuilder = new PartitionKeyBuilder();

            foreach (var propertyName in partitionKeyPropertyNames)
            {
                PropertyInfo propertyInfo = entity.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Property '{propertyName}' not found on entity.");
                }
                
                var value = propertyInfo.GetValue(entity);
                if (value == null)
                {
                    throw new ArgumentNullException($"Value of property '{propertyName}' cannot be null.");
                }

                switch (value)
                {
                    case double doubleValue:
                        partitionKeyBuilder.Add(doubleValue);
                        break;
                    case bool boolValue:
                        partitionKeyBuilder.Add(boolValue);
                        break;
                    default:
                        partitionKeyBuilder.Add(value.ToString());
                        break;
                }
            }

            return partitionKeyBuilder.Build();
        }
    }
}
