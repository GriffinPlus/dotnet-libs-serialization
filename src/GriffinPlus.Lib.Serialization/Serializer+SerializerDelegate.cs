///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Buffers;

namespace GriffinPlus.Lib.Serialization
{

	public partial class Serializer
	{
		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <param name="serializer">Serializer instance performing the serialization.</param>
		/// <param name="writer">Buffer writer to write to.</param>
		/// <param name="obj">Object to serialize.</param>
		/// <param name="context">A serialization context (may be <c>null</c>)</param>
		private delegate void SerializerDelegate(
			Serializer          serializer,
			IBufferWriter<byte> writer,
			object              obj,
			object              context);
	}

}
