namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class SynchronizedList<E>
{
    private volatile List<E> _list;
    private volatile Mutex _mutex;

    public SynchronizedList()
    {
        _list = new List<E>();
        _mutex = new Mutex();
    }
    
    public void Add(E obj)
    {
        _mutex.WaitOne();
        _list.Add(obj);
        _mutex.ReleaseMutex();
    }

    public void Remove(E obj)
    {
        try
        {
            _mutex.WaitOne();
            if (!_list.Contains(obj)) return;
            
            _list.Remove(obj);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public bool Contains(E obj)
    {
        _mutex.WaitOne();
        var hasElement = _list.Contains(obj);
        _mutex.ReleaseMutex();
        return hasElement;
    }
}