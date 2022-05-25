///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// The payload type.
	/// </summary>
	enum PayloadType : byte
	{
		/// <summary>
		/// A null reference.
		/// </summary>
		NullReference,

		/// <summary>
		/// A generic type (may be a generic type definition or a closed constructed generic type).
		/// </summary>
		GenericType,

		/// <summary>
		/// A closed type.
		/// </summary>
		Type,

		/// <summary>
		/// A reference to a previously serialized type
		/// </summary>
		TypeId,

		/// <summary>
		/// Start of a serializer archive.
		/// </summary>
		ArchiveStart,

		/// <summary>
		/// End of a serializer archive.
		/// </summary>
		ArchiveEnd,

		/// <summary>
		/// Start of a serializer archive for a base class.
		/// </summary>
		BaseArchiveStart, // archives containing base class data do not have an end tag

		/// <summary>
		/// A buffer.
		/// </summary>
		Buffer,

		/// <summary>
		/// A <see cref="System.Boolean"/> value being <c>false</c>.
		/// </summary>
		BooleanFalse,

		/// <summary>
		/// A <see cref="System.Boolean"/> value being <c>true</c>.
		/// </summary>
		BooleanTrue,

		/// <summary>
		/// A <see cref="System.Char"/> value (native encoding).
		/// </summary>
		Char_Native,

		/// <summary>
		/// A <see cref="System.Char"/> value (LEB128 encoding).
		/// </summary>
		Char_LEB128,

		/// <summary>
		/// A <see cref="System.SByte"/> value.
		/// </summary>
		SByte,

		/// <summary>
		/// A <see cref="System.Int16"/> value (native encoding).
		/// </summary>
		Int16_Native,

		/// <summary>
		/// A <see cref="System.Int16"/> value (LEB128 encoding).
		/// </summary>
		Int16_LEB128,

		/// <summary>
		/// A <see cref="System.Int32"/> value (native encoding).
		/// </summary>
		Int32_Native,

		/// <summary>
		/// A <see cref="System.Int32"/> value (LEB128 encoding).
		/// </summary>
		Int32_LEB128,

		/// <summary>
		/// A <see cref="System.Int64"/> value (native encoding).
		/// </summary>
		Int64_Native,

		/// <summary>
		/// A <see cref="System.Int64"/> value (LEB128 encoding).
		/// </summary>
		Int64_LEB128,

		/// <summary>
		/// A <see cref="System.Byte"/> value.
		/// </summary>
		Byte,

		/// <summary>
		/// A <see cref="System.UInt16"/> value (native encoding).
		/// </summary>
		UInt16_Native,

		/// <summary>
		/// A <see cref="System.UInt16"/> value (LEB128 encoding).
		/// </summary>
		UInt16_LEB128,

		/// <summary>
		/// A <see cref="System.UInt32"/> value (native encoding).
		/// </summary>
		UInt32_Native,

		/// <summary>
		/// A <see cref="System.UInt32"/> value (LEB128 encoding).
		/// </summary>
		UInt32_LEB128,

		/// <summary>
		/// A <see cref="System.UInt64"/> value (native encoding).
		/// </summary>
		UInt64_Native,

		/// <summary>
		/// A <see cref="System.UInt64"/> value (LEB128 encoding).
		/// </summary>
		UInt64_LEB128,

		/// <summary>
		/// A <see cref="System.Single"/> value.
		/// </summary>
		Single,

		/// <summary>
		/// A <see cref="System.Double"/> value.
		/// </summary>
		Double,

		/// <summary>
		/// A <see cref="System.Decimal"/> value.
		/// </summary>
		Decimal,

		/// <summary>
		/// A <see cref="System.String"/> value (UTF-8 encoding).
		/// </summary>
		String_UTF8,

		/// <summary>
		/// A <see cref="System.String"/> value (UTF-16 encoding, endianness depends on the system).
		/// </summary>
		String_UTF16,

		/// <summary>
		/// A <see cref="System.DateTime"/> value.
		/// </summary>
		DateTime,

		/// <summary>
		/// A <see cref="System.DateTimeOffset"/> value.
		/// </summary>
		DateTimeOffset,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Boolean"/>.
		/// </summary>
		ArrayOfBoolean,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Char"/>.
		/// </summary>
		ArrayOfChar,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.SByte"/>.
		/// </summary>
		ArrayOfSByte,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int16"/>.
		/// </summary>
		ArrayOfInt16,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int32"/>.
		/// </summary>
		ArrayOfInt32,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int64"/>.
		/// </summary>
		ArrayOfInt64,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Byte"/>.
		/// </summary>
		ArrayOfByte,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt16"/>.
		/// </summary>
		ArrayOfUInt16,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt32"/>.
		/// </summary>
		ArrayOfUInt32,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt64"/>.
		/// </summary>
		ArrayOfUInt64,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Single"/>.
		/// </summary>
		ArrayOfSingle,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Double"/>.
		/// </summary>
		ArrayOfDouble,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Decimal"/>.
		/// </summary>
		ArrayOfDecimal,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.String"/>.
		/// </summary>
		ArrayOfString,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.DateTime"/>.
		/// </summary>
		ArrayOfDateTime,

		/// <summary>
		/// An one-dimensional, zero-based array of serializable objects.
		/// </summary>
		ArrayOfObjects,

		/// <summary>
		/// An object which was already serialized before.
		/// </summary>
		AlreadySerialized,

		/// <summary>
		/// An enumeration type.
		/// </summary>
		Enum,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Boolean"/>.
		/// </summary>
		MultidimensionalArrayOfBoolean,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Char"/>.
		/// </summary>
		MultidimensionalArrayOfChar,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.SByte"/>.
		/// </summary>
		MultidimensionalArrayOfSByte,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int16"/>.
		/// </summary>
		MultidimensionalArrayOfInt16,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int32"/>.
		/// </summary>
		MultidimensionalArrayOfInt32,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int64"/>.
		/// </summary>
		MultidimensionalArrayOfInt64,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Byte"/>.
		/// </summary>
		MultidimensionalArrayOfByte,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt16"/>.
		/// </summary>
		MultidimensionalArrayOfUInt16,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt32"/>.
		/// </summary>
		MultidimensionalArrayOfUInt32,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt64"/>.
		/// </summary>
		MultidimensionalArrayOfUInt64,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Single"/>.
		/// </summary>
		MultidimensionalArrayOfSingle,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Double"/>.
		/// </summary>
		MultidimensionalArrayOfDouble,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Decimal"/>.
		/// </summary>
		MultidimensionalArrayOfDecimal,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.String"/>.
		/// </summary>
		MultidimensionalArrayOfString,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.DateTime"/>.
		/// </summary>
		MultidimensionalArrayOfDateTime,

		/// <summary>
		/// A multi-dimensional array of serializable objects.
		/// </summary>
		MultidimensionalArrayOfObjects,

		/// <summary>
		/// A type object.
		/// </summary>
		TypeObject,

		/// <summary>
		/// An instance of <see cref="System.Object"/>.
		/// </summary>
		Object,

		/// <summary>
		/// Terminator (last enumeration value + 1).
		/// </summary>
		Terminator
	}

}
