///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

using System;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests_Base
	{
		public struct TestStructWithExternalObjectSerializer
		{
			internal bool           BooleanFalse                 { get; set; }
			internal bool           BooleanTrue                  { get; set; }
			internal char           Char                         { get; set; }
			internal sbyte          SByte                        { get; set; }
			internal byte           Byte                         { get; set; }
			internal short          Int16                        { get; set; }
			internal ushort         UInt16                       { get; set; }
			internal int            Int32                        { get; set; }
			internal uint           UInt32                       { get; set; }
			internal long           Int64                        { get; set; }
			internal ulong          UInt64                       { get; set; }
			internal float          Single                       { get; set; }
			internal double         Double                       { get; set; }
			internal decimal        Decimal                      { get; set; }
			internal string         String                       { get; set; }
			internal DateTime       DateTime                     { get; set; }
			internal DateTimeOffset DateTimeOffset               { get; set; }
			internal Guid           Guid                         { get; set; }
			internal Type           NonGenericType               { get; set; }
			internal Type           GenericTypeDefinition        { get; set; }
			internal Type           ClosedConstructedGenericType { get; set; }
			internal object         NullReference                { get; set; }
			internal TestEnum_S8    Enum_S8                      { get; set; }
			internal TestEnum_U8    Enum_U8                      { get; set; }
			internal TestEnum_S16   Enum_S16                     { get; set; }
			internal TestEnum_U16   Enum_U16                     { get; set; }
			internal TestEnum_S32   Enum_S32                     { get; set; }
			internal TestEnum_U32   Enum_U32                     { get; set; }
			internal TestEnum_S64   Enum_S64                     { get; set; }
			internal TestEnum_U64   Enum_U64                     { get; set; }
			internal List<int>      SerializableObject           { get; set; }
			internal byte[]         Buffer1                      { get; set; }
			internal byte[]         Buffer2                      { get; set; }

			public TestStructWithExternalObjectSerializer Init()
			{
				BooleanFalse = false;
				BooleanTrue = true;
				Char = 'X';
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
				String = "A String";
				DateTime = DateTime.Now;
				DateTimeOffset = DateTimeOffset.Now;
				Guid = Guid.NewGuid();
				NonGenericType = typeof(int);
				GenericTypeDefinition = typeof(Dictionary<,>);
				ClosedConstructedGenericType = typeof(Dictionary<int, string>);
				NullReference = null;
				Enum_S8 = TestEnum_S8.B;
				Enum_U8 = TestEnum_U8.B;
				Enum_S16 = TestEnum_S16.B;
				Enum_U16 = TestEnum_U16.B;
				Enum_S32 = TestEnum_S32.B;
				Enum_U32 = TestEnum_U32.B;
				Enum_S64 = TestEnum_S64.B;
				Enum_U64 = TestEnum_U64.B;
				SerializableObject = new List<int> { 1, 2, 3, 4, 5 };
				Buffer1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				Buffer2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				return this;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = BooleanFalse.GetHashCode();
					hashCode = (hashCode * 397) ^ BooleanTrue.GetHashCode();
					hashCode = (hashCode * 397) ^ Char.GetHashCode();
					hashCode = (hashCode * 397) ^ SByte.GetHashCode();
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
					hashCode = (hashCode * 397) ^ String.GetHashCode();
					hashCode = (hashCode * 397) ^ DateTime.GetHashCode();
					hashCode = (hashCode * 397) ^ DateTimeOffset.GetHashCode();
					hashCode = (hashCode * 397) ^ Guid.GetHashCode();
					hashCode = (hashCode * 397) ^ NonGenericType.GetHashCode();
					hashCode = (hashCode * 397) ^ GenericTypeDefinition.GetHashCode();
					hashCode = (hashCode * 397) ^ ClosedConstructedGenericType.GetHashCode();
					hashCode = (hashCode * 397) ^ (NullReference != null ? NullReference.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ Enum_S8.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_U8.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_S16.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_U16.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_S32.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_U32.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_S64.GetHashCode();
					hashCode = (hashCode * 397) ^ Enum_U64.GetHashCode();
					hashCode = (hashCode * 397) ^ SerializableObject.GetHashCode();
					hashCode = (hashCode * 397) ^ ByteArrayEqualityComparer.GetHashCode(Buffer1);
					hashCode = (hashCode * 397) ^ ByteArrayEqualityComparer.GetHashCode(Buffer2);
					return hashCode;
				}
			}

			public bool Equals(TestStructWithExternalObjectSerializer other)
			{
				return BooleanFalse == other.BooleanFalse &&
				       BooleanTrue == other.BooleanTrue &&
				       Char == other.Char &&
				       SByte == other.SByte &&
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
				       String == other.String &&
				       DateTime == other.DateTime &&
				       DateTimeOffset == other.DateTimeOffset &&
				       Guid == other.Guid &&
				       NonGenericType == other.NonGenericType &&
				       GenericTypeDefinition == other.GenericTypeDefinition &&
				       ClosedConstructedGenericType == other.ClosedConstructedGenericType &&
				       NullReference == other.NullReference &&
				       Enum_S8 == other.Enum_S8 &&
				       Enum_U8 == other.Enum_U8 &&
				       Enum_S16 == other.Enum_S16 &&
				       Enum_U16 == other.Enum_U16 &&
				       Enum_S32 == other.Enum_S32 &&
				       Enum_U32 == other.Enum_U32 &&
				       Enum_S64 == other.Enum_S64 &&
				       Enum_U64 == other.Enum_U64 &&
				       SerializableObject.SequenceEqual(other.SerializableObject) &&
				       ByteArrayEqualityComparer.AreEqual(Buffer1, other.Buffer1) &&
				       ByteArrayEqualityComparer.AreEqual(Buffer2, other.Buffer2);
			}

			public override bool Equals(object obj)
			{
				if (obj == null) return false;
				if (obj.GetType() != typeof(TestStructWithExternalObjectSerializer)) return false;
				var other = (TestStructWithExternalObjectSerializer)obj;
				return Equals(other);
			}
		}
	}

}
