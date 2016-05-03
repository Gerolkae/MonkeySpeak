using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Monkeyspeak.lexical;

namespace Monkeyspeak
{
	internal class Lexer : ILexer
	{
		Regex endOfLineRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
		IList<TokenDefinition> tokenDefinitions = new List<TokenDefinition>();

		public void AddDefinition(TokenDefinition tokenDefinition)
		{
			tokenDefinitions.Add(tokenDefinition);
		}

		public IEnumerable<Token> Tokenize(string source)
		{
			int currentIndex = 0;
			int currentLine = 1;
			int currentColumn = 0;

			while (currentIndex < source.Length)
			{
				TokenDefinition matchedDefinition = null;
				int matchLength = 0;

				for (int i = 0; i <= tokenDefinitions.Count - 1; i++)
				{
					TokenDefinition rule = tokenDefinitions[i];
					Match match = rule.Regex.Match(source, currentIndex);

					if (match.Success && (match.Index - currentIndex) == 0)
					{
						matchedDefinition = rule;
						matchLength = match.Length;
						break;
					}
				}

				if (matchedDefinition == null)
				{
					throw new Exception(string.Format("Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).", source[currentIndex], currentIndex, currentLine, currentColumn));
				}
				else
				{
					string value = source.Substring(currentIndex, matchLength);

					if (!matchedDefinition.IsIgnored)
						yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentLine, currentColumn));

					var endOfLineMatch = endOfLineRegex.Match(value);
					if (endOfLineMatch.Success)
					{
						currentLine += 1;
						currentColumn = value.Length - (endOfLineMatch.Index + endOfLineMatch.Length);
					}
					else
					{
						currentColumn += matchLength;
					}

					currentIndex += matchLength;
				}
			}

			yield return new Token(TokenType.EOF, null, new TokenPosition(currentIndex, currentLine, currentColumn));
		}
	}
}
