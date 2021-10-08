///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Stores some information about an external object serializer.
	/// </summary>
	class ExternalObjectSerializerInfo
	{
		/// <summary>
		/// Intializes a new instance of the <see cref="ExternalObjectSerializerInfo"/> class.
		/// </summary>
		/// <param name="serializer">External object serializer.</param>
		/// <param name="version">Version of the external object serializer.</param>
		public ExternalObjectSerializerInfo(IExternalObjectSerializer serializer, uint version)
		{
			Serializer = serializer;
			Version = version;
		}

		/// <summary>
		/// Gets the external object serializer.
		/// </summary>
		public IExternalObjectSerializer Serializer { get; }

		/// <summary>
		/// Gets the version of the external serializer.
		/// </summary>
		public uint Version { get; }
	}

}
