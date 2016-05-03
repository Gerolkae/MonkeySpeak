using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkeyspeak.Editor
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			MainWindow editor = new MainWindow();
			editor.ShowDialog();
		}
	}
}