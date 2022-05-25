///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Exception that is thrown when a serializer fails serializing an object due to an unsupported
	/// serializer version.
	/// </summary>
	[Serializable]
	public class VersionNotSupportedException : SerializationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class.
		/// </summary>
		/// <param name="archive">Serializer archive passed in during serialization.</param>
		public VersionNotSupportedException(SerializationArchive archive) :
			base($"Specified serializer version ({archive.Version}) is not supported for type '{archive.DataType.FullName}'.")
		{
			Type = archive.DataType;
			RequestedVersion = archive.Version;
			MaxVersion = Serializer.GetSerializerVersion(Type);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class.
		/// </summary>
		/// <param name="archive">Serializer archive passed in during deserialization.</param>
		public VersionNotSupportedException(DeserializationArchive archive) :
			base($"Specified serializer version ({archive.Version}) is not supported for type '{archive.DataType.FullName}'.")
		{
			Type = archive.DataType;
			RequestedVersion = archive.Version;
			MaxVersion = Serializer.GetSerializerVersion(Type);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotSupportedException"/> class (used during deserialization).
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected VersionNotSupportedException(SerializationInfo info, StreamingContext context) :
			base(info, context)
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
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Type", Type);
			info.AddValue("RequestedVersion", RequestedVersion);
			info.AddValue("MaxVersion", MaxVersion);
		}

		/// <summary>
		/// Gets the type that failed serialization/deserialization.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Gets the requested serializer version.
		/// </summary>
		public uint RequestedVersion { get; }

		/// <summary>
		/// Gets the maximum serializer version that is supported for the type specified in the <see cref="Type"/> property.
		/// </summary>
		public uint MaxVersion { get; }
	}

}
