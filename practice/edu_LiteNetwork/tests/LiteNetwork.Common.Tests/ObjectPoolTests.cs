using LiteNetwork.Internal;
using Xunit;

namespace LiteNetwork.Network.Tests;

public class ObjectPoolTests
{
    private class ObjectTest
    {
        public int Id { get; set; }
    }

    [Fact]
    public void GetAndReturnObjectToPoolTest()
    {
        ObjectPool<ObjectTest> pool = new(() => new ObjectTest());

        ObjectTest firstObject = pool.Get();
        Assert.NotNull(firstObject);

        ObjectTest secondObject = pool.Get();
        Assert.NotNull(secondObject);

        Assert.NotEqual(firstObject, secondObject);

        pool.Return(firstObject);
        pool.Return(secondObject);
    }
}
