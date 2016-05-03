using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Monkeyspeak
{
	/// <summary>
	///
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class TriggerHandlerAttribute : Attribute
	{
		private TriggerCategory triggerCategory;
		private int triggerID;
		private string description;
		internal MethodInfo owner;

		/// <summary>
		///
		/// </summary>
		/// <param name="category">Trigger Category</param>
		/// <param name="id">Trigger ID</param>
		/// <param name="description">Trigger Description</param>
		public TriggerHandlerAttribute(TriggerCategory category, int id, string description)
		{
			this.triggerCategory = category;
			this.triggerID = id;
			this.description = description;
		}

		internal void Register(Page page)
		{
			TriggerHandler handler = (TriggerHandler)Delegate.CreateDelegate(typeof(TriggerHandler), owner, false);
			if (handler != null)
				page.SetTriggerHandler(triggerCategory, triggerID, (TriggerHandler)Delegate.CreateDelegate(typeof(TriggerHandler), owner, false), description);
		}
	}
}
