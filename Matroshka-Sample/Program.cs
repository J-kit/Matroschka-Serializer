using Matroschka_Serializer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matroshka_Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			List<string> level1 = new List<string> {
				"Hey",
				"I am",
				"Level One",
			};
			var lstSeri = level1.Serialize();
			var lvl1de = lstSeri.DeSerialize<string>();

			lvl1de.DoSerializion();


			//Let's create a dummy dictionary with some entities
			Dictionary<string, string> myDictionary = new Dictionary<string, string>();
			myDictionary["HalloWelt"] = "lola";
			myDictionary["HalloWelt1"] = "lola2";

			//Serialize the dictionary to a byte array
			var serialized = myDictionary.Serialize();
			//Deserialize that byte array to a dictionary<string,string>
			var deseri = serialized.DeSerialize<string, string>();
			Console.WriteLine(deseri["HalloWelt"]);

			//Same, but not as pretty as the previous example
			serialized = DictionarySerializer<string, string>.Serialize(myDictionary);
			deseri = DictionarySerializer<string, string>.Deserialize(serialized);

			var myDictionary1 = new Dictionary<string, Dictionary<string, string>>
			{
				["HalloWelt"] = new Dictionary<string, string>
				{
					["a"] = "b",
					["c"] = "ba",
					["d"] = "bs",
					["e"] = "bfa",
				},
				["HalloWelt1"] = new Dictionary<string, string>
				{
					["xa"] = "xb",
					["xc"] = "xba",
					["xd"] = "xbs",
					["xe"] = "xbfa",
				},
			};
			serialized = myDictionary1.Serialize();
			var deserialized = serialized.DeSerialize<string, Dictionary<string, Dictionary<string, int>>>();
			Console.ReadLine();
		}
	}
}
