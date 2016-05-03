using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Monkeyspeak
{
	[Serializable]
	internal class TriggerList : List<Trigger>
	{
		public TriggerList() { }

		public TriggerList(int initialCapacity) :
			base(initialCapacity)
		{

		}

		public TriggerList(params Trigger[] triggers) :
			base(triggers)
		{
		}

	    public bool HasTrigger(TriggerCategory cat, int id)
	    {
	        for (int i = 0; i <= Count - 1; i++)
	        {
	            Trigger trigger = this[i];
	            if (trigger.Category == cat && trigger.Id == id)
	            {
	                return true;
	            }
	        }
	        return false;
	    }
	}
}
