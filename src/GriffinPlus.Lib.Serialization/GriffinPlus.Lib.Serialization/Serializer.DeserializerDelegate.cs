///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace GriffinPlus.Lib.Serialization
{

	public partial class Serializer
	{
		/// <summary>
		/// Deserializes a certain object from the specified stream.
		/// </summary>
		/// <param name="serializer">Serializer instance performing the deserialization.</param>
		/// <param name="stream">Stream containing data to deserialize.</param>
		/// <param name="context">A serialization context (can be <c>null</c>)</param>
		/// <returns>The deserialized object.</returns>
		private delegate object DeserializerDelegate(Serializer serializer, Stream stream, object context);
	}

}
