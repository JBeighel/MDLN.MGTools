using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;

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
			Results = TypeTools.UshortToBytes(StartVal);
			Assert.AreEqual(0xFF, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x11, Results[1], TestName + "Verify second byte.");

			TestName = "UshortToBytes() LSBFirst=true: ";
			Results = TypeTools.UshortToBytes(StartVal, true);
			Assert.AreEqual(0xFF, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x11, Results[1], TestName + "Verify second byte.");

			TestName = "UshortToBytes() LSBFirst=false: ";
			Results = TypeTools.UshortToBytes(StartVal, false);
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
			Results = TypeTools.Uint32ToBytes(StartVal);
			Assert.AreEqual(0x44, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x33, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x22, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x11, Results[3], TestName + "Verify fourth byte.");

			TestName = "Uint32ToBytes() LSBFirst=true: ";
			Results = TypeTools.Uint32ToBytes(StartVal, true);
			Assert.AreEqual(0x44, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x33, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x22, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x11, Results[3], TestName + "Verify fourth byte.");

			TestName = "Uint32ToBytes() LSBFirst=false: ";
			Results = TypeTools.Uint32ToBytes(StartVal, false);
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
			Results = TypeTools.IntTo6Bytes(StartVal, true);
			Assert.AreEqual(0x88, Results[0], TestName + "Verify first byte.");
			Assert.AreEqual(0x77, Results[1], TestName + "Verify second byte.");
			Assert.AreEqual(0x66, Results[2], TestName + "Verify third byte.");
			Assert.AreEqual(0x55, Results[3], TestName + "Verify fourth byte.");
			Assert.AreEqual(0x44, Results[4], TestName + "Verify fifth byte.");
			Assert.AreEqual(0x33, Results[5], TestName + "Verify sixth byte.");

			TestName = "Uint32ToBytes() LSBFirst=false: ";
			Results = TypeTools.IntTo6Bytes(StartVal, false);
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
			Result = TypeTools.BytesToUshort(0x11, 0xFF);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			//Default behavior is least significant byte first
			TestName = "BytesToUshort() LSBFirst=default: ";
			Result = TypeTools.BytesToUshort(StartVal);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() LSBFirst=true: ";
			Result = TypeTools.BytesToUshort(StartVal, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() LSBFirst=false: ";
			Result = TypeTools.BytesToUshort(StartVal, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=0 LSBFirst=true: ";
			Result = TypeTools.BytesToUshort(StartVal, 0, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=0 LSBFirst=false: ";
			Result = TypeTools.BytesToUshort(StartVal, 0, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");

			StartVal = new byte[] { 0x01, 0x02, 0xFF, 0x11, 0x03, 0x04 };

			TestName = "BytesToUshort() Offset=2 LSBFirst=true: ";
			Result = TypeTools.BytesToUshort(StartVal, 2, true);
			Assert.AreEqual(0x11FF, Result, TestName + "Verify conversion.");

			TestName = "BytesToUshort() Offset=2 LSBFirst=false: ";
			Result = TypeTools.BytesToUshort(StartVal, 2, false);
			Assert.AreEqual(0xFF11, Result, TestName + "Verify conversion.");
		}

		[Test]
		public void CalculateAngleFromPoints() {
			Vector2 Point1, Point2;
			float AngleRadians;
			string TestName;

			TestName = "Horizontal line, 0 degrees = 0 radians";
			Point1.X = 1;
			Point1.Y = 1;

			Point2.X = 2;
			Point2.Y = 1;

			AngleRadians = MGMath.GetAngleFromPoints(Point1, Point2);
			Assert.AreEqual(0.0f, AngleRadians, TestName);

			TestName = "Horizontal line, 180 degrees = 3.14 radians";
			Point1.X = 1;
			Point1.Y = 1;

			Point2.X = 2;
			Point2.Y = 1;

			AngleRadians = MGMath.GetAngleFromPoints(Point2, Point1);
			Assert.AreEqual((float)Math.PI, AngleRadians, TestName);

			TestName = "Diagonal line, 45 degrees = 0.785 radians";
			Point1.X = 1;
			Point1.Y = 1;

			Point2.X = 2;
			Point2.Y = 2;

			AngleRadians = MGMath.GetAngleFromPoints(Point1, Point2);
			Assert.AreEqual((float)(Math.PI / 4), AngleRadians, TestName);

			TestName = "Diagonal line, 135 degrees = 2.356 radians";
			Point1.X = -2;
			Point1.Y = -2;

			Point2.X = -3;
			Point2.Y = -1;

			AngleRadians = MGMath.GetAngleFromPoints(Point1, Point2);
			Assert.AreEqual((float)(Math.PI * 3 / 4), AngleRadians, TestName);

			TestName = "Diagonal line, 225 degrees = 3.93 radians";
			Point1.X = 1;
			Point1.Y = 1;

			Point2.X = 2;
			Point2.Y = 2;

			AngleRadians = MGMath.GetAngleFromPoints(Point2, Point1);
			Assert.AreEqual((float)(Math.PI + (Math.PI / 4)), AngleRadians, TestName);

			TestName = "Diagonal line, 225 degrees = 3.93 radians";
			Point1.X = 0;
			Point1.Y = 0;

			Point2.X = -1;
			Point2.Y = -1;

			AngleRadians = MGMath.GetAngleFromPoints(Point1, Point2);
			Assert.AreEqual((float)(Math.PI + (Math.PI / 4)), AngleRadians, TestName);

			TestName = "Diagonal line, 315 degrees = 5.498 radians";
			Point1.X = 0;
			Point1.Y = 0;

			Point2.X = 1;
			Point2.Y = -1;

			AngleRadians = MGMath.GetAngleFromPoints(Point1, Point2);
			Assert.AreEqual((float)(Math.PI + (Math.PI * 3 / 4)), AngleRadians, TestName);
		}
	}
}

