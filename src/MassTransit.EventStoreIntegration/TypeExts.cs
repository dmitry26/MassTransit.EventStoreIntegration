using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyModel;

namespace MassTransit.EventStoreIntegration
{	
	public static class TypeExts
	{
		public static Type FindType(string typeName,string asmName = null,Func<string,bool> asmResolver = null) =>
			(string.IsNullOrWhiteSpace(asmName)
				? Type.GetType(typeName)
				: Type.GetType($"{typeName}, {asmName}") ?? Type.GetType(typeName))
				?? GetAssemblies(asmResolver ?? (_ => true)).Select(a => a.GetType(typeName)).FirstOrDefault();

		private static IEnumerable<Assembly> GetAssemblies(Func<string,bool> asmResolver) =>
			DependencyContext.Default.RuntimeLibraries.Where(l => asmResolver(l.Name))
				.Select(l => Assembly.Load(new AssemblyName(l.Name)));

		public static Type GetMsgType(this Type src) => src.IsProxyType() ? src.GetInterface(src.Name) ?? src : src;
		
		public static bool IsProxyType(this Type src) => src?.FullName.StartsWith("GreenPipes.DynamicInternal") ?? false;
	}
}
