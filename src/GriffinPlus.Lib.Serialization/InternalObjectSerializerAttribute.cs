///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Attribute that must be attached to a struct/class that is able to serialize/deserialize itself by implementing an
	/// internal object serializer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple=false)]
	public class InternalObjectSerializerAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InternalObjectSerializerAttribute"/> class.
		/// </summary>
		/// <param name="version">Current version of the annotated internal object serializer.</param>
		public InternalObjectSerializerAttribute(uint version)
		{
			Version = version;
		}

		/// <summary>
		/// Gets the current version of the internal object serializer.
		/// </summary>
		public uint Version { get; }

	}
}
