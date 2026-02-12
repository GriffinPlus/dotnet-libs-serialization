///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.Immutable;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for the <see cref="ImmutableStack{T}"/> class.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_ImmutableStack<T> : ExternalObjectSerializer<ImmutableStack<T>>
{
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="stack">The stack to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, ImmutableStack<T> stack)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// collect items into a list to determine the count and write them bottom-up
			var items = new List<T>();
			foreach (T item in stack)
			{
				items.Add(item);
			}

			// write number of items
			archive.Write(items.Count);

			// write items in reverse order (bottom up) to restore the original order when deserializing
			for (int i = items.Count - 1; i >= 0; i--)
			{
				archive.Write(items[i], archive.Context);
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
	public override ImmutableStack<T> Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			// read number of items
			int count = archive.ReadInt32();

			// read items from the archive and push them onto the stack
			var stack = ImmutableStack<T>.Empty;
			for (int i = 0; i < count; i++)
			{
				var item = (T)archive.ReadObject(archive.Context);
				stack = stack.Push(item);
			}

			return stack;
		}

		throw new VersionNotSupportedException(archive);
	}
}
