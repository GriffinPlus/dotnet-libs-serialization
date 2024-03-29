///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization;

public partial class Serializer
{
	/// <summary>
	/// Serializes an object to the specified archive using its internal object serializer.
	/// </summary>
	/// <param name="ios">The object to serialize (implements an internal object serializer).</param>
	/// <param name="archive">Serialization archive to write to.</param>
	internal delegate void IosSerializeDelegate(IInternalObjectSerializer ios, SerializationArchive archive);
}
