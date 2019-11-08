using MediatR;

namespace HomeControl.Common.Messaging.Messages
{
    public class MqttMessagePublished : INotification
    {
        public string Topic { get; }
        public string Payload { get;  }

        public MqttMessagePublished(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}
