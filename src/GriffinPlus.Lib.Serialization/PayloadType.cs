///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization
{

	enum PayloadType
	{
		// null reference
		NullReference,

		// a generic type (may be a generic type definition or a closed constructed generic type)
		GenericType,

		// a closed type
		Type,

		// reference to a previously serialized type
		TypeId,

		// serializer archive
		ArchiveStart,
		ArchiveEnd,
		BaseArchiveStart, // archives containing base class data do not have an end tag

		// buffer
		Buffer,

		// primitive Types
		Boolean,
		Char,
		SByte,
		Int16,
		Int32,
		Int64,
		Byte,
		UInt16,
		UInt32,
		UInt64,
		Single,
		Double,
		Decimal,
		String,
		DateTime,

		// one-dimensional arrays of primitive values
		ArrayOfBoolean,
		ArrayOfChar,
		ArrayOfSByte,
		ArrayOfInt16,
		ArrayOfInt32,
		ArrayOfInt64,
		ArrayOfByte,
		ArrayOfUInt16,
		ArrayOfUInt32,
		ArrayOfUInt64,
		ArrayOfSingle,
		ArrayOfDouble,
		ArrayOfDecimal,
		ArrayOfString,
		ArrayOfDateTime,

		// array of objects
		ArrayOfObjects,

		// object which was already serialized
		AlreadySerialized,

		// enum types
		Enum,

		// serializer archive (for generic types)
		GenericArchiveStart,

		// multidimensional arrays of primitive values
		MultidimensionalArrayOfBoolean,
		MultidimensionalArrayOfChar,
		MultidimensionalArrayOfSByte,
		MultidimensionalArrayOfInt16,
		MultidimensionalArrayOfInt32,
		MultidimensionalArrayOfInt64,
		MultidimensionalArrayOfByte,
		MultidimensionalArrayOfUInt16,
		MultidimensionalArrayOfUInt32,
		MultidimensionalArrayOfUInt64,
		MultidimensionalArrayOfSingle,
		MultidimensionalArrayOfDouble,
		MultidimensionalArrayOfDecimal,
		MultidimensionalArrayOfString,
		MultidimensionalArrayOfDateTime,

		// array of objects
		MultidimensionalArrayOfObjects,

		// type objects
		TypeObject,

		// a System.Object object
		Object
	}

}
