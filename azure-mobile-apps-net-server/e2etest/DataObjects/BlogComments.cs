// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.Azure.Mobile.Server;
using Newtonsoft.Json;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class BlogComments : EntityData
    {
        public string PostId { get; set; }
        public string CommentText { get; set; }
        public string Name { get; set; }
        public int Test { get; set; }
    }
}
