using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryCacheTests.Abstract
{
    public abstract class MemoryCacheTestsBase : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        protected MemoryCacheTestsBase()
        {
            _serviceProvider = BuildServiceProvider();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton(_ => new SemaphoreSlim(1));
            return serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        protected async Task<TItem> CreateScopeAndGetValueFromCache<TItem>(Func<Task<TItem>> getValueTask)
        {
            using var scope = _serviceProvider.CreateScope();
            var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var semaphore = scope.ServiceProvider.GetRequiredService<SemaphoreSlim>();

            await semaphore.WaitAsync();
            try
            {
                return await memoryCache.GetOrCreateAsync(1, async entry => await getValueTask.Invoke());
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}