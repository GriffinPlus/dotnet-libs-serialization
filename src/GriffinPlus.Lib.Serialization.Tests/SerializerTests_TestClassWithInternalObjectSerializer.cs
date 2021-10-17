///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[InternalObjectSerializer(1)]
		public class TestClassWithInternalObjectSerializer : IInternalObjectSerializer
		{
			public sbyte   SByte   { get; set; }
			public byte    Byte    { get; set; }
			public short   Int16   { get; set; }
			public ushort  UInt16  { get; set; }
			public int     Int32   { get; set; }
			public uint    UInt32  { get; set; }
			public long    Int64   { get; set; }
			public ulong   UInt64  { get; set; }
			public float   Single  { get; set; }
			public double  Double  { get; set; }
			public decimal Decimal { get; set; }
			public char    Char    { get; set; }

			public TestClassWithInternalObjectSerializer()
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

			public TestClassWithInternalObjectSerializer(SerializerArchive archive)
			{
				if (archive.Version == 1)
				{
					SByte = archive.ReadSByte();
					Byte = archive.ReadByte();
					Int16 = archive.ReadInt16();
					UInt16 = archive.ReadUInt16();
					Int32 = archive.ReadInt32();
					UInt32 = archive.ReadUInt32();
					Int64 = archive.ReadInt64();
					UInt64 = archive.ReadUInt64();
					Single = archive.ReadSingle();
					Double = archive.ReadDouble();
					Decimal = archive.ReadDecimal();
					Char = archive.ReadChar();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}
			}

			public void Serialize(SerializerArchive archive, uint version)
			{
				if (version == 1)
				{
					archive.Write(SByte);
					archive.Write(Byte);
					archive.Write(Int16);
					archive.Write(UInt16);
					archive.Write(Int32);
					archive.Write(UInt32);
					archive.Write(Int64);
					archive.Write(UInt64);
					archive.Write(Single);
					archive.Write(Double);
					archive.Write(Decimal);
					archive.Write(Char);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClassWithInternalObjectSerializer), version);
				}
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

			protected bool Equals(TestClassWithInternalObjectSerializer other)
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
				       Double.Equals(other.Double) && Decimal == other.Decimal && Char == other.Char;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is TestClassWithInternalObjectSerializer other)) return false;
				return Equals(other);
			}
		}
	}

}
