using DynaCache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DynaCache.MemoryCache.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddMemoryCacheService(this IServiceCollection services)
		{
			services.AddSingleton<IDynaCacheService, MemoryCacheService>();
		}
	}
}