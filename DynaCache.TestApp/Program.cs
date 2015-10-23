//#region Copyright 2012 Mike Goatly
//// This source is subject to the the MIT License (MIT)
//// All rights reserved.
//#endregion

//namespace DynaCache.TestApp
//{
//	using System;
//	using System.Threading;
//	using Ninject;

//	/// <summary>
//	/// A simple test application to demonstrate the use of the DynaCache library.
//	/// </summary>
//	public class Program
//	{
//		/// <summary>
//		/// The main application entry point.
//		/// </summary>
//		public static void Main()
//		{
//			// Using ninject to do the dependency injection - there's no reason why you shouldn't use something else though.
//			var kernel = new StandardKernel();

//			// Bind the IDynaCacheService to the our sample memory cache service.
//			kernel.Bind<IDynaCacheService>().To<TestMemoryCacheService>();

//			// Bind our test service interface to its _cacheable_ concrete implementation
//			kernel.Bind<IRandomService>().To(Cacheable.CreateType<RandomService>());

//			// Use the DI container to construct our cacheable concrete instance of ITestService
//			var service = kernel.Get<IRandomService>();

//			for (var i = 0; i < 12; i++)
//			{
//				// The result from the GetRandomNumber method on the service has its results cached for 1 second
//				// therefore the displayed results should only change every 4th iteration.
//				Console.WriteLine("Random number between 1 and 10: {0}", service.GetRandomNumber(1, 10));
//				Console.WriteLine("Random number between 1 and 20: {0}", service.GetRandomNumber(1, 20));

//				Thread.Sleep(250);
//			}
//		}
//	}
//}
