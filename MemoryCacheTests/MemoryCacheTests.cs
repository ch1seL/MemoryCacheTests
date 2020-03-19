using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MemoryCacheTests.Abstract;
using Xunit;

namespace MemoryCacheTests
{
    public class MemoryCacheTests : MemoryCacheTestsBase
    {
        private static async Task<int> GetLongTaskValue(Action callback)
        {
            callback?.Invoke();
            await Task.Delay(TimeSpan.FromSeconds(1));
            return 1;
        }

        [Fact]
        public async Task ThousandQueryToCacheShouldInvokeTaskOnce()
        {
            var counter = 0;

            await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ =>
                CreateScopeAndGetValueFromCache(() => GetLongTaskValue(() => counter++))));

            counter.Should().Be(1);
        }

        [Fact]
        public async Task ValueFromLongTaskShouldBeObtainedFromCache()
        {
            await CreateScopeAndGetValueFromCache(() => GetLongTaskValue(null));

            var res = await CreateScopeAndGetValueFromCache(() => Task.FromResult(2));

            res.Should().Be(1);
        }
    }
}