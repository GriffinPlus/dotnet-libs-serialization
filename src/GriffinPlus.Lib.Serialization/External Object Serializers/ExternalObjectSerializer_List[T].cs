///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="List{T}"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_List<T> : ExternalObjectSerializer<List<T>>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="list">The list to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, List<T> list)
		{
			if (archive.Version == 1)
			{
				// write number of list items
				int count = list.Count;
				archive.Write(count);

				// write list items
				for (int i = 0; i < count; i++)
				{
					archive.Write(list[i], archive.Context);
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
		public override List<T> Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of list items
				int count = archive.ReadInt32();

				// read items from the archive and put them into the list
				var list = new List<T>(count);
				for (int i = 0; i < count; i++)
				{
					var item = (T)archive.ReadObject(archive.Context);
					list.Add(item);
				}

				return list;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
