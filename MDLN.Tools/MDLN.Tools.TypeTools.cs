using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace MDLN.Tools {
	/// <summary>
	/// Collection of tools to mainupate data types, performing conversions from one format to another
	/// </summary>
	public static class TypeTools {
		/// <summary>
		/// Converts an array of byte values into a hexadecimal string.
		/// </summary>
		/// <param name="ByteList">List of bytes to convert into hexadecimal text</param>
		/// <returns>The bytes in a hexadecimal text string</returns>
		public static string ByteArrayToString(byte[] ByteList) {
			StringBuilder HexStr = new StringBuilder(ByteList.Length * 2);

			foreach (byte Value in ByteList) {
				HexStr.AppendFormat("{0:x2}h ", Value);
			}

			return HexStr.ToString();
		}

		/// <summary>
		/// Converts a ushort value to a byte array.  The byte array will have the lower bits in the 0 index and the higher bits in the 1 index.
		/// </summary>
		/// <param name="Value">The value to convert</param>
		/// <returns>Returns the array containing the byte version of the value</returns>
		public static byte[] UshortToBytes(ushort Value) {
			return UshortToBytes(Value, true);
		}

		/// <summary>
		/// Converts a ushort value to a byte array.
		/// </summary>
		/// <param name="Value">The value to convert</param>
		/// <param name="LSBFirst">Set true if the lower bits should be in the ower indexes of the array, false to put hte lower bits at the higher indexes</param>
		/// <returns>Returns the array containing the byte version of the value</returns>
		public static byte[] UshortToBytes(ushort Value, bool LSBFirst) {
			byte[] Bytes = new byte[2];

			if (LSBFirst == true) {
				Bytes[0] = (byte)(0x00FF & Value);
				Bytes[1] = (byte)(Value >> 8);
			} else {
				Bytes[0] = (byte)(Value >> 8);
				Bytes[1] = (byte)(0x00FF & Value);
			}

			return Bytes;
		}

		/// <summary>
		/// Convert an integer value ot a byte attay.  Assumes the lower bits will be in the lower indexes of the array
		/// </summary>
		/// <param name="Value">The value to convert</param>
		/// <returns>Returns the array containing the byte version of the value</returns>
		public static byte[] Uint32ToBytes(UInt32 Value) {
			return Uint32ToBytes(Value, true);
		}

		/// <summary>
		/// Convert an 32 bit integer value ot a 4 byte attay
		/// </summary>
		/// <param name="Value">The value to convert</param>
		/// <param name="LSBFirst">Set true if the lower bits should be in the ower indexes of the array, false to put hte lower bits at the higher indexes</param>
		/// <returns>Returns the array containing the byte version of the value</returns>
		public static byte[] Uint32ToBytes(UInt32 Value, bool LSBFirst) {
			byte[] Bytes = new byte[4];
			int Ctr;

			if (LSBFirst == true) {
				for (Ctr = 0; Ctr < 4; Ctr++) {
					Bytes[Ctr] = (byte)(0x000000FF & Value);
					Value = Value >> 8;
				}
			} else {
				for (Ctr = 3; Ctr >= 0; Ctr--) {
					Bytes[Ctr] = (byte)(0x000000FF & Value);
					Value = Value >> 8;
				}
			}

			return Bytes;
		}

		/// <summary>
		/// Convert a 64 bit integer value ot a 6 byte attay (64 bit integers have 8 bytes of information, the two highest order bits are ignored)
		/// </summary>
		/// <param name="Value">The value to convert</param>
		/// <param name="LSBFirst">Set true if the lower bits should be in the ower indexes of the array, false to put hte lower bits at the higher indexes</param>
		/// <returns>Returns the array containing the byte version of the value</returns>
		public static byte[] IntTo6Bytes(UInt64 Value, bool LSBFirst) {
			int Ctr;
			byte[] Bytes = new byte[6];

			if (LSBFirst == true) {
				for (Ctr = 0; Ctr < 6; Ctr++) {
					Bytes[Ctr] = (byte)(0x000000FF & Value);
					Value = Value >> 8;
				}
			} else {
				for (Ctr = 5; Ctr >= 0; Ctr--) {
					Bytes[Ctr] = (byte)(0x000000FF & Value);
					Value = Value >> 8;
				}
			}

			return Bytes;
		}

		/// <summary>
		/// Convert an array of byte values into a 6 byte integer.  The return is a 8 but integer, so the highest two bytes will be left 0
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <param name="Offset">If the 6 bytes for the integer aren't at the beginning of the array specify the starting point</param>
		/// <param name="LSBFirst">True if the lowest order bytes are first in the array, false if the highest order bits are first</param>
		/// <returns>The value as an integer</returns>
		public static UInt64 BytesTo6ByteInt(byte[] Bytes, int Offset, bool LSBFirst) {
			byte[] IntBytes = new byte[6];
			UInt64 Result;
			int Ctr;

			//Get the data that's for this integer
			for (Ctr = Offset; (Ctr < Bytes.Length) && (Ctr < Offset + 6); Ctr++) {
				IntBytes[Ctr - Offset] = Bytes[Ctr];
			}

			//Put hte data into integer format
			Result = 0;
			if (LSBFirst == true) {
				for (Ctr = 5; Ctr >= 0; Ctr--) {
					Result = Result << 8;
					Result = Result + IntBytes[Ctr];
				}
			} else {
				for (Ctr = 0; Ctr < 6; Ctr++) {
					Result = Result << 8;
					Result = Result + IntBytes[Ctr];
				}
			}

			return Result;
		}

		/// <summary>
		/// Convert bytevalues into a Ushort value
		/// </summary>
		/// <param name="MSB">Highets order bits</param>
		/// <param name="LSB">Lowest order bits</param>
		/// <returns></returns>
		public static ushort BytesToUshort(byte MSB, byte LSB) {
			return (ushort)((MSB << 8) | LSB);
		}

		/// <summary>
		/// Convert an array of byte values into a 2 byte integer
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <param name="LSBFirst">True if the lowest order bytes are first in the array, false if the highest order bits are first</param>
		/// <returns>The value as an integer</returns>
		public static ushort BytesToUshort(byte[] Bytes, bool LSBFirst) {
			byte Byte1;

			if (Bytes.Length == 1) { //Correct for arrays that are single bytes
				Byte1 = 0;
			} else {
				Byte1 = Bytes[1];
			}

			if (LSBFirst == true) {
				return BytesToUshort(Byte1, Bytes[0]);
			} else {
				return BytesToUshort(Bytes[0], Byte1);
			}
		}

		/// <summary>
		/// Convert an array of byte values into a 2 byte integer
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <param name="Offset">If the bytes for the integer aren't at the beginning of the array specify the starting point</param>
		/// <param name="LSBFirst">True if the lowest order bytes are first in the array, false if the highest order bits are first</param>
		/// <returns>The value as an integer</returns>
		public static ushort BytesToUshort(byte[] Bytes, int Offset, bool LSBFirst) {
			byte Byte0, Byte1;

			switch (Bytes.Length - Offset) { //Adjust for arrays that are too short
				case 0:
					Byte0 = 0;
					Byte1 = 0;
					break;
				case 1:
					Byte0 = Bytes[Offset + 0];
					Byte1 = 0;
					break;
				default:
					Byte0 = Bytes[Offset + 0];
					Byte1 = Bytes[Offset + 1];
					break;
			}

			if (LSBFirst == true) {
				return BytesToUshort(Byte1, Byte0);
			} else {
				return BytesToUshort(Byte0, Byte1);
			}
		}

		/// <summary>
		/// Convert an array of byte values into a 2 byte integer.  Assumes the array has the lowest order bytes first
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <returns>The value as an integer</returns>
		public static ushort BytesToUshort(byte[] Bytes) {
			return BytesToUshort(Bytes, true);
		}

		/// <summary>
		/// Convert four byte values into a 32 bit integer
		/// </summary>
		/// <param name="MSB">Highest order bits</param>
		/// <param name="Byte2">Second highest bits</param>
		/// <param name="Byte3">Third highest bits</param>
		/// <param name="LSB">Lowest order bits</param>
		/// <returns>The value in integet format</returns>
		public static UInt32 BytesToUint32(byte MSB, byte Byte2, byte Byte3, byte LSB) {
			UInt32 Value = (UInt32)((MSB << 8) | Byte2);
			Value = Value << 16;
			Value = (UInt32)(Value + ((Byte3 << 8) | LSB));

			return Value;
		}

		/// <summary>
		/// Convert an array of byte values into a 4 byte integer
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <param name="Offset">If the bytes for the integer aren't at the beginning of the array specify the starting point</param>
		/// <param name="LSBFirst">True if the lowest order bytes are first in the array, false if the highest order bits are first</param>
		/// <returns>The value as an integer</returns>
		public static UInt32 BytesToUint32(byte[] Bytes, int Offset, bool LSBFirst) {
			byte Byte0, Byte1, Byte2, Byte3;

			switch (Bytes.Length - Offset) { //Adjust for arrays that are too short
				case 0:
					Byte0 = 0;
					Byte1 = 0;
					Byte2 = 0;
					Byte3 = 0;
					break;
				case 1:
					Byte0 = Bytes[Offset + 0];
					Byte1 = 0;
					Byte2 = 0;
					Byte3 = 0;
					break;
				case 2:
					Byte0 = Bytes[Offset + 0];
					Byte1 = Bytes[Offset + 1];
					Byte2 = 0;
					Byte3 = 0;
					break;
				case 3:
					Byte0 = Bytes[Offset + 0];
					Byte1 = Bytes[Offset + 1];
					Byte2 = Bytes[Offset + 2];
					Byte3 = 0;
					break;
				default :
					Byte0 = Bytes[Offset + 0];
					Byte1 = Bytes[Offset + 1];
					Byte2 = Bytes[Offset + 2];
					Byte3 = Bytes[Offset + 3];
					break;
			}

			if (LSBFirst == true) {
				return BytesToUint32(Byte3, Byte2, Byte1, Byte0);
			} else {
				return BytesToUint32(Byte0, Byte1, Byte2, Byte3);
			}
		}

		/// <summary>
		/// Convert an array of byte values into a 4 byte integer
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <param name="LSBFirst">True if the lowest order bytes are first in the array, false if the highest order bits are first</param>
		/// <returns>The value as an integer</returns>
		public static UInt32 BytesToUint32(byte[] Bytes, bool LSBFirst) {
			return BytesToUint32(Bytes, 0, LSBFirst);
		}

		/// <summary>
		/// Convert an array of byte values into a 4 byte integer.  Assumes the lowest order bytes are first in the array
		/// </summary>
		/// <param name="Bytes">Array of byte values</param>
		/// <returns>The value as an integer</returns>
		public static UInt32 BytesToUint32(byte[] Bytes) {
			return BytesToUint32(Bytes, 0, true);
		}

		/// <summary>
		/// Convert a string of 2 character hexadecimal values into an array of byte values.  This will remove all spaces and 'h' characters from
		/// the string before pulling out hext values, sot hte format can be any of the following:
		/// 0102A1A2
		/// 01 02 A1 A2
		/// 01h 02h A1h A2h
		/// </summary>
		/// <param name="HexString"></param>
		/// <returns></returns>
		public static byte[] HexStringToBytes(string HexString) {
			List<byte> ByteList = new List<byte>();
			byte ByteValue;
			string HexValue;

			//remove all extraneous characters
			HexString = HexString.Replace(" ", "");
			HexString = HexString.Replace("h", "");

			while (HexString.CompareTo("") != 0) {
				HexValue = HexString.Substring(0, 2);
				HexString = HexString.Substring(2);

				ByteValue = (byte)Convert.ToInt32(HexValue, 16);
				ByteList.Add(ByteValue);
			}

			return ByteList.ToArray();
		}

		/// <summary>
		/// Convert a string of binary digits into an integer value.  This will remove all spaces and 'b' characters from
		/// the string before pulling out hext values, sot hte format can be any of the following:
		/// 0011000
		/// 0111 0111
		/// 01b 10b 11b
		/// </summary>
		/// <param name="BinaryString">The string to convert from text bits to an integer value.  Can not exceed 32 bits</param>
		/// <returns>The integer value represented by the string's bits</returns>
		public static UInt32 BinaryStringToUInt32(string BinaryString) {
			UInt32 Value = 0;
			string CurrentBit;

			//remove all extraneous characters
			BinaryString = BinaryString.Replace(" ", "");
			BinaryString = BinaryString.Replace("h", "");

			while (BinaryString.CompareTo("") != 0) {
				CurrentBit = BinaryString.Substring(0, 1); //Get the most significant bit
				BinaryString = BinaryString.Substring(1); //Remove the most significant bit from the string

				if (String.Compare(CurrentBit, "1") == 0) {
					Value <<= 1;
					Value += 1;
				} else if (String.Compare(CurrentBit, "0") == 0) {
					Value <<= 1;
				} else {
					throw new Exception("Encountered invalid binary character: " + CurrentBit);
				}
			}

			return Value;
		}

		/// <summary>
		/// Retrieves the version number of the assembly that began execution.
		/// </summary>
		/// <param name="MajorMinorOnly">True to gets the major and minor numbers only, false to includ revision and build as well</param>
		/// <returns>The version number as a string</returns>
		public static string GetExecutableAssemblyVersion(bool MajorMinorOnly) {
			string VersionText;
			Version AssemblyVer = Assembly.GetEntryAssembly().GetName().Version;

			VersionText = AssemblyVer.Major + "." + AssemblyVer.Minor;

			if (MajorMinorOnly == false) {
				VersionText = String.Format("{0}.{1}.{2}", VersionText, AssemblyVer.Build, AssemblyVer.Revision);
			}

			return VersionText;
		}

		/// <summary>
		/// Converts a DateTime value into astandardized date and time string.
		/// YYYY/MM/DD HH:MM:SS.mmm
		/// </summary>
		/// <param name="Time">Date and time value to convert</param>
		/// <returns>Date and time in a standard string format</returns>
		public static string DateTimeToStandardString(DateTime Time) {
			string TimeStamp;

			TimeStamp = Time.Year + "/";

			if (Time.Month < 10) {
				TimeStamp += "0" + Time.Month + "/";
			} else {
				TimeStamp += Time.Month + "/";
			}

			if (Time.Day < 10) {
				TimeStamp += "0" + Time.Day + " ";
			} else {
				TimeStamp += Time.Day + " ";
			}

			if (Time.Hour < 10) {
				TimeStamp += "0" + Time.Hour + ":";
			} else {
				TimeStamp += Time.Hour + ":";
			}

			if (Time.Minute < 10) {
				TimeStamp += "0" + Time.Minute + ":";
			} else {
				TimeStamp += Time.Minute + ":";
			}

			if (Time.Second < 10) {
				TimeStamp += "0" + Time.Second + ".";
			} else {
				TimeStamp += Time.Second + ".";
			}

			if (Time.Millisecond < 10) {
				TimeStamp += "00" + Time.Millisecond;
			} else if (Time.Millisecond < 100) {
				TimeStamp += "0" + Time.Millisecond;
			} else {
				TimeStamp += Time.Millisecond;
			}

			return TimeStamp;
		}
	}
}

