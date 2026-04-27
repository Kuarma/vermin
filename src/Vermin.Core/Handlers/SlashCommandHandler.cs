using Discord.Interactions;

namespace Vermin.Handlers;

public class SlashCommandHandler(
        ILogger<SlashCommandHandler> logger) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<SlashCommandHandler> _logger = logger;
}