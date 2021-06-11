using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace push_to_tags
{
    public class TodoItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string Tags { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }

        public string DisplayText
        {
            get { return string.Format("{0} ({1})", Text, Tags); }
        }
    }
}
