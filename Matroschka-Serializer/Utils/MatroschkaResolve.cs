using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	/// <summary>
	/// Really Really unsexy thing....
	/// More documentation is comming soon :/
	/// </summary>
	public class MatroschkaResolve
	{
		public static Dictionary<ulong, Delegate> _writeCache = new Dictionary<ulong, Delegate>();
		public static Dictionary<ulong, Delegate> _readCache = new Dictionary<ulong, Delegate>();

		public static Delegate GetWriteMethodInfo<T>(T inputDict)
		{
			var typeOfT = typeof(T);

			Type[] args = null;
			if (typeOfT.IsDictionary() && (args = typeOfT.GetGenericArguments()) != null && args.Length == 2)
			{
				var metadataToken = unchecked(((ulong)(uint)args[0].MetadataToken << 32) | (ulong)(uint)args[1].MetadataToken);
				if (_writeCache.TryGetValue(metadataToken, out var dstMethod))
				{
					return dstMethod;
				}
				else
				{
					var methodArray = typeof(TBinaryExtendor).UnderlyingSystemType.GetMethods().Where(m => m.GetParameters().Length == 1).ToArray();
					MethodInfo selected = null;
					for (var i = 0; i < methodArray.Length; i++)
					{
						var genArgs = methodArray[i].GetGenericArguments();
						foreach (Type t in genArgs)
						{
							if (genArgs[i].IsGenericParameter || t == args[i])
							{
								selected = methodArray[i];
							}
							else
							{
								selected = null;
								break;
							}
						}
						if (selected != null)
						{
							break;
						}
					}
					dstMethod = selected?.MakeGenericMethod(args).CreateDelegate(typeof(Func<T, byte[]>));
					if (dstMethod != null)
					{
						_writeCache[metadataToken] = dstMethod;
						return dstMethod;
					}
				}
			}
			return null;
		}

		public static Delegate GetReadMethodInfo<T>()
		{
			Type[] args = null;
			var typeOfT = typeof(T);
			if (typeOfT.IsDictionary() && (args = typeOfT.GetGenericArguments()) != null && args.Length == 2)
			{
				var metadataToken = unchecked(((ulong)(uint)args[0].MetadataToken << 32) | (ulong)(uint)args[1].MetadataToken);
				if (_readCache.TryGetValue(metadataToken, out var dstMethod))
				{
					return dstMethod;
				}
				else
				{
					var method = TypeSearcher.GetMethodsFromType(typeof(TBinaryExtendor), new Type[] { typeof(BinaryReader) }).FirstOrDefault();
					dstMethod = method?.MakeGenericMethod(args).CreateDelegate(typeof(Func<BinaryReader, T>));

					if (dstMethod != null)
					{
						_readCache[metadataToken] = dstMethod;
						return dstMethod;
					}
				}
			}
			return null;
		}
	}

	public static class ReflectionExtensions
	{
		public static bool IsDictionary(this Type input) => (input.IsGenericType && input.GetGenericTypeDefinition() == typeof(Dictionary<,>));
	}

	public class TypeSearcher
	{
		/// <summary>
		/// Gets us all methods from a class which match our needs
		/// </summary>
		/// <param name="classToSearchIn">The parent class where the methods are located</param>
		/// <param name="matchInputParams">null means any</param>
		/// <param name="outputparam">null means any</param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethodsFromType(Type classToSearchIn, Type[] matchInputParams = null, Type outputparam = null)
		{
			IEnumerable<MethodInfo> methodArray = classToSearchIn.UnderlyingSystemType.GetMethods();

			if (matchInputParams != null)
			{
				methodArray = methodArray.Where(m => m.GetParameters().Length == matchInputParams.Length);

				if (matchInputParams.Length > 0)
				{
					methodArray = methodArray
						.Where(a => a.GetParameters()
							.Where((ParameterInfo info, int i) => info.ParameterType == matchInputParams[i])
							.Any());
				}
			}

			if (outputparam != null)
				methodArray = methodArray.Where(m => m.ReturnParameter?.ParameterType == outputparam);

			return methodArray;
		}
	}
}