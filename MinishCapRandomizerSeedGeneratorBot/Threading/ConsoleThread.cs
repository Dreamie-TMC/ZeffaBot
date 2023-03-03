namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class ConsoleThread
{
    internal ThreadDispatcher Dispatcher { get; set; }
    internal ScheduledEventDispatcher ScheduledEventDispatcher { get; set; }

    public ConsoleThread(ThreadDispatcher dispatcher, ScheduledEventDispatcher scheduledEventDispatcher)
    {
        Dispatcher = dispatcher;
        ScheduledEventDispatcher = scheduledEventDispatcher;
    }
    
    public void Loop()
    {
        var hasExited = false;
        while (!hasExited)
        {
            var result = Console.ReadLine();

            if (!string.IsNullOrEmpty(result) && result.Equals("Exit", StringComparison.OrdinalIgnoreCase))
            {
                Dispatcher.SetExitingState();
                ScheduledEventDispatcher.SetExiting();
                hasExited = true;
            }

            Thread.Sleep(1000);
        }
    }
}