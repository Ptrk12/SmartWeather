using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Core.Extensions
{
    public static class CacheExtension
    {
        public async static Task<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> task, DistributedCacheEntryOptions? options = null)
        {
            var cachedData = await cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                Console.WriteLine("GET IN CACHE");
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            T data = await task();

            if (data != null)
            {
                options ??= new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                await cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);
                Console.WriteLine("SAVE FROM CACHE");
            }

            return data;
        }
    }
}
