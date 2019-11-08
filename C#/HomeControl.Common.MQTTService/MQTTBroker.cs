using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;
using HomeControl.Common.MQTTService.Config;

namespace HomeControl.Common.MQTTService
{
    public class MQTTBroker : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IOptions<MQTTBrokerConfig> _config;
        private readonly IMqttServer _mqttServer;

        public MQTTBroker(ILogger<MQTTBroker> logger, IMqttServer mqttServer, IOptions<MQTTBrokerConfig> config)
        {
            _logger = logger;
            _mqttServer = mqttServer;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Service on port " + _config.Value.Port);

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(1000)
                .WithDefaultEndpointPort(_config.Value.Port)
                .WithConnectionValidator(c => {
                    if (c.Username != _config.Value.Username)
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                        return;
                    }

                    if (c.Password != _config.Value.Password)
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                        return;
                    }

                    c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
                });

            _mqttServer.ClientSubscribedTopic += _mqttServer_ClientSubscribedTopic;
            _mqttServer.ClientUnsubscribedTopic += _mqttServer_ClientUnsubscribedTopic;
            _mqttServer.ClientConnected += _mqttServer_ClientConnected;
            _mqttServer.ClientDisconnected += _mqttServer_ClientDisconnected;
            _mqttServer.ApplicationMessageReceived += _mqttServer_ApplicationMessageReceived;

            return _mqttServer.StartAsync(optionsBuilder.Build());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Service");
            return _mqttServer.StopAsync();
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing MQTT Service");
        }

        private void _mqttServer_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e) => _logger.LogDebug(e.ClientId + " published message to topic " + e.ApplicationMessage.Topic);

        private void _mqttServer_ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e) => _logger.LogDebug(e.ClientId + " Disonnected");

        private void _mqttServer_ClientConnected(object sender, MqttClientConnectedEventArgs e) => _logger.LogDebug(e.ClientId + " Connected");

        private void _mqttServer_ClientUnsubscribedTopic(object sender, MqttClientUnsubscribedTopicEventArgs e) => _logger.LogDebug(e.ClientId + " unsubscribed to " + e.TopicFilter);

        private void _mqttServer_ClientSubscribedTopic(object sender, MqttClientSubscribedTopicEventArgs e) => _logger.LogDebug(e.ClientId + " subscribed to " + e.TopicFilter);
    }
}
