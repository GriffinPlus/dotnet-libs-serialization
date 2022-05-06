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
		/// Serializes a <see cref="System.Guid"/>.
		/// </summary>
		/// <param name="archive">Archive to put the specified GUID into.</param>
		/// <param name="version">Serializer version to use.</param>
		/// <param name="obj">The <see cref="System.Guid"/> to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public void Serialize(SerializationArchive archive, uint version, object obj)
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
		/// Deserializes a <see cref="System.Guid"/>.
		/// </summary>
		/// <param name="archive">Archive containing the serialized GUID.</param>
		/// <returns>The deserialized <see cref="System.Guid"/>.</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public object Deserialize(DeserializationArchive archive)
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
