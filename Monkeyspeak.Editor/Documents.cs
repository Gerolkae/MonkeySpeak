using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Monkeyspeak.Editor
{
	internal struct Document
	{
		private TextBox editBox;
		private TabPage page;

		public Document(TabPage page, TextBox editBox)
		{
			this.page = page;
			this.editBox = editBox;
		}
	}

	public static class Documents
	{
		private static MainWindow editorWindow;
		private static List<Document> docs;

		public static void Initialize(MainWindow editorWindow)
		{
			Documents.editorWindow = editorWindow;
			docs = new List<Document>();
		}

		public static void Create(string name)
		{
			TabControl editorTabControl = editorWindow.Controls["editorTabControl"] as TabControl;
			editorTabControl.TabPages.Add(name);
		}
	}
}