using System.Collections.Concurrent;

public class PlayerLockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public SemaphoreSlim Get(string playerId)
    {
        return _locks.GetOrAdd(playerId, _ => new SemaphoreSlim(1, 1));
    }

    public void Remove(string playerId)
    {
        _locks.TryRemove(playerId, out var sem);
        sem?.Dispose();
    }

    //TODO: in future could create this 
    //public async Task WithLock(string key, Func<Task> action)
    //It would paralelize operations

    /// Wrappers
    public async Task<T> WithLock<T>(string playerId, Func<Task<T>> action)
    {
        var sem = Get(playerId);

        await sem.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task WithLock(string playerId, Func<Task> action)
    {
        var sem = Get(playerId);

        await sem.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            sem.Release();
        }
    }
}