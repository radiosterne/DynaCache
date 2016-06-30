using Ninject.Modules;

namespace DynaCache.MemoryCache.Ninject
{
	public class MemoryCacheModule : NinjectModule
	{
		public override void Load()
		{
			//Bind<IConcreteKeyPatternProvider>().To<MemoryCacheKeyPatternProvider>().WhenInjectedInto<MemoryCacheService>().InSingletonScope();
		}
	}
}
