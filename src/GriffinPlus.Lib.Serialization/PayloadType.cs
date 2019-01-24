///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
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
		ArchiveStart, ArchiveEnd,
		BaseArchiveStart, // archives containing base class data do not have an end tag

		// buffer
		Buffer,

		// primitive Types
		Boolean, Byte, Char, Decimal, Single, Double, Int16, Int32,
		Int64, SByte, UInt16, UInt32, UInt64, DateTime, String,

		// onedimensional arrays of primitive values
		ArrayOfBoolean, ArrayOfByte, ArrayOfChar, ArrayOfDecimal, ArrayOfSingle,
		ArrayOfDouble, ArrayOfInt16, ArrayOfInt32, ArrayOfInt64, ArrayOfSByte,
		ArrayOfUInt16, ArrayOfUInt32, ArrayOfUInt64, ArrayOfDateTime, ArrayOfString,
		
		// array of objects
		ArrayOfObjects,

		// object which was already serialized
		AlreadySerialized,

		// enum types
		Enum,

		// serializer archive (for generic types)
		GenericArchiveStart,

		// multidimensional arrays of primitive values
		MultidimensionalArrayOfBoolean, MultidimensionalArrayOfByte, MultidimensionalArrayOfChar, MultidimensionalArrayOfDecimal, MultidimensionalArrayOfSingle,
		MultidimensionalArrayOfDouble, MultidimensionalArrayOfInt16, MultidimensionalArrayOfInt32, MultidimensionalArrayOfInt64, MultidimensionalArrayOfSByte,
		MultidimensionalArrayOfUInt16, MultidimensionalArrayOfUInt32, MultidimensionalArrayOfUInt64, MultidimensionalArrayOfDateTime, MultidimensionalArrayOfString,

		// array of objects
		MultidimensionalArrayOfObjects,

		// type objects
		TypeObject,

		// a System.Object object
		Object
	}
}
