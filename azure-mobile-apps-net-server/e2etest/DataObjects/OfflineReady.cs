// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.Azure.Mobile.Server;

namespace ZumoE2EServerApp.DataObjects
{
    public class OfflineReady : EntityData
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public double Float { get; set; }

        public DateTimeOffset Date { get; set; }

        public bool Bool { get; set; }
    }
}