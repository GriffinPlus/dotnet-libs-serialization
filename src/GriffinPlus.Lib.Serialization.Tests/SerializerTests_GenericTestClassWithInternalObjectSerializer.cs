///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

using System;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[InternalObjectSerializer(1)]
		internal class GenericTestClassWithInternalObjectSerializer<T1, T2> : IInternalObjectSerializer
		{
			internal bool         Boolean                      { get; set; }
			internal char         Char                         { get; set; }
			internal sbyte        SByte                        { get; set; }
			internal byte         Byte                         { get; set; }
			internal short        Int16                        { get; set; }
			internal ushort       UInt16                       { get; set; }
			internal int          Int32                        { get; set; }
			internal uint         UInt32                       { get; set; }
			internal long         Int64                        { get; set; }
			internal ulong        UInt64                       { get; set; }
			internal float        Single                       { get; set; }
			internal double       Double                       { get; set; }
			internal decimal      Decimal                      { get; set; }
			internal string       String                       { get; set; }
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
			internal byte[]       Buffer                       { get; set; }

			public GenericTestClassWithInternalObjectSerializer()
			{
				Boolean = true;
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
				Buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			}

			public unsafe GenericTestClassWithInternalObjectSerializer(SerializerArchive archive)
			{
				if (archive.Version == 1)
				{
					Boolean = archive.ReadBoolean();
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

					int bufferSize = archive.ReadInt32();
					Buffer = new byte[bufferSize];
					fixed (byte* pBuffer = &Buffer[0]) archive.ReadBuffer(new IntPtr(pBuffer), bufferSize);
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}
			}

			public unsafe void Serialize(SerializerArchive archive, uint version)
			{
				if (version == 1)
				{
					archive.Write(Boolean);
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

					archive.Write(Buffer.Length);
					fixed (byte* pBuffer = &Buffer[0]) archive.Write(new IntPtr(pBuffer), Buffer.Length);
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
					int hashCode = Boolean.GetHashCode();
					;
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
					hashCode = (hashCode * 397) ^ ByteArrayEqualityComparer.GetHashCode(Buffer);
					return hashCode;
				}
			}

			protected bool Equals(GenericTestClassWithInternalObjectSerializer<T1, T2> other)
			{
				return Boolean == other.Boolean &&
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
				       ByteArrayEqualityComparer.AreEqual(Buffer, other.Buffer);
			}

			public override bool Equals(object obj)
			{
				if (!(obj is GenericTestClassWithInternalObjectSerializer<T1, T2> other)) return false;
				return Equals(other);
			}
		}
	}

}
