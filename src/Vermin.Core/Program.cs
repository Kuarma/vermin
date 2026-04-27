using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Serilog;
using Vermin.Models;
using Vermin.Services;

namespace Vermin;

internal static class Program
{
    private static async Task Main(
            string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(
                    args: args);

            builder.Services.AddSerilog((services, loggerConfig) => loggerConfig
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        path: "logs/log.log",
                        shared: true,
                        rollingInterval: RollingInterval.Hour,
                        rollOnFileSizeLimit: true,
                        retainedFileTimeLimit: TimeSpan.FromDays(12))
                    .WriteTo.Console()
                    .ReadFrom.Services(services));

            builder.Services.Configure<TokenOptions>(
                    builder.Configuration.GetSection(
                        key: nameof(TokenOptions)));

            builder.Services.AddSingleton(
                    new InteractionServiceConfig
                    {
                        AutoServiceScopes = true,
                        DefaultRunMode = RunMode.Async,
                        UseCompiledLambda = true,
                        EnableAutocompleteHandlers = true,
                        LogLevel = LogSeverity.Info
                    });
            builder.Services.AddSingleton(
                    new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                        AlwaysDownloadUsers = true,
                        LogGatewayIntentWarnings = false,
                        UseInteractionSnowflakeDate = false,
                        MessageCacheSize = 100,
                        LogLevel = LogSeverity.Info
                    });
            builder.Services.AddSingleton<DiscordSocketClient>();
            builder.Services.AddSingleton<InteractionService>();
            builder.Services.AddSingleton<IRestClientProvider>(sp => sp
                    .GetRequiredService<DiscordSocketClient>());

            builder.Services.AddHostedService<BotStartupService>();
            builder.Services.AddHostedService<InteractionStartupService>();

            var app = builder.Build();

            await app.RunAsync();
        }
        catch (Exception exception)
        {
            Log.Fatal(
                    exception: exception,
                    messageTemplate: "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}