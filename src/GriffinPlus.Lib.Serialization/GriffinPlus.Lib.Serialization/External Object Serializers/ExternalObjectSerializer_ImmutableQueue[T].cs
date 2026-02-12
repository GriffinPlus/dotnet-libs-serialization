///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Immutable;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="ImmutableQueue{T}"/> class.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ImmutableQueue<T> : ExternalObjectSerializer<ImmutableQueue<T>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="queue">The queue to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ImmutableQueue<T> queue)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// count items (ImmutableQueue does not have a Count property)
			int count = 0;
			foreach (T _ in queue)
			{
				count++;
			}

			// write number of items
			archive.Write(count);

			// write items
			foreach (T item in queue)
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
	public override ImmutableQueue<T> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of items
			int count = archive.ReadInt32();

			// read items from the archive and enqueue them
			var queue = ImmutableQueue<T>.Empty;
			for (int i = 0; i < count; i++)
			{
				var item = (T)archive.ReadObject(archive.Context);
				queue = queue.Enqueue(item);
			}

			return queue;
		}

		throw new VersionNotSupportedException(archive);
	}
}
