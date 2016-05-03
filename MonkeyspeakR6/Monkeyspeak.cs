using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Monkeyspeak.lexical;

namespace Monkeyspeak
{
	[Serializable]
	public sealed class MonkeyspeakException : Exception
	{
		public MonkeyspeakException() { }

		public MonkeyspeakException(string message) : base(message) { }

        public MonkeyspeakException(string format, params object[] message) : base(String.Format(format, message)) { }

		public MonkeyspeakException(string message, Exception inner) : base(message, inner) { }

		protected MonkeyspeakException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	public sealed class MonkeyspeakEngine
	{
		private ILexer lexer;
		private AbstractParser parser;
		private List<Page> pages;
		public Options options;

		public MonkeyspeakEngine()
		{
			pages = new List<Page>();
			
			CreateDefaultOptions();

			CreateDefaultLexer();
			parser = new Parser(this, lexer);
		}

		public MonkeyspeakEngine(Options options)
		{
			pages = new List<Page>();

			this.options = options;

			CreateDefaultLexer();
			parser = new Parser(this, lexer);
		}

		internal void CreateDefaultOptions()
		{
			options = new Options
			{
			    CanOverrideTriggerHandlers = false, 
                StringBeginSymbol = '{', 
                StringEndSymbol = '}', 
                VariableDeclarationSymbol = '%', 
                CommentSymbol = "*", 
                TriggerLimit = 6000, 
                VariableCountLimit = 1000, 
                StringLengthLimit = Int32.MaxValue, 
                TimerLimit = 100,
                Version = Assembly.GetExecutingAssembly().GetName().Version
			};
		}

		internal void CreateDefaultLexer()
		{
			lexer = new Lexer();

			lexer.AddDefinition(new TokenDefinition(
				TokenType.Trigger,
				new Regex(@"\([0-9]{1}\:[0-9]{1," + Int32.MaxValue + @"}\)", RegexOptions.Compiled)));

			lexer.AddDefinition(new TokenDefinition(
				TokenType.Variable,
				new Regex(String.Format(@"\{0}[\ba-zA-Z\d\D][\ba-zA-Z\d_]*", options.VariableDeclarationSymbol), RegexOptions.Compiled)));

			lexer.AddDefinition(new TokenDefinition(
				TokenType.String,
				new Regex(@"\" + options.StringBeginSymbol + @"(.*?)\" + options.StringEndSymbol, RegexOptions.Compiled)));

			lexer.AddDefinition(new TokenDefinition(
				TokenType.Number,
				new Regex(@"[-+]?([0-9]*\.[0-9]+|[0-9]+)", RegexOptions.Compiled)));

			lexer.AddDefinition(new TokenDefinition(
				TokenType.Comment,
				new Regex(@"\" + options.CommentSymbol + @".*[\r|\n]", RegexOptions.Compiled), true));
			lexer.AddDefinition(new TokenDefinition(
				TokenType.Word,
				new Regex(@"\w+", RegexOptions.Compiled), true));
			lexer.AddDefinition(new TokenDefinition(
				TokenType.Symbol,
				new Regex(@"\W", RegexOptions.Compiled), true));
			lexer.AddDefinition(new TokenDefinition(
				TokenType.WhiteSpace,
				new Regex(@"\s+", RegexOptions.Compiled), true));
		}

		public Options Options
		{
			get { return options; }
			set { options = value; }
		}

		/// <summary>
		/// Loads a Monkeyspeak script from a string into a <see cref="Monkeyspeak.Page"/>.
		/// </summary>
		/// <param name="chunk">String that contains the Monkeyspeak script source.</param>
		/// <returns></returns>
		public Page LoadFromString(string chunk)
		{
			try
			{
				Page page = new Page(this);
				page.Write(parser.Parse(chunk));
				pages.Add(page);
				return page;
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not load script from chunk.  Reason:{0}", ex.Message));
			}
		}



		/// <summary>
		/// Loads a Monkeyspeak script from a string into <paramref name="existingPage"/>. and
		/// clears the old page
		/// </summary>
		/// <param name="existingPage">Reference to an existing Page</param>
		/// <param name="chunk">String that contains the Monkeyspeak script source.</param>
		/// <returns></returns>
		public Page LoadFromString(ref Page existingPage, string chunk)
		{
			try
			{
				existingPage.OverWrite(parser.Parse(chunk));
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not load script from chunk.  Reason:{0}", ex.Message));
			}
			return existingPage;
		}

		/// <summary>
		/// Loads compiled script from file
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public Page LoadCompiledFile(string filePath)
		{
			try
			{
				using (Stream stream = new FileStream(filePath, FileMode.Open))
				{
					return LoadCompiledStream(stream);
				}
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not load compiled script from file.  Reason:{0}", ex.Message), ex);
			}
		}

		/// <summary>
		/// Loads a compiled script from stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public Page LoadCompiledStream(Stream stream)
		{
			try
			{
				var page = new Page(this);
				var compiler = new Compiler(this);
				using (Stream zipStream = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress))
				{
					page.Write(compiler.DecompileFromStream(zipStream));
				}
				return page;
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not load compiled script from stream.  Reason:{0}", ex.Message), ex);
			}
		}

		/// <summary>
		/// Compiles a Page to a file
		/// </summary>
		/// <param name="page"></param>
		/// <param name="filePath"></param>
		public void CompileToFile(Page page, string filePath)
		{
			page.CompileToFile(filePath);
		}

		/// <summary>
		/// Compiles a Page to a stream
		/// </summary>
		/// <param name="page"></param>
		/// <param name="stream"></param>
		public void CompileToStream(Page page, Stream stream)
		{
			page.CompileToStream(stream);
		}
	}
}
