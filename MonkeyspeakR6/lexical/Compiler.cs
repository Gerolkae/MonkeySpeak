using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monkeyspeak.lexical
{
	internal class Compiler
	{
		private Version version;

		public Compiler(MonkeyspeakEngine engine)
		{
			version = engine.Options.Version;
		}

		/// <summary>
		/// Compiler version number
		/// </summary>
		public Version Version
		{
			get { return version; }
		}

		public List<TriggerList> DecompileFromStream(Stream stream)
		{
			List<TriggerList> triggerLists = new List<TriggerList>();
			using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
			{
				Version fileVersion = new Version(reader.ReadString()); // use for versioning comparison
				
				TriggerList triggerList = new TriggerList();
				int triggerListCount = reader.ReadInt32();
				for (int i = 0; i <= triggerListCount - 1; i++)
				{
					int triggerCount = reader.ReadInt32();
					for (int j = 0; j <= triggerCount - 1; j++)
					{
						Trigger trigger = new Trigger();
						trigger.Category = (TriggerCategory)reader.ReadByte();
						trigger.Id = reader.ReadInt32();
						trigger.Description = reader.ReadString();

						int triggerContentCount = reader.ReadInt32();
						for (int k = 0; k <= triggerContentCount - 1; k++)
						{
							if (reader.ReadBoolean() == true)
							{
								short type = reader.ReadInt16();
								if (type == 1) // String
								{
									trigger.contents.Enqueue(reader.ReadString());
								}

								if (type == 2)
								{
									trigger.contents.Enqueue(reader.ReadDouble());
								}
							}
						}
						triggerList.Add(trigger);
					}
				}
				triggerLists.Add(triggerList);
			}
			return triggerLists;
		}

		public void CompileToStream(List<TriggerList> triggers, Stream stream)
		{
			List<TriggerList> triggerLists = triggers;
			using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
			{
				writer.Write(version.ToString(4));

				writer.Write(triggerLists.Count);
				for (int i = 0; i <= triggerLists.Count - 1; i++)
				{
					TriggerList triggerList = triggerLists[i];
					writer.Write(triggerList.Count);
					for (int j = 0; j <= triggerList.Count - 1; j++)
					{
						Trigger trigger = triggerList[j];
						writer.Write((byte)trigger.Category);
						writer.Write(trigger.Id);
						writer.Write(trigger.Description);

						writer.Write(trigger.contents.Count);
						var enumerator = trigger.contents.GetEnumerator();
						while (enumerator.MoveNext())
						{
							object content = enumerator.Current;
							if (content is string)
							{
								writer.Write(true);
								writer.Write((short)1);
								writer.Write((string)content);
							}
							else if (content is double)
							{
								writer.Write(true);
								writer.Write((short)2);
								writer.Write((double)content);
							}
							else writer.Write(false);
						}
					}
				}
			}
		}
	}
}
