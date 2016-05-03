using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// Removed by Gerolkae Looking for threads
//using System.Threading;

namespace Monkeyspeak
{
    [Serializable]
    public class TriggerReaderException : Exception
    {
        public TriggerReaderException() { }

        public TriggerReaderException(string message) : base(message) { }

        public TriggerReaderException(string message, Exception inner) : base(message, inner) { }

        protected TriggerReaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// A Reader that is used to get Variables, Strings, and Numbers from Triggers
    /// </summary>
    [CLSCompliant(true)]
    public sealed class TriggerReader
    {
        private Trigger cloneTrigger, originalTrigger;
        private Page page;

        private readonly object syncObject = new object();
       
        
        /// <summary>
        /// A Reader that is used to get Variables, Strings, and Numbers from Triggers
        /// </summary>
        /// <param name="page"></param>
        /// <param name="trigger"></param>
        public TriggerReader(Page page, Trigger trigger)
        {
            originalTrigger = trigger;
            cloneTrigger = originalTrigger.Clone();
            this.page = page;
        }

        internal TriggerReader(Page page)
        {
            this.page = page;
        }

        public Trigger Trigger
        {
            get { return cloneTrigger; }
            internal set
            {
                originalTrigger = value;
                cloneTrigger = originalTrigger.Clone();
            }
        }

        public TriggerCategory TriggerCategory
        {
            get { return cloneTrigger.Category; }
        }

        public int TriggerId
        {
            get { return cloneTrigger.Id; }
        }

        public Page Page
        {
            get { return page; }
        }

        /// <summary>
        /// Resets the reader's indexes to 0
        /// </summary>
        public void Reset()
        {
            if (originalTrigger != null)
            {
                cloneTrigger.contents = new Queue<object>(originalTrigger.contents);
            }
        }

        /// <summary>
        /// Reads the next String, throws TriggerReaderException on failure
        /// </summary>
        /// <returns></returns>
        public string ReadString(bool processVariables = true)
        {
            if (cloneTrigger.contents.Count == 0) throw new TriggerReaderException("Unexpected end of values.");

            try
            {
                string str = Convert.ToString(cloneTrigger.contents.Dequeue());
                if (processVariables)
                {
                    for (int i = 0; i <= page.Scope.Count - 1; i++)
                    {
                      // replaced string.replace with Regex because
                      //  %ListName would replace %ListName2 leaving the 2 at the end
                      //- Gerolkae
                        string pattern = page.Scope[i].Name + @"\b" ;
                        string replace = (page.Scope[i].Value != null) ? page.Scope[i].Value.ToString() : "null";
                         str = Regex.Replace(str, pattern, replace);
                    }
                }
                return str;
            }
            catch
            {
                throw new TriggerReaderException("No value found.");
            }
        }

        

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekString()
        {
            if (cloneTrigger.contents.Count == 0) return false;
            return cloneTrigger.contents.Peek() is string;
        }

        public bool TryReadString(out string str)
        {
            if (PeekString() == false)
            {
                str = String.Empty;
                return false;
            }

            str = Convert.ToString(cloneTrigger.contents.Dequeue());
            return true;
        }

        /// <summary>
        /// Reads the next Variable available, throws TriggerReaderException on failure
        /// </summary>
        /// <returns>Variable</returns>
        public Variable ReadVariable()
        {
            return ReadVariable(false);
        }

        /// <summary>
        /// Reads the next Variable available, throws TriggerReaderException on failure
        /// </summary>
        /// <param name="addIfNotExist">Add the Variable if it doesn't exist and return that Variable with a Value equal to null.</param>
        /// <returns>Variable</returns>
        public Variable ReadVariable(bool addIfNotExist)
        {
            if (cloneTrigger.contents.Count == 0) throw new TriggerReaderException("Unexpected end of values.");
            
            try
            {
                Variable var;
                string varRef = Convert.ToString(cloneTrigger.contents.Dequeue());
                if (page.HasVariable(varRef, out var) == false)
                   if (addIfNotExist)
                        var = page.SetVariable(varRef, "", false);


                return var;
            }
            catch
            {
                throw new TriggerReaderException("No value found.");
            }
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekVariable()
        {
            if (cloneTrigger.contents.Count == 0) return false;
            string nextContent = cloneTrigger.contents.Peek().ToString();
            return (!String.IsNullOrEmpty(nextContent)) && nextContent[0] == page.Engine.Options.VariableDeclarationSymbol;
        }

        /// <summary>
        /// Trys to read the next Variable available
        /// </summary>
        /// <param name="var">Variable is assigned on success</param>
        /// <returns>bool on success</returns>
        public bool TryReadVariable(out Variable var)
        {
            if (PeekVariable() == false)
            {
                var = Variable.NoValue;
                return false;
            }
            string varRef = Convert.ToString(cloneTrigger.contents.Dequeue());
            var = page.GetVariable(varRef);
            return true;
        }

        /// <summary>
        /// Reads the next Double available, throws TriggerReaderException on failure
        /// </summary>
        /// <returns>Double</returns>
        public double ReadNumber()
        {
            if (cloneTrigger.contents.Count == 0) throw new TriggerReaderException("Unexpected end of values.");
            try
            {
                double number = Convert.ToDouble(cloneTrigger.contents.Dequeue());
                return number;
            }
            catch
            {
                throw new TriggerReaderException("No value found.");
            }
        }

        /// <summary>
        /// Peeks at the next value
        /// </summary>
        /// <returns></returns>
        public bool PeekNumber()
        {
            if (cloneTrigger.contents.Count == 0) return false;
            return cloneTrigger.contents.Peek() is double;
        }

        public bool TryReadNumber(out double number)
        {
            if (PeekNumber() == false)
            {
                number = double.NaN;
                return false;
            }
            number = Convert.ToDouble(cloneTrigger.contents.Dequeue());
            return true;
        }
    }
}