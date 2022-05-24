///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[ExternalObjectSerializer(typeof(TestStructWithExternalObjectSerializer), 1)]
		public class TestStructWithGenericObjectSerializer_ExternalObjectSerializer : IExternalObjectSerializer
		{
			public unsafe void Serialize(SerializationArchive archive, uint version, object obj)
			{
				var other = (TestStructWithExternalObjectSerializer)obj;

				if (version == 1)
				{
					archive.Write(other.BooleanFalse);
					archive.Write(other.BooleanTrue);
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

					// deserialize buffer via pointer
					archive.Write(other.Buffer1.Length);
					fixed (byte* pBuffer = &other.Buffer1[0]) archive.Write(pBuffer, other.Buffer1.Length);

					// serializer buffer via stream
					archive.Write(other.Buffer2.Length);
					archive.Write(new MemoryStream(other.Buffer2));
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestStructWithExternalObjectSerializer), version);
				}
			}

			public unsafe object Deserialize(DeserializationArchive archive)
			{
				var obj = new TestStructWithExternalObjectSerializer();

				if (archive.Version == 1)
				{
					obj.BooleanFalse = archive.ReadBoolean();
					obj.BooleanTrue = archive.ReadBoolean();
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

					// deserialize buffer via pointer
					int bufferSize = archive.ReadInt32();
					obj.Buffer1 = new byte[bufferSize];
					fixed (byte* pBuffer = &obj.Buffer1[0]) archive.ReadBuffer(pBuffer, bufferSize);

					// deserialize buffer via Stream
					int buffer2Size = archive.ReadInt32();
					var stream = archive.ReadStream();
					obj.Buffer2 = new byte[buffer2Size];
					int readByteCount = stream.Read(obj.Buffer2, 0, obj.Buffer2.Length);
					stream.Dispose();
					Debug.Assert(readByteCount == obj.Buffer2.Length);
					Debug.Assert(stream.Length == obj.Buffer2.Length);
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
