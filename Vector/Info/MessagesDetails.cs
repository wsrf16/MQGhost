using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vector
{
    public class MessagesDetails
    {

        [JsonProperty("rate")]
        public float Rate { get; set; }
    }

    public class MessagesReadyDetails
    {

        [JsonProperty("rate")]
        public float Rate { get; set; }
    }

    public class MessagesUnacknowledgedDetails
    {

        [JsonProperty("rate")]
        public float Rate { get; set; }
    }

    public class QueueTotals
    {

        [JsonProperty("messages")]
        public int Messages { get; set; }

        [JsonProperty("messages_details")]
        public MessagesDetails MessagesDetails { get; set; }

        [JsonProperty("messages_ready")]
        public int MessagesReady { get; set; }

        [JsonProperty("messages_ready_details")]
        public MessagesReadyDetails MessagesReadyDetails { get; set; }

        [JsonProperty("messages_unacknowledged")]
        public int MessagesUnacknowledged { get; set; }

        [JsonProperty("messages_unacknowledged_details")]
        public MessagesUnacknowledgedDetails MessagesUnacknowledgedDetails { get; set; }
    }

}
