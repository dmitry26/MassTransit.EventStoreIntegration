using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MassTransit.EventStoreIntegration
{
	/// <summary>
	/// Event resolve type cache. Can be used in "default" mode when assembly name
	/// is used to compose the full type name, or in static resolve mode
	/// when you can define own mappings to be more implementation agnostic
	/// </summary>
	public static class TypeMapping
	{
		/// <summary>
		/// Add type to cache
		/// </summary>
		/// <param name="key">Type key</param>
		/// <param name="type">Type</param>
		public static void Add(string key,Type type) => Cached.AddOrUpdate(key,type);

		/// <summary>
		/// Add type to cache
		/// </summary>
		/// <param name="key">Type key</param>
		/// <typeparam name="T">Type</typeparam>
		public static void Add<T>(string key) => Add(key,typeof(T));

		/// <summary>
		/// Retrieve type from cache or find using reflections
		/// </summary>
		/// <param name="key">Type key or type name</param>
		/// <param name="assemblyName">Optional: assembly names where to search the type in</param>
		/// <returns></returns>
		public static Type Get(string key,string assemblyName) =>
			Cached.GetOrAdd(key,k => TypeExts.FindType(k,assemblyName));

		/// <summary>
		/// Get the type name, either from mapping or full assembly name
		/// </summary>
		/// <param name="type">Type that you need a name for</param>
		/// <returns>Type name string</returns>
		public static string GetTypeName(Type type)
		{
			var mtype = type.GetMsgType();

			return Cached.TryGetValue(mtype,out string val)
				? val
				: mtype.FullName;
		}

		private static class Cached
		{
			private static readonly ConcurrentDictionary<string,Type> _instance =
				new ConcurrentDictionary<string,Type>();

			private static readonly ConcurrentDictionary<Type,string> _reverseInstance =
				new ConcurrentDictionary<Type,string>();

			internal static bool TryGetValue(string key,out Type value) =>
				_instance.TryGetValue(key,out value);

			internal static bool TryGetValue(Type key,out string value) =>
				_reverseInstance.TryGetValue(key,out value);

			internal static Type GetOrAdd(string key,Func<string,Type> valFact)
			{
				bool adding = false;

				var val = _instance.GetOrAdd(key,k =>
				{
					adding = true;
					return valFact(k) ?? throw new TypeLoadException($"Type object not found for: {key}");
				});

				if (adding)
					_reverseInstance.AddOrUpdate(val,key,(k,v) => key);

				return val;
			}

			internal static void AddOrUpdate(string key,Type type)
			{
				_instance.AddOrUpdate(key,type,(k,ov) => type);
				_reverseInstance.AddOrUpdate(type,key,(k,ov) => key);
			}
		}
	}
}