using MinishCapRandomizerSeedGeneratorBot.Threading.Models;

namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class SynchronizedQueue : IDisposable
{
    private volatile Queue<SeedGenerationRequest> _dispatchQueue;
    private volatile int _currentQueueSize;

    private Mutex _mutex;

    public SynchronizedQueue()
    {
        _dispatchQueue = new Queue<SeedGenerationRequest>(16);
        _currentQueueSize = 0;
        _mutex = new Mutex();
    }

    ~SynchronizedQueue()
    {
        Dispose(false);
    }

    public int GetQueueCount()
    {
        _mutex.WaitOne();
        var count = _currentQueueSize;
        _mutex.ReleaseMutex();
        return count;
    }

    public int Enqueue(SeedGenerationRequest request)
    {
        _mutex.WaitOne();
        _dispatchQueue.Enqueue(request);
        Interlocked.Add(ref _currentQueueSize, 1);
        var count = _currentQueueSize;
        _mutex.ReleaseMutex();
        return count;
    }

    public List<SeedGenerationRequest> GetElementsInQueue()
    {
        var list = new List<SeedGenerationRequest>();
        _mutex.WaitOne();
        for (; 0 < _dispatchQueue.Count; )
        {
            list.Add(_dispatchQueue.Dequeue());
        }
        _mutex.ReleaseMutex();
        return list;
    }

    public SeedGenerationRequest Dequeue()
    {
        _mutex.WaitOne();
        var result = _dispatchQueue.Dequeue();
        _mutex.ReleaseMutex();
        return result;
    }

    public bool HasElement()
    {
        _mutex.WaitOne();
        var result = _dispatchQueue.Count > 0;
        _mutex.ReleaseMutex();
        return result;
    }

    public void UpdateQueuePosition()
    {
        _mutex.WaitOne();
        Interlocked.Add(ref _currentQueueSize, -1);
        _mutex.ReleaseMutex();
    }

    private void ReleaseUnmanagedResources()
    {
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _mutex.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}