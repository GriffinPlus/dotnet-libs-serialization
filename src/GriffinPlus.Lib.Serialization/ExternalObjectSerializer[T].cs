///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Base class for external object serializers that add support for existing types without modifying these types.
	/// </summary>
	/// <typeparam name="T">
	/// Type to serialize
	/// (may be a regular type, a closed generic type, a generic type definition or an interface).
	/// </typeparam>
	/// <remarks>
	/// A class deriving from this class must be stateless, since an external object serializer is created only once
	/// for the entire application domain. Therefore the same instance is used again and again and multiple threads can
	/// access it simultaneously.
	/// </remarks>
	public abstract class ExternalObjectSerializer<T> : IExternalObjectSerializer
	{
		/// <summary>
		/// Gets the type the serializer can process
		/// (can be a regular type, a closed generic type, a generic type definition or an interface).
		/// </summary>
		public Type SerializedType => typeof(T);

		/// <summary>
		/// Serializes the specified object to the specified archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the specified object to.</param>
		/// <param name="obj">The object to serialize.</param>
		void IExternalObjectSerializer.Serialize(SerializationArchive archive, object obj)
		{
			Serialize(archive, (T)obj);
		}

		/// <summary>
		/// Serializes the specified object to the specified archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the specified object to.</param>
		/// <param name="obj">The object to serialize.</param>
		public abstract void Serialize(SerializationArchive archive, T obj);

		/// <summary>
		/// Deserializes an object from the specified archive.
		/// </summary>
		/// <param name="archive">Archive to deserialize the object from.</param>
		/// <returns>Deserialized object.</returns>
		/// <remarks>
		/// The archive contains version information which may tell you how to deserialize it properly,
		/// if different serializer versions are available.
		/// </remarks>
		object IExternalObjectSerializer.Deserialize(DeserializationArchive archive)
		{
			return Deserialize(archive);
		}

		/// <summary>
		/// Deserializes an object from the specified archive.
		/// </summary>
		/// <param name="archive">Archive to deserialize the object from.</param>
		/// <returns>Deserialized object.</returns>
		/// <remarks>
		/// The archive contains version information which may tell you how to deserialize it properly,
		/// if different serializer versions are available.
		/// </remarks>
		public abstract T Deserialize(DeserializationArchive archive);
	}

}
