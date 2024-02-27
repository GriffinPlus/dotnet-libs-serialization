///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Serialization;

public partial class ExternalObjectSerializerFactory
{
	/// <summary>
	/// Stores some information about an external object serializer.
	/// </summary>
	[DebuggerDisplay("Serializee: {SerializeeType.FullName}")]
	public class SerializeeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SerializeeInfo"/> class.
		/// </summary>
		/// <param name="serializeeType">Type of the serializee.</param>
		/// <param name="serializerInfo">Information about the serializer handling the serializee.</param>
		/// <param name="serializer">The external object serializer instance.</param>
		public SerializeeInfo(Type serializeeType, SerializerInfo serializerInfo, IExternalObjectSerializer serializer)
		{
			SerializeeType = serializeeType;
			SerializerInfo = serializerInfo;
			Serializer = serializer;
		}

		/// <summary>
		/// Gets information type of the serializee.
		/// </summary>
		public Type SerializeeType { get; }

		/// <summary>
		/// Gets information about the serializer handling the serializee.
		/// </summary>
		public SerializerInfo SerializerInfo { get; }

		/// <summary>
		/// Gets the external object serializer instance handling the serializee.
		/// </summary>
		public IExternalObjectSerializer Serializer { get; }
	}
}
