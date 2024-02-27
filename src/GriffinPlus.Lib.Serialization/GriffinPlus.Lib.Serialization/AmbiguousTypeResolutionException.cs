///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// Exception that is thrown when the serializer cannot resolve a type unambiguously during deserialization.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class AmbiguousTypeResolutionException : TypeResolutionException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AmbiguousTypeResolutionException"/> class.
	/// </summary>
	/// <param name="message">Message describing why the exception is thrown.</param>
	/// <param name="typeNameToResolve">Name of the type that failed resolution.</param>
	/// <param name="resolvedTypes">Type objects the type name was resolved to.</param>
	public AmbiguousTypeResolutionException(string message, string typeNameToResolve, Type[] resolvedTypes) :
		base(message, typeNameToResolve)
	{
		ResolvedTypes = resolvedTypes;
	}

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Initializes a new instance of the <see cref="AmbiguousTypeResolutionException"/> class (used during deserialization).
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> that receives the serialized object data about the object.</param>
	/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
	protected AmbiguousTypeResolutionException(SerializationInfo info, StreamingContext context) :
		base(info, context)
	{
		ResolvedTypes = (Type[])info.GetValue("ResolvedTypes", typeof(Type[]));
	}

	/// <summary>
	/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the object.</param>
	/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ResolvedTypes", ResolvedTypes);
	}
#endif

	/// <summary>
	/// Gets the types the <see cref="TypeResolutionException.TypeNameToResolve"/> was unambiguously resolved to.
	/// </summary>
	public Type[] ResolvedTypes { get; }
}
