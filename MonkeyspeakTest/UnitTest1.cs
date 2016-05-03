using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MonkeyspeakTest
{
	[TestClass]
	public class UnitTest1
	{
		private string testScript = @"
*This is a comment
(0:0) when the script is started,
		(5:100) set %hello to {Hello World}.
		(5:10000) create a debug breakpoint here,
		(5:102) print {%hello} to the console.
		(5:102) print {%testVariable} to the console.

(0:0) when the script is started,
		(5:102) print {This is a test script.} to the console.
";

		[TestMethod]
		public void DebugTest()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(testScript);

			page.Error += DebugAllErrors;

			List<Monkeyspeak.Trigger> triggers = new List<Monkeyspeak.Trigger>();

			page.LoadSysLibrary();
			page.LoadDebugLibrary();

			Monkeyspeak.Variable var = page.SetVariable("%testVariable", "Hello WOrld", true);

			page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

			// Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
			Console.WriteLine("Trigger Count: " + page.Size);

			page.Execute(0);
		}

		[TestMethod]
		public void DurabilityParseTest()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();

			// Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
			engine.Options.TriggerLimit = int.MaxValue;

			var bigScript = testScript;
			for (int i = 0; i <= 6000; i++)
			{
				bigScript += testScript + "\n";
			}
			Monkeyspeak.Page page = engine.LoadFromString(bigScript);

			page.Error += DebugAllErrors;

			page.LoadSysLibrary();

			page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

			Console.WriteLine("Page Trigger Count: " + page.Size);
			//page.Execute(Monkeyspeak.TriggerCategory.Cause, 0);
		}

		[TestMethod]
		public void DemoTest()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(testScript);

			page.Error += DebugAllErrors;

			List<Monkeyspeak.Trigger> triggers = new List<Monkeyspeak.Trigger>();

			page.LoadSysLibrary();

			Monkeyspeak.Variable var = page.SetVariable("%testVariable", "Hello WOrld", true);

			page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

			// Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
			Console.WriteLine("Trigger Count: " + page.Size);

			page.Execute(0);
		}

		[TestMethod]
		public void SetGetVariableTest()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(testScript);

			page.Error += DebugAllErrors;

			for (int i = 0; i <= 10; i++)
				page.SetVariable("Name", true.ToString(), false);
			foreach (var variable in page.Scope)
			{
				System.Diagnostics.Debug.WriteLine(variable);
			}
		}

		[TestMethod]
		public void IOLibraryTest()
		{
			var ioTestString = @"
(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.
	(5:102) print {%file} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exist,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
";
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(ioTestString);

			page.Error += DebugAllErrors;

			page.LoadSysLibrary();
			page.LoadIOLibrary();

			page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

			page.Execute(0);
		}

		[TestMethod]
		public void ErrorTriggerTest()
		{
			var errorTestScript = @"
(0:0) when the script starts,
		(5:102) print {This is a test of the new error system} to the console.
		(5:105) raise an error.
		(5:102) print {This will NOT be displayed because an error was raised} to the console.
";
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(errorTestScript);

			page.Error += DebugAllErrors;

			page.LoadSysLibrary();
			try
			{
				page.Execute(0);
			}
			catch (Monkeyspeak.MonkeyspeakException ex) { System.Diagnostics.Debug.WriteLine("A Monkeyspeak Exception was raised!"); }
		}

		[TestMethod]
		public void GetTriggerDescriptionsTest()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString("");

			page.Error += DebugAllErrors;

			page.LoadSysLibrary();
			page.LoadIOLibrary();
			page.LoadMathLibrary();
			page.LoadTimerLibrary();

			foreach (string desc in page.GetTriggerDescriptions())
			{
				Console.WriteLine(desc);
			}
		}

		[TestMethod]
		public void TimerLibraryTest()
		{
			var timerLibTestScript = @"
(0:0) when the script starts,
	(5:101) set variable %timer to 1.
	(5:300) create timer %timer to go off every 2 second(s).

(0:300) when timer %timer goes off,
	(5:102) print {Timer %timer went off.} to the console.
";
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			Monkeyspeak.Page page = engine.LoadFromString(timerLibTestScript);

			page.Error += DebugAllErrors;

			page.LoadSysLibrary();
			page.LoadTimerLibrary();

			page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

			page.Execute(0);
			System.Threading.Thread.Sleep(4000);
		}

		public bool HandleAllCauses(Monkeyspeak.TriggerReader reader)
		{
			return true;
		}

		private void DebugAllErrors(Monkeyspeak.Trigger trigger, Exception ex)
		{
			Console.WriteLine("Error with " + trigger.ToString());
#if DEBUG
			System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
		}
	}
}