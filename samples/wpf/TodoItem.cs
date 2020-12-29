using Microsoft.WindowsAzure.MobileServices;
using System;

namespace ZumoQuickstart
{
    /// <summary>
    /// Model for a single TodoItem
    /// </summary>
    public class TodoItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }

        [UpdatedAt]
        public DateTimeOffset? UpdatedAt { get; set; }

        [Version]
        public string Version { get; set; }
    }
}
