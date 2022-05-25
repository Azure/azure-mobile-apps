// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// An abstract implementation of the <see cref="ITableData"/> interface required by <see cref="TableController{T}"/>
    /// as well as an implementation of <see cref="TableEntity"/> required by Azure Table Storage. It provides a 
    /// default mapping from the <see cref="TableEntity"/> properties including partition key, row key, and timestamp
    /// into the corresponding properties on <see cref="ITableData"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class StorageData : TableEntity, ITableData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageData"/> class.
        /// </summary>
        protected StorageData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageData"/> class
        /// with a given <paramref name="partitionKey"/> and <paramref name="rowKey"/>.
        /// </summary>
        /// <param name="partitionKey">The partition key of the <see cref="TableEntity"/> to be initialized.</param>
        /// <param name="rowKey">The row key of the <see cref="TableEntity"/> to be initialized.</param>
        protected StorageData(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        /// <summary>
        /// The id is composed by combining the <see cref="M:PartitionKey"/> and the <see cref="M:RowKey"/> using a 
        /// <see cref="CompositeTableKey"/> instance which serializes the partition key and row key as a comma separated
        /// tuple of values. By setting this property, the partition key and row key will also get updated. As such the
        /// id property itself is not mapped to the actual storage table as it strictly contains the same information provided
        /// by the partition key and row key.
        /// </summary>
        [NotMapped, IgnoreProperty]
        public string Id
        {
            get
            {
                CompositeTableKey key = new CompositeTableKey(this.PartitionKey, this.RowKey);
                return key.ToString();
            }

            set
            {
                CompositeTableKey key;
                if (!CompositeTableKey.TryParse(value, out key) || key.Segments.Count != 2)
                {
                    string msg = ASResources.StorageTable_InvalidKey.FormatForUser(value);
                    throw new ArgumentException(msg);
                }

                this.PartitionKey = key.Segments[0];
                this.RowKey = key.Segments[1];
            }
        }

        /// <summary>
        /// The version is a <see cref="T:byte[]"/> representation of the <see cref="M:Etag"/> property which is maintained by 
        /// the Azure storage SDK. The etag represents the version of this entity as obtained from the server and by setting
        /// this property the etag is also updated. As for the id property, this property is not mapped to the actual storage 
        /// table as its information is provided in the etag.
        /// </summary>
        [NotMapped, IgnoreProperty]
        public byte[] Version
        {
            get
            {
                return this.ETag != null ? Encoding.UTF8.GetBytes(this.ETag) : null;
            }

            set
            {
                this.ETag = value != null ? Encoding.UTF8.GetString(value) : null;
            }
        }

        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// This property timestamp at which the entity was created. By setting this property, the <see cref="M:Timestamp"/>
        /// property is updated as well. The property is not mapped to the actual storage table as the information is maintained
        /// by the timestamp property.
        /// </summary>
        [NotMapped, IgnoreProperty]
        public DateTimeOffset? UpdatedAt
        {
            get
            {
                return this.Timestamp;
            }

            set
            {
                this.Timestamp = value.HasValue ? value.Value : DateTimeOffset.UtcNow;
            }
        }

        /// <summary>
        /// Indicates whether this entity has been marked for deletion.
        /// </summary>
        public bool Deleted { get; set; }
    }
}
