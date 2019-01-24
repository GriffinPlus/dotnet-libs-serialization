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
	/// Stores some information about an external object serializer.
	/// </summary>
	internal class ExternalObjectSerializerInfo
	{
		/// <summary>
		/// Intializes a new instance of the <see cref="ExternalObjectSerializerInfo"/> class.
		/// </summary>
		/// <param name="serializer">External object serializer.</param>
		/// <param name="version">Version of the external object serializer.</param>
		public ExternalObjectSerializerInfo(IExternalObjectSerializer serializer, uint version)
		{
			Serializer = serializer;
			Version = version;
		}

		/// <summary>
		/// Gets the external object serializer.
		/// </summary>
		public IExternalObjectSerializer Serializer { get; }

		/// <summary>
		/// Gets the version of the external serializer.
		/// </summary>
		public uint Version { get; }

	}
}
