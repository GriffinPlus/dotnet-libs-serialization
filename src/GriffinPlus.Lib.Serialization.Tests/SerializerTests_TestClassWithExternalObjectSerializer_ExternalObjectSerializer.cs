///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[ExternalObjectSerializer(typeof(TestClassWithExternalObjectSerializer), 1)]
		public class TestClassWithGenericObjectSerializer_ExternalObjectSerializer : IExternalObjectSerializer
		{
			public void Serialize(SerializerArchive archive, uint version, object obj)
			{
				var other = (TestClassWithExternalObjectSerializer)obj;

				if (version == 1)
				{
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
					archive.Write(other.Char);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClassWithExternalObjectSerializer), version);
				}
			}

			public object Deserialize(SerializerArchive archive)
			{
				var obj = new TestClassWithExternalObjectSerializer();

				if (archive.Version == 1)
				{
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
					obj.Char = archive.ReadChar();
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
