///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for GUIDs (type <see cref="System.Guid"/>).
	/// </summary>
	[ExternalObjectSerializer(typeof(Guid), 1)]
	class GuidSerializer : IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes a GUID to a stream.
		/// </summary>
		/// <param name="archive">Archive to put the specified GUID into.</param>
		/// <param name="version">Requested version when serializing the GUID.</param>
		/// <param name="obj">Object to serialize (type System.Guid).</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public void Serialize(SerializerArchive archive, uint version, object obj)
		{
			if (version == 1)
			{
				var guid = (Guid)obj;
				archive.Write(guid.ToUuidByteArray());
			}
			else
			{
				throw new VersionNotSupportedException(typeof(Guid), version);
			}
		}

		/// <summary>
		/// Deserializes a GUID from a stream.
		/// </summary>
		/// <param name="archive">Archive containing the serialized GUID.</param>
		/// <returns>Deserialized object (type System.Guid).</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public object Deserialize(SerializerArchive archive)
		{
			if (archive.Version == 1)
			{
				byte[] buffer = (byte[])archive.ReadObject();
				return buffer.ToRfc4122Guid(0);
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
