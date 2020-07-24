using Azure.Mobile.Server.Entity;

namespace E2EServer.DataObjects
{
    public class Unit : EntityTableData
    {
        public string Data { get; set; }
    }

    public class SUnit : Unit
    {
    }

    public class HUnit : Unit
    {
    }
}
