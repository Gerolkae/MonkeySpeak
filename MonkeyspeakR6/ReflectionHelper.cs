using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Monkeyspeak
{
	internal class ReflectionHelper
	{
		public static Type[] GetAllTypes(Assembly assembly)
		{
			return assembly.GetTypes();
		}

		public static IEnumerable<T> GetAllAttributesFromMethod<T>(MethodInfo methodInfo) where T : Attribute
		{
			if (methodInfo.IsDefined(typeof(T), false))
			{
				T[] attributes = (T[])methodInfo.GetCustomAttributes(typeof(T), false);
				for (int k = 0; k <= attributes.Length - 1; k++)
				{
					yield return (T)attributes[k];
				}
			}
		}

		public static IEnumerable<MethodInfo> GetAllMethods(Type[] types)
		{
			for (int i = 0; i <= types.Length - 1; i++)
			{
				MethodInfo[] methods = types[i].GetMethods();
				for (int j = 0; j <= methods.Length - 1; j++)
				{
					yield return methods[j];
				}
			}
		}

		public static bool TryLoad(string assemblyFile, out Assembly asm)
		{
			try
			{
				asm = Assembly.LoadFile(Path.GetFullPath(assemblyFile));
				return true;
			}
#if DEBUG
			catch (Exception ex)
#else
			catch
#endif
			{
				asm = null;
#if DEBUG
				throw ex;
#else
				return false;
#endif
			}
		}
	}
}