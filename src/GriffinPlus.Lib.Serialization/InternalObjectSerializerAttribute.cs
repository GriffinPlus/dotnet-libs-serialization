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
