using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	public static class TBinaryExtendor
	{
		/// <summary>
		/// Serializes a Dictionary<T,TX> to a handy byte array
		/// </summary>
		/// <typeparam name="T">Dictionary Key</typeparam>
		/// <typeparam name="TX">Dictionary Value</typeparam>
		/// <param name="input">Input dictionary</param>
		/// <returns></returns>
		public static byte[] Serialize<T, TX>(this Dictionary<T, TX> input)
		{
			return DictionarySerializer<T, TX>.Serialize(input);
		}

		/// <summary>
		/// Deserializes the given byte array to a dictionary of your choice
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TX"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Dictionary<T, TX> DeSerialize<T, TX>(this byte[] input)
		{
			return DictionarySerializer<T, TX>.Deserialize(input);
		}

		public static Dictionary<T, TX> DeSerialize<T, TX>(this BinaryReader input)
		{
			return DictionarySerializer<T, TX>.Deserialize(input);
		}
	}

	/// <summary>
	/// A serialized Dictionary conists of (for example, string):
	///		(int32)Amount of entities
	///		Entity:
	///			(int32)keylength (amount of bytes after theese 4 bytes)
	///			(string)key[keylength]
	///			(int32)valuelength
	///			(string)key[valuelength]
	///
	/// Please note that this serializer is not able to de/serialize non-primitive types by default,
	///			if you want to do so, please have a look at TBinaryAccessor.RegisterExtension
	/// </summary>
	/// <typeparam name="T">Dictionary Key</typeparam>
	/// <typeparam name="TX">Dictionary Value</typeparam>
	public static class DictionarySerializer<T, TX>
	{
		public static byte[] Serialize(Dictionary<T, TX> input)
		{
			//Let's create a Memorystream and write our bytes conviniently with a BinaryWriter onto it
			using (var msa = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(msa))
			{
				//Write the amount of KeyValuePairs onto the stream
				bw.Write(input.Count);
				foreach (KeyValuePair<T, TX> val in input)
				{
					Serialize(val, bw);
				}
				return msa.ToArray();
			}
		}

		/// <summary>
		/// Writes the Key+Value onto the stream and let the TBinaryAccessor handle how it's done.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bw"></param>
		public static void Serialize(KeyValuePair<T, TX> input, BinaryWriter bw)
		{
			//Write the key onto the stream
			TBinaryAccessor<T>.Write(bw, input.Key);
			//Write the key onto the stream
			TBinaryAccessor<TX>.Write(bw, input.Value);
		}

		/// <summary>
		/// Basically the same as Serialize(Dictionary<T, TX> input) but now the other way round
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Dictionary<T, TX> Deserialize(byte[] input)
		{
			using (MemoryStream ms = new MemoryStream(input))
			using (BinaryReader br = new BinaryReader(ms))
			{
				return Deserialize(br);
			}
		}

		public static Dictionary<T, TX> Deserialize(BinaryReader br)
		{
			//Read the amount of KeyValuePairs from the "header"
			var dictSize = br.ReadInt32();
			//Create a new instance of the Dictionary requested, use the constructor which accepts the dictSize to "boost" the performance a little
			var dstDict = new Dictionary<T, TX>(dictSize);
			for (int i = 0; i < dictSize; i++)
			{
				//Read the first value, set it as key and then read the second one and set it as value
				//As above, let the TBinaryAccessor handle how it's done.
				dstDict[TBinaryAccessor<T>.Read(br)] = TBinaryAccessor<TX>.Read(br);
			}
			return dstDict;
		}
	}
}