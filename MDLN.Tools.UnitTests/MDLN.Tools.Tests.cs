using MDLN.Tools;

using NUnit.Framework;

using System;

namespace MDLN.Tools.UnitTests {
	[TestFixture]
	public class Tests {
		[SetUp]
		public void Init() {
		}

		/// <summary>
		/// Test to verify all conversions from UInt16 to byte arrays work
		/// </summary>
		[Test]
		public void UInt16ToBytes() {
			ushort StartVal = 0x11FF;
			byte[] Results;
			string TestName;

			//Default behavior is least significant byte first
			TestName = "UshortToBytes() LSBFirst=default: ";
			Results = Tools.UshortToBytes(StartVal);
			Assert.AreEqual(0xFF, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x11, Results[1], TestName + "Verify second byte.");

			TestName = "UshortToBytes() LSBFirst=true: ";
			Results = Tools.UshortToBytes(StartVal, true);
			Assert.AreEqual(0xFF, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x11, Results[1], TestName + "Verify second byte.");

			TestName = "UshortToBytes() LSBFirst=false: ";
			Results = Tools.UshortToBytes(StartVal, false);
			Assert.AreEqual(0x11, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0xFF, Results[1], TestName + "Verify second byte.");
		}

		/// <summary>
		/// Test to verify all conversions from UInt32 to byte arrays work
		/// </summary>
		[Test]
		public void UInt32ToBytes() {
			uint StartVal = 0x11223344;
			byte[] Results;
			string TestName;

			//Default behavior is least significant byte first
			TestName = "Uint32ToBytes() LSBFirst=default: ";
			Results = Tools.Uint32ToBytes(StartVal);
			Assert.AreEqual(0x44, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x33, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x22, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x11, Results[3], TestName + "Verify fourth byte.");

			TestName = "Uint32ToBytes() LSBFirst=true: ";
			Results = Tools.Uint32ToBytes(StartVal, true);
			Assert.AreEqual(0x44, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x33, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x22, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x11, Results[3], TestName + "Verify fourth byte.");

			TestName = "Uint32ToBytes() LSBFirst=false: ";
			Results = Tools.Uint32ToBytes(StartVal, false);
			Assert.AreEqual(0x11, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x22, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x33, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x44, Results[3], TestName + "Verify fourth byte.");
		}

		/// <summary>
		/// Test to verify all conversions from UInt64 to byte arrays work
		/// </summary>
		[Test]
		public void UInt64To6Bytes() {
			ulong StartVal = 0x1122334455667788;
			byte[] Results;
			string TestName;

			TestName = "Uint32ToBytes() LSBFirst=true: ";
			Results = Tools.IntTo6Bytes(StartVal, true);
			Assert.AreEqual(0x88, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x77, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x66, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x55, Results[3], TestName + "Verify fourth byte.");
			Assert.AreEqual(0x44, Results[4], TestName + "Verify fifth byte.");
			Assert.AreEqual(0x33, Results[5], TestName + "Verify sixth byte.");

			TestName = "Uint32ToBytes() LSBFirst=false: ";
			Results = Tools.IntTo6Bytes(StartVal, false);
			Assert.AreEqual(0x33, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x44, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x55, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x66, Results[3], TestName + "Verify fourth byte.");
			Assert.AreEqual(0x77, Results[4], TestName + "Verify fifth byte.");
			Assert.AreEqual(0x88, Results[5], TestName + "Verify sixth byte.");
		}

		/// <summary>
		/// Test conversions from byte arrays to UInt16 values
		/// </summary>
		[Test]
		public void BytesToUInt16() {
			byte[] StartVal = new byte[] {0xFF, 0x11};
			ushort Result;
			string TestName;

			TestName = "BytesToUshort() Specify bytes: ";
			Result = Tools.BytesToUshort(0x11, 0xFF);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			//Default behavior is least significant byte first
			TestName = "BytesToUshort() LSBFirst=default: ";
			Result = Tools.BytesToUshort(StartVal);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() LSBFirst=true: ";
			Result = Tools.BytesToUshort(StartVal, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() LSBFirst=false: ";
			Result = Tools.BytesToUshort(StartVal, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=0 LSBFirst=true: ";
			Result = Tools.BytesToUshort(StartVal, 0, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=0 LSBFirst=false: ";
			Result = Tools.BytesToUshort(StartVal, 0, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");

			StartVal = new byte[] { 0x01, 0x02, 0xFF, 0x11, 0x03, 0x04 };

			TestName = "BytesToUshort() Offset=2 LSBFirst=true: ";
			Result = Tools.BytesToUshort(StartVal, 2, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=2 LSBFirst=false: ";
			Result = Tools.BytesToUshort(StartVal, 2, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");
		}
	}
}

