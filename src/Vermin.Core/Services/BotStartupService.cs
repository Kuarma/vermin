using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Vermin.Models;

namespace Vermin.Services;

public class BotStartupService(
        ILogger<BotStartupService> logger,
        DiscordSocketClient socketClient,
        InteractionService interactionService,
        IOptions<TokenOptions> tokenOptions) : IHostedService
{
    private readonly ILogger<BotStartupService> _logger = logger;
    private readonly DiscordSocketClient _socketClient = socketClient;
    private readonly InteractionService _interactionService = interactionService;
    private readonly TokenOptions _tokenOptions = tokenOptions.Value;

    public async Task StartAsync(
            CancellationToken stoppingToken)
    {
        _socketClient.Log += Log;
        _interactionService.Log += Log;

        await _socketClient.LoginAsync(
                tokenType: TokenType.Bot,
                token: _tokenOptions.DiscordBotToken,
                validateToken: true);

        await _socketClient.StartAsync();
    }

    public async Task StopAsync(
            CancellationToken cancellationToken)
    {
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