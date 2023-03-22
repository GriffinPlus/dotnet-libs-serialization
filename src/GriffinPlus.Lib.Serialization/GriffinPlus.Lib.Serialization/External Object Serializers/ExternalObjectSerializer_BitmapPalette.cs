///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using GriffinPlus.Lib.Imaging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="BitmapPalette"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_BitmapPalette : ExternalObjectSerializer<BitmapPalette>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="palette">The palette to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, BitmapPalette palette)
		{
			if (archive.Version == 1)
			{
				// write the list of colors to the archive
				// PartialList<T> does not have a parameterless constructor
				// => generic IList<T> serializer is not an option
				// => convert to an array first...
				archive.Write(palette.Colors.ToArray());
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
		public override BitmapPalette Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				var colors = (Color[])archive.ReadObject();
				return new BitmapPalette(colors);
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
