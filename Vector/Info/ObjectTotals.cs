﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vector
{

    public class ObjectTotals
    {

        [JsonProperty("consumers")]
        public int Consumers { get; set; }

        [JsonProperty("queues")]
        public int Queues { get; set; }

        [JsonProperty("exchanges")]
        public int Exchanges { get; set; }

        [JsonProperty("connections")]
        public int Connections { get; set; }

        [JsonProperty("channels")]
        public int Channels { get; set; }
    }

}
