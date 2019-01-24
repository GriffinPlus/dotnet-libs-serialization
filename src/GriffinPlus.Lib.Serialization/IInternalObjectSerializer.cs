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
	/// Interface for classes that can serialize themselves using the <see cref="Serializer"/> class.
	/// </summary>
	/// <remarks>
	/// Classes providing persistence using the <see cref="Serializer"/> class must derive from this interface and
	/// implement its <see cref="Serialize"/> method to write an instance of itself into an instance of the
	/// <see cref="SerializerArchive"/> class. Deserialization requires a special constructor receiving an instance
	/// of the <see cref="SerializerArchive"/> class as its argument. Due to the fact that constructors cannot be
	/// declared in interfaces you must take care for implementing the constructor on your own, otherwise deserialization
	/// will fail.
	/// </remarks>
	public interface IInternalObjectSerializer
	{
		// public MyConstructor(SerializerArchive archive)

		/// <summary>
		/// Serializes the current object into a serializer archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the current object into.</param>
		/// <param name="version">Requested version of the current object to serialize.</param>
		void Serialize(SerializerArchive archive, uint version);
	}
}
