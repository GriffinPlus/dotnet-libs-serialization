///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Attribute attached to an external object serializer class telling the serializer to use the annotated class
	/// for serializing/deserializing the specified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ExternalObjectSerializerAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalObjectSerializerAttribute"/> class.
		/// </summary>
		/// <param name="typeToSerialize">
		/// The class/struct the annotated external object serializer provides serialization/deserialization support for.
		/// </param>
		/// <param name="version">Current version of the annotated external object serializer.</param>
		public ExternalObjectSerializerAttribute(Type typeToSerialize, uint version)
		{
			TypeToSerialize = typeToSerialize;
			Version = version;
		}

		/// <summary>
		/// Gets the class/struct that should be serialized using the annotated external object serializer class.
		/// </summary>
		public Type TypeToSerialize { get; }

		/// <summary>
		/// Gets the current version of the external object serializer.
		/// </summary>
		public uint Version { get; }
	}

}
