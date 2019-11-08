using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using MQTTnet;
using MQTTnet.Client;
using HomeControl.Common.Hardware.Interfaces;
using HomeControl.Common.Messaging.Messages;

namespace HomeControl.Common.Messaging.Handlers
{
    public class RelayBoardMqttMessagePublishedHandler : INotificationHandler<MqttMessagePublished>
    {
        private readonly ILogger<RelayBoardMqttMessagePublishedHandler> _logger;
        private readonly IRelayBoard _relayBoard;

        public RelayBoardMqttMessagePublishedHandler(ILogger<RelayBoardMqttMessagePublishedHandler> logger, IRelayBoard relayBoard)
        {
            _logger = logger;
            _relayBoard = relayBoard;
        }

        public async Task Handle(MqttMessagePublished notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handle: MqttMessagePublishedHandler");

            //TODO: Handle: MqttMessagePublishedHandler

            await Task.CompletedTask;
        }
    }
}
