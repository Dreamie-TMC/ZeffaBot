using MinishCapRandomizerSeedGeneratorBot.Actions;

namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class ThreadDispatcher
{
    private SynchronizedQueue _dispatchQueue;
    private SeedGenerationAction _seedGenerationAction;
    private AsyncGenerationAction _asyncGenerationAction;
    private Thread? _currentThread; 

    private volatile bool _exiting;

    public ThreadDispatcher(SynchronizedQueue queue, SeedGenerationAction seedGenerationAction, AsyncGenerationAction asyncGenerationAction)
    {
        _dispatchQueue = queue;
        _seedGenerationAction = seedGenerationAction;
        _asyncGenerationAction = asyncGenerationAction;
    }

    public void SetExitingState()
    {
        _exiting = true;
    }

    public void ThreadLoop()
    {
        while (!_exiting)
        {
            while (_dispatchQueue.HasElement())
            {
                var request = _dispatchQueue.Dequeue();
                if (request.IsAsync)
                {
                    _currentThread = new Thread(async () => await _asyncGenerationAction.GenerateAsyncSeed(request));
                    _currentThread.Start();
                    _currentThread.Join();
                    _dispatchQueue.UpdateQueuePosition();
                }
                else
                {
                    _currentThread = new Thread(async () => await _seedGenerationAction.Randomize(request));
                    _currentThread.Start();
                    _currentThread.Join();
                    _dispatchQueue.UpdateQueuePosition();
                }
            }

            Thread.Sleep(1000);
        }

        _currentThread?.Join();
    }
}