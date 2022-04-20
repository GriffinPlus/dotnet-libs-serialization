///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	public partial class Serializer
	{
		/// <summary>
		/// Stores some information about an external object serializer.
		/// </summary>
		private class ExternalObjectSerializerInfo
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ExternalObjectSerializerInfo"/> class.
			/// </summary>
			/// <param name="serializer">An instance of the external object serializer.</param>
			/// <param name="version">Version of the external object serializer.</param>
			public ExternalObjectSerializerInfo(IExternalObjectSerializer serializer, uint version)
			{
				Serializer = serializer;
				SerializerVersion = version;
			}

			/// <summary>
			/// Gets the external object serializer instance.
			/// </summary>
			public IExternalObjectSerializer Serializer { get; }

			/// <summary>
			/// Gets the version of the external serializer.
			/// </summary>
			public uint SerializerVersion { get; }
		}
	}

}
