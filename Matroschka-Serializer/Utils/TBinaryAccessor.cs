using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroschka_Serializer.Utils
{
	/// <summary>
	/// Handles the type de/serialization
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class TBinaryAccessor<T>
	{
		//For each access we need a delegate which handles the de/serialization by accepting
		//in both cases a stream and reads/writes on/from it

		//This delegate reads the given type from the stream
		private static Func<BinaryReader, T> _storReadFunc;

		//--"-- writes the given data from the given instance of type T onto the stream
		private static Action<BinaryWriter, T> _storWriteAct;

		/// <summary>
		/// As this serializer isn't able to de/serialize non-primitive types on it's own by default, you could just
		/// Extend its functionality by registering a custom Type
		/// </summary>
		/// <param name="readFunc">null won't change the delegate</param>
		/// <param name="writeAct">null won't change the delegate</param>
		public static void RegisterExtension(Func<BinaryReader, T> readFunc = null, Action<BinaryWriter, T> writeAct = null)
		{
			_storReadFunc = readFunc ?? _storReadFunc;
			_storWriteAct = writeAct ?? _storWriteAct;
		}

		/// <summary>
		/// If you want to use a different Encoding for string en/decoding, you can do that here
		/// </summary>
		public static Encoding BaseEncoding = Encoding.UTF8;

		/// <summary>
		/// Static initializer;
		/// Basically searches for a method in the BinaryWriter/Reader class to automatically handle the given types
		/// </summary>
		static TBinaryAccessor()
		{
			var typeOfT = typeof(T);

			if (typeOfT == typeof(string))
			{
				//Looks a bit ugly, but ¯\_(ツ)_/¯
				//For reading:
				//	1.Read the len of the next string
				//	2.Read the amount of len of bytes and decode it to a string
				//For writing:
				//	1: Convert the given string to a byte[]
				//	2: Write the len of bytes in the array onto the stream
				//	3: Write the bytes onto the stream
				RegisterExtension(new Func<BinaryReader, string>(x => BaseEncoding.GetString(x.ReadBytes(x.ReadInt32()))) as Func<BinaryReader, T>, new Action<BinaryWriter, string>((m, x) =>
				{
					var dstBytes = BaseEncoding.GetBytes(x);
					m.Write(dstBytes.Length);
					m.Write(dstBytes);
				}) as Action<BinaryWriter, T>
				);
			}
			else if (typeOfT.IsPrimitive)
			{
				//Search for a method to read the given type
				var readMethodInfo = TypeSearcher.GetMethodsFromType(typeof(BinaryReader), new Type[0], typeOfT).FirstOrDefault(m => m.Name.StartsWith("Read") && m.Name != "Read");
				if (readMethodInfo != null)
				{
					//Create a delegate, so that we don't have to use Methodinfo.Invoke which slows down the whole process
					_storReadFunc = (Func<BinaryReader, T>)readMethodInfo.CreateDelegate(typeof(Func<BinaryReader, T>));
				}

				//Search for a method to write the given type onto a stream
				var writeMethodInfo = TypeSearcher.GetMethodsFromType(typeof(BinaryWriter), new Type[] { typeOfT }).FirstOrDefault();
				if (writeMethodInfo != null)
				{
					_storWriteAct = (Action<BinaryWriter, T>)writeMethodInfo.CreateDelegate(typeof(Action<BinaryWriter, T>));
				}
			}
			else
			{
				_storWriteAct = (bw, data) =>
				{
					var serializer = (Func<T, byte[]>)MatroschkaResolve.GetWriteMethodInfo(data);
					if (serializer != null) bw.Write(serializer(data));
				};

				_storReadFunc = br =>
				{
					var eval = (Func<BinaryReader, T>)MatroschkaResolve.GetReadMethodInfo<T>();
					return eval == null ? default(T) : eval(br);
				};
			}
		}

		/// <summary>
		/// Use the previosly resolved method to read the given type
		/// </summary>
		/// <param name="binaryReader"></param>
		/// <returns></returns>
		public static T Read(BinaryReader binaryReader)
		{
			if (_storReadFunc != null)
			{
				return _storReadFunc(binaryReader);
			}
			return default(T);
		}

		/// <summary>
		/// Use the previosly resolved method to write the given object onto the stream
		/// </summary>
		/// <param name="bw"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool Write(BinaryWriter bw, T data)
		{
			if (_storWriteAct != null)
			{
				_storWriteAct(bw, data);
				return true;
			}
			return false;
		}
	}
}