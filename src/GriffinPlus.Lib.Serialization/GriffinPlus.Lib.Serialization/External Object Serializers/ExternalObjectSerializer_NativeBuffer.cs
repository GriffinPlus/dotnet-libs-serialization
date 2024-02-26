///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="NativeBuffer"/> struct.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public unsafe class ExternalObjectSerializer_NativeBuffer : ExternalObjectSerializer<NativeBuffer>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="buffer">The buffer to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, NativeBuffer buffer)
		{
			if (archive.Version == 1)
			{
				archive.Write(buffer.Size);
				archive.Write((void*)buffer.UnsafeAddress, buffer.Size);
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
		public override NativeBuffer Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				long size = archive.ReadInt64();
				if (sizeof(nint) == 4 && size > int.MaxValue) throw new OutOfMemoryException();
				var buffer = NativeBuffer.CreatePageAligned((nint)size); // safe alignment choice
				archive.ReadBuffer((void*)buffer.UnsafeAddress, size);
				return buffer;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
