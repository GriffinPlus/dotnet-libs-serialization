///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
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
