using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkeyspeak.Libraries
{
	internal class StringOperations : AbstractBaseLibrary
	{
		public StringOperations()
		{
			Add(TriggerCategory.Effect, 400, StringArrayEntryCopy,
				"(5:400) use string {...} as an array and copy entry # of it into variable %Variable.");
			Add(TriggerCategory.Effect, 401, StringArrayEntrySet,
				"(5:401) use string variable %Variable as an array and set entry # of it to {...}.");
			Add(TriggerCategory.Effect, 402, StringArrayEntryRemove,
				"(5:402) use string variable %Variable as an array and remove entry # of it.");
			Add(TriggerCategory.Effect, 403, GetWordCountIntoVariable,
				"(5:403) with string {...} get word count and put it into variable %Variable.");
			Add(TriggerCategory.Effect, 404, AddStringToVar,
				"(5:404) with string {...} add it to string variable %Variable.");
			Add(TriggerCategory.Effect, 405, StringLength,
				"(5:405) take string {...} and put its character length in variable %Variable.");
		}

		bool StringArrayEntryCopy(TriggerReader reader)
		{
			string[] words = reader.ReadString().Split(' ');
			double index = -1;
			if (reader.PeekVariable())
			{
				Variable numVar = reader.ReadVariable();
				if (numVar.Value is double)
					index = (double)numVar.Value;
			}
			else if (reader.PeekNumber())
			{
				index = reader.ReadNumber();
			}

			if ((int)index <= words.Length - 1)
			{
				Variable var = reader.ReadVariable(true);
				var.Value = words[(int)index];
			}
			return true;
		}

		bool StringArrayEntrySet(TriggerReader reader)
		{
			Variable sentence = reader.ReadVariable();
			string[] words = ((string)sentence.Value).Split(' ');
			double index = -1;
			if (reader.PeekVariable())
			{
				Variable entryVar = reader.ReadVariable();
				if (entryVar.Value is double)
					index = (double)entryVar.Value;
			}
			else if (reader.PeekNumber())
			{
				index = reader.ReadNumber();
			}

			if ((int)index <= words.Length - 1)
			{
				string str = reader.ReadString();
				words[(int)index] = str;
			    sentence.Value = String.Concat(words);
			}
			return true;
		}

		bool StringArrayEntryRemove(TriggerReader reader)
		{
			Variable sentence = reader.ReadVariable();
			string[] words = ((string)sentence.Value).Split(' ');
			double index = -1;
			if (reader.PeekVariable())
			{
				Variable entryVar = reader.ReadVariable();
				if (entryVar.Value is double)
					index = (double)entryVar.Value;
			}
			else if (reader.PeekNumber())
			{
				index = reader.ReadNumber();
			}

			if ((int)index <= words.Length - 1)
			{
				words[(int)index] = "";
				sentence.Value = String.Concat(words);
			}
			return true;
		}

		bool GetWordCountIntoVariable(TriggerReader reader)
		{
			string[] words = reader.ReadString().Split(' ');
			Variable var = reader.ReadVariable(true);
			var.Value = words.Length;
			return true;
		}

		bool AddStringToVar(TriggerReader reader)
		{
			string str = reader.ReadString();
			Variable var = reader.ReadVariable(true);
		    var.Value = var.Value + str;
			return true;
		}
		
		bool StringLength(TriggerReader reader)
		{
			string str = reader.ReadString();
			Variable var = reader.ReadVariable(true);
			double len = (double)str.Length;
			var.Value = len;
			return true;
		}
	}
}
