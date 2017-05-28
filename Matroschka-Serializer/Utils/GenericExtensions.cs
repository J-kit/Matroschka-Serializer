using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	public static class GenericExtensions
	{
		/// <summary>
		/// Tries to change the type of the input to the desired <typeparamref name="T"/>, returns default(T) if it cannot be converted
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		public static T TryChangeType<T>(this object input)
		{
			if (input.GetType() == typeof(T))
			{
				return (T)Convert.ChangeType(input, typeof(T));
			}
			return default(T);
		}

		public static void BooledForEach<T>(this IEnumerable<T> input, Func<T, bool> state, Action<T> toRunAct)
		{
			foreach (var put in input)
			{
				if (state(put))
				{
					toRunAct(put);
				}
			}
		}

		public static void ForEach<T>(this IEnumerable<T> input, Action<T> toRunAct)
		{
			foreach (var put in input)
			{
				toRunAct(put);
			}
		}

		public static void ForEach<T, TX>(this IEnumerable<Tuple<T, TX>> input, Action<T, TX> toRunAct)
		{
			foreach (var put in input)
			{
				toRunAct(put.Item1, put.Item2);
			}
		}

		public static void ForEach<T, TX, TY>(this IEnumerable<Tuple<T, TX, TY>> input, Action<T, TX, TY> toRunAct)
		{
			foreach (var put in input)
			{
				toRunAct(put.Item1, put.Item2, put.Item3);
			}
		}
	}
}