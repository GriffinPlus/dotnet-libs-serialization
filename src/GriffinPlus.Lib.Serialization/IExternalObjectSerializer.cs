///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Interface of classes providing serialization support for existing classes without modifying these classes.
	/// </summary>
	/// <remarks>
	/// A class implementing this interface must be stateless, since an external object serializer is created only once
	/// for the entire application domain. Therefore the same instance is used again and again and multiple threads can
	/// access it simultaneously.
	/// </remarks>
	public interface IExternalObjectSerializer
	{
		/// <summary>
		/// Serializes the specified object to the specified archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the specified object to.</param>
		/// <param name="version">Serializer version to use.</param>
		/// <param name="obj">The object to serialize.</param>
		void Serialize(SerializationArchive archive, uint version, object obj);

		/// <summary>
		/// Deserializes an object from the specified archive.
		/// </summary>
		/// <param name="archive">Archive to deserialize the object from.</param>
		/// <returns>Deserialized object.</returns>
		/// <remarks>
		/// The archive contains version information which may tell you how to deserialize it properly,
		/// if different serializer versions are available.
		/// </remarks>
		object Deserialize(DeserializationArchive archive);
	}

}
