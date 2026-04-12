using Discord;
using Discord.WebSocket;
using Gitier.Helper;
using Gitier.Services;
using Serilog;

namespace Gitier;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

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

            builder.Services.Configure<DiscordTokenOptions>(
                    builder.Configuration.GetSection(
                        key: nameof(DiscordTokenOptions)));

            builder.Services.AddHostedService<DiscordStartupService>();

            builder.Services.AddSingleton(_ =>
                    {
                        var config = new DiscordSocketConfig
                        {
                            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                            AlwaysDownloadUsers = true,
                            LogGatewayIntentWarnings = false,
                            UseInteractionSnowflakeDate = true,
                            MessageCacheSize = 100,
                        };

                        return new DiscordSocketClient(config);
                    });

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