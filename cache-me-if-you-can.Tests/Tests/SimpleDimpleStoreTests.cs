namespace cache_me_if_you_can.Tests;

public class SimpleDimpleStoreTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SimpleDimpleStore _simpleDimpleStore = new();
    private long _simpleStoreSetItemsCount;

    public SimpleDimpleStoreTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var array = new[]
        {
            "SET user:1 data1",
            "SET user:2 data2",
            "SET user:3 data3",
            "SET user:4 data4",
            "SET user:5 data5"
        };

        foreach (var item in array)
        {
            var multispan = ComandanteParser.Parse(item.AsSpan());
            _simpleDimpleStore.Set(multispan.Key.ToString(), multispan.Value.ToByteArray());
        }

        var stats = _simpleDimpleStore.GetStats();
        _simpleStoreSetItemsCount = stats.setCount;
    }

    [Theory]
    [InlineData(3,
        new[]
        {
            "SET user:6 data6",
            "SET user:8 data8",
            "SET user:10 data10"
        })]
    [InlineData(1,
        new[]
        {
            "SET user:12 data12"
        })]
    public void SimpleDimpleStore_SetValue_InlineDataCountEqualStatsSet(int inputCount, string[] strings)
    {
        Parallel.ForEach(strings, str =>
        {
            try
            {
                var multispan = ComandanteParser.Parse(str.AsSpan());
                _simpleDimpleStore.Set(multispan.Key.ToString(), multispan.Value.ToByteArray());
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        });

        var stats = _simpleDimpleStore.GetStats();
        _simpleStoreSetItemsCount += inputCount;
        Assert.Equal(_simpleStoreSetItemsCount, stats.setCount);
    }

    [Theory]
    [InlineData(2,
        new[]
        {
            "DEL user:1",
            "DEL user:3"
        })]
    [InlineData(1,
        new[]
        {
            "SET user:5 data5"
        })]
    public void SimpleDimpleStore_DeleteValue_InlineDataCountEqualStatsDelete(int inputCount, string[] strings)
    {
        Parallel.ForEach(strings, str =>
        {
            try
            {
                var multispan = ComandanteParser.Parse(str.AsSpan());
                _simpleDimpleStore.Delete(multispan.Key.ToString());
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        });

        var stats = _simpleDimpleStore.GetStats();
        Assert.Equal(inputCount, stats.deleteCount);
    }

    [Theory]
    [InlineData(2,
        new[]
        {
            "GET user:1",
            "GET user:3"
        })]
    [InlineData(1,
        new[]
        {
            "GET user:1"
        })]
    public void SimpleDimpleStore_GetValue_InlineDataCountEqualStatsGet(int totalCount, string[] strings)
    {
        Parallel.ForEach(strings, str =>
        {
            try
            {
                var multispan = ComandanteParser.Parse(str.AsSpan());
                _simpleDimpleStore.Get(multispan.Key.ToString());
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        });

        var stats = _simpleDimpleStore.GetStats();
        Assert.Equal(totalCount, stats.getCount);
    }

    [Fact]
    public async Task SimpleDimpleStore_TestRaceCondition_GetStatsEqual()
    {
        const int taskCount = 100;
        const int iterationsPerTask = 100;
        const int totalCount = taskCount * iterationsPerTask;

        await RunParallelOperation(taskCount, iterationsPerTask, (taskId, j) =>
        {
            var str = $"SET user:{taskId} data{j}";
            var multispan = ComandanteParser.Parse(str.AsSpan());
            _simpleDimpleStore.Set(multispan.Key.ToString(), multispan.Value.ToByteArray());
        });

        await RunParallelOperation(taskCount, iterationsPerTask, (taskId, _) =>
        {
            var str = $"GET user:{taskId}";
            var multispan = ComandanteParser.Parse(str.AsSpan());
            _simpleDimpleStore.Get(multispan.Key.ToString());
        });

        await RunParallelOperation(taskCount, iterationsPerTask, (taskId, _) =>
        {
            var str = $"DEL user:{taskId}";
            var multispan = ComandanteParser.Parse(str.AsSpan());
            _simpleDimpleStore.Delete(multispan.Key.ToString());
        });

        var stats = _simpleDimpleStore.GetStats();

        Assert.Multiple(
            () => Assert.Equal(_simpleStoreSetItemsCount + totalCount, stats.setCount),
            () => Assert.Equal(totalCount, stats.getCount),
            () => Assert.Equal(totalCount, stats.deleteCount)
        );
    }

    private static async Task RunParallelOperation(
        int taskCount, 
        int iterationsPerTask, 
        Action<int, int> operation)
    {
        var barrier = new Barrier(taskCount);
    
        var tasks = Enumerable.Range(0, taskCount)
            .Select(taskId => Task.Run(() =>
            {
                barrier.SignalAndWait();

                for (var j = 0; j < iterationsPerTask; j++)
                {
                    operation(taskId, j);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }
}