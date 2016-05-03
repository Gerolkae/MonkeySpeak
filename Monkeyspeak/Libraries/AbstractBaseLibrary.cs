using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkeyspeak.Libraries
{
	public abstract class AbstractBaseLibrary
	{
		internal Dictionary<Trigger, TriggerHandler> handlers;
		internal List<string> descriptions;

		/// <summary>
		/// Base abstract class for Monkeyspeak Libraries
		/// </summary>
		protected AbstractBaseLibrary()
		{
			handlers = new Dictionary<Trigger, TriggerHandler>();
			descriptions = new List<string>();
		}

		/// <summary>
		/// Raises a MonkeyspeakException
		/// </summary>
		/// <param name="reason">Reason for the error</param>
		public void RaiseError(string reason)
		{
			throw new MonkeyspeakException(reason);
		}

		/// <summary>
		/// Registers a Trigger to the TriggerHandler with optional description
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="handler"></param>
		/// <param name="description"></param>
		protected void Add(Trigger trigger, TriggerHandler handler, string description = null)
		{
			if (description != null) trigger.Description = description;
			if (handlers.ContainsKey(trigger) == false)
				handlers.Add(trigger, handler);
			else throw new UnauthorizedAccessException("Attempt to override existing Trigger handler.");
		}

		/// <summary>
		/// Registers a Trigger to the TriggerHandler with optional description
		/// </summary>
		/// <param name="cat"></param>
		/// <param name="id"></param>
		/// <param name="handler"></param>
		/// <param name="description"></param>
		protected void Add(TriggerCategory cat, int id, TriggerHandler handler, string description = null)
		{
			Trigger trigger = new Trigger(cat, id);
			if (description != null) trigger.Description = description;
			if (handlers.ContainsKey(trigger) == false)
				handlers.Add(trigger, handler);
                
			else throw new UnauthorizedAccessException("Attempt to override existing Trigger handler.");
		}

		/// <summary>
		/// Registers this library to a Page
		/// </summary>
		/// <param name="page"></param>
		internal void Register(Page page)
		{
			lock (page.syncObj)
			{
				for (int i = 0; i <= handlers.Count - 1; i++)
				{
					var kv = handlers.ElementAt(i);
					page.SetTriggerHandler(kv.Key, kv.Value);
				}
				// Ensure that this library can only be loaded once.
				handlers.Clear();
			}
		}

		/// <summary>
		/// Builds a string representation of the descriptions of each trigger.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i <= descriptions.Count - 1; i++)
			{
				builder.Append(descriptions[i]).Append(Environment.NewLine);
			}
			return builder.ToString();
		}
	}
}
