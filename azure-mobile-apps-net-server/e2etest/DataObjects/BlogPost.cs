// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.Azure.Mobile.Server;
using Newtonsoft.Json;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class BlogPost : EntityData
    {
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public bool ShowComments { get; set; }
        public string Data { get; set; }
    }
}
