using System;

namespace Monkeyspeak
{
	internal enum TokenType : byte
	{
		Trigger, String, Number, Variable, Comment, WhiteSpace, Word, Symbol, EOF
	}

	internal class Token
	{
		public Token(TokenType type, string value, TokenPosition position)
		{
			Type = type;
			Value = value;
			Position = position;
		}

		public TokenPosition Position { get; set; }

		public TokenType Type { get; set; }

		public string Value { get; set; }

		public override string ToString()
		{
			return string.Format("Token: {{ Type: \"{0}\", Value: \"{1}\", Position: {{ Index: \"{2}\", Line: \"{3}\", Column: \"{4}\" }} }}", Type, Value, Position.Index, Position.Line, Position.Column);
		}
	}
}
