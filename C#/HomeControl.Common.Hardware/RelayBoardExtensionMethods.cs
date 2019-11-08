using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using HomeControl.Common.Hardware.Interfaces;

namespace HomeControl.Common.Hardware
{
    public static class RelayBoardExtensionMethods
    {
        public static IServiceCollection AddRelayBoard(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var relayConfiguration = services
                    .BuildServiceProvider()
                    .GetService<IConfiguration>()
                    .GetSection("Relay");

                services.Configure<RelayBoardConfig>(relayConfiguration);

                services.AddSingleton(InitRelayList(services, x => Relay.Initialize(x)));
            }
            else
            {
                services.Configure<RelayBoardConfig>(config => {
                    config.RelayPins = new List<int>() { 0, 1 };
                });

                services.AddSingleton(InitRelayList(services, x => FakeRelay.Initialize(x)));
            }

            services.AddScoped<IRelayBoard, RelayBoard>();

            return services;
        }

        private static IList<IRelay> InitRelayList(IServiceCollection services, Func<int, IRelay> action)
        {
            var pinList = services
                            .BuildServiceProvider()
                            .GetService<IOptions<RelayBoardConfig>>()
                            .Value.RelayPins;

            var relayList = new List<IRelay>();
            foreach (var pinNumber in pinList)
            {
                relayList.Add(action.Invoke(pinNumber));
            }

            return relayList;
        }
    }
}
