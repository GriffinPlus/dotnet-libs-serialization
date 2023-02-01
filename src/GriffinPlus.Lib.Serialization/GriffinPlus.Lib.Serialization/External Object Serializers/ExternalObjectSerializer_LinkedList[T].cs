///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="LinkedList{T}"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_LinkedList<T> : ExternalObjectSerializer<LinkedList<T>>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="list">The list to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, LinkedList<T> list)
		{
			if (archive.Version == 1)
			{
				// write number of list items
				int count = list.Count;
				archive.Write(count);

				// write list items
				LinkedListNode<T> node = list.First;
				while (node != null)
				{
					archive.Write(node.Value, archive.Context);
					node = node.Next;
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
		public override LinkedList<T> Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of list items
				int count = archive.ReadInt32();

				// read items from the archive and put them into the list
				var list = new LinkedList<T>();
				for (int i = 0; i < count; i++)
				{
					var item = (T)archive.ReadObject(archive.Context);
					list.AddLast(item);
				}

				return list;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
