using Newtonsoft.Json;
using System;

namespace TheThingsNetworkGateway
{
    public class Telemetry
    {
        public UInt32 Level { get; set; }
        public string Schema { get; set; } = "1";
        public string ToJson(UInt32 level)
        {
            this.Level = level;
            return JsonConvert.SerializeObject(this);
        }
    }
}