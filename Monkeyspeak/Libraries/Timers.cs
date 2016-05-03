using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Diagnostics;
namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// A TimerTask object contains Timer and Page Owner.  Timer is not started from a TimerTask constructor.
    /// </summary>
    internal sealed class TimerTask
    {
        private Timer timer;
        private object _timerLock = new object();
        private double interval = 0;
        private int _timerlockSync = 0;
        public double Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        public Timer Timer
        {
            get { return timer; }
            set { timer = value; }
        }

        private Page owner;

        public Page Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private double id;

        public double ID
        {
            get { return id; }
            set { id = value; }
        }

        public TimerTask() { timer = new Timer(timer_Elapsed); }

        public TimerTask(Page owner, double interval, double Id)
        {
            ID = Id;
            this.owner = owner;
            this.interval = interval;
            timer = new Timer(timer_Elapsed, this, TimeSpan.Zero, TimeSpan.FromSeconds(interval));

        }

        public static TimerTask CurrentTimer = null;

        private void timer_Elapsed(object sender)
        {
            // Lets Capture the Current Triggering Timer
            // ~Gerolkae

            // Prevent lultiple threads from executing the same timer and incrememnting
            // Variables before the next cycle
            if (0 == Interlocked.Exchange(ref _timerlockSync, 1))
            {
                try
                {
                    CurrentTimer = (TimerTask)sender;
                    owner.Execute(300);
                    CurrentTimer = null;
                }
                catch
                {
                    // Eat the exception.. yummy!
                }
                Interlocked.Exchange(ref _timerlockSync, 0);
            }
        }
    }

    // Changed from Internal to public in order to expose DestroyTimers() - Gerolkae
	public class Timers : AbstractBaseLibrary
	{
		private static readonly Dictionary<double, TimerTask> timers = new Dictionary<double, TimerTask>();
		private static readonly object lck = new object();
		/// <summary>
		/// Closes and removes all Timers
		/// </summary>
		public static void DestroyTimers()
		{
			
			var keys = new List<double>(timers.Keys);
			lock (lck)
			{
				for (int i=0;i<=keys.Count-1;i++)
				{
                    double key = keys[i];
					/*
					 * MSDN A Dictionary<TKey, TValue> can support multiple readers 
					 * concurrently, as long as the collection is not modified. Even 
					 * so, enumerating through a collection is intrinsically not a 
					 * thread-safe procedure. In the rare case where an enumeration 
					 * contends with write accesses, the collection must be locked 
					 * during the entire enumeration. To allow the collection to be 
					 * accessed by multiple threads for reading and writing, you must 
					 * implement your own synchronization.
					 */

				if (timers[key].Timer != null)
					  timers[key].Timer.Dispose();
				timers.Remove(key);  
				}
 
			}
		}

		/// <summary>
		/// Default Timer Library.  Call static method Timers.DestroyTimers() when your application closes.
		/// </summary>
		public Timers()
		{
			// (0:300) When timer # goes off,
			Add(new Trigger(TriggerCategory.Cause, 300), WhenTimerGoesOff,
				"(0:300) When timer # goes off,");

			// (1:300) and timer # is running,
			Add(new Trigger(TriggerCategory.Condition, 300), AndTimerIsRunning,
				"(1:300) and timer # is running,");
			// (1:301) and timer # is not running,
			Add(new Trigger(TriggerCategory.Condition, 301), AndTimerIsNotRunning,
				"(1:301) and timer # is not running,");

			// (5:300) create timer # to go off every # second(s).
			Add(new Trigger(TriggerCategory.Effect, 300), CreateTimer,
				"(5:300) create timer # to go off every # second(s).");

			// (5:301) stop timer #.
			Add(new Trigger(TriggerCategory.Effect, 301), StopTimer,
				"(5:301) stop timer #.");
		}

		private bool TryGetTimerFrom(TriggerReader reader, out TimerTask timerTask)
		{
			double num = double.NaN;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value is double)
					num = (double)var.Value;
			}
			else if (reader.PeekNumber())
			{
				num = reader.ReadNumber();
			}

			if (double.IsNaN(num) == false)
			{
				if (timers.ContainsKey(num) == false)
				{
					// Don't add a timer to the Dictionary if it don't 
					// exist. Just return a blank timer
					// - Gerolkae
					timerTask = new TimerTask();
					return false;
				}
			    timerTask = timers[num];
			    return true;
			}
			timerTask = null;
			return false;
		}

        private bool WhenTimerGoesOff(TriggerReader reader)
        {
            return true;
            // Make sure only the Current Timer triggers
            // ~Gerolkae
            // Changed your code, it was unnecessary.  This is triggered by the Timers themselves.
            // Run the TimersLibraryTest unit test to see what I mean ~Squizzle
        }

		private bool AndTimerIsRunning(TriggerReader reader)
		{
			TimerTask timerTask;
			if (TryGetTimerFrom(reader, out timerTask) == false) 
				return false;
			bool test = timerTask.Timer != null;

			return test;
		}

		private bool AndTimerIsNotRunning(TriggerReader reader)
		{
			bool test = AndTimerIsRunning(reader) == false;
			return test;
		}

		private bool CreateTimer(TriggerReader reader)
		{
		    if (timers.Count > reader.Page.Engine.Options.TimerLimit)
		    {
                throw new MonkeyspeakException("The amount of timers has exceeded the limit by {0}", timers.Count - reader.Page.Engine.Options.TimerLimit);
		    }
			TimerTask timerTask = new TimerTask();
		    double id = double.NaN;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value is double)
					id = (double)var.Value;
			}
			else if (reader.PeekNumber())
			{
				id = reader.ReadNumber();
			}

			double interval = double.NaN;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value is double)
					interval = (double)var.Value;
			}
			else if (reader.PeekNumber())
			{
				interval = reader.ReadNumber();
			}

			if (double.IsNaN(interval)) return false;


			lock (lck)
			{
                if (timerTask.Timer != null) timerTask.Timer.Dispose();
                timerTask = new TimerTask(reader.Page, interval, id);
                if (timers.Keys.Contains(id))
                {
                    Console.WriteLine("WARNING: Replacing existing timer {0} may cause any triggers dependent" +
                                      " on that timer to behave differently.", id);
                    timers[id].Timer.Dispose();
                    timers[id] = timerTask;
                }
                else
                    timers.Add(id, timerTask);
                
			}

			return true;
		}

		private bool StopTimer(TriggerReader reader)
		{
			// Does NOT destroy the Timer.
			//Now it Does! TryGetTimerFrom(reader) uses Dictionary.ContainsKey
		    double num = 0;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (Double.TryParse(var.Value.ToString(), out num) == false)
							   num = 0;
			}
			else if (reader.PeekNumber())
			{
				num = reader.ReadNumber();
			}
		    try
		    {
		        lock (lck)
		        {
		            if (timers.ContainsKey(num))
		            {
		                timers[num].Timer.Dispose();
		                timers[num].Timer = null;
		                timers.Remove(num);
		            }
		        }
		    }
		    catch (Exception x)
		    {
		        Console.WriteLine(x.Message);
		    }
		    return true;
		}
	}
}