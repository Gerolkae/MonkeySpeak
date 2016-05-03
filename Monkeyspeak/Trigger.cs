using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkeyspeak
{
	[Serializable]
  
	public enum TriggerCategory : int
	{
		/// <summary>
		/// A trigger defined with a 0
		/// <para>Example: (0:1) when someone says something, </para>
		/// </summary>
		Cause = 0,
		/// <summary>
		/// A trigger defined with a 1
		/// <para>Example: (1:2) and they moved # units left, </para>
		/// </summary>
		Condition = 1,
		/// <summary>
		/// A trigger defined with a 5
		/// <para>Example: (5:1) print {Hello World} to the console. </para>
		/// </summary>
		Effect = 5,
		/// <summary>
		/// A trigger that was not defined.  You should never encounter this
		/// if you do then something isn't quite right.
		/// </summary>
		Undefined = -1
	}
 
	[Serializable]
 	public sealed class Trigger : IEquatable<Trigger> 
	{
           
		private TriggerCategory category;
		private int id;
		internal Queue<object> contents = new Queue<object>(16);

		private string description = "";

		internal Trigger()
		{
			category = TriggerCategory.Undefined;
			id = -1;
		}

		public Trigger(TriggerCategory cat, int id)
		{
			category = cat;
			this.id = id;
		}

		public string Description
		{
			get { return description; }
			 set { description = value; }
		}
        
		public TriggerCategory Category
		{
			get { return category; }
			internal set { category = value; }
		}

		public int Id
		{
			get { return id; }
			internal set { id = value; }
		}

		internal Queue<object> Contents
		{
			get { return contents; }
			set { contents = value; }
		}

		internal Trigger Clone()
		{
			Trigger clone = new Trigger(category, id);
			clone.contents = new Queue<object>(this.contents);
			return clone;
		}

		public bool Equals(Trigger other)
		{
			return other.category == this.category && other.id == this.id;
		}

		public override bool Equals(object obj)
		{
			if (obj is Trigger)
			{
				var other = (Trigger)obj;
				return other.category == this.category && other.id == this.id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((int)this.category * this.id);
		}

		public override string ToString()
		{
			return String.Format("({0}:{1})", (int)category, id);
		}
	}
}
