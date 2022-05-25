// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class TableColumnAttributeTests
    {
        private TableColumnAttribute attr = new TableColumnAttribute(TableColumnType.Version);

        [Fact]
        public void ColumnType_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.attr, h => h.ColumnType, defaultValue: TableColumnType.Version, roundtripValue: TableColumnType.Deleted);
        }
    }
}
