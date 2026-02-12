///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Immutable;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="ImmutableArray{T}"/> struct.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ImmutableArray<T> : ExternalObjectSerializer<ImmutableArray<T>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="array">The immutable array to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ImmutableArray<T> array)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// write number of items
			int count = array.Length;
			archive.Write(count);

			// write items
			for (int i = 0; i < count; i++)
			{
				archive.Write(array[i], archive.Context);
			}

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
	public override ImmutableArray<T> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of items
			int count = archive.ReadInt32();

			// read items from the archive and put them into the array
			ImmutableArray<T>.Builder builder = ImmutableArray.CreateBuilder<T>(count);
			for (int i = 0; i < count; i++)
			{
				var item = (T)archive.ReadObject(archive.Context);
				builder.Add(item);
			}

			return builder.MoveToImmutable();
		}

		throw new VersionNotSupportedException(archive);
	}
}
