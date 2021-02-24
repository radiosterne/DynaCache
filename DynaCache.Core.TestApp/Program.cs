using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DynaCache.Extensions;
using DynaCache.MemoryCache.Extensions;
using DynaCache.Services;
using DynaCache.TestApp.CustomConverters;
using Microsoft.Extensions.DependencyInjection;

namespace DynaCache.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			Cacheable.AddCustomConverter<Test>(CacheConverters.TestConvert);
			Cacheable.AddCustomConverter<List<int>>(CacheConverters.ListConvert);
			var services = new ServiceCollection();

			services.AddMemoryCacheService();
			services.AddCacheable<IRandomService, RandomService>();

			services.AddTransient<Random>();

			var serviceProvider = services.BuildServiceProvider();

			var service = serviceProvider.GetRequiredService<IRandomService>();

			var cacheService = serviceProvider.GetRequiredService<IDynaCacheService>();

			Task.Run(() =>
			{
				while (true)
				{
					Console.ReadKey();
					cacheService.ClearCache();
				}
			});

			while (true)
			{
				var test = new Test()
				{
					Kekos = 1
				};

				var list = new List<int>
				{
					1
				};
				
				// The result from the GetRandomNumber method on the service has its results cached for 1 second
				// therefore the displayed results should only change every 4th iteration.
				Console.WriteLine("Random number between 1 and 10: {0}", service.GetRandomNumber(list));

				Thread.Sleep(250);
			}
		}
	}
}