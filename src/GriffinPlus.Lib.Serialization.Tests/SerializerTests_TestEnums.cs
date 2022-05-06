///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		public enum TestEnum_S8 : sbyte
		{
			A = SByte.MinValue,
			B = 0,
			C = SByte.MaxValue
		}

		public enum TestEnum_U8 : byte
		{
			A = Byte.MinValue,
			B = Byte.MaxValue / 2,
			C = Byte.MaxValue
		}

		public enum TestEnum_S16 : short
		{
			A = Int16.MinValue,
			B = 0,
			C = Int16.MaxValue
		}

		public enum TestEnum_U16 : ushort
		{
			A = UInt16.MinValue,
			B = UInt16.MaxValue / 2,
			C = UInt16.MaxValue
		}

		public enum TestEnum_S32
		{
			A = Int32.MinValue,
			B = 0,
			C = Int32.MaxValue
		}

		public enum TestEnum_U32 : uint
		{
			A = UInt32.MinValue,
			B = UInt32.MaxValue / 2,
			C = UInt32.MaxValue
		}

		public enum TestEnum_S64 : long
		{
			A = Int64.MinValue,
			B = 0,
			C = Int64.MaxValue
		}

		public enum TestEnum_U64 : ulong
		{
			A = UInt64.MinValue,
			B = UInt64.MaxValue / 2,
			C = UInt64.MaxValue
		}
	}

}
