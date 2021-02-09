using System;
using System.Threading;
using DynaCache.MemoryCache;
using DynaCache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DynaCache.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();

			services.AddSingleton<IDynaCacheService, MemoryCacheService>();

			services.AddTransient(typeof(IRandomService), Cacheable.CreateType<RandomService>());

			var serviceProvider = services.BuildServiceProvider();

			// Use the DI container to construct our cacheable concrete instance of ITestService
			var service = serviceProvider.GetRequiredService<IRandomService>();

			for (var i = 0; i < 12; i++)
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