using System;
using System.Threading;
using System.Threading.Tasks;
using DynaCache.Extensions;
using DynaCache.MemoryCache.Extensions;
using DynaCache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DynaCache.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
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
				// The result from the GetRandomNumber method on the service has its results cached for 1 second
				// therefore the displayed results should only change every 4th iteration.
				Console.WriteLine("Random number between 1 and 10: {0}", service.GetRandomNumber(1, 10));
				Console.WriteLine("Random number between 1 and 20: {0}", service.GetRandomNumber(1, 20));

				Thread.Sleep(250);
			}
		}
	}
}