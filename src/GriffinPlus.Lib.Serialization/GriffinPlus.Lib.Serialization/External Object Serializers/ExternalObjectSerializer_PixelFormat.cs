///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GriffinPlus.Lib.Imaging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="PixelFormat"/> struct.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_PixelFormat : ExternalObjectSerializer<PixelFormat>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="format">The format to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, PixelFormat format)
		{
			if (archive.Version == 1)
			{
				archive.Write(format.Id);
				return;
			}

			throw new VersionNotSupportedException(archive);
		}

		/// <summary>
		/// Deserializes an object.
		/// </summary>
		/// <param name="archive">Archive containing the serialized object.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override PixelFormat Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				return PixelFormat.FromId(archive.ReadInt32());
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
