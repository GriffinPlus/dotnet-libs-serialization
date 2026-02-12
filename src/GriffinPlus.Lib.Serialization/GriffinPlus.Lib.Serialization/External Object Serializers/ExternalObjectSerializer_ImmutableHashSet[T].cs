///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Immutable;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="ImmutableHashSet{T}"/> class.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ImmutableHashSet<T> : ExternalObjectSerializer<ImmutableHashSet<T>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="set">The set to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ImmutableHashSet<T> set)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// write number of items
			int count = set.Count;
			archive.Write(count);

			// write items
			foreach (T item in set)
			{
				archive.Write(item, archive.Context);
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
	public override ImmutableHashSet<T> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of items
			int count = archive.ReadInt32();

			// read items from the archive and put them into the hash set
			ImmutableHashSet<T>.Builder builder = ImmutableHashSet.CreateBuilder<T>();
			for (int i = 0; i < count; i++)
			{
				var item = (T)archive.ReadObject(archive.Context);
				builder.Add(item);
			}

			return builder.ToImmutable();
		}

		throw new VersionNotSupportedException(archive);
	}
}
