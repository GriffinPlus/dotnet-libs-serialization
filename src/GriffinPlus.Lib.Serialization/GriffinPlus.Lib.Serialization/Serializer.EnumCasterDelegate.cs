///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization;

public partial class Serializer
{
	/// <summary>
	/// Casts an integer to a specific enumeration type.
	/// </summary>
	/// <param name="value">Integer value to convert to the enumeration type.</param>
	/// <returns>The resulting enumeration value.</returns>
	private delegate Enum EnumCasterDelegate(long value);
}
