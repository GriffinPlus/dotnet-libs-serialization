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

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// External object serializer for GUIDs (type <see cref="System.Guid"/>).
	/// </summary>
	[ExternalObjectSerializer(typeof(Guid), 1)]
	internal class GuidSerializer : IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes a GUID to a stream.
		/// </summary>
		/// <param name="archive">Archive to put the specfied GUID into.</param>
		/// <param name="version">Requested version when serializing the GUID.</param>
		/// <param name="obj">Object to serialize (type System.Guid).</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public void Serialize(SerializerArchive archive, uint version, object obj)
		{
			if (version == 1) {
				Guid guid = (Guid)obj;
				archive.Write(guid.ToUuidByteArray());
			} else {
				throw new VersionNotSupportedException(typeof(Guid), version);
			}
		}

		/// <summary>
		/// Deserializes a GUID from a stream.
		/// </summary>
		/// <param name="archive">Archive containing the serialized GUID.</param>
		/// <returns>Deserialized object (type System.Guid).</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public object Deserialize(SerializerArchive archive)
		{
			if (archive.Version == 1) {
				byte[] buffer = (byte[])archive.ReadObject();
				return buffer.ToRfc4122Guid(0);
			} else {
				throw new VersionNotSupportedException(archive);
			}
		}

	}
}
