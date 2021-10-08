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

		// type metadata.
		Type,

		// reference to previously serialized type metadata
		TypeId,

		// serializer archive
		ArchiveStart,
		ArchiveEnd,
		BaseArchiveStart, // archives containing base class data do not have an end tag

		// buffer
		Buffer,

		// primitive Types
		Boolean,
		Byte,
		Char,
		Decimal,
		Single,
		Double,
		Int16,
		Int32,
		Int64,
		SByte,
		UInt16,
		UInt32,
		UInt64,
		DateTime,
		String,

		// onedimensional arrays of primitive values
		ArrayOfBoolean,
		ArrayOfByte,
		ArrayOfChar,
		ArrayOfDecimal,
		ArrayOfSingle,
		ArrayOfDouble,
		ArrayOfInt16,
		ArrayOfInt32,
		ArrayOfInt64,
		ArrayOfSByte,
		ArrayOfUInt16,
		ArrayOfUInt32,
		ArrayOfUInt64,
		ArrayOfDateTime,
		ArrayOfString,

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
		MultidimensionalArrayOfByte,
		MultidimensionalArrayOfChar,
		MultidimensionalArrayOfDecimal,
		MultidimensionalArrayOfSingle,
		MultidimensionalArrayOfDouble,
		MultidimensionalArrayOfInt16,
		MultidimensionalArrayOfInt32,
		MultidimensionalArrayOfInt64,
		MultidimensionalArrayOfSByte,
		MultidimensionalArrayOfUInt16,
		MultidimensionalArrayOfUInt32,
		MultidimensionalArrayOfUInt64,
		MultidimensionalArrayOfDateTime,
		MultidimensionalArrayOfString,

		// array of objects
		MultidimensionalArrayOfObjects,

		// type objects
		TypeObject,

		// a System.Object object
		Object
	}

}
