///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GriffinPlus.Lib.Collections;
using GriffinPlus.Lib.Conversion;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// External object serializer for converters implementing the <see cref="IConverter"/> interface.
/// The serializer assumes that the converter does not have any state, so it is thread-safe by design.
/// If the converter has some state, consider implementing specific serializer for the converter.
/// </summary>
[ExternalObjectSerializer(1)]
public class ExternalObjectSerializer_IConverter : ExternalObjectSerializer<IConverter>
{
	/// <summary>
	/// Cache storing instances of converters.
	/// Instances are thread-safe as they do not have any internal state.
	/// </summary>
	private static readonly TypeKeyedDictionary<IConverter> sConverters = new();

	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="archive">Archive to serialize into.</param>
	/// <param name="converter">The converter to serialize.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override void Serialize(SerializationArchive archive, IConverter converter)
	{
		if (archive.Version == 1)
			return;

		throw new VersionNotSupportedException(archive);
	}

	/// <summary>
	/// Deserializes an object.
	/// </summary>
	/// <param name="archive">Archive containing the serialized converter.</param>
	/// <returns>The deserialized collection.</returns>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	public override IConverter Deserialize(DeserializationArchive archive)
	{
		// ReSharper disable once InvertIf
		if (archive.Version == 1)
		{
			lock (sConverters)
			{
				if (sConverters.TryGetValue(archive.DataType, out IConverter converter))
					return converter;

				converter = (IConverter)FastActivator.CreateInstance(archive.DataType);
				sConverters.Add(archive.DataType, converter);

				return converter;
			}
		}

		throw new VersionNotSupportedException(archive);
	}
}
