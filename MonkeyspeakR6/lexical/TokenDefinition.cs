using System;
using System.Text.RegularExpressions;

namespace Monkeyspeak
{
	internal class TokenDefinition
	{
		public TokenDefinition(
			TokenType type,
			Regex regex)
			: this(type, regex, false)
		{
		}

		public TokenDefinition(
			TokenType type,
			Regex regex,
			bool isIgnored)
		{
			Type = type;
			Regex = regex;
			IsIgnored = isIgnored;
		}

		public bool IsIgnored { get; private set; }

		public Regex Regex { get; private set; }

		public TokenType Type { get; private set; }
	}
}
