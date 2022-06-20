///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Determines whether to optimize for speed or for size when serializing.
	/// </summary>
	public enum SerializationOptimization
	{
		/// <summary>
		/// Optimize for speed.
		/// </summary>
		Speed,

		/// <summary>
		/// Optimize for size.
		/// </summary>
		Size
	}

}
