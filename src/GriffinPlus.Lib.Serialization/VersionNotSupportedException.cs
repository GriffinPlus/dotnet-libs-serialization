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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Exception that is thrown when a serializer fails serializing an object due to an unsupported
	/// serializer version.
	/// </summary>
	[Serializable]
	public class VersionNotSupportedException : SerializationException, ISerializable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class.
		/// </summary>
		/// <param name="type">Type that was tried to serialize (must be serializable).</param>
		/// <param name="version">Requested serializer version.</param>
		public VersionNotSupportedException(Type type, uint version) :
			base(string.Format("Specified serializer version ({0}) is not supported for type '{1}'.", version, type.FullName))
		{
			Type = type;
			RequestedVersion = version;
			MaxVersion = Serializer.GetSerializerVersion(Type);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class.
		/// </summary>
		/// <param name="archive">Serializer archive passed in during deserialization.</param>
		public VersionNotSupportedException(SerializerArchive archive) :
			base(string.Format("Specified serializer version ({0}) is not supported for type '{1}'.", archive.Version, archive.Type.FullName))
		{
			Type = archive.Type;
			RequestedVersion = archive.Version;
			MaxVersion = Serializer.GetSerializerVersion(Type);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class (used during deserialization).
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected VersionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Type = (Type)info.GetValue("Type", typeof(Type));
			RequestedVersion = info.GetUInt32("RequestedVersion");
			MaxVersion = info.GetUInt32("MaxVersion");
		}

		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Type", Type);
			info.AddValue("RequestedVersion", RequestedVersion);
			info.AddValue("MaxVersion", MaxVersion);
		}

		/// <summary>
		/// Gets the type that failed serializing.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Gets the requested serializer version.
		/// </summary>
		public uint RequestedVersion { get; }

		/// <summary>
		/// Gets the maximum serializer version that is supported for the type specified in the 'Type' property.
		/// </summary>
		public uint MaxVersion { get; }
	}
}
