using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MQTTnet;

namespace HomeControl.Common.MQTTService
{
    public static class MQTTExtensionMethods
    {
        public static IServiceCollection AddMQTTBroker(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var mqttConfiguration = services
                .BuildServiceProvider()
                .GetService<IConfiguration>()
                .GetSection("MQTTBroker");

            services.Configure<Config.MQTTBroker>(mqttConfiguration);

            services.AddSingleton(new MqttFactory().CreateMqttServer());
            services.AddSingleton<IHostedService, MQTTBroker>();

            return services;
        }

        public static IServiceCollection AddMQTTClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var mqttConfiguration = services
                .BuildServiceProvider()
                .GetService<IConfiguration>()
                .GetSection("MQTTClient");

            services.Configure<Config.MQTTClient>(mqttConfiguration);

            services.AddSingleton(new MqttFactory().CreateMqttClient());
            services.AddSingleton<IHostedService, MQTTClient>();

            return services;
        }
    }
}
