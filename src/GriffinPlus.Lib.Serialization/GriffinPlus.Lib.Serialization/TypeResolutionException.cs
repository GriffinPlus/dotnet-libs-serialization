///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Exception that is thrown when the serializer cannot resolve a type during deserialization.
	/// </summary>
	[Serializable]
	public class TypeResolutionException : SerializationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class.
		/// </summary>
		/// <param name="message">Message describing why the exception is thrown.</param>
		/// <param name="typeNameToResolve">Name of the type that failed resolution.</param>
		public TypeResolutionException(string message, string typeNameToResolve) :
			base(message)
		{
			TypeNameToResolve = typeNameToResolve;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class (used during deserialization).
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected TypeResolutionException(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			TypeNameToResolve = (string)info.GetValue("TypeNameToResolve", typeof(string));
		}

		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("TypeNameToResolve", TypeNameToResolve);
		}

		/// <summary>
		/// Gets the name of the type that that failed resolution.
		/// </summary>
		public string TypeNameToResolve { get; }
	}

}
