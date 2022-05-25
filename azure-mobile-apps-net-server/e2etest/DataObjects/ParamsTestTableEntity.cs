// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.Azure.Mobile.Server.Tables;

namespace ZumoE2EServerApp.DataObjects
{
    public sealed class ParamsTestTableEntity : ITableData
    {
        public int Id { get; set; }
        public string Parameters { get; set; }

        public ParamsTestTableEntity()
        {
            this.Id = 1;
        }

        string ITableData.Id { get; set; }

        byte[] ITableData.Version { get; set; }

        DateTimeOffset? ITableData.CreatedAt { get; set; }

        DateTimeOffset? ITableData.UpdatedAt { get; set; }

        bool ITableData.Deleted { get; set; }
    }
}