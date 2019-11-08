using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using MediatR;
using HomeControl.Common.Messaging.Messages;
using HomeControl.Common.MQTTService.Config;

namespace HomeControl.Common.MQTTService
{
    public class MQTTClient : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IMqttClient _client;
        private readonly IOptions<MQTTClientConfig> _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MQTTClient(ILogger<MQTTBroker> logger, IMqttClient mqttClient, IOptions<MQTTClientConfig> config, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _client = mqttClient;
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting MQTT Client ({_config.Value.Server}:{_config.Value.Port}) ");

            var optionsBuilder = _getBuilder();

            _client.Disconnected += _mqttClient_Disconnected;
            _client.ApplicationMessageReceived += _mqttClient_ApplicationMessageReceived;
            _client.Connected += _mqttClient_Connected;

            return _client.ConnectAsync(optionsBuilder.Build());
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Client");
            return _client.DisconnectAsync();
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing MQTT Client");
        }

        private MqttClientOptionsBuilder _getBuilder()
        {
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.Value.Server, _config.Value.Port);

            if (_config.Value.Username != null && _config.Value.Password != null)
            {
                optionsBuilder = optionsBuilder.WithCredentials(_config.Value.Username, _config.Value.Password);
            }

            return optionsBuilder;
        }

        private async void _mqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            // Subscribe to topics
            foreach (var topic in _config.Value.TopicList)
            {
                await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
            }

            Console.WriteLine("### SUBSCRIBED ###");
        }

        private void _mqttClient_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.LogInformation("### DISCONNECTED FROM SERVER ###");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            
            try
            {
                var optionsBuilder = _getBuilder();

                _logger.LogInformation("### RECONNECTING ###");
                _client.ConnectAsync(optionsBuilder.Build()).Wait();
                _logger.LogInformation("### RECONNECTING OK ###");
            }
            catch
            {
                _logger.LogInformation("### RECONNECTING FAILED ###");
            }
        }

        private async void _mqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogDebug("### RECEIVED APPLICATION MESSAGE ###" +
                             $"\n Topic = {e.ApplicationMessage.Topic}" +
                             $"\n Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}" +
                             $"\n QoS = {e.ApplicationMessage.QualityOfServiceLevel}" +
                             $"\n Retain = {e.ApplicationMessage.Retain}");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var mediator = serviceProvider.GetService<IMediator>();
                await mediator.Publish(new MqttMessagePublished(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload)));
            }
        }
}
}
