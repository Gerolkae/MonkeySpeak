using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monkeyspeak;

namespace MonkeyspeakTest
{
	[TestClass]
	public class UnitTest2
	{
		private string testScript = @"
*This is a comment
(0:0) when the script is started,
		(5:100) set %hello to {Hello World}.
		(5:102) print {%hello} to the console.
";

		private class TestReflection
		{
			[Monkeyspeak.TriggerHandler(TriggerCategory.Cause, 1000, "(0:1000) TestTriggerHandlerMethod")]
			public bool TestTriggerHandlerCauseMethod(TriggerReader reader)
			{
				return true;
			}

			[Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1001, "(5:1001) test print 2 {...}")]
			[Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1000, "(5:1000) test print {...}")]
			public bool TestTriggerHandlerEffectMethod(TriggerReader reader)
			{
				if (reader.PeekString()) Console.WriteLine(reader.ReadString());
				else Console.WriteLine("Error!!!... no value.");
				return true;
			}
		}

		[TestMethod]
		public void TestReflectionLoader()
		{
			MonkeyspeakEngine engine = new MonkeyspeakEngine();
			Page page = engine.LoadFromString(@"
(0:1000)
(5:1000) {Hello Reflection Test}
(5:1001) {Hello Reflection Test 2}
");
			page.LoadLibraryFromAssembly(Assembly.GetExecutingAssembly().Location);

			page.Execute(1000);
		}

		[TestMethod]
		public void TestCompileToFile()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			engine.Options.TriggerLimit = int.MaxValue;

			var bigScript = testScript;
			for (int i = 0; i <= 6000; i++)
			{
				bigScript += testScript + "\n";
			}
			Monkeyspeak.Page oldPage = engine.LoadFromString(bigScript);

			Console.WriteLine("Page Trigger Count: " + oldPage.Size);

			oldPage.CompileToFile("test.msx");
		}

		[TestMethod]
		public void TestLoadCompiledFile()
		{
			Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
			engine.Options.TriggerLimit = int.MaxValue;

			Monkeyspeak.Page page = engine.LoadCompiledFile("test.msx");

			page.LoadSysLibrary();
			page.LoadIOLibrary();
			page.LoadMathLibrary();
			page.LoadTimerLibrary();

			page.Execute(0);
			Console.WriteLine("Page Trigger Count: " + page.Size);
		}

//        [TestMethod]
//        public void TestParallelExecute()
//        {
//            var ioTestString = @"
//(0:0) when the script starts,
//		(5:100) set variable %file to {test.txt}.
//		(5:102) print {%file} to the console.
//		(5:150) take variable %increment and add 1 to it.
//		(5:102) print {Execution increment %increment} to the console.
//
//(0:0) when the script starts,
//	(1:200) and the file {%file} exist,
//		(5:202) delete file {%file}.
//		(5:203) create file {%file}.
//
//(0:0) when the script starts,
//	(1:200) and the file {%file} exists,
//	(1:203) and the file {%file} can be written to,
//		(5:200) append {Hello World from Monkeyspeak!} to file {%file}.
//
//(0:0) when the script starts,
//	(5:150) take variable %test and add 2 to it.
//	(5:102) print {%test} to the console.
//";

//            Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
//            engine.Options.TriggerLimit = int.MaxValue;

//            Monkeyspeak.Page page = engine.LoadFromString(ioTestString);

//            page.LoadSysLibrary();
//            page.LoadIOLibrary();
//            page.LoadMathLibrary();
//            page.LoadTimerLibrary();

//            for (int i=0;i<10000;i++)
//                page.ExecuteAsync(0);

//            Console.WriteLine("Page Trigger Count: " + page.Size);

//            // Result is execution is parallel! Awesome!
//        }

        //[TestMethod]
        //public void TestR6Features()
        //{
        //    MonkeyspeakEngine engine = new MonkeyspeakEngine();
        //    Page page = engine.LoadFromString(testScript);
        //    engine.LoadFromString(ref page, testScript);

        //    page.LoadSysLibrary();
        //    page.LoadIOLibrary();
        //    page.LoadMathLibrary();
        //    page.LoadTimerLibrary();

        //    page.Execute(0);
        //}

		public bool HandleAllCauses(Monkeyspeak.TriggerReader reader)
		{
			return true;
		}

		private void DebugAllErrors(Monkeyspeak.Trigger trigger, Exception ex)
		{
			System.Diagnostics.Debug.WriteLine("Error with " + trigger.ToString());
#if DEBUG
			throw ex;
#endif
		}
	}
}