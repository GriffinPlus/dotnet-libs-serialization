///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Serialization
{

	public partial class ExternalObjectSerializerFactory
	{
		/// <summary>
		/// Stores some information about an external object serializer.
		/// </summary>
		[DebuggerDisplay("Serializer: {SerializerType.FullName}, Serializee: {SerializeeType.FullName}, Version: {Version}")]
		public class SerializerInfo
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="SerializerInfo"/> class.
			/// </summary>
			/// <param name="serializer">The external object serializer type.</param>
			/// <param name="serializee">The type the external object serializer handles.</param>
			/// <param name="version">Version of the external object serializer.</param>
			public SerializerInfo(Type serializer, Type serializee, uint version)
			{
				SerializerType = serializer;
				SerializeeType = serializee;
				Version = version;
			}

			/// <summary>
			/// Gets the external object serializer type.
			/// </summary>
			public Type SerializerType { get; }

			/// <summary>
			/// Gets the type the external object serializer handles.
			/// </summary>
			public Type SerializeeType { get; }

			/// <summary>
			/// Gets the version of the external serializer.
			/// </summary>
			public uint Version { get; }
		}
	}

}
