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

using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// External object serializer for generic lists (type <see cref="System.Collections.Generic.List{T}"/>).
	/// </summary>
	[ExternalObjectSerializer(typeof(List<>), 1)]
	internal class ListTSerializer : IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes a generic list to a stream.
		/// </summary>
		/// <param name="archive">Archive to put the specified list into.</param>
		/// <param name="version">Serializer version to use.</param>
		/// <param name="obj">List to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public void Serialize(SerializerArchive archive, uint version, object obj)
		{
			if (version == 1)
			{
				IList list = (IList)obj;
				int count = list.Count;
				archive.Write(count);
				for (int i = 0; i < count; i++) {
					archive.Write(list[i], archive.Context);
				}
			}
			else
			{
				throw new VersionNotSupportedException(typeof(List<>), version);
			}
		}

		/// <summary>
		/// Deserializes a generic list from a stream.
		/// </summary>
		/// <param name="archive">Archive containing a serialized generic list object.</param>
		/// <returns>Deserialized list.</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public object Deserialize(SerializerArchive archive)
		{
			if (archive.Version == 1)
			{
				// read number of elements and set the capacity of the list appropriately to
				// avoid resizing while populating the list
				int count = archive.ReadInt32();

				// read elements from the archive and put them into the list
				IList collection = (IList)FastActivator.CreateInstance(archive.Type, count);
				for (int i = 0; i < count; i++) {
					object obj = archive.ReadObject(archive.Context);
					collection.Add(obj);
				}

				return collection;
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}
	}
}
