using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Monkeyspeak.lexical;

namespace Monkeyspeak
{
	[Serializable]
	public class TypeNotSupportedException : Exception
	{
		public TypeNotSupportedException() { }

		public TypeNotSupportedException(string message) : base(message) { }

		public TypeNotSupportedException(string message, Exception inner) : base(message, inner) { }

		protected TypeNotSupportedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	/// <summary>
	/// Used for handling triggers at runtime.
	/// </summary>
	/// <param name="reader"></param>
	/// <returns>True = Continue to the Next Trigger, False = Stop executing current TriggerList</returns>
	public delegate bool TriggerHandler(TriggerReader reader);

	public delegate void TriggerAddedEventHandler(Trigger trigger, TriggerHandler handler);

	public delegate bool TriggerHandledEventHandler(Trigger trigger);

	/// <summary>
	/// Event for any errors that occur during execution
	/// If not assigned Exceptions will be thrown.
	/// </summary>
	/// <param name="trigger"></param>
	/// <param name="ex"></param>
	public delegate void TriggerHandlerErrorEvent(Trigger trigger, Exception ex);

	[Serializable]
	public sealed class Page
	{
		public object syncObj = new Object();

		private List<TriggerList> triggerBlocks;
		private volatile List<Variable> scope;

		private volatile Dictionary<Trigger, TriggerHandler> handlers = new Dictionary<Trigger, TriggerHandler>();
		private MonkeyspeakEngine engine;

		/// <summary>
		/// Called when an Exception is raised during execution
		/// </summary>
		public event TriggerHandlerErrorEvent Error;

		/// <summary>
		/// Called when a Trigger and TriggerHandler is added to the Page
		/// </summary>
		public event TriggerAddedEventHandler TriggerAdded;

		/// <summary>
		/// Called before the Trigger's TriggerHandler is called.  If there is no TriggerHandler for that Trigger
		/// then this event is not raised.
		/// </summary>
		public event TriggerHandledEventHandler BeforeTriggerHandled;

		/// <summary>
		/// Called after the Trigger's TriggerHandler is called.  If there is no TriggerHandler for that Trigger
		/// then this event is not raised.
		/// </summary>
		public event TriggerHandledEventHandler AfterTriggerHandled;

		internal Page(MonkeyspeakEngine engine)
		{
			this.engine = engine;
			triggerBlocks = new List<TriggerList>();
			scope = new List<Variable>();
			scope.Add(Variable.NoValue.Clone());
		}

		internal void Write(List<TriggerList> blocks)
		{
			triggerBlocks.AddRange(blocks);
			if (Size > engine.Options.TriggerLimit)
				throw new Exception("Trigger limit exceeded.");
		}

		internal void OverWrite(List<TriggerList> blocks)
		{
			triggerBlocks.Clear();
			triggerBlocks.AddRange(blocks);
			if (Size > engine.Options.TriggerLimit)
				throw new Exception("Trigger limit exceeded.");
		}
		internal bool CheckType(object value)
		{
			if (value == null) return true;

			if (value is String ||
				value is Double)
				return true;
			return false;
		}

		internal MonkeyspeakEngine Engine
		{
			get { return engine; }
		}

		public void CompileToStream(Stream stream)
		{
			try
			{
				Compiler compiler = new Compiler(engine);
				using (Stream zipStream = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Compress))
				{
					compiler.CompileToStream(triggerBlocks, zipStream);
				}
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not compile to file.  Reason:{0}", ex.Message), ex);
			}
		}

		public void CompileToFile(string filePath)
		{
			try
			{
				Compiler compiler = new Compiler(engine);
				using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					using (Stream zipStream = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Compress))
					{
						compiler.CompileToStream(triggerBlocks, zipStream);
					}
				}
			}
			catch (IOException ioex)
			{
				throw ioex;
			}
			catch (Exception ex)
			{
				throw new MonkeyspeakException(String.Format("Could not compile to file.  Reason:{0}", ex.Message), ex);
			}
		}

		/// <summary>
		/// Clears all Variables and optionally clears all TriggerHandlers from this Page.
		/// </summary>
		public void Reset(bool resetTriggerHandlers = false)
		{
			scope.Clear();
			scope.Add(Variable.NoValue.Clone());

			if (resetTriggerHandlers) handlers.Clear();
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>IEnumerable of Triggers</returns>
		public IEnumerable<string> GetTriggerDescriptions()
		{
			lock (syncObj)
			{
				foreach (var handler in handlers.OrderBy(kv => kv.Key.Category))
				{
					yield return handler.Key.Description;
				}
			}
		}

		public ReadOnlyCollection<Variable> Scope
		{
			get { return scope.AsReadOnly(); }
		}

		/// <summary>
		/// Loads Monkeyspeak Sys Library into this Page
		/// <para>Used for System operations involving the Environment or Operating System</para>
		/// </summary>
		public void LoadSysLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.Sys());
		}

		/// <summary>
		/// Loads Monkeyspeak String Library into this Page
		/// <para>Used for basic String operations</para>
		/// </summary>
		public void LoadStringLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.StringOperations());
		}

		/// <summary>
		/// Loads Monkeyspeak IO Library into this Page
		/// <para>Used for File Input/Output operations</para>
		/// </summary>
		public void LoadIOLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.IO());
		}

		/// <summary>
		/// Loads Monkeyspeak Math Library into this Page
		/// <para>Used for Math operations (add, subtract, multiply, divide)</para>
		/// </summary>
		public void LoadMathLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.Math());
		}

		/// <summary>
		/// Loads Monkeyspeak Timer Library into this Page
		/// </summary>
		public void LoadTimerLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.Timers());
		}

		/// <summary>
		/// Loads Monkeyspeak Debug Library into this Page
		/// <para>Used for Debug breakpoint insertion. Won't work without Debugger attached.</para>
		/// </summary>
		public void LoadDebugLibrary()
		{
			LoadLibrary(new Monkeyspeak.Libraries.Debug());
		}

		/// <summary>
		/// Loads a <see cref="Monkeyspeak.Libraries.AbstractBaseLibrary"/> into this Page
		/// </summary>
		/// <param name="lib"></param>
		public void LoadLibrary(Monkeyspeak.Libraries.AbstractBaseLibrary lib)
		{
			lib.Register(this);
		}

		/// <summary>
		/// Loads trigger handlers from a assembly dll file
		/// </summary>
		/// <param name="assemblyFile">The assembly in the local file system</param>
		public void LoadLibraryFromAssembly(string assemblyFile)
		{
			Assembly asm;
			if (File.Exists(assemblyFile) == false) throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed, file not found.");
			else if (ReflectionHelper.TryLoad(assemblyFile, out asm) == false)
			{
				throw new MonkeyspeakException("Load library from file '" + assemblyFile + "' failed.");
			}

			Type[] types = ReflectionHelper.GetAllTypes(asm);
			foreach (MethodInfo method in ReflectionHelper.GetAllMethods(types))
			{
				foreach (TriggerHandlerAttribute attribute in ReflectionHelper.GetAllAttributesFromMethod<TriggerHandlerAttribute>(method))
				{
					attribute.owner = method;
					try
					{
						attribute.Register(this);
					}
					catch (Exception ex)
					{
						throw new MonkeyspeakException(String.Format("Load library from file '{0}' failed, couldn't bind to method '{1}.{2}'", assemblyFile,method.DeclaringType.Name, method.Name), ex);
					}
				}
			}
		}

		public Variable SetVariable(string name, object value, bool isConstant)
		{
			if (!CheckType(value)) throw new TypeNotSupportedException(String.Format("{0} is not a supported type. Expecting string or double.", value.GetType().Name));

			if (name.StartsWith(engine.Options.VariableDeclarationSymbol.ToString()) == false) name = engine.Options.VariableDeclarationSymbol + name;

			Variable var;

			lock (syncObj)
			{
				for (int i = scope.Count - 1; i >= 0; i--)
				{
					if (scope[i].Name.Equals(name))
					{
						var = scope[i];
						var.IsConstant = isConstant;
						var.ForceAssignValue(value);
						return var;
					}
				}

				if (scope.Count + 1 > engine.Options.VariableCountLimit) throw new Exception("Variable limit exceeded operation cannot complete.");
				var = new Variable(name, value, isConstant);
				scope.Add(var);
				return var;
			}
		}

		/// <summary>
		/// Gets a Variable with Name set to <paramref name="name"/>
		/// <b>Throws Exception if Variable not found.</b>
		/// </summary>
		/// <param name="name">The name of the Variable to retrieve</param>
		/// <returns>The variable found with the specified <paramref name="name"/> or throws Exception</returns>
		public Variable GetVariable(string name)
		{
			if (name.StartsWith(engine.Options.VariableDeclarationSymbol.ToString()) == false) name = engine.Options.VariableDeclarationSymbol + name;

			lock (syncObj)
			{
				for (int i = scope.Count - 1; i >= 0; i--)
				{
					if (scope[i].Name.Equals(name))
						return scope[i];
				}
				throw new Exception("Variable \"" + name + "\" not found.");
			}
		}

		/// <summary>
		/// Checks the scope for the Variable with Name set to <paramref name="name"/>
		/// </summary>
		/// <param name="name">The name of the Variable to retrieve</param>
		/// <returns>True on Variable found.  <para>False if Variable not found.</para></returns>
		public bool HasVariable(string name)
		{
			if (name.StartsWith(engine.Options.VariableDeclarationSymbol.ToString()) == false) name = engine.Options.VariableDeclarationSymbol + name;

			lock (syncObj)
			{
				for (int i = scope.Count - 1; i >= 0; i--)
				{
					if (scope[i].Name.Equals(name)) return true;
				}
				return false;
			}
		}

		public bool HasVariable(string name, out Variable var)
		{
			if (name.StartsWith(engine.Options.VariableDeclarationSymbol.ToString()) == false) name = engine.Options.VariableDeclarationSymbol + name;

			lock (syncObj)
			{
				for (int i = scope.Count - 1; i >= 0; i--)
				{
					if (scope[i].Name.Equals(name))
					{
						var = scope[i];
						return true;
					}
				}
				var = Variable.NoValue;
				return false;
			}
		}

		/// <summary>
		/// Assigns the TriggerHandler to a trigger with <paramref name="category"/> and <paramref name="id"/>
		/// </summary>
		/// <param name="category"></param>
		/// <param name="id"></param>
		/// <param name="handler"></param>
		/// <param name="description"></param>
		public void SetTriggerHandler(TriggerCategory category, int id, TriggerHandler handler, string description = null)
		{
			SetTriggerHandler(new Trigger(category, id), handler, description);
		}

		/// <summary>
		/// Assigns the TriggerHandler to <paramref name="trigger"/>
		/// </summary>
		/// <param name="trigger"><see cref="Monkeyspeak.Trigger"/></param>
		/// <param name="handler"><see cref="Monkeyspeak.TriggerHandler"/></param>
		/// <param name="description">optional description of the trigger, normally the human readable form of the trigger
		/// <para>Example: "(0:1) when someone says something,"</para></param>
		public void SetTriggerHandler(Trigger trigger, TriggerHandler handler, string description = null)
		{
			lock (syncObj)
			{
				if (description != null)
					trigger.Description = description;
				if (handlers.ContainsKey(trigger) == false)
				{
					handlers.Add(trigger, handler);
					sizeChanged = true;
					if (TriggerAdded != null) TriggerAdded(trigger, handler);
				}
				else
					if (engine.Options.CanOverrideTriggerHandlers)
					{
						handlers[trigger] = handler;
					}
					else throw new UnauthorizedAccessException("Attempt to override existing Trigger handler.");
			}
		}

		public void RemoveTriggerHandler(TriggerCategory cat, int id)
		{
			lock (syncObj)
			{
				handlers.Remove(new Trigger(cat,id));
				sizeChanged = true;
			}
		}

		public void RemoveTriggerHandler(Trigger trigger)
		{
			lock (syncObj)
			{
				handlers.Remove(trigger);
				sizeChanged = true;
			}
		}

		private bool sizeChanged = false;
		private int size = -9999;
		/// <summary>
		/// Returns the Trigger count on this Page.
		/// </summary>
		/// <returns></returns>
		public int Size
		{
			get
			{
				lock (syncObj)
				{
					if (size == -9999 || sizeChanged)
					{
						size = triggerBlocks.Count;
						for (int i = 0; i <= triggerBlocks.Count - 1; i++)
							size += triggerBlocks[i].Count - 1;
						sizeChanged = false;
					}
					return size;
				}
			}
		}

		// Changed id to Params fo multiple Trigger processing.
		// This Compensates for a Design Flaw Lothus Marque spotted - Gerolkae

		/*
		 * [1/7/2013 9:26:22 PM] Lothus Marque: Okay. Said feeling doesn't explain why 48 is 
		 * happening before 46, since your execute does them in increasing order. But what I 
		 * was suddenly staring at is that this has the definite potential to "run all 46, 
		 * then run 47, then run 48" ... and they're not all at once, in sequence.
		 */
		private bool ExecuteBlock(int[] id, TriggerList triggerBlock)
		{
			TriggerReader reader = new TriggerReader(this);
			bool foundExecutableTrigger = false;
			/*
			 * 0 = Cause mode
			 * 1 = Condition mode
			 * 2 = Effect/Execution mode
			 */
			int mode = 0;
			for (int j = 0; j <= triggerBlock.Count - 1; j++)
			{
				bool pass = false;
				var current = triggerBlock[j];

				// using id.contains checks params against current block to properly fire the triggers

				if (handlers.ContainsKey(current))
				{
					reader.Trigger = current;
					try
					{
						if ((mode == 0) && !pass) //Cause mode
						{
							if (current.Category == TriggerCategory.Cause && id.Contains(current.Id))
							{
								if (BeforeTriggerHandled != null) BeforeTriggerHandled(current);
								bool conditiontest = handlers[current](reader);
								if (!conditiontest)
								{
									if (AfterTriggerHandled != null) AfterTriggerHandled(current);
									//Do nothing, keep scanning.
                                								}
								else
								{
									mode = 1;
									pass = true;
									
									if (AfterTriggerHandled != null) AfterTriggerHandled(current);
									//System.Diagnostics.Debug.WriteLine("Cause found at: " + j);
								}
                                
							}
							else if (current.Category == TriggerCategory.Effect)
							{ //We just ran into an effect while looking for causes.
								//Might as well die here and cut the scan short.
								break;
							}
						}

						if ((mode == 1) && !pass) //Condition mode
						{
							if (current.Category == TriggerCategory.Condition)
							{
								if (BeforeTriggerHandled != null) BeforeTriggerHandled(current);
								bool conditiontest = handlers[current](reader);
								if (!conditiontest)
								{
									//System.Diagnostics.Debug.WriteLine("Failed test, back to Cause mode: " + j);
									if (AfterTriggerHandled != null) AfterTriggerHandled(current);
									conditiontest = false;
                                   	mode = 0; //Back to Cause mode.
								}
								else
								{
									if (AfterTriggerHandled != null) AfterTriggerHandled(current);
								}
							}
							else if  ((current.Category == TriggerCategory.Effect) || (current.Category == TriggerCategory.Cause))
							{
								//If we've stayed in condition mode and hit another cause or an effect, this means we've SUCCEEDED!
								//This means we need to move on to execute mode.
                                if (mode == 1)
                                {
                                    //System.Diagnostics.Debug.WriteLine("Switched to Execute mode at " + j);
                                    mode = 2;
                                }
							}
						}

						if (mode == 2) //Execute mode. Run everything to the end that isn't a condition or cause.
						{
							if (current.Category == TriggerCategory.Effect)
							{
                                
							    //System.Diagnostics.Debug.WriteLine("Attempted to execute " + j);
								if (BeforeTriggerHandled != null) BeforeTriggerHandled(current);
								if (handlers[current](reader) == false)
                                    return foundExecutableTrigger;
								if (AfterTriggerHandled != null) AfterTriggerHandled(current);
							}
						}
					}
					catch (Exception e)
					{
						var ex = new Exception(String.Format("Error in library {0}, at {1} with trigger {2}.",
								handlers[current].Target.GetType().Name,
								handlers[current].Method.Name,
								current.ToString()), e);
						if (Error != null)
							Error(current, ex);
						else throw ex;

						break;
					}
				}
				//End of main loop
			}
			return foundExecutableTrigger;
		}


        [Obsolete("Use Execute(params int[]) instead.", true)]
		public void Execute(TriggerCategory cat, int id)
		{
			Execute(id);
		}

		/// <summary>
		/// Executes a trigger block containing TriggerCategory.Cause with ID equal to <param name="id" />
		/// </summary>
		///
		// Changed id to Params fo multiple Trigger processing.
		// This Compensates for a Design Flaw Lothus Marque spotted - Gerolkae
		public bool Execute(params int[] id)
		{
            bool Executed = false;
			//var trigger = new Trigger(TriggerCategory.Cause, id);
			if (triggerBlocks.Count > 0)
			{

				for (int i = 0; i <= triggerBlocks.Count - 1; i++)
				{
					//if (ExecuteBlock(id, triggerBlocks[i]) == true)
					//break; - Break isn't needed for a Full replace system - Gerolkae
				   // lock (syncObj)
					//{
                    if (ExecuteBlock(id, triggerBlocks[i]))
                        Executed = true;
					//}
				}
			}
            return Executed;
		}

	}
}