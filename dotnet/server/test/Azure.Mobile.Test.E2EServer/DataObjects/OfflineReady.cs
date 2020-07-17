using Azure.Mobile.Server.Entity;
using System;

namespace Azure.Mobile.Test.E2EServer.DataObjects
{
    public class OfflineReady : EntityTableData
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Float { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool Bool { get; set; }
    }
}
