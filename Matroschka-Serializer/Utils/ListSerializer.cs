using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	public class ListSerializer
	{
		public static byte[] Serialize<T>(IEnumerable<T> input)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				input.ForEach(x => TBinaryAccessor<T>.Write(bw, x));
				return ms.ToArray();
			}
		}

		public static List<T> DeSerialize<T>(byte[] input)
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
}