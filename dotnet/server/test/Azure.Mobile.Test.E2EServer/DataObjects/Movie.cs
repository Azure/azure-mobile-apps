using Azure.Mobile.Server.Entity;
using System;

namespace Azure.Mobile.Test.E2EServer.DataObjects
{
    public class Movie : EntityTableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }
}
