using System;
using System.Collections.Concurrent;

namespace LiteNetwork.Internal;

internal class ObjectPool<TObject> where TObject : class
{
    private readonly ConcurrentBag<TObject> _objects;
    private readonly Func<TObject> _objectFactory;

    public ObjectPool(Func<TObject> objectFactory)
    {
        _objects = new();
        _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
    }

    public TObject Get()
    {
        return _objects.TryTake(out var @object) ? @object : _objectFactory();
    }

    public void Return(TObject @object)
    {
        _objects.Add(@object);
    }
}
