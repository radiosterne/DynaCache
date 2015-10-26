#region Copyright 2012 Mike Goatly, 2014-2015 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System.Text.RegularExpressions;

namespace DynaCache
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;
	using System.Threading.Tasks;

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
				var methodInfo = converter.Method;
				if (methodInfo.IsPublic)
				{
					//if underlying method is public, cache it right away
					CustomConverters.Add(typeof (T), converter.GetMethodInfo());
				}
				else
				{
					//here's a tricky part:
					//method is not accessible for our proxy method
					//so we can either cache the delegate itself, pass it to proxy and Invoke there, which I'm apparently too lazy to do
					//or create wrapper with public method which will call invoke for us
					var converterType = Module.DefineType(String.Format("DynaCache_{0}_StringConverter", CreateSafeStringForMethodAndTypeName(typeof(T).FullName)), TypeAttributes.Abstract | TypeAttributes.Sealed);
					var delegateField = converterType.DefineField("_delegate", typeof (Func<T, string>),
						FieldAttributes.Public | FieldAttributes.Static);

					var invokeMethod = converterType.DefineMethod("CallDelegate",
						MethodAttributes.Static| MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot, typeof (string),
						new[] {typeof (T)});

					var invokeMethodGenerator = invokeMethod.GetILGenerator();
					invokeMethodGenerator.Emit(OpCodes.Ldsfld, delegateField);
					invokeMethodGenerator.Emit(OpCodes.Ldarg_0);
					invokeMethodGenerator.Emit(OpCodes.Callvirt, typeof (Func<T, string>).GetMethod("Invoke"));
					invokeMethodGenerator.Emit(OpCodes.Ret);

					var createdConverterType = converterType.CreateType();
					//we can not call delegateField.SetValue(null, converter), it's not currently supported
					//retrieving field again from type and setting
					createdConverterType.GetField("_delegate").SetValue(null, converter);

					CustomConverters.Add(typeof (T), invokeMethod);
				}
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

			var signatureStringRepresentation = CreateMethodSignatureStringRepresentation(methodInfo);
			var cacheKeyTemplate = CreateCacheKeyTemplate(signatureStringRepresentation, methodParams);
			var methodActionWrapperConstructorAndDelegate = CreateMethodActionWrapperType(signatureStringRepresentation, methodInfo, methodParams);

			var method = cacheableModule.DefineMethod(
				methodInfo.Name,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot,
				methodInfo.ReturnType,
				methodParams.Select(pa => pa.ParameterType).ToArray());

			var il = method.GetILGenerator();
			var cacheKeyLocal = il.DeclareLocal(typeof(string));
			var returnValueLocal = il.DeclareLocal(method.ReturnType);
			var cacheOutValueLocal = il.DeclareLocal(typeof(MemoryCacheEntry));
			var stringFormatParametersArrayLocal = il.DeclareLocal(typeof(object[]));

			//=========== formatting cache key ===========
			//create object[] to pass to String.Format as parameters
			il.Emit(OpCodes.Ldc_I4, methodParams.Length);
			il.Emit(OpCodes.Newarr, typeof(object));
			il.Emit(OpCodes.Stloc, stringFormatParametersArrayLocal);

			for (var i = 0; i < methodParams.Length; i++)
			{
				//loading array and index
				il.Emit(OpCodes.Ldloc, stringFormatParametersArrayLocal);
				il.Emit(OpCodes.Ldc_I4, i);

				//loading argument to push
				il.Emit(OpCodes.Ldarg, i + 1);
				if (!methodParams[i].ParameterType.IsClass)
				{
					//box if it's value type
					il.Emit(OpCodes.Box, methodParams[i].ParameterType);
				}
				else if (CustomConverters.ContainsKey(methodParams[i].ParameterType))
				{
					//call converter if specified
					il.Emit(OpCodes.Call, CustomConverters[methodParams[i].ParameterType]);
				}

				//now we have array, index and object (maybe its converted representation) on stack
				il.Emit(OpCodes.Stelem_Ref);
			}

			//calling format and storing
			il.Emit(OpCodes.Ldstr, cacheKeyTemplate);
			il.Emit(OpCodes.Ldloc, stringFormatParametersArrayLocal);
			il.EmitCall(OpCodes.Call, StringFormat, null);
			il.Emit(OpCodes.Stloc, cacheKeyLocal);

			//=========== loading value at cache key from underlying cache service ===========

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, cacheServiceField);

			//we can actually eliminate the need in cacheKeyLocal by using it right from the stack
			//but it greatly diminishes readability
			il.Emit(OpCodes.Ldloc, cacheKeyLocal);
			il.EmitCall(OpCodes.Callvirt, DynaCacheServiceTryGetCachedObject, null);
			il.Emit(OpCodes.Stloc, cacheOutValueLocal);

			//=========== implementing switch table ===========

			// 0 == Actual
			// 1 == Loading
			// 2 == Stale
			var switchLabels = new[]
			{
				il.DefineLabel(),
				il.DefineLabel(),
				il.DefineLabel()
			};

			var defaultLabel = il.DefineLabel();

			//pushing result state to stack
			il.Emit(OpCodes.Ldloc, cacheOutValueLocal);
			il.Emit(OpCodes.Callvirt, MemoryCacheEntryStateGetter);

			//switch table or jumping to default label
			il.Emit(OpCodes.Switch, switchLabels);
			il.Emit(OpCodes.Br, defaultLabel);
			
			//hard case — before returning value, initiate asynchronous loading
			il.MarkLabel(switchLabels[2]);

			//try get loading lock
			il.Emit(OpCodes.Ldloc, cacheOutValueLocal);
			il.EmitCall(OpCodes.Call, MemoryCacheEntryGetLoadingLock, null);
			//if failed, return value
			il.Emit(OpCodes.Brfalse, switchLabels[0]);

			//load wrapper constructor parameters
			il.Emit(OpCodes.Ldarg_0);
			for (var i = 0; i < methodParams.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i + 1);
			}

			il.Emit(OpCodes.Newobj, methodActionWrapperConstructorAndDelegate.Item1);
			//calling method to return us delegate
			il.Emit(OpCodes.Call, methodActionWrapperConstructorAndDelegate.Item2);

			//calling Task.Run with our method action wrapper delegate
			il.EmitCall(OpCodes.Call, TaskRun
				.MakeGenericMethod(method.ReturnType), null);

			//now we have our task on stack,
			//so attaching continuation to it

			//Create renewer
			il.Emit(OpCodes.Ldloc, cacheOutValueLocal);
			il.Emit(OpCodes.Newobj, RenewWrapperConstructor);

			//Get delegate from renewer
			il.Emit(OpCodes.Call, RenewWrapperGetRenewerWrapperDelegate.MakeGenericMethod(method.ReturnType));

			//call ContinueWith
			il.Emit(OpCodes.Call, GetTypedTaskContinueWith(method.ReturnType));

			//our task is fire-and-forget, so pop return value of ContinueWith from stack
			il.Emit(OpCodes.Pop);

			//easy case — simply pushing value to stack if it's actual or loading already in progress
			il.MarkLabel(switchLabels[0]);
			il.MarkLabel(switchLabels[1]);

			il.Emit(OpCodes.Ldloc, cacheOutValueLocal);
			il.Emit(OpCodes.Callvirt, MemoryCacheEntryCalueGetter);
			il.Emit(returnValueLocal.LocalType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, returnValueLocal.LocalType);
			il.Emit(OpCodes.Ret);

			//default case — get value from underlying function, store it in cache and return
			il.MarkLabel(defaultLabel);

			//load all our parameters
			il.Emit(OpCodes.Ldarg_0);
			for (var i = 1; i <= methodParams.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i);
			}

			//call base method and store it 
			il.EmitCall(OpCodes.Call, methodInfo, null);
			il.Emit(OpCodes.Stloc, returnValueLocal);

			//load service
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, cacheServiceField);
			il.Emit(OpCodes.Ldloc, cacheKeyLocal);
			il.Emit(OpCodes.Ldloc, returnValueLocal);
			// ReSharper disable once PossibleNullReferenceException -- not null
			if (!returnValueLocal.LocalType.IsClass)
			{
				il.Emit(OpCodes.Box, returnValueLocal.LocalType);
			}

			il.Emit(OpCodes.Ldc_I4, cacheParams.CacheSeconds);

			//send base method return value to cache service
			il.EmitCall(OpCodes.Callvirt, DynaCacheServiceSetCachedObject, null);

			il.Emit(OpCodes.Ldloc, returnValueLocal);
			il.Emit(OpCodes.Ret);
		}

		/// <summary>
		/// Creates a template for a method's cache key, based on the class it is contained within and the number
		/// of parameters it takes. The cache key template is used at runtime to generate a unique cache key for 
		/// a method and it's parameter variations.
		/// </summary>
		/// <param name="signatureRepresentation">The method information as string.</param>
		/// <param name="methodParams">The method's parameters.</param>
		/// <returns>The cache key template.</returns>
		private static string CreateCacheKeyTemplate(string signatureRepresentation, ParameterInfo[] methodParams)
		{
			var cacheKeyTemplate = new StringBuilder();
			// ReSharper disable once PossibleNullReferenceException -- we know that DeclaringType exists for sure
			cacheKeyTemplate.Append(signatureRepresentation);

			for (var i = 0; i < methodParams.Length; i++)
			{
				cacheKeyTemplate.Append("_{").Append(i);
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
		/// Creates safe representation of method signature, including method declaring type.
		/// </summary>
		/// <param name="methodInfo">The method information.</param>
		/// <returns>The method information as string.</returns>
		private static string CreateMethodSignatureStringRepresentation(MethodInfo methodInfo)
		{
			// ReSharper disable once PossibleNullReferenceException -- not null by design
			// we're doing a lot of Replace's to make type fit into our module without creating another namespace
			return String.Format("{0}_{1}", CreateSafeStringForMethodAndTypeName(methodInfo.DeclaringType.FullName), CreateSafeStringForMethodAndTypeName(methodInfo.ToString()));
		}

		/// <summary>
		/// Creates wrapper type for a method, storing method parameters as private fields and exposing parameterless Func delegate to call it
		/// </summary>
		/// <param name="signatureStringRepresentation">The method information as string.</param>
		/// <param name="methodInfo">The method information.</param>
		/// <param name="methodParams">The method parameters information.</param>
		/// <returns></returns>
		private static Tuple<ConstructorInfo, MethodInfo> CreateMethodActionWrapperType(string signatureStringRepresentation, MethodInfo methodInfo, ParameterInfo[] methodParams)
		{
			var wrapperType = Module.DefineType(String.Format("MethodActionWrapperType_{0}", signatureStringRepresentation));

			//in our type, we'll have a private field for every original method parameter and another one for an object on which we'll call it

			// ReSharper disable once AssignNullToNotNullAttribute -- definitely not null by design
			var calleeObject = wrapperType.DefineField("_object", methodInfo.DeclaringType, FieldAttributes.Private);

			// ReSharper disable once PossibleNullReferenceException -- definitely not null by design
			var parametersAsFields =
				methodParams.Select(
				param => wrapperType.DefineField(String.Format("_{0}", param.Name), GetConstructedGenericType(param.ParameterType, methodInfo.DeclaringType), FieldAttributes.Private))
					.ToArray();

			//a constructor to initialize our private fields with passed parameters
			var constructor = wrapperType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
				new[] {methodInfo.DeclaringType}.Concat(methodParams.Select(p => p.ParameterType)).ToArray());

			var constructorIl = constructor.GetILGenerator();

			//load this and call Object.ctor()
			constructorIl.Emit(OpCodes.Ldarg_0);
			constructorIl.Emit(OpCodes.Call, DefaultObjectConstructorInfo);

			//load this and entry on stack and set field value
			constructorIl.Emit(OpCodes.Ldarg_0);
			constructorIl.Emit(OpCodes.Ldarg_1);
			constructorIl.Emit(OpCodes.Stfld, calleeObject);

			for (var i = 0; i < methodParams.Length; i++)
			{
				//load this and parameter on stack and set field value
				constructorIl.Emit(OpCodes.Ldarg_0);
				constructorIl.Emit(OpCodes.Ldarg, i + 2);
				constructorIl.Emit(OpCodes.Stfld, parametersAsFields[i]);
			}

			constructorIl.Emit(OpCodes.Ret);

			//defining method T MethodAction()
			var method = wrapperType.DefineMethod("MethodAction", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot);

			var realReturnType = GetConstructedGenericType(methodInfo.ReturnType, methodInfo.DeclaringType);

			method.SetReturnType(realReturnType);

			var methodIl = method.GetILGenerator();

			//load entry on stack
			methodIl.Emit(OpCodes.Ldarg_0);
			methodIl.Emit(OpCodes.Ldfld, calleeObject);

			//load parameters on stack
			for (var i = 0; i < methodParams.Length; i++)
			{
				methodIl.Emit(OpCodes.Ldarg_0);
				methodIl.Emit(OpCodes.Ldfld, parametersAsFields[i]);
			}

			//call the method as usual
			methodIl.EmitCall(OpCodes.Callvirt, methodInfo, null);

			//now we have original method return value on stack so we just return it
			methodIl.Emit(OpCodes.Ret);

			//defining method Func<T> GetMethodActionDelegate()
			//we create method delegate in separate method in this type to make logic of our basic method simplier
			var methodDelegate = wrapperType.DefineMethod("GetMethodActionDelegate", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot);

			var delegateType = typeof(Func<>).MakeGenericType(realReturnType);

			methodDelegate.SetReturnType(delegateType);

			var methodDelegateIl = methodDelegate.GetILGenerator();

			//loading this into stack and getting pointer to a function
			methodDelegateIl.Emit(OpCodes.Ldarg_0);
			methodDelegateIl.Emit(OpCodes.Ldftn, method);

			//creating delegate and returning it
			methodDelegateIl.Emit(OpCodes.Newobj, delegateType.GetConstructors()[0]);
			methodDelegateIl.Emit(OpCodes.Ret);

			wrapperType.CreateType();

			return Tuple.Create((ConstructorInfo)constructor, (MethodInfo)methodDelegate);
		}

		private static Type GetConstructedGenericType(Type possiblyOpenType, Type declaringType)
		{
			return possiblyOpenType.IsGenericTypeDefinition
				? possiblyOpenType.MakeGenericType(declaringType.GetGenericArguments())
				: possiblyOpenType.IsGenericParameter
					? declaringType.GetGenericArguments()[possiblyOpenType.GenericParameterPosition]
					: possiblyOpenType;
		}

		/// <summary>
		/// Creates wrapper type for MemoryCacheEntry, storing it as private fields and exposing Action{Task{T}} delegate to call it
		/// </summary>
		static Cacheable()
		{
			//Creating in our dynamic assembly wrapper type
			//for renewing cache entry state
			var renewerType = Module.DefineType("CacheEntryRenewerWrapper");
			var field = renewerType.DefineField("_entry", typeof(MemoryCacheEntry), FieldAttributes.Private | FieldAttributes.InitOnly);
			var constructor = renewerType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(MemoryCacheEntry) });
			var constructorIl = constructor.GetILGenerator();

			//load this and call Object.ctor()
			constructorIl.Emit(OpCodes.Ldarg_0);
			constructorIl.Emit(OpCodes.Call, DefaultObjectConstructorInfo);

			//load this and entry on stack and set field value
			constructorIl.Emit(OpCodes.Ldarg_0);
			constructorIl.Emit(OpCodes.Ldarg_1);
			constructorIl.Emit(OpCodes.Stfld, field);
			constructorIl.Emit(OpCodes.Ret);

			//caching ConstructorInfo for reusage
			RenewWrapperConstructor = constructor;

			//defining method void RenewWrapper<T>(Task<T> task)
			//if task completed succesfuully, renew entry value
			//else reset it's state to Stale
			var renewerMethod = renewerType.DefineMethod(
				"RenewWrapper",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot);

			renewerMethod.SetReturnType(typeof(void));

			var typeBuilders = renewerMethod.DefineGenericParameters("T");
			var typebuilder = typeBuilders[0];

			var parameterType = typeof(Task<>).MakeGenericType(typebuilder);

			renewerMethod.SetParameters(parameterType);

			var renewerMethodIl = renewerMethod.GetILGenerator();

			var elseBranchLabel = renewerMethodIl.DefineLabel();
			//if(task.IsCompleted)
			renewerMethodIl.Emit(OpCodes.Ldarg_1);
			renewerMethodIl.Emit(OpCodes.Callvirt, TaskIsCompletedGetter);
			renewerMethodIl.Emit(OpCodes.Brfalse, elseBranchLabel);

			//load entry on stack
			renewerMethodIl.Emit(OpCodes.Ldarg_0);
			renewerMethodIl.Emit(OpCodes.Ldfld, field);

			//load task result on stack
			renewerMethodIl.Emit(OpCodes.Ldarg_1);
			renewerMethodIl.Emit(OpCodes.Callvirt, TypeBuilder.GetMethod(parameterType, TaskResultGetter));

			//box it to be sure
			renewerMethodIl.Emit(OpCodes.Box, typebuilder);

			//call Renew on entry with task result
			renewerMethodIl.Emit(OpCodes.Callvirt, MemoryCacheEntryRenew);
			renewerMethodIl.Emit(OpCodes.Ret);

			// else task failed, should revert entry to stale state
			renewerMethodIl.MarkLabel(elseBranchLabel);

			//load entry on stack
			renewerMethodIl.Emit(OpCodes.Ldarg_0);
			renewerMethodIl.Emit(OpCodes.Ldfld, field);

			//call LoadingFailed
			renewerMethodIl.Emit(OpCodes.Callvirt, MemoryCacheEntryLoadingFailed);

			renewerMethodIl.Emit(OpCodes.Ret);

			//defining method Action<Task<T>> GetRenewerWrapperDelegate<T>()
			//wraps RenewWrapper in Action to make Cacheable target method easier
			var methodDelegate = renewerType.DefineMethod("GetRenewerWrapperDelegate", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot);

			var delegateTypeBuilders = methodDelegate.DefineGenericParameters("T");
			var delegateTypeBuilder = delegateTypeBuilders[0];

			var delegateParameterType = typeof(Task<>).MakeGenericType(delegateTypeBuilder);

			var delegateType = typeof(Action<>).MakeGenericType(delegateParameterType);

			methodDelegate.SetReturnType(delegateType);

			var methodDelegateIl = methodDelegate.GetILGenerator();

			//loading this into stack and getting pointer to a function
			methodDelegateIl.Emit(OpCodes.Ldarg_0);
			methodDelegateIl.Emit(OpCodes.Ldftn, renewerMethod.MakeGenericMethod(delegateTypeBuilder));

			//to get constructor of our delegate we first need to get constructor of it's open type...
			var openTypeConstructor = typeof(Action<>).GetConstructors()[0];
			//... and then «instantiate» it using TypeBuilder
			var closedTypeConstructor = TypeBuilder.GetConstructor(delegateType, openTypeConstructor);

			//creating delegate and returning it
			methodDelegateIl.Emit(OpCodes.Newobj, closedTypeConstructor);
			methodDelegateIl.Emit(OpCodes.Ret);

			//caching MethodInfo for reusage
			RenewWrapperGetRenewerWrapperDelegate = methodDelegate;

			//look, mum, I'm re-implementing lambda function compilation logic
			//with no hands!

			renewerType.CreateType();
		}

		private static string CreateSafeStringForMethodAndTypeName(string input)
		{
			return MethodNameProhibitedSymbolsPattern.Replace(input, "_");
		}

		private static readonly Dictionary<Type, string> TypeFormats = new Dictionary<Type, string>
																{
																	{ typeof(DateTime), ":O" },
																	{ typeof(DateTime?), ":O" },
																	{ typeof(DateTimeOffset), ":O" },
																	{ typeof(DateTimeOffset?), ":O" }
																};

		private static readonly Regex MethodNameProhibitedSymbolsPattern = new Regex("[\\(\\)\\.\\[\\]\\-:=,` ]");

		#region pre-loaded MethodInfos

		private static readonly MethodInfo StringFormat = typeof(string).GetMethod("Format",
			new[] { typeof(string), typeof(object[]) });

		private static readonly MethodInfo DynaCacheServiceTryGetCachedObject =
			typeof(IDynaCacheService).GetMethod("TryGetCachedObject");

		private static readonly MethodInfo DynaCacheServiceSetCachedObject =
			typeof(IDynaCacheService).GetMethod("SetCachedObject");

		private static readonly MethodInfo MemoryCacheEntryStateGetter =
			typeof(MemoryCacheEntry).GetProperty("State", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

		private static readonly MethodInfo MemoryCacheEntryGetLoadingLock = typeof(MemoryCacheEntry).GetMethod("GetLoadingLock");

		private static readonly MethodInfo MemoryCacheEntryCalueGetter =
			typeof(MemoryCacheEntry).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

		private static readonly MethodInfo MemoryCacheEntryLoadingFailed = typeof(MemoryCacheEntry).GetMethod("LoadingFailed", BindingFlags.Instance | BindingFlags.Public);

		private static readonly MethodInfo MemoryCacheEntryRenew = typeof (MemoryCacheEntry).GetMethod("Renew",
			BindingFlags.Instance | BindingFlags.Public);

		private static readonly MethodInfo TaskIsCompletedGetter =
			typeof (Task).GetProperty("IsCompleted", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

		private static readonly MethodInfo TaskResultGetter =
			typeof (Task<>).GetProperty("Result", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

		//GetMethod does not handle generic methods properly, soooo let's do magic
		// ReSharper disable UseCollectionCountProperty -- can not use extension methods here
		private static readonly MethodInfo TaskRun = typeof (Task).GetMethods(BindingFlags.Static | BindingFlags.Public)
			.Where(m => m.Name == "Run" &&
						m.IsGenericMethodDefinition &&
						m.GetGenericArguments().Count() == 1 &&
						m.GetParameters().Count() == 1 &&
						 m.GetParameters()[0].ParameterType.GetGenericArguments()[0].BaseType != typeof (Task))
			.ToArray()[0];

		//another magic to get right ContinueWith override
		private static MethodInfo GetTypedTaskContinueWith(Type t)
		{
			return typeof(Task<>).MakeGenericType(t).GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(m => m.Name == "ContinueWith" &&
							m.GetParameters().Count() == 1).ToArray()[0];
		}

		// ReSharper restore UseCollectionCountProperty



		private static readonly ConstructorInfo DefaultObjectConstructorInfo = typeof(object).GetConstructors()[0];

		private static readonly ConstructorInfo RenewWrapperConstructor;

		private static readonly MethodInfo RenewWrapperGetRenewerWrapperDelegate;

		#endregion
	}
}
