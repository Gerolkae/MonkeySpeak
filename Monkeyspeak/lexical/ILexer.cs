using System;
using System.Collections.Generic;
using Monkeyspeak.lexical;

namespace Monkeyspeak
{
	internal interface ILexer
	{
		void AddDefinition(TokenDefinition tokenDefinition);

		IEnumerable<Token> Tokenize(string source);
	}
}
