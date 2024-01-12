///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if !NET8_0_OR_GREATER
using System;
using System.Runtime.Serialization;
#endif

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Exception that is thrown when the serializer detects a cyclic dependency when serializing.
	/// </summary>
#if !NET8_0_OR_GREATER
	[Serializable]
#endif
	public class CyclicDependencyDetectedException : SerializationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CyclicDependencyDetectedException"/> class.
		/// </summary>
		/// <param name="message">Message describing why the exception is thrown.</param>
		public CyclicDependencyDetectedException(string message) :
			base(message) { }

#if !NET8_0_OR_GREATER
		/// <summary>
		/// Initializes a new instance of the <see cref="CyclicDependencyDetectedException"/> class (used during deserialization).
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected CyclicDependencyDetectedException(SerializationInfo info, StreamingContext context) :
			base(info, context) { }
#endif
	}

}
