using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Monkeyspeak;

namespace Monkeyspeak.WindowsForms.Lib
{
	public class ControlLib
	{
		internal static Dictionary<double, Control> controls = new Dictionary<double, Control>();
		internal static Random rand = new Random();

		internal static double CreateRandomID()
		{
			return Math.Abs(rand.NextDouble() * rand.Next());
		}

		public static void RegisterControlMouseClicked(Page page, Control control)
		{
			control.MouseClick += delegate(object sender, MouseEventArgs e)
			{
				page.Execute(TriggerCategory.Cause, 5000);
			};
		}

		[TriggerHandler(TriggerCategory.Cause, 5000, "(0:5000) when a control is clicked,")]
		public static bool ControlClicked(TriggerReader reader)
		{
			return true;
		}

		[TriggerHandler(TriggerCategory.Condition, 5000, "(1:5000) and the triggered control's id is #,")]
		public static bool AndControlIs(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			return controls.ContainsKey(reader.ReadNumber());
		}

		[TriggerHandler(TriggerCategory.Effect, 5000, "(5:5000) set control # text to {...}.")]
		public static bool SetText(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			Control control;
			bool containsControl = controls.TryGetValue(reader.ReadNumber(), out control);
			if (containsControl == false) return false;

			control.Text = (reader.PeekString()) ? reader.ReadString() : "";
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5001, "(5:5003) make control # visible.")]
		public static bool MakeControlVisible(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			Control control;
			bool containsControl = controls.TryGetValue(reader.ReadNumber(), out control);
			if (containsControl == false) return false; // Maybe throw exception for control not found

			control.Visible = true;
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5002, "(5:5004) make control # not visible.")]
		public static bool MakeControlInVisible(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			Control control;
			bool containsControl = controls.TryGetValue(reader.ReadNumber(), out control);
			if (containsControl == false) return false; // Maybe throw exception for control not found

			control.Visible = false;
			return true;
		}

		[TriggerHandler(TriggerCategory.Effect, 5010, "(5:5010) make control # the triggered control. (see variable \"%triggered.control\"")]
		public static bool MakeTriggeredControl(TriggerReader reader)
		{
			if (reader.PeekNumber() == false) return false;
			double id = reader.ReadNumber();
			reader.Page.SetVariable("triggered.control", id, true);
			return true;
		}
	}
}