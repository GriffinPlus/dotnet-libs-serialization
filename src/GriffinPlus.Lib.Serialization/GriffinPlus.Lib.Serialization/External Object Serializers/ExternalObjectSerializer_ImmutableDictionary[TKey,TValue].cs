///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.Immutable;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="ImmutableDictionary{TKey,TValue}"/> class.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ImmutableDictionary<TKey, TValue> : ExternalObjectSerializer<ImmutableDictionary<TKey, TValue>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="dictionary">The dictionary to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ImmutableDictionary<TKey, TValue> dictionary)
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
	public override ImmutableDictionary<TKey, TValue> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of dictionary entries
			int count = archive.ReadInt32();

			// read elements from the archive and put them into the dictionary
			ImmutableDictionary<TKey, TValue>.Builder builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();
			for (int i = 0; i < count; i++)
			{
				var key = (TKey)archive.ReadObject(archive.Context);
				var value = (TValue)archive.ReadObject(archive.Context);
				builder.Add(key, value);
			}

			return builder.ToImmutable();
		}

		throw new VersionNotSupportedException(archive);
	}
}
