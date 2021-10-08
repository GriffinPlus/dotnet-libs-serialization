///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Exception that is thrown when something unexpected occurs while serializing or deserializing an object.
	/// </summary>
	[Serializable]
	public class SerializationException : Exception, ISerializable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationException"/> class.
		/// </summary>
		/// <param name="message">Message describing the cause of the exception.</param>
		public SerializationException(string message) :
			base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationException"/> class.
		/// </summary>
		/// <param name="format">Format string for the message describing the cause of the exception.</param>
		/// <param name="args">Arguments to use for formatting the message.</param>
		public SerializationException(string format, params object[] args) :
			base(string.Format(format, args))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationException"/> class.
		/// </summary>
		/// <param name="message">Message describing the cause of the exception.</param>
		/// <param name="innerException">Exception that led to the SerializerException.</param>
		public SerializationException(string message, Exception innerException) :
			base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationException"/> class (used during deserialization).
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected SerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
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
		}
	}

}
