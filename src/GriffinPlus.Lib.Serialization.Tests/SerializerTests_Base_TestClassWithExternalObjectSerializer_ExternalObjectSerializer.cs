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

	public partial class SerializerTests_Base
	{
		[ExternalObjectSerializer(1)]
		public class TestClassWithGenericObjectSerializer_ExternalObjectSerializer : ExternalObjectSerializer<TestClassWithExternalObjectSerializer>
		{
			public override unsafe void Serialize(SerializationArchive archive, TestClassWithExternalObjectSerializer obj)
			{
				if (archive.Version == 1)
				{
					archive.Write(obj.BooleanFalse);
					archive.Write(obj.BooleanTrue);
					archive.Write(obj.Char);
					archive.Write(obj.SByte);
					archive.Write(obj.Byte);
					archive.Write(obj.Int16);
					archive.Write(obj.UInt16);
					archive.Write(obj.Int32);
					archive.Write(obj.UInt32);
					archive.Write(obj.Int64);
					archive.Write(obj.UInt64);
					archive.Write(obj.Single);
					archive.Write(obj.Double);
					archive.Write(obj.Decimal);
					archive.Write(obj.String);
					archive.Write(obj.DateTime);
					archive.Write(obj.DateTimeOffset);
					archive.Write(obj.Guid);
					archive.Write(obj.NonGenericType);
					archive.Write(obj.GenericTypeDefinition);
					archive.Write(obj.ClosedConstructedGenericType);
					archive.Write(obj.NullReference);
					archive.Write(obj.Enum_S8);
					archive.Write(obj.Enum_U8);
					archive.Write(obj.Enum_S16);
					archive.Write(obj.Enum_U16);
					archive.Write(obj.Enum_S32);
					archive.Write(obj.Enum_U32);
					archive.Write(obj.Enum_S64);
					archive.Write(obj.Enum_U64);
					archive.Write(obj.SerializableObject);

					// deserialize buffer via pointer
					archive.Write(obj.Buffer1.Length);
					fixed (byte* pBuffer = &obj.Buffer1[0]) archive.Write(pBuffer, obj.Buffer1.Length);

					// serializer buffer via stream
					archive.Write(obj.Buffer2.Length);
					archive.Write(new MemoryStream(obj.Buffer2));
					return;
				}

				throw new VersionNotSupportedException(archive);
			}

			public override unsafe TestClassWithExternalObjectSerializer Deserialize(DeserializationArchive archive)
			{
				var obj = new TestClassWithExternalObjectSerializer();

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
					obj.DateTime = archive.ReadDateTime();
					obj.DateTimeOffset = archive.ReadDateTimeOffset();
					obj.Guid = archive.ReadGuid();
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
