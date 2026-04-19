using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Vermin.Services;

public class InteractionStartupService(
        InteractionService service,
        IServiceProvider serviceProvider,
        DiscordSocketClient socketClient,
        ILogger<InteractionStartupService> logger) : BackgroundService
{
    private readonly InteractionService _interactionService = service;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly DiscordSocketClient _socketClient = socketClient;
    private readonly ILogger<InteractionStartupService> _logger = logger;

    protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
    {
        await _interactionService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _serviceProvider);

        _socketClient.Ready += RegisterCommandsAsync;
        _socketClient.InteractionCreated += InteractionCreatedAsync;
        _interactionService.InteractionExecuted += InteractionExecutedAsync;
    }

    private async Task InteractionCreatedAsync(
            SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(
                    client: _socketClient,
                    interaction: interaction);

            await _interactionService.ExecuteCommandAsync(
                    context: context,
                    services: _serviceProvider);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                    exception: exception,
                    message: "Interaction execution failed");
        }
    }

    private async Task InteractionExecutedAsync(
        ICommandInfo command,
        IInteractionContext context,
        IResult result)
    {
        if (result.IsSuccess)
            return;

        _logger.LogError(
                message: "Command {CommandName} failed - Reason: {ErrorReason}",
                args: [command.Name, result.ErrorReason]);

        var response = $"Command {command.Name} failed";

        if (context.Interaction.HasResponded)
        {
            await context.Interaction.FollowupAsync(
                    text: response,
                    ephemeral: true);
            return;
        }

        await context.Interaction.RespondAsync(
                text: response,
                ephemeral: true);
    }

    private async Task RegisterCommandsAsync()
    {
        await _interactionService.RegisterCommandsGloballyAsync(
                deleteMissing: true);
    }
}