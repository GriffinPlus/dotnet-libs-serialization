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
	/// Attribute attached to an external object serializer class telling the serializer to use the annotated class
	/// for serializing/deserializing the specified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public class ExternalObjectSerializerAttribute : Attribute
	{
		private readonly Type   mTypeToSerialize;
		private readonly uint   mVersion;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalObjectSerializerAttribute"/> class.
		/// </summary>
		/// <param name="typeToSerialize">
		/// The class/struct the annotated external object serializer provides serialization/deserialization support for.
		/// </param>
		/// <param name="version">Current version of the annotated external object serializer.</param>
		public ExternalObjectSerializerAttribute(Type typeToSerialize, uint version)
		{
			mTypeToSerialize = typeToSerialize;
			mVersion = version;
		}

		/// <summary>
		/// Gets the class/struct that should be serialized using the annotated external object serializer class.
		/// </summary>
		public Type TypeToSerialize
		{
			get {
				return mTypeToSerialize;
			}
		}

		/// <summary>
		/// Gets the current version of the external object serializer.
		/// </summary>
		public uint Version
		{
			get {
				return mVersion;
			}
		}

	}
}
