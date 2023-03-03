namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class ConsoleThread
{
    internal ThreadDispatcher Dispatcher { get; set; }

    public ConsoleThread(ThreadDispatcher dispatcher)
    {
        Dispatcher = dispatcher;
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
                hasExited = true;
            }

            Thread.Sleep(1000);
        }
    }
}