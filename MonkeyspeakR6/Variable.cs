using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkeyspeak
{
	[Serializable]
	public class VariableIsConstantException : Exception
	{
		public VariableIsConstantException() { }

		public VariableIsConstantException(string message) : base(message) { }

		public VariableIsConstantException(string message, Exception inner) : base(message, inner) { }

		protected VariableIsConstantException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
    [CLSCompliant(true)]
	public class Variable
	{
		public static readonly Variable NoValue = new Variable("%none", "", false);

		private bool isConstant;

		public bool IsConstant
		{
			get
			{
				return isConstant;
			}
			set
			{
				isConstant = value;
			}
		}

        private object value;

		public object Value
		{
			get
			{
				return value;
			}
			  set
			{
                 // removed Value = as it interfered with page.setVariable - Gerolkae
                if (CheckType(value) == false) throw new TypeNotSupportedException(value.GetType().Name +
    " is not a supported type. Expecting string or double.");

                if (IsConstant == false)
                    this.value = value;
                else throw new VariableIsConstantException("Attempt to assign a _value to constant \"" + Name + "\"");
			}
		}

		private string name;

		public string Name
		{
			get
			{
				return name;
			}
			internal set
			{
				name = value;
			}
		}

        // Variable var = new variable(); 
        // Preset reader.readvariable with default data
        // Needed for Conditions  checking Variables that haven't been defined yet.
        // -Gerolkae
        private Variable()
        {
            isConstant = false;
            name = "%none";
            value = null;
        }

		internal Variable(string Name, object value, bool constant = false)
		{
			isConstant = constant;
			name = Name;
			this.value = value;
		}

		public void ForceAssignValue(object _value)
		{
			if (CheckType(_value) == false) throw new TypeNotSupportedException(_value.GetType().Name +
" is not a supported type. Expecting string or double.");
			value = _value;
		}

		private bool CheckType(object _value)
		{
			if (_value == null) return true;

			return _value is string ||
			       _value is double;
		}

		/// <summary>
		/// Returns a const identifier if the variable is constant followed by name,
		/// <para>otherwise just the name is returned.</para>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ((IsConstant) ? "const " : "") + Name + " = " + ((Value == null) ? "null" : Value.ToString());
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="asConstant">Clone as Constant</param>
		/// <returns></returns>
		public Variable Clone(bool asConstant = false)
		{
			return new Variable(Name, Value, asConstant);
		}

		public static bool operator ==(Variable varA, Variable varB)
		{
			return varA.Value == varB.Value;
		}

		public static bool operator !=(Variable varA, Variable varB)
		{
            return varA.Value != varB.Value;
		}

		public override bool Equals(object obj)
		{
            return ((Variable)obj).Name.Equals(Name) && ((Variable)obj).Value.Equals(Value);
		}
	}
}