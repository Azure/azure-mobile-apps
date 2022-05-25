// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.Azure.Mobile.Server;
using Newtonsoft.Json;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class Dates : EntityData
    {
        public DateTime Date { get; set; }
        public DateTimeOffset DateOffset { get; set; }
    }
}
