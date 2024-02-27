///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for collections implementing the <see cref="ICollection{T}"/> interface.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ICollection<T> : ExternalObjectSerializer<ICollection<T>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="collection">The collection to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ICollection<T> collection)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// write number of items
			int count = collection.Count;
			archive.Write(count);

			// write items
			foreach (T item in collection)
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
	/// <param name="archive">Archive containing the serialized collection.</param>
	/// <returns>The deserialized collection.</returns>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override ICollection<T> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of elements
			int count = archive.ReadInt32();

			// read elements from the archive and put them into the collection
			var collection = (ICollection<T>)FastActivator.CreateInstance(archive.DataType);
			for (int i = 0; i < count; i++)
			{
				var item = (T)archive.ReadObject(archive.Context);
				collection.Add(item);
			}

			return collection;
		}

		throw new VersionNotSupportedException(archive);
	}
}
