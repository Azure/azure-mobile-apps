// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using Microsoft.AspNet.OData.Query;
using System;

namespace Microsoft.Zumo.Server.Test.E2EServer.DataObjects
{
    [Filter]
    [OrderBy]
    public class Movie : EntityTableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }

    [Filter]
    [OrderBy]
    public class RMovie : EntityTableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }
}
