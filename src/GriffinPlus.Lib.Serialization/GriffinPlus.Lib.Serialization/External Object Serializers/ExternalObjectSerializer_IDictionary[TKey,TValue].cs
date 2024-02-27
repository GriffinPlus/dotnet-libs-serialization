///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for collections implementing the <see cref="IDictionary{TKey,TValue}"/> interface.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_IDictionary<TKey, TValue> : ExternalObjectSerializer<IDictionary<TKey, TValue>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="dictionary">The dictionary to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, IDictionary<TKey, TValue> dictionary)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// write number of dictionary entries
			int count = dictionary.Count;
			archive.Write(count);

			// write dictionary entries
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				archive.Write(pair.Key, archive.Context);
				archive.Write(pair.Value, archive.Context);
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
	public override IDictionary<TKey, TValue> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of dictionary entries
			int count = archive.ReadInt32();

			// read elements from the archive and put them into the dictionary
			var dictionary = (IDictionary<TKey, TValue>)FastActivator.CreateInstance(archive.DataType);
			for (int i = 0; i < count; i++)
			{
				var key = (TKey)archive.ReadObject(archive.Context);
				var value = (TValue)archive.ReadObject(archive.Context);
				dictionary.Add(key, value);
			}

			return dictionary;
		}

		throw new VersionNotSupportedException(archive);
	}
}
