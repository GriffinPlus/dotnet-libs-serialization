///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for generic lists (type <see cref="List{T}"/>).
	/// </summary>
	[ExternalObjectSerializer(typeof(List<>), 1)]
	class ListTSerializer : IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes a <see cref="List{T}"/>.
		/// </summary>
		/// <param name="archive">Archive to serialize the specified list into.</param>
		/// <param name="version">Serializer version to use.</param>
		/// <param name="obj">The list to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public void Serialize(SerializationArchive archive, uint version, object obj)
		{
			if (version == 1)
			{
				var list = (IList)obj;
				int count = list.Count;
				archive.Write(count);
				for (int i = 0; i < count; i++)
				{
					archive.Write(list[i], archive.Context);
				}
			}
			else
			{
				throw new VersionNotSupportedException(typeof(List<>), version);
			}
		}

		/// <summary>
		/// Deserializes a <see cref="List{T}"/>.
		/// </summary>
		/// <param name="archive">Archive containing a serialized <see cref="List{T}"/> object.</param>
		/// <returns>The deserialized <see cref="List{T}"/>.</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public object Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of elements and set the capacity of the list appropriately to
				// avoid resizing while populating the list
				int count = archive.ReadInt32();

				// read elements from the archive and put them into the list
				var collection = (IList)FastActivator.CreateInstance(archive.DataType, count);
				for (int i = 0; i < count; i++)
				{
					object obj = archive.ReadObject(archive.Context);
					collection.Add(obj);
				}

				return collection;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
