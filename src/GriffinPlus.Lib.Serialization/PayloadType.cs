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
		/// An object which was already serialized before.
		/// </summary>
		AlreadySerialized,

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
		/// A type object.
		/// </summary>
		TypeObject,

		/// <summary>
		/// An instance of <see cref="System.Object"/>.
		/// </summary>
		Object,

		/// <summary>
		/// A buffer.
		/// </summary>
		Buffer,

		/// <summary>
		/// An enumeration type.
		/// </summary>
		Enum,

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

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Boolean
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Boolean"/> value being <c>false</c>.
		/// </summary>
		BooleanFalse,

		/// <summary>
		/// A <see cref="System.Boolean"/> value being <c>true</c>.
		/// </summary>
		BooleanTrue,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Boolean"/> (native encoding).
		/// </summary>
		ArrayOfBoolean_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Boolean"/> (compact encoding)
		/// </summary>
		ArrayOfBoolean_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Boolean"/>.
		/// </summary>
		MultidimensionalArrayOfBoolean,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Char
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Char"/> value (native encoding).
		/// </summary>
		Char_Native,

		/// <summary>
		/// A <see cref="System.Char"/> value (LEB128 encoding).
		/// </summary>
		Char_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Char"/> (native encoding).
		/// </summary>
		ArrayOfChar_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Char"/> (compact encoding).
		/// </summary>
		ArrayOfChar_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Char"/>.
		/// </summary>
		MultidimensionalArrayOfChar,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.SByte
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.SByte"/> value.
		/// </summary>
		SByte,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.SByte"/>.
		/// </summary>
		ArrayOfSByte,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.SByte"/>.
		/// </summary>
		MultidimensionalArrayOfSByte,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Int16
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Int16"/> value (native encoding).
		/// </summary>
		Int16_Native,

		/// <summary>
		/// A <see cref="System.Int16"/> value (LEB128 encoding).
		/// </summary>
		Int16_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int16"/> (native encoding).
		/// </summary>
		ArrayOfInt16_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int16"/> (compact encoding).
		/// </summary>
		ArrayOfInt16_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int16"/>.
		/// </summary>
		MultidimensionalArrayOfInt16,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Int32
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Int32"/> value (native encoding).
		/// </summary>
		Int32_Native,

		/// <summary>
		/// A <see cref="System.Int32"/> value (LEB128 encoding).
		/// </summary>
		Int32_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int32"/> (native encoding).
		/// </summary>
		ArrayOfInt32_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int32"/> (compact encoding).
		/// </summary>
		ArrayOfInt32_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int32"/>.
		/// </summary>
		MultidimensionalArrayOfInt32,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Int64
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Int64"/> value (native encoding).
		/// </summary>
		Int64_Native,

		/// <summary>
		/// A <see cref="System.Int64"/> value (LEB128 encoding).
		/// </summary>
		Int64_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int64"/> (native encoding).
		/// </summary>
		ArrayOfInt64_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Int64"/> (compact encoding).
		/// </summary>
		ArrayOfInt64_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Int64"/>.
		/// </summary>
		MultidimensionalArrayOfInt64,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Byte
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Byte"/> value.
		/// </summary>
		Byte,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Byte"/>.
		/// </summary>
		ArrayOfByte,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Byte"/>.
		/// </summary>
		MultidimensionalArrayOfByte,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.UInt16
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.UInt16"/> value (native encoding).
		/// </summary>
		UInt16_Native,

		/// <summary>
		/// A <see cref="System.UInt16"/> value (LEB128 encoding).
		/// </summary>
		UInt16_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt16"/> (native encoding).
		/// </summary>
		ArrayOfUInt16_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt16"/> (compact encoding).
		/// </summary>
		ArrayOfUInt16_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt16"/>.
		/// </summary>
		MultidimensionalArrayOfUInt16,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.UInt32
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.UInt32"/> value (native encoding).
		/// </summary>
		UInt32_Native,

		/// <summary>
		/// A <see cref="System.UInt32"/> value (LEB128 encoding).
		/// </summary>
		UInt32_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt32"/> (native encoding).
		/// </summary>
		ArrayOfUInt32_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt32"/> (compact encoding).
		/// </summary>
		ArrayOfUInt32_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt32"/>.
		/// </summary>
		MultidimensionalArrayOfUInt32,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.UInt64
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.UInt64"/> value (native encoding).
		/// </summary>
		UInt64_Native,

		/// <summary>
		/// A <see cref="System.UInt64"/> value (LEB128 encoding).
		/// </summary>
		UInt64_LEB128,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt64"/> (native encoding).
		/// </summary>
		ArrayOfUInt64_Native,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.UInt64"/> (compact encoding).
		/// </summary>
		ArrayOfUInt64_Compact,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.UInt64"/>.
		/// </summary>
		MultidimensionalArrayOfUInt64,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Single
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Single"/> value.
		/// </summary>
		Single,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Single"/> (native encoding).
		/// </summary>
		ArrayOfSingle,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Single"/> (native encoding).
		/// </summary>
		MultidimensionalArrayOfSingle,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Double
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Double"/> value.
		/// </summary>
		Double,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Double"/> (native encoding).
		/// </summary>
		ArrayOfDouble,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Double"/> (native encoding).
		/// </summary>
		MultidimensionalArrayOfDouble,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Decimal
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Decimal"/> value.
		/// </summary>
		Decimal,

		/// <summary>
		/// An one-dimensional, zero-based array of <see cref="System.Decimal"/>.
		/// </summary>
		ArrayOfDecimal,

		/// <summary>
		/// A multi-dimensional array of <see cref="System.Decimal"/>.
		/// </summary>
		MultidimensionalArrayOfDecimal,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.String
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.String"/> value (UTF-8 encoding).
		/// </summary>
		String_UTF8,

		/// <summary>
		/// A <see cref="System.String"/> value (UTF-16 encoding, endianness depends on the system).
		/// </summary>
		String_UTF16,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.DateTime
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.DateTime"/> value.
		/// </summary>
		DateTime,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.DateTimeOffset
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.DateTimeOffset"/> value.
		/// </summary>
		DateTimeOffset,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// System.Guid
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// A <see cref="System.Guid"/> value.
		/// </summary>
		Guid,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Arrays of serializable objects
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// An one-dimensional, zero-based array of serializable objects.
		/// </summary>
		ArrayOfObjects,

		/// <summary>
		/// A multi-dimensional array of serializable objects.
		/// </summary>
		MultidimensionalArrayOfObjects,

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// End of the payload types
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Terminator (last enumeration value + 1).
		/// </summary>
		Terminator
	}

}
