///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization;

public partial class Serializer
{
	/// <summary>
	/// Data structure storing an assembly-qualified type and the corresponding type object
	/// (used to cache type information when deserializing types).
	/// </summary>
	private struct TypeItem
	{
		/// <summary>
		/// The assembly-qualified name of the type.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The type object the assembly-qualified type name was mapped to.
		/// </summary>
		public readonly Type Type;

		/// <summary>
		/// An empty type item.
		/// </summary>
		public static readonly TypeItem Empty = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeItem"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public TypeItem(string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}
}
