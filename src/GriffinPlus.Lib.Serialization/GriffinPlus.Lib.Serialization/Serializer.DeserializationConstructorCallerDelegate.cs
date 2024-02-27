///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization;

public partial class Serializer
{
	/// <summary>
	/// Creates a new object using the deserialization constructor.
	/// </summary>
	/// <param name="archive">Deserialization archive to pass to the constructor.</param>
	private delegate object DeserializationConstructorCallerDelegate(ref DeserializationArchive archive);
}
