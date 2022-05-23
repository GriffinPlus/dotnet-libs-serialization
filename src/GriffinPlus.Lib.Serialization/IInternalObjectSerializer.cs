///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Interface for classes that can serialize themselves using the <see cref="Serializer"/> class.
	/// </summary>
	/// <remarks>
	/// Classes providing persistence using the <see cref="Serializer"/> class must derive from this interface and
	/// implement its <see cref="Serialize"/> method to write an instance of itself into an instance of the
	/// <see cref="SerializationArchive"/> class. Deserialization requires a special constructor receiving an instance
	/// of the <see cref="DeserializationArchive"/> class as its argument. Due to the fact that constructors cannot be
	/// declared in interfaces you must take care for implementing the constructor on your own, otherwise deserialization
	/// will fail.
	/// </remarks>
	public interface IInternalObjectSerializer
	{
		// public MyConstructor(DeserializationArchive archive)

		/// <summary>
		/// Serializes the current object into a serializer archive.
		/// </summary>
		/// <param name="archive">Archive to serialize the current object into.</param>
		/// <param name="version">Requested version of the current object to serialize.</param>
		void Serialize(SerializationArchive archive, uint version);
	}

}
