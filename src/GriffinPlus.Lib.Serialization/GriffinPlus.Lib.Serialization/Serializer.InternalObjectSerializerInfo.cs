///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	public partial class Serializer
	{
		/// <summary>
		/// Stores some information about an internal object serializer.
		/// </summary>
		private class InternalObjectSerializerInfo
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="InternalObjectSerializerInfo"/> class.
			/// </summary>
			/// <param name="version">Version of the internal object serializer.</param>
			public InternalObjectSerializerInfo(uint version)
			{
				SerializerVersion = version;
			}

			/// <summary>
			/// Gets the version of the internal object serializer.
			/// </summary>
			public uint SerializerVersion { get; }
		}
	}

}
