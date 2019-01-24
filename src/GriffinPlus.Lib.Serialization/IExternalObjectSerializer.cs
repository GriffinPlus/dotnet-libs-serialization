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

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Interface of classes providing serialization support for existing classes without modifying these classes.
	/// </summary>
	/// <remarks>
	/// A class implementing this interface must be stateless, since an external object serializer is created only once
	/// for the entire application domain. Therefore the same instance is used again and again and multiple threads can
	/// access it simultaneously.
	/// </remarks>
	public interface IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes the specified object to the specified archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the specified object to.</param>
		/// <param name="version">Serializer version to use.</param>
		/// <param name="obj">The object to serialize.</param>
		void Serialize(SerializerArchive archive, uint version, object obj);

		/// <summary>
		/// Deserializes an object from the specified archive.
		/// </summary>
		/// <param name="archive">Archive to deserialize the object from.</param>
		/// <returns>Deserialized object.</returns>
		/// <remarks>
		/// The archive contains version information which may tell you how to deserialize it properly, if different
		/// serializer versions are available.
		/// </remarks>
		object Deserialize(SerializerArchive archive);
	}
}
