using Microsoft.Extensions.DependencyInjection;

namespace DynaCache.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddCacheable<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
			where T : class
		{
			services.AddCacheable<T, T>(serviceLifetime);
		}
		
		public static void AddCacheable<TFrom, TTo>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
			where TTo : class
		{
			switch (serviceLifetime)
			{
				case ServiceLifetime.Singleton:
					services.AddSingleton(typeof(TFrom), Cacheable.CreateType<TTo>());
					break;
				case ServiceLifetime.Transient:
					services.AddTransient(typeof(TFrom), Cacheable.CreateType<TTo>());
					break;
				case ServiceLifetime.Scoped:
					services.AddScoped(typeof(TFrom), Cacheable.CreateType<TTo>());
					break;
			}
		}
	}
}