using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace push_to_tags
{
    class Tags
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }
    }
}
