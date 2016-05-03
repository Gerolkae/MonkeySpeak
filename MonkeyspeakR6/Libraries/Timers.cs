using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
namespace Monkeyspeak.Libraries
{
	/// <summary>
	/// A TimerInfo object contains Timer and Page Owner.  Timer is not started from a TimerInfo constructor.
	/// </summary>
	internal class TimerInfo
	{
        private object lck = new object();
		private Timer timer;
        private object _timerLock = new object();
		private double interval = 0;

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

		public TimerInfo()
		{
			timer = new Timer(timer_Elapsed);
		}

		public TimerInfo(Page owner, double interval, double Id)
		{
            this.ID = Id;
			this.owner = owner;
			this.interval = interval;
			timer = new Timer(timer_Elapsed, this, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
           
		}
        public static object CurrentTimer = null;
        private void timer_Elapsed(object sender)
		{
            // Lets Capture the Current Triggering Timer
            // ~Gerolkae
                if (!Monitor.TryEnter(_timerLock))
                {
                    // something has the lock. Probably shutting down.
                    return;
                }
                try
                {
                    
                	CurrentTimer = (TimerInfo)sender;
                    owner.Execute(300,301);
                    CurrentTimer = null;
                }
                catch (Exception  Ex)
                {
                    System.Diagnostics.Debug.Print(Ex.Message);
                }
                finally
                {
                    Monitor.Exit(_timerLock);
                }
           }
		
	}

	// Changed from Internal to public in order to expose DestroyTimers() - Gerolkae
	public class Timers : AbstractBaseLibrary
	{
		private static Dictionary<double, TimerInfo> timers = new Dictionary<double, TimerInfo>();
		private static object lck = new object();
		/// <summary>
		/// Closes and removes all Timers
		/// </summary>
		public static void DestroyTimers()
		{
			
			List<double> keys = new List<double>(timers.Keys);
			lock (lck)
			{
				foreach (double key in keys)
				{
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

			Add(new Trigger(TriggerCategory.Cause, 301), WhenAnyTimerGoesOff,
				"(0:301) When any timer goes off,");
			
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

		private bool TryGetTimerFrom(TriggerReader reader, out TimerInfo timerInfo)
		{
			double num = double.NaN;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value != null && var.Value is double)
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
					timerInfo = new TimerInfo();
					return false;
				}
				else
				{
					timerInfo = timers[num];
					return true;
				}
				
			}
			timerInfo = null;
			return false;
		}

        private bool WhenTimerGoesOff(TriggerReader reader)
        {
            TimerInfo timerInfo;
            lock (lck)
            {

                if (TryGetTimerFrom(reader, out timerInfo) == true)
                {
                    // Make sure only the Current Timer triggers
                    // ~Gerolkae
                    //System.Diagnostics.Debug.Print(timerInfo.ID.ToString() + "Timer Triggered");
                    return timerInfo == TimerInfo.CurrentTimer;
                }

                return false;
            }
        }
        
        private bool WhenAnyTimerGoesOff(TriggerReader reader)
        {
			return true;
        }

		private bool AndTimerIsRunning(TriggerReader reader)
		{
			TimerInfo timerInfo;
			if (TryGetTimerFrom(reader, out timerInfo) == false) 
				return false;
			bool test = timerInfo.Timer != null;

			return test;
		}

		private bool AndTimerIsNotRunning(TriggerReader reader)
		{
			bool test = AndTimerIsRunning(reader) == false;
			return test;
		}

		private bool CreateTimer(TriggerReader reader)
		{
			TimerInfo timerInfo = new TimerInfo();
			double idx = new double();
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value != null && var.Value is double)
					idx = (double)var.Value;
			}
			else if (reader.PeekNumber())
			{
				idx = reader.ReadNumber();
			}

			double num = double.NaN;
			if (reader.PeekVariable())
			{
				Variable var = reader.ReadVariable();
				if (var.Value.GetType() == typeof(double))
					num = Convert.ToDouble(var.Value);
			}
			else if (reader.PeekNumber())
			{
				num = reader.ReadNumber();
			}

			if (double.IsNaN(num)) return false;


			lock (lck)
			{
                if (timerInfo.Timer != null) timerInfo.Timer.Dispose();
                timerInfo = new TimerInfo(reader.Page, num, idx);
                timerInfo.Timer.InitializeLifetimeService();
                if (timers.Keys.Contains(idx) == true)
                {
                    System.Diagnostics.Debug.Print("New Timer Disposing old timer " + idx.ToString());
                    timers[idx].Timer.Dispose();
                    timers[idx] = timerInfo;
                }
                else
                    timers.Add(idx, timerInfo);
                
			}

			return true;
		}

		private bool StopTimer(TriggerReader reader)
		{
			// Does NOT destroy the Timer.
			//Now it Does! TryGetTimerFrom(reader) uses Dictionary.ContainsKey
			TimerInfo timerInfo = new TimerInfo();
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
                        System.Diagnostics.Debug.Print("Stop Timer " + num.ToString());
                        timers[num].Timer.Dispose();
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