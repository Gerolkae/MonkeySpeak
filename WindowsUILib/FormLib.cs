using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Monkeyspeak;

namespace Monkeyspeak.WindowsForms.Lib
{
	public class FormLib
	{
		[TriggerHandler(TriggerCategory.Effect, 5100, "(5:5100) show message box with message {...}.")]
		public static bool CreateMessageBox(TriggerReader reader)
		{
			string msg = (reader.PeekString()) ? reader.ReadString() : "";
			MessageBox.Show(msg);
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5101, "(5:5101) create windows form with title {...} and set the id to variable #.")]
		public static bool CreateForm(TriggerReader reader)
		{
			string formName = (reader.PeekString()) ? reader.ReadString() : "Form";
			if (reader.PeekVariable() == false) return false;
			Variable var = reader.ReadVariable(true);
			Form form = new Form();
			form.Text = formName;
			double id = ControlLib.CreateRandomID();
			reader.Page.SetVariable(var.Name, id, var.IsConstant);
			form.Name = id.ToString();
			ControlLib.controls.Add(id, form);
			form.ShowDialog();
			form.Visible = true;
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5110, "(5:110) show windows form #.")]
		public static bool ShowForm(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			Control control;
			bool containsControl = ControlLib.controls.TryGetValue(reader.ReadNumber(), out control);
			if (containsControl == false) return false; // Maybe throw exception for control not found
			if ((control is Form) == false) return false;

			((Form)control).ShowDialog();
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5111, "(5:111) hide windows form #.")]
		public static bool HideForm(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			Control control;
			bool containsControl = ControlLib.controls.TryGetValue(reader.ReadNumber(), out control);
			if (containsControl == false) return false; // Maybe throw exception for control not found
			if ((control is Form) == false) return false;

			((Form)control).Hide();
			return true;
		}
	}
}