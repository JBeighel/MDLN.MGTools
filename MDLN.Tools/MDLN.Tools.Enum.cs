using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace MDLN.Tools {
	/// <summary>
	/// Generic text attribute.  Used for enums that need a string value in addition to their descriptions
	/// </summary>
	public class StringValue : System.Attribute {
		private readonly string cStringValue;

		/// <summary>
		/// Constructor that sets the value of the string attribute
		/// </summary>
		/// <param name="Value">Value to save in the attribute</param>
		public StringValue(string Value) {
			cStringValue = Value;
		}

		/// <summary>
		/// Property to retrieve the attribute through
		/// </summary>
		public string Value {
			get {
				return cStringValue;
			}
		}
	}

	/// <summary>
	/// Generic UInt32 attribute.  Used for enums that need an unsigned integer value attribute
	/// </summary>
	public class UInt32Value : System.Attribute {
		private readonly UInt32 cUIntValue;

		/// <summary>
		/// Constructor that sets the value of the UInt32 attribute
		/// </summary>
		/// <param name="Value">Value to save in the attribute</param>
		public UInt32Value(UInt32 Value) {
			cUIntValue = Value;
		}

		/// <summary>
		/// Property to retrieve the attribute through
		/// </summary>
		public UInt32 Value {
			get {
				return cUIntValue;
			}
		}
	}

	/// <summary>
	/// Class containing utility functions for working on enum data types
	/// </summary>
	public static class EnumTools {
		/// <summary>
		/// Attempts to convert a string to a value from an enum.
		/// </summary>
		/// <typeparam name="T">Type of enum to convert to</typeparam>
		/// <param name="Value">String value to convert</param>
		/// <param name="IgnoreCase">True to do a case insensitive conversion, false for case sensitive</param>
		/// <returns>The value from the enum</returns>
		public static T ParseEnum<T>(string Value, bool IgnoreCase) {
			return (T)Enum.Parse(typeof(T), Value, IgnoreCase);
		}

		/// <summary>
		/// Gets an enumerable list of all values in an enum
		/// </summary>
		/// <typeparam name="T">Enum type to get values from</typeparam>
		/// <returns>A read only list containing all values in the enum</returns>
		public static IReadOnlyList<T> GetEnumValuesList<T>() {
			return (T[])Enum.GetValues(typeof(T));
		}

		/// <summary>
		/// Retrieve the description attribute of a Enumerated type value.
		/// If no description is set it returns the value as a string
		/// </summary>
		/// <param name="Value">Enumerated type value</param>
		/// <returns>The description attribute of the value</returns>
		public static string GetEnumDescriptionAttribute(Enum Value) {
			FieldInfo Field = Value.GetType().GetField(Value.ToString());

			DescriptionAttribute[] AttribList = (DescriptionAttribute[])Field.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if ((AttribList != null) && (AttribList.Length > 0)) {
				return AttribList[0].Description;
			} else {
				return Value.ToString();
			}
		}

		/// <summary>
		/// Retrieve the string custom attribute of a Enumerated type value.
		/// If no string is set it returns the value as a string
		/// </summary>
		/// <param name="Value">Enumerated type value</param>
		/// <returns>The string custom attribute of the value</returns>
		public static string GetEnumStringAttribute(Enum Value) {
			FieldInfo Field = Value.GetType().GetField(Value.ToString());

			StringValue[] AttribList = (StringValue[])Field.GetCustomAttributes(typeof(StringValue), false);

			if ((AttribList != null) && (AttribList.Length > 0)) {
				return AttribList[0].Value;
			} else {
				return Value.ToString();
			}
		}

		/// <summary>
		/// Retrieve the UInt32 custom attribute of a Enumerated type value.
		/// If no UInt32 is set it returns 0
		/// </summary>
		/// <param name="Value">Enumerated type value</param>
		/// <returns>The UInt32 custom attribute of the value</returns>
		public static UInt32 GetEnumUInt32Attribute(Enum Value) {
			FieldInfo Field = Value.GetType().GetField(Value.ToString());

			UInt32Value[] AttribList = (UInt32Value[])Field.GetCustomAttributes(typeof(UInt32Value), false);

			if ((AttribList != null) && (AttribList.Length > 0)) {
				return AttribList[0].Value;
			} else {
				return 0;
			}
		}
	}
}
