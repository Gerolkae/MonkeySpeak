using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Monkeyspeak;

namespace ZipLibrary
{
	public class ZipTriggerHandlers
	{
		[TriggerHandler(TriggerCategory.Effect, 700, "(5:700) create a zip file named {...}.")]
		public static bool CreateZipFile(TriggerReader reader)
		{
			string zipFile = reader.ReadString();

			try
			{
				using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFile))
				{
					zip.Save();
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 701, "(5:701) write file {...} to zip file {...}.")]
		public static bool WriteFileToZip(TriggerReader reader)
		{
			string fileToZip = reader.ReadString();
			string zipFile = reader.ReadString();

			try
			{
				using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFile))
				{
					zip.AddFile(fileToZip);
					zip.Save();
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 702, "(5:702) extract file {...} from zip file {...}.")]
		public static bool ExtractFileFromZip(TriggerReader reader)
		{
			string fileToSnatch = reader.ReadString();
			string zipFile = reader.ReadString();

			try
			{
				using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFile))
				{
					foreach (ZipEntry entry in zip.Entries)
					{
						if (entry.FileName.Equals(fileToSnatch))
						{
							entry.Extract();
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 703, "(5:703) extract all from zip file {...}.")]
		public static bool ExtractAllFromZip(TriggerReader reader)
		{
			string zipFile = reader.ReadString();

			try
			{
				Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFile);
				zip.ExtractAll("");
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}
	}
}
