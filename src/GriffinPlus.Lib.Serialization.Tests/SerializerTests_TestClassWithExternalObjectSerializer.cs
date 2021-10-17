///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		public class TestClassWithExternalObjectSerializer
		{
			public sbyte   SByte;
			public byte    Byte;
			public short   Int16;
			public ushort  UInt16;
			public int     Int32;
			public uint    UInt32;
			public long    Int64;
			public ulong   UInt64;
			public float   Single;
			public double  Double;
			public decimal Decimal;
			public char    Char;

			public TestClassWithExternalObjectSerializer()
			{
				SByte = 1;
				Byte = 2;
				Int16 = 3;
				UInt16 = 4;
				Int32 = 5;
				UInt32 = 6;
				Int64 = 7;
				UInt64 = 8;
				Single = 9.0f;
				Double = 10.0;
				Decimal = 11;
				Char = 'X';
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = SByte.GetHashCode();
					hashCode = (hashCode * 397) ^ Byte.GetHashCode();
					hashCode = (hashCode * 397) ^ Int16.GetHashCode();
					hashCode = (hashCode * 397) ^ UInt16.GetHashCode();
					hashCode = (hashCode * 397) ^ Int32;
					hashCode = (hashCode * 397) ^ (int)UInt32;
					hashCode = (hashCode * 397) ^ Int64.GetHashCode();
					hashCode = (hashCode * 397) ^ UInt64.GetHashCode();
					hashCode = (hashCode * 397) ^ Single.GetHashCode();
					hashCode = (hashCode * 397) ^ Double.GetHashCode();
					hashCode = (hashCode * 397) ^ Decimal.GetHashCode();
					hashCode = (hashCode * 397) ^ Char.GetHashCode();
					return hashCode;
				}
			}

			protected bool Equals(TestClassWithExternalObjectSerializer other)
			{
				return SByte == other.SByte &&
				       Byte == other.Byte &&
				       Int16 == other.Int16 &&
				       UInt16 == other.UInt16 &&
				       Int32 == other.Int32 &&
				       UInt32 == other.UInt32 &&
				       Int64 == other.Int64 &&
				       UInt64 == other.UInt64 &&
				       Single.Equals(other.Single) &&
				       Double.Equals(other.Double) &&
				       Decimal == other.Decimal &&
				       Char == other.Char;
			}

			public override bool Equals(object obj)
			{
				var other = obj as TestClassWithExternalObjectSerializer;
				if (other == null) return false;
				return Equals(other);
			}
		}
	}

}
