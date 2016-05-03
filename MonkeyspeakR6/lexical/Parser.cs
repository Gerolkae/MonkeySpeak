using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monkeyspeak.lexical;

namespace Monkeyspeak
{
	internal sealed class Parser : AbstractParser
	{

		public Parser(MonkeyspeakEngine engine, ILexer lexer) :
			base(engine,lexer)
		{
		}

		public override List<TriggerList> Parse(string source)
		{
			var triggerBlocks = new List<TriggerList>(5000);
			var block = new TriggerList(1000);
			Trigger currentTrigger = null, prevTrigger = null;
			Token token = null;
			try
			{
				using (var iter = Lexer.Tokenize(source).GetEnumerator())
				{
					while (iter.MoveNext())
					{
						token = iter.Current;
						switch (token.Type)
						{
							case TokenType.Trigger:
								if (currentTrigger != null)
								{
									if (prevTrigger != null)
									{
										if (prevTrigger.Category == TriggerCategory.Effect && currentTrigger.Category == TriggerCategory.Cause)
										{
											triggerBlocks.Add(block);
											block = new TriggerList();
										}
									}
									block.Add(currentTrigger);
									prevTrigger = currentTrigger;
								}
								currentTrigger = new Trigger
								{
								    Category = (TriggerCategory) IntParse(token.Value.Substring(1, token.Value.IndexOf(':') - 1))
								};
						        string id = token.Value.Substring(token.Value.IndexOf(':') + 1);
								id = id.Substring(0, id.Length - 1);
								currentTrigger.Id = IntParse(id);
								break;
							case TokenType.Variable:
								//if (currentTrigger.VariableNameReferences.Contains(token.Value) == false)
								currentTrigger.contents.Enqueue(token.Value);
								break;
							case TokenType.String:
								token.Value = token.Value.Substring(1, token.Value.Length - 2);
								if (token.Value.Length > Engine.Options.StringLengthLimit) throw new Exception("String length limit exceeded.");
								currentTrigger.contents.Enqueue(token.Value);
								break;
							case TokenType.Number:
								double val = double.Parse(token.Value, System.Globalization.NumberStyles.AllowDecimalPoint);
								currentTrigger.contents.Enqueue(val);
								break;
							case TokenType.EOF:
								if (currentTrigger != null)
								{
									//if (currentTrigger.Category != TriggerCategory.Cause || currentTrigger.Category != TriggerCategory.Condition ||
									if (currentTrigger.Category != TriggerCategory.Undefined)
									{
										block.Add(currentTrigger);
										triggerBlocks.Add(block);
									}
								}
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Error at {0} with {1}", token, currentTrigger), ex);
			}
			return triggerBlocks;
		}

		private int IntParse(string value)
		{
			int result = 0;
			for (int i = 0; i < value.Length; i++)
			{
				result = 10 * result + (value[i] - 48);
			}
			return result;
		}
	}
}
