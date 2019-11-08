using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using MQTTnet;
using MQTTnet.Client;
using HomeControl.Common.Messaging.Messages;

namespace HomeControl.Common.Messaging.Handlers
{
    public class PinStateChangedHandler : INotificationHandler<PinStateChanged>
    {
        private readonly ILogger<PinStateChangedHandler> _logger;
        private readonly IMqttClient _mqttClient;

        public PinStateChangedHandler(ILogger<PinStateChangedHandler> logger, IMqttClient mqttClient)
        {
            _logger = logger;
            _mqttClient = mqttClient;
        }

        public async Task Handle(PinStateChanged notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handle: (PinStateChanged) pin -> { notification.PinNumber.ToString() } changed to { notification.NewState.ToString() }");

            var message = new MqttApplicationMessageBuilder()
                                .WithTopic($"/relayboard/notifications/")
                                .WithPayload(new
                                    {
                                        Pin = notification.PinNumber,
                                        State = notification.NewState
                                    }.ToString())
                                .WithExactlyOnceQoS()
                                .Build();

            await _mqttClient.PublishAsync(message);
        }
    }
}
