using System.Collections.Generic;

namespace HomeControl.Common.MQTTService.Config
{
    public class MQTTClientConfig : MQTTBrokerConfig
    {
        public string Server { get; set; }

        public List<string> TopicList { get; set; }
    }
}
