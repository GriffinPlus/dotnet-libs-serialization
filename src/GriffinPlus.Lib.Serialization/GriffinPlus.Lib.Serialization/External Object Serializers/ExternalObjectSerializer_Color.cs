///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GriffinPlus.Lib.Imaging;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="Color"/> struct.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_Color : ExternalObjectSerializer<Color>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="color">The color to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, Color color)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			archive.Write(color.ScA);
			archive.Write(color.ScR);
			archive.Write(color.ScG);
			archive.Write(color.ScB);
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
	public override Color Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			float a = archive.ReadSingle();
			float r = archive.ReadSingle();
			float g = archive.ReadSingle();
			float b = archive.ReadSingle();
			return Color.FromScRgb(a, r, g, b);
		}

		throw new VersionNotSupportedException(archive);
	}
}
