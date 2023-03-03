using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinishCapRandomizerSeedGeneratorBot;
using MinishCapRandomizerSeedGeneratorBot.Actions;
using MinishCapRandomizerSeedGeneratorBot.Api;
using MinishCapRandomizerSeedGeneratorBot.Api.Handlers;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using RandomizerCore.Controllers;

internal class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Console.SetOut(TextWriter.Synchronized(Console.Out));
            Console.SetError(TextWriter.Synchronized(Console.Error));
            var services = BuildServiceProvider();
            var connector = services.GetRequiredService<APIConnector>();
            await connector.BuildSocketClient();
            var dispatcher = services.GetRequiredService<ThreadDispatcher>();
            var thread = new Thread(dispatcher.ThreadLoop);
            thread.Start();
            var consoleHandler = services.GetRequiredService<ConsoleThread>();
            var consoleThread = new Thread(consoleHandler.Loop);
            consoleThread.Start();
            thread.Join();
            services.GetRequiredService<SynchronizedQueue>().Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception occurred!");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.TryAddSingleton<DiscordSocketClient>();
        serviceCollection.TryAddSingleton<DiscordSocketConfig>();
        serviceCollection.TryAddSingleton<APIConnector>();
        serviceCollection.TryAddSingleton<SlashCommandHandler>();
        serviceCollection.TryAddSingleton<GuildJoinHandler>();
        serviceCollection.TryAddSingleton<BotInitializer>();
        serviceCollection.TryAddSingleton<SeedHandler>();
        serviceCollection.TryAddSingleton<AboutHandler>();
        serviceCollection.TryAddSingleton<AsyncHandler>();
        serviceCollection.TryAddSingleton<UpdateInfoHandler>();
        serviceCollection.TryAddSingleton<ShufflerController>();
        serviceCollection.TryAddSingleton<SynchronizedQueue>();
        serviceCollection.TryAddSingleton<SeedGenerationAction>();
        serviceCollection.TryAddSingleton<AsyncGenerationAction>();
        serviceCollection.TryAddSingleton<ThreadDispatcher>();
        serviceCollection.TryAddSingleton<AppSettings>();
        serviceCollection.TryAddSingleton<ConsoleThread>();

        return serviceCollection.BuildServiceProvider();
    }
}