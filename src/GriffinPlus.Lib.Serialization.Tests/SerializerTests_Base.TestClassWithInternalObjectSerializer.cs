﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests;

public partial class SerializerTests_Base
{
	[InternalObjectSerializer(1)]
	public class TestClassWithInternalObjectSerializer : IInternalObjectSerializer
	{
		internal bool           BooleanFalse   { get; set; }
		internal bool           BooleanTrue    { get; set; }
		internal char           Char           { get; set; }
		internal sbyte          SByte          { get; set; }
		internal byte           Byte           { get; set; }
		internal short          Int16          { get; set; }
		internal ushort         UInt16         { get; set; }
		internal int            Int32          { get; set; }
		internal uint           UInt32         { get; set; }
		internal long           Int64          { get; set; }
		internal ulong          UInt64         { get; set; }
		internal float          Single         { get; set; }
		internal double         Double         { get; set; }
		internal decimal        Decimal        { get; set; }
		internal string         String         { get; set; }
		internal DateTime       DateTime       { get; set; }
		internal DateTimeOffset DateTimeOffset { get; set; }
#if NET6_0_OR_GREATER
		internal DateOnly DateOnly { get; set; }
		internal TimeOnly TimeOnly { get; set; }
#endif
		internal Guid         Guid                         { get; set; }
		internal Type         NonGenericType               { get; set; }
		internal Type         GenericTypeDefinition        { get; set; }
		internal Type         ClosedConstructedGenericType { get; set; }
		internal object       NullReference                { get; set; }
		internal TestEnum_S8  Enum_S8                      { get; set; }
		internal TestEnum_U8  Enum_U8                      { get; set; }
		internal TestEnum_S16 Enum_S16                     { get; set; }
		internal TestEnum_U16 Enum_U16                     { get; set; }
		internal TestEnum_S32 Enum_S32                     { get; set; }
		internal TestEnum_U32 Enum_U32                     { get; set; }
		internal TestEnum_S64 Enum_S64                     { get; set; }
		internal TestEnum_U64 Enum_U64                     { get; set; }
		internal List<int>    SerializableObject           { get; set; }
		internal byte[]       Buffer1                      { get; set; }
		internal byte[]       Buffer2                      { get; set; }

		public TestClassWithInternalObjectSerializer()
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
#if NET6_0_OR_GREATER
			DateOnly = DateOnly.FromDateTime(DateTime);
			TimeOnly = TimeOnly.FromDateTime(DateTime);
#endif
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
			SerializableObject = [1, 2, 3, 4, 5];
			Buffer1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
			Buffer2 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
		}

		public unsafe TestClassWithInternalObjectSerializer(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				BooleanFalse = archive.ReadBoolean();
				BooleanTrue = archive.ReadBoolean();
				Char = archive.ReadChar();
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
				String = archive.ReadString();
				DateTime = archive.ReadDateTime();
				DateTimeOffset = archive.ReadDateTimeOffset();
#if NET6_0_OR_GREATER
				DateOnly = archive.ReadDateOnly();
				TimeOnly = archive.ReadTimeOnly();
#endif
				Guid = archive.ReadGuid();
				NonGenericType = archive.ReadType();
				GenericTypeDefinition = archive.ReadType();
				ClosedConstructedGenericType = archive.ReadType();
				NullReference = archive.ReadObject();
				Enum_S8 = archive.ReadEnum<TestEnum_S8>();
				Enum_U8 = archive.ReadEnum<TestEnum_U8>();
				Enum_S16 = archive.ReadEnum<TestEnum_S16>();
				Enum_U16 = archive.ReadEnum<TestEnum_U16>();
				Enum_S32 = archive.ReadEnum<TestEnum_S32>();
				Enum_U32 = archive.ReadEnum<TestEnum_U32>();
				Enum_S64 = archive.ReadEnum<TestEnum_S64>();
				Enum_U64 = archive.ReadEnum<TestEnum_U64>();
				SerializableObject = (List<int>)archive.ReadObject();

				// deserialize buffer via pointer
				int buffer1Size = archive.ReadInt32();
				Buffer1 = new byte[buffer1Size];
				fixed (byte* pBuffer = &Buffer1[0]) archive.ReadBuffer(pBuffer, buffer1Size);

				// deserialize buffer via Stream
				int buffer2Size = archive.ReadInt32();
				Stream stream = archive.ReadStream();
				Buffer2 = new byte[buffer2Size];
				int readByteCount = stream.Read(Buffer2, 0, Buffer2.Length);
				stream.Dispose();
				Debug.Assert(readByteCount == Buffer2.Length);
				Debug.Assert(stream.Length == Buffer2.Length);
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}

		public unsafe void Serialize(SerializationArchive archive)
		{
			// ReSharper disable once InvertIf
			if (archive.Version == 1)
			{
				archive.Write(BooleanFalse);
				archive.Write(BooleanTrue);
				archive.Write(Char);
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
				archive.Write(String);
				archive.Write(DateTime);
				archive.Write(DateTimeOffset);
#if NET6_0_OR_GREATER
				archive.Write(DateOnly);
				archive.Write(TimeOnly);
#endif
				archive.Write(Guid);
				archive.Write(NonGenericType);
				archive.Write(GenericTypeDefinition);
				archive.Write(ClosedConstructedGenericType);
				archive.Write(NullReference);
				archive.Write(Enum_S8);
				archive.Write(Enum_U8);
				archive.Write(Enum_S16);
				archive.Write(Enum_U16);
				archive.Write(Enum_S32);
				archive.Write(Enum_U32);
				archive.Write(Enum_S64);
				archive.Write(Enum_U64);
				archive.Write(SerializableObject);

				// serialize buffer via pointer
				archive.Write(Buffer1.Length);
				fixed (byte* pBuffer = &Buffer1[0]) archive.Write(pBuffer, Buffer1.Length);

				// serializer buffer via stream
				archive.Write(Buffer2.Length);
				archive.Write(new MemoryStream(Buffer2));
				return;
			}

			throw new VersionNotSupportedException(archive);
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
#if NET6_0_OR_GREATER
				hashCode = (hashCode * 397) ^ DateOnly.GetHashCode();
				hashCode = (hashCode * 397) ^ TimeOnly.GetHashCode();
#endif
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
				return hashCode;
			}
		}

		protected bool Equals(TestClassWithInternalObjectSerializer other)
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
#if NET6_0_OR_GREATER
			       DateOnly == other.DateOnly &&
			       TimeOnly == other.TimeOnly &&
#endif
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
			       ByteArrayEqualityComparer.AreEqual(Buffer1, other.Buffer1);
		}

		public override bool Equals(object obj)
		{
			return obj is TestClassWithInternalObjectSerializer other && Equals(other);
		}
	}
}
