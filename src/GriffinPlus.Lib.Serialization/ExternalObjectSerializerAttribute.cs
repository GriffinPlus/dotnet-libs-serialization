///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Attribute attached to an external object serializer class telling the serializer to use the annotated class
	/// for serializing/deserializing a specific type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class ExternalObjectSerializerAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalObjectSerializerAttribute"/> class.
		/// </summary>
		/// <param name="version">Current version of the annotated external object serializer.</param>
		public ExternalObjectSerializerAttribute(uint version)
		{
			Version = version;
		}

		/// <summary>
		/// Gets the current version of the external object serializer.
		/// </summary>
		public uint Version { get; }
	}

}
