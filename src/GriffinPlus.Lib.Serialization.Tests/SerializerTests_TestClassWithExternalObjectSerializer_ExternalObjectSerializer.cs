///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[ExternalObjectSerializer(typeof(TestClassWithExternalObjectSerializer), 1)]
		public class TestClassWithGenericObjectSerializer_ExternalObjectSerializer : IExternalObjectSerializer
		{
			public unsafe void Serialize(SerializerArchive archive, uint version, object obj)
			{
				var other = (TestClassWithExternalObjectSerializer)obj;

				if (version == 1)
				{
					archive.Write(other.Boolean);
					archive.Write(other.Char);
					archive.Write(other.SByte);
					archive.Write(other.Byte);
					archive.Write(other.Int16);
					archive.Write(other.UInt16);
					archive.Write(other.Int32);
					archive.Write(other.UInt32);
					archive.Write(other.Int64);
					archive.Write(other.UInt64);
					archive.Write(other.Single);
					archive.Write(other.Double);
					archive.Write(other.Decimal);
					archive.Write(other.String);
					archive.Write(other.NonGenericType);
					archive.Write(other.GenericTypeDefinition);
					archive.Write(other.ClosedConstructedGenericType);
					archive.Write(other.NullReference);
					archive.Write(other.Enum_S8);
					archive.Write(other.Enum_U8);
					archive.Write(other.Enum_S16);
					archive.Write(other.Enum_U16);
					archive.Write(other.Enum_S32);
					archive.Write(other.Enum_U32);
					archive.Write(other.Enum_S64);
					archive.Write(other.Enum_U64);
					archive.Write(other.SerializableObject);

					archive.Write(other.Buffer.Length);
					fixed (byte* pBuffer = &other.Buffer[0]) archive.Write(new IntPtr(pBuffer), other.Buffer.Length);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClassWithExternalObjectSerializer), version);
				}
			}

			public unsafe object Deserialize(SerializerArchive archive)
			{
				var obj = new TestClassWithExternalObjectSerializer();

				if (archive.Version == 1)
				{
					obj.Boolean = archive.ReadBoolean();
					obj.Char = archive.ReadChar();
					obj.SByte = archive.ReadSByte();
					obj.Byte = archive.ReadByte();
					obj.Int16 = archive.ReadInt16();
					obj.UInt16 = archive.ReadUInt16();
					obj.Int32 = archive.ReadInt32();
					obj.UInt32 = archive.ReadUInt32();
					obj.Int64 = archive.ReadInt64();
					obj.UInt64 = archive.ReadUInt64();
					obj.Single = archive.ReadSingle();
					obj.Double = archive.ReadDouble();
					obj.Decimal = archive.ReadDecimal();
					obj.String = archive.ReadString();
					obj.NonGenericType = archive.ReadType();
					obj.GenericTypeDefinition = archive.ReadType();
					obj.ClosedConstructedGenericType = archive.ReadType();
					obj.NullReference = archive.ReadObject();
					obj.Enum_S8 = archive.ReadEnum<TestEnum_S8>();
					obj.Enum_U8 = archive.ReadEnum<TestEnum_U8>();
					obj.Enum_S16 = archive.ReadEnum<TestEnum_S16>();
					obj.Enum_U16 = archive.ReadEnum<TestEnum_U16>();
					obj.Enum_S32 = archive.ReadEnum<TestEnum_S32>();
					obj.Enum_U32 = archive.ReadEnum<TestEnum_U32>();
					obj.Enum_S64 = archive.ReadEnum<TestEnum_S64>();
					obj.Enum_U64 = archive.ReadEnum<TestEnum_U64>();
					obj.SerializableObject = (List<int>)archive.ReadObject();

					int bufferSize = archive.ReadInt32();
					obj.Buffer = new byte[bufferSize];
					fixed (byte* pBuffer = &obj.Buffer[0]) archive.ReadBuffer(new IntPtr(pBuffer), bufferSize);
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}

				return obj;
			}
		}
	}

}
