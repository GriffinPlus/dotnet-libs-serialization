///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="Stack{T}"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public class ExternalObjectSerializer_Stack<T> : ExternalObjectSerializer<Stack<T>>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="stack">The queue to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, Stack<T> stack)
		{
			if (archive.Version == 1)
			{
				// write number of items
				int count = stack.Count;
				archive.Write(count);

				// write items (top down)
				foreach (var item in stack)
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
		public override Stack<T> Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of items
				int count = archive.ReadInt32();

				// read items from the archive and put them onto the stack
				var stack = new Stack<T>(count);
				for (int i = 0; i < count; i++)
				{
					var item = (T)archive.ReadObject(archive.Context);
					stack.Push(item);
				}

				return stack;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
