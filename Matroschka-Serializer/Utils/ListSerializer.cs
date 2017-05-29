using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	public static class ListSerializer
	{
		public static byte[] Serialize<T>(this IEnumerable<T> input)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				input.ForEach(x => TBinaryAccessor<T>.Write(bw, x));
				return ms.ToArray();
			}
		}

		public static List<T> DeSerialize<T>(this byte[] input)
		{
			var retVar = new List<T>();
			using (MemoryStream ms = new MemoryStream(input))
			using (BinaryReader br = new BinaryReader(ms))
			{
				while (br.BaseStream.Position != input.Length)
				{
					retVar.Add(TBinaryAccessor<T>.Read(br));
				}
			}
			return retVar;
		}
	}

	public static class SeriExt
	{
		public static byte[] DoSerializion<T>(this T input)
		{
			var typeofT = typeof(T);
			if (typeofT.IsDictionary())
			{
				//This Is A dict
				Debugger.Break();

			}
			else if (typeofT.GetInterfaces().Contains(typeof(IEnumerable<>)))
			{
				//This is a enum
				Debugger.Break();
			}
			return null;
		}
	}
}