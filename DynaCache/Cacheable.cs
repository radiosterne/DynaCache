#region Copyright 2012 Mike Goatly, 2014 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	/// <summary>
	/// Cacheable provides the ability to create a dynamic cache proxy type for a class.
	/// </summary>
	public static class Cacheable
	{
		/// <summary>
		/// A cache of dynamic cache types, keyed against the type they were generated for.
		/// </summary>
		private static readonly Dictionary<Type, Type> CacheableTypeCache = new Dictionary<Type, Type>();

		/// <summary>
		/// A collection of types with ToString implementation, suitable for cache key generation.
		/// </summary>
		private static readonly HashSet<Type> ToStringableTypes = new HashSet<Type>
		{
			typeof(string),
			typeof(sbyte),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(byte),
			typeof(ushort),
			typeof(uint),
			typeof(ulong),
			typeof(float),
			typeof(float),
			typeof(double),
			typeof(bool),
			typeof(char),
			typeof(decimal),
			typeof(DateTime)
		};

		/// <summary>
		/// A dictionary of custom converters of objects to its cache key part representation.
		/// </summary>
		private static readonly Dictionary<Type, MethodInfo> CustomConverters = new Dictionary<Type, MethodInfo>(); 

		/// <summary>
		/// The thread synchronization object.
		/// </summary>
		private static readonly object SyncLock = new object();

		/// <summary>
		/// The dynamic assembly that the cacheable types will be created in.
		/// </summary>
		private static readonly AssemblyName AssemblyName = new AssemblyName("Dynamic Cacheable Proxies");

#if DEBUG
		/// <summary>
		/// The dynamic assembly build that will be used to define the cacheable types.
		/// </summary>
		private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.RunAndSave);

		/// <summary>
		/// The dynamic module that the cacheable types will be created in.
		/// </summary>
		private static readonly ModuleBuilder Module = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name, "test.dll");
#else
		/// <summary>
		/// The dynamic assembly build that will be used to define the cacheable types.
		/// </summary>
		private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);

		/// <summary>
		/// The dynamic module that the cacheable types will be created in.
		/// </summary>
		private static readonly ModuleBuilder Module = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name);
#endif

#if DEBUG
		/// <summary>
		/// Saves the underlying assembly.
		/// </summary>
		[Conditional("DEBUG")]
		public static void SaveAssembly()
		{
			AssemblyBuilder.Save("test.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
		}
#endif

		/// <summary>
		/// Creates a dynamic cache proxy type for a given type. Any methods that are decorated with <see cref="CacheableMethodAttribute"/>
		/// will be automatically overridden and their results cached as appropriate.
		/// </summary>
		/// <remarks>
		/// Any methods that are decorated with <see cref="CacheableMethodAttribute"/> must be marked as virtual. Additionally, T must be a publicly
		/// accessible class.
		/// </remarks>
		/// <typeparam name="T">The type to create the cache proxy type for.</typeparam>
		/// <returns>The generated type, or T, if T doesn't have any methods that are decorated with <see cref="CacheableMethodAttribute"/>.</returns>
		public static Type CreateType<T>()
		{
			return CreateType(typeof(T));
		}

		/// <summary>
		/// Creates a dynamic cache proxy type for a given type. Any methods that are decorated with <see cref="CacheableMethodAttribute"/>
		/// will be automatically overridden and their results cached as appropriate.
		/// </summary>
		/// <param name="baseType">The type to create the cache proxy type for.</param>
		/// <remarks>
		/// Any methods that are decorated with <see cref="CacheableMethodAttribute"/> must be marked as virtual. Additionally, 
		/// <paramref name="baseType"/> must be a publicly accessible class.
		/// </remarks>
		/// <returns>The generated type, or <paramref name="baseType"/>, if <paramref name="baseType"/> doesn't have any methods that are 
		/// decorated with <see cref="CacheableMethodAttribute"/>.</returns>
		public static Type CreateType(Type baseType)
		{
			lock (SyncLock)
			{
				Type cacheableType;
				if (CacheableTypeCache.TryGetValue(baseType, out cacheableType))
				{
					return cacheableType;
				}

				cacheableType = CreateCacheableType(baseType);
				CacheableTypeCache.Add(baseType, cacheableType);
				return cacheableType;
			}
		}

		/// <summary>
		/// Adds custom converter from type to string to enable this type usage in cacheable methods parameters
		/// </summary>
		/// <typeparam name="T">Type to convert from</typeparam>
		/// <param name="converter">Converter function from T to string</param>
		public static void AddCustomConverter<T>(Func<T, string> converter) where T : class
		{
			lock (SyncLock)
			{
				CustomConverters.Add(typeof(T), converter.Method);
			}
		}

		/// <summary>
		/// Creates a cacheable type for the given type.
		/// </summary>
		/// <param name="type">The type to create the cacheable proxy for.</param>
		/// <returns>The generated type instance, or the given type if no caching is required for it.</returns>
		private static Type CreateCacheableType(Type type)
		{
			// Get the methods for which we need to override and cache data
			var methods = type.GetMethods()
				.Select(m => new { Method = m, Attribute = (CacheableMethodAttribute)m.GetCustomAttributes(typeof(CacheableMethodAttribute), true).FirstOrDefault() })
				.Where(m => m.Attribute != null)
				.ToArray();

			if (methods.Length == 0)
			{
				// No caching needed - no need to generate a caching layer
				return type;
			}

			if (!type.IsPublic)
			{
				throw new DynaCacheException("Type must be public");
			}

			var cacheableModule = Module.DefineType("Cacheable" + type.Name);
			var cacheServiceField = cacheableModule.DefineField("cacheService", typeof(IDynaCacheService), FieldAttributes.Private);

			cacheableModule.SetParent(type);
			DefineConstructor(type, cacheableModule, cacheServiceField);

			foreach (var method in methods)
			{
				DefineMethod(cacheableModule, cacheServiceField, method.Method, method.Attribute);
			}

			return cacheableModule.CreateType();
		}

		/// <summary>
		/// Defines the constructor for the cacheable type.
		/// </summary>
		/// <param name="type">The base type that the cacheable type derives from.</param>
		/// <param name="cacheableModule">The cacheable module.</param>
		/// <param name="cacheServiceField">The cache service field.</param>
		private static void DefineConstructor(Type type, TypeBuilder cacheableModule, FieldBuilder cacheServiceField)
		{
			var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if (constructors.Length > 1)
			{
				throw new DynaCacheException("Only one constructor is supported at the moment - sorry.");
			}

			var constructor = constructors[0];
			var constructorParameters = (new[] { typeof(IDynaCacheService) }).Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
			var constructorDef = cacheableModule.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameters);
			var gen = constructorDef.GetILGenerator();

			// Call the base constructor
			// Load the pointer to "this"
			gen.Emit(OpCodes.Ldarg_0);

			// Load the other constructor parameters - skipping the first one that is the cache service
			for (var i = 1; i < constructorParameters.Length; i++)
			{
				gen.Emit(OpCodes.Ldarg, i + 1);
			}

			// Make the call
			gen.Emit(OpCodes.Call, constructor);

			// Store the cache service
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stfld, cacheServiceField);

			gen.Emit(OpCodes.Ret);
		}

		/// <summary>
		/// Defines a method in the dynamic type that wraps the caching behavior around the underlying type's method call.
		/// </summary>
		/// <param name="cacheableModule">The cacheable module.</param>
		/// <param name="cacheServiceField">The cache service field.</param>
		/// <param name="methodInfo">The method info.</param>
		/// <param name="cacheParams">The cacheable method attribute data that describes the cache behavior for the method.</param>
		private static void DefineMethod(TypeBuilder cacheableModule, FieldBuilder cacheServiceField, MethodInfo methodInfo, CacheableMethodAttribute cacheParams)
		{
			if (methodInfo.IsFinal)
			{
				throw new DynaCacheException("Cacheable methods must be overridable.");
			}

			var methodParams = methodInfo.GetParameters().ToArray();
			if (methodParams.Any(p => p.ParameterType.IsByRef))
			{
				throw new DynaCacheException("Reference parameters (out/ref) are not supported for cacheable methods.");
			}

			foreach(var methodParam in methodParams)
			{
				var paramType = methodParam.ParameterType;

				if (!(paramType.IsEnum || paramType.ContainsGenericParameters || ToStringableTypes.Contains(paramType) || (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>) && ToStringableTypes.Contains(paramType.GetGenericArguments()[0]))))
				{
					if (paramType.GetCustomAttributes(typeof (ToStringableAttribute), false).Any())
					{
						ToStringableTypes.Add(paramType);
					}
					else if(!CustomConverters.ContainsKey(paramType))
					{
						throw new DynaCacheException(
							String.Format(
							"Cacheable method has parameter without unique ToString() implementation: consider writing it and mark parameter type with ToStringable attribute." +
							"Method: {0}, Parameter {1} of type {2}",
							methodInfo.Name, methodParam.Name, paramType));
					}
				}
			}

			var method = cacheableModule.DefineMethod(
				methodInfo.Name,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot,
				methodInfo.ReturnType,
				methodParams.Select(pa => pa.ParameterType).ToArray());

			var il = method.GetILGenerator();
			var cacheKeyLocal = il.DeclareLocal(typeof(string));
			var returnValueLocal = il.DeclareLocal(method.ReturnType);
			var cacheOutValueLocal = il.DeclareLocal(method.ReturnType);

			var cacheKeyTemplate = CreateCacheKeyTemplate(methodInfo, methodParams);
			FormatCacheKey(methodParams, il, cacheKeyLocal, cacheKeyTemplate);
			TryGetFromCache(cacheOutValueLocal, cacheKeyLocal, returnValueLocal, il, cacheServiceField);
			CallBaseMethod(methodInfo, methodParams, il, returnValueLocal);
			CacheResult(il, returnValueLocal, cacheKeyLocal, cacheServiceField, cacheParams);

			il.Emit(OpCodes.Ldloc, returnValueLocal);
			il.Emit(OpCodes.Ret);
		}

		private static readonly Dictionary<Type, string> TypeFormats = new Dictionary<Type, string>
															  {
																  { typeof(DateTime), ":O" },
																  { typeof(DateTime?), ":O" },
																  { typeof(DateTimeOffset), ":O" },
																  { typeof(DateTimeOffset?), ":O" }
															  };

		/// <summary>
		/// Creates a template for a method's cache key, based on the class it is contained within and the number
		/// of parameters it takes. The cache key template is used at runtime to generate a unique cache key for 
		/// a method and it's parameter variations.
		/// </summary>
		/// <param name="methodInfo">The method information.</param>
		/// <param name="methodParams">The method's parameters.</param>
		/// <returns>The cache key template.</returns>
		private static string CreateCacheKeyTemplate(MethodInfo methodInfo, ParameterInfo[] methodParams)
		{
			var cacheKeyTemplate = new StringBuilder();
			// ReSharper disable once PossibleNullReferenceException -- we know that DeclaringType exists for sure
			cacheKeyTemplate.Append(methodInfo.DeclaringType.FullName)
				.Append('_')
				.Append(methodInfo);

			for (var i = 0; i < methodParams.Length; i++)
			{
				cacheKeyTemplate.Append(".{").Append(i);
				string format;
				if (TypeFormats.TryGetValue(methodParams[i].ParameterType, out format))
				{
					cacheKeyTemplate.Append(format);
				}
				
				cacheKeyTemplate.Append('}');
			}

			return cacheKeyTemplate.ToString();
		}

		/// <summary>
		/// Generates the IL to formats the cache key.
		/// </summary>
		/// <param name="methodParams">The method parameters.</param>
		/// <param name="il">The il generator to use.</param>
		/// <param name="cacheKeyLocal">The local variable that contains a reference to the calculated cache key.</param>
		/// <param name="cacheKeyTemplate">The cache key template that will be combined with the method parameters to create
		/// the formatted cache key.</param>
		private static void FormatCacheKey(ParameterInfo[] methodParams, ILGenerator il, LocalBuilder cacheKeyLocal, string cacheKeyTemplate)
		{
			var objectArrayLocal = il.DeclareLocal(typeof(object[]));

			il.Emit(OpCodes.Ldc_I4, methodParams.Length);
			il.Emit(OpCodes.Newarr, typeof(object));
			il.Emit(OpCodes.Stloc, objectArrayLocal);

			for (var i = 0; i < methodParams.Length; i++)
			{
				il.Emit(OpCodes.Ldloc, objectArrayLocal);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldarg, i + 1);
				if (!methodParams[i].ParameterType.IsClass)
				{
					il.Emit(OpCodes.Box, methodParams[i].ParameterType);
				} 
				else if (CustomConverters.ContainsKey(methodParams[i].ParameterType))
				{
					il.Emit(OpCodes.Call, CustomConverters[methodParams[i].ParameterType]);
				}
				
				il.Emit(OpCodes.Stelem_Ref);
			}

			il.Emit(OpCodes.Ldstr, cacheKeyTemplate);
			il.Emit(OpCodes.Stloc, cacheKeyLocal);
			il.Emit(OpCodes.Ldloc, cacheKeyLocal);
			il.Emit(OpCodes.Ldloc, objectArrayLocal);
			il.EmitCall(OpCodes.Call, typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) }), null);
			il.Emit(OpCodes.Stloc, cacheKeyLocal);
		}

		/// <summary>
		/// Writes the IL to try reading the already cached data from the cache, using the cache key.
		/// </summary>
		/// <param name="cacheOutValueLocal">The local variable that will contain the out value from the TryGetCachedObject method.</param>
		/// <param name="cacheKeyLocal">The local variable that contains a reference to the calculated cache key.</param>
		/// <param name="returnValueLocal">The local that the result of reading from the cache will be stored into.</param>
		/// <param name="il">The il generator to use.</param>
		/// <param name="cacheServiceField">The field that contains a reference to the cache service.</param>
		private static void TryGetFromCache(LocalBuilder cacheOutValueLocal, LocalBuilder cacheKeyLocal, LocalBuilder returnValueLocal, ILGenerator il, FieldBuilder cacheServiceField)
		{
			var notInCacheLabel = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, cacheServiceField);
			il.Emit(OpCodes.Ldloc, cacheKeyLocal);
			il.Emit(OpCodes.Ldloca_S, cacheOutValueLocal);
			var methodInfo = typeof(IDynaCacheService)
				.GetMethod("TryGetCachedObject")
				.MakeGenericMethod(returnValueLocal.LocalType);
			il.EmitCall(OpCodes.Callvirt, methodInfo, null);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brtrue, notInCacheLabel);

			// Value was in cache
			il.Emit(OpCodes.Ldloc, cacheOutValueLocal);
			il.Emit(OpCodes.Ret);

			// Value wasn't in cache
			il.MarkLabel(notInCacheLabel);
		}

		/// <summary>
		/// Generates the IL to call the corresponding method in the base class.
		/// </summary>
		/// <param name="methodInfo">The method info for the base method call.</param>
		/// <param name="methodParams">The method parameters.</param>
		/// <param name="il">The il generator to use.</param>
		/// <param name="returnValueLocal">The local that the result of calling the base method will be stored into.</param>
		private static void CallBaseMethod(MethodInfo methodInfo, ParameterInfo[] methodParams, ILGenerator il, LocalBuilder returnValueLocal)
		{
			il.Emit(OpCodes.Ldarg_0);
			for (var i = 0; i < methodParams.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i + 1);
			}

			il.EmitCall(OpCodes.Call, methodInfo, null);

			il.Emit(OpCodes.Stloc, returnValueLocal);
		}

		/// <summary>
		/// Defines the IL that caches the result of calling the base method.
		/// </summary>
		/// <param name="il">The il generator to use.</param>
		/// <param name="returnValueLocal">The local that the result of reading from the cache will be stored into.</param>
		/// <param name="cacheKeyLocal">The local variable that contains a reference to the calculated cache key.</param>
		/// <param name="cacheServiceField">The field that contains a reference to the cache service.</param>
		/// <param name="cacheParams">The cacheable method attribute data that describes the cache behavior for the method.</param>
		private static void CacheResult(ILGenerator il, LocalBuilder returnValueLocal, LocalBuilder cacheKeyLocal, FieldBuilder cacheServiceField, CacheableMethodAttribute cacheParams)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, cacheServiceField);
			il.Emit(OpCodes.Ldloc, cacheKeyLocal);
			il.Emit(OpCodes.Ldloc, returnValueLocal);
			il.Emit(OpCodes.Ldc_I4, cacheParams.CacheSeconds);

			var methodInfo = typeof(IDynaCacheService)
				.GetMethod("SetCachedObject")
				.MakeGenericMethod(returnValueLocal.LocalType);
			il.EmitCall(OpCodes.Callvirt, methodInfo, null);
		}
	}
}
