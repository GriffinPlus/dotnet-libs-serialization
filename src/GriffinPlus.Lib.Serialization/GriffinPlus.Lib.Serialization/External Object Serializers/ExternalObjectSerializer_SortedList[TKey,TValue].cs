///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="SortedList{TKey,TValue}"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_SortedList<TKey, TValue> : ExternalObjectSerializer<SortedList<TKey, TValue>>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="list">The list to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, SortedList<TKey, TValue> list)
		{
			if (archive.Version == 1)
			{
				// write number of list items
				int count = list.Count;
				archive.Write(count);

				// write list items
				foreach (KeyValuePair<TKey, TValue> pair in list)
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
		public override SortedList<TKey, TValue> Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of list items
				int count = archive.ReadInt32();

				// read items from the archive and put them into the list
				var list = new SortedList<TKey, TValue>(count);
				for (int i = 0; i < count; i++)
				{
					var key = (TKey)archive.ReadObject(archive.Context);
					var value = (TValue)archive.ReadObject(archive.Context);
					list.Add(key, value);
				}

				return list;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
