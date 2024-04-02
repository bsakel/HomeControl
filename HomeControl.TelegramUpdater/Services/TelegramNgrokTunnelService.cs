using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeControl.TelegramUpdater.Services
{
    public class TelegramNgrokTunnelService : BackgroundService
    {
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _config;
        private readonly ILogger<TelegramNgrokTunnelService> _logger;

        private string _publicUrl;

        public TelegramNgrokTunnelService(IServer server, IHostApplicationLifetime hostApplicationLifetime, IConfiguration config, ILogger<TelegramNgrokTunnelService> logger)
        {
            _server = server;
            _hostApplicationLifetime = hostApplicationLifetime;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForApplicationStarted();

            var urls = _server.Features.Get<IServerAddressesFeature>()!.Addresses;
            // Use https:// if you authenticated ngrok, otherwise, you can only use http://
            var localUrl = urls.Single(u => u.StartsWith("https://"));

            _logger.LogInformation("Starting ngrok tunnel for {LocalUrl}", localUrl);
            var ngrokTask = StartNgrokTunnel(localUrl, stoppingToken);

            _publicUrl = await GetNgrokPublicUrl();
            _logger.LogInformation("Public ngrok URL: {NgrokPublicUrl}", _publicUrl);

            _logger.LogInformation("Update Telegram Webhook");
            await UpdateTelegramWebhook(_publicUrl);

            await ngrokTask;

            _logger.LogInformation("Ngrok tunnel stopped");
        }

        private Task WaitForApplicationStarted()
        {
            var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _hostApplicationLifetime.ApplicationStarted.Register(() => completionSource.TrySetResult());
            return completionSource.Task;
        }

        private CommandTask<CommandResult> StartNgrokTunnel(string localUrl, CancellationToken stoppingToken)
        {
            var cliWrapCommand = Cli.Wrap("ngrok")
                .WithArguments(args => args
                    .Add("http")
                    .Add(localUrl)
                    .Add("--log")
                    .Add("stdout")
                    .Add("--authtoken")
                    .Add(_config.GetValue<string>("NgrokToken")))
                .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogDebug(s)))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)));

            _logger.LogInformation($"Starting ngrok with arguments: {cliWrapCommand.Arguments}");

            return cliWrapCommand.ExecuteAsync(stoppingToken);
        }

        private async Task<string> GetNgrokPublicUrl()
        {
            using var httpClient = new HttpClient();
            for (var ngrokRetryCount = 0; ngrokRetryCount < 10; ngrokRetryCount++)
            {
                _logger.LogDebug("Get ngrok tunnels attempt: {RetryCount}", ngrokRetryCount + 1);

                try
                {
                    var json = await httpClient.GetFromJsonAsync<JsonNode>("http://127.0.0.1:4040/api/tunnels");
                    var publicUrl = json["tunnels"].AsArray()
                        .Select(e => e["public_url"].GetValue<string>())
                        .SingleOrDefault(u => u.StartsWith("https://"));
                    if (!string.IsNullOrEmpty(publicUrl)) return publicUrl;
                }
                catch
                {
                    _logger.LogError("Get Ngrok public url threw an error");
                }

                await Task.Delay(200);
            }

            throw new Exception("Ngrok dashboard did not start in 10 tries");
        }

        private async Task<bool> UpdateTelegramWebhook(string url)
        {
            var telegramBotToken = _config.GetValue<string>("TelegramBotToken");
            var setWebhookUrl = $"https://api.telegram.org/bot{telegramBotToken}/setWebhook?url={url}/api/TelegramWebhook";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(setWebhookUrl);

            _logger.LogInformation("Telegram webhook set response code: {responseCode}", response.StatusCode.ToString());

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
