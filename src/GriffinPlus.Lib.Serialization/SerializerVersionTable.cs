///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Represents a table containing a mapping between types and the serializer version to use for them.
	/// </summary>
	public class SerializerVersionTable
	{
		private Dictionary<Type, uint> mTable;

		/// <summary>
		/// Initializes a new instance of the SerializerVersionTable class.
		/// </summary>
		public SerializerVersionTable()
		{
			mTable = new Dictionary<Type,uint>();
		}

		/// <summary>
		/// Sets a mapping.
		/// </summary>
		/// <param name="type">Serializable type.</param>
		/// <param name="version">Requested serializer version.</param>
		public void Set(Type type, uint version)
		{
			mTable[type] = version;
		}

		/// <summary>
		/// Trys to get a mapping.
		/// </summary>
		/// <param name="type">Type to look for.</param>
		/// <param name="version">Receives the requested serializer version for the specified type.</param>
		/// <returns>true if the type has a requested serializer version assigned.</returns>
		public bool TryGet(Type type, out uint version)
		{
			return mTable.TryGetValue(type, out version);
		}
	}
}
