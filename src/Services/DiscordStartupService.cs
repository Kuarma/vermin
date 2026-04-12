using Discord;
using Discord.WebSocket;
using Gitier.Helper;
using Microsoft.Extensions.Options;

namespace Gitier.Services;

public class DiscordStartupService(
        ILogger<DiscordStartupService> logger,
        DiscordSocketClient socketClient,
        IOptions<DiscordTokenOptions> tokenOptions) : IHostedService
{
    private readonly ILogger<DiscordStartupService> _logger = logger;
    private readonly DiscordSocketClient _socketClient = socketClient;
    private readonly IOptions<DiscordTokenOptions> _tokenOptions = tokenOptions;

    public async Task StartAsync(
            CancellationToken cancellationToken)
    {
        _socketClient.Log += Log;

        await _socketClient.LoginAsync(
                tokenType: TokenType.Bot,
                token: _tokenOptions.Value.DiscordBotToken,
                validateToken: true);

        await _socketClient.StartAsync();
    }

    public async Task StopAsync(
            CancellationToken cancellationToken)
    {
        _socketClient.Log -= Log;

        await _socketClient.StopAsync();
    }
    private Task Log(
            LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            _ => LogLevel.Debug
        };

        _logger.Log(
                logLevel: level,
                eventId: default,
                state: message,
                exception: message.Exception,
                formatter: (state, exception) => $"{state.Message} - {exception}");

        return Task.CompletedTask;
    }
}