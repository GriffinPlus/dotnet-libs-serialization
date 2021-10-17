///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		public enum TestEnum_S8 : sbyte
		{
			A = sbyte.MinValue,
			B = 0,
			C = sbyte.MaxValue
		}

		public enum TestEnum_U8 : byte
		{
			A = byte.MinValue,
			B = byte.MaxValue / 2,
			C = byte.MaxValue
		}

		public enum TestEnum_S16 : short
		{
			A = short.MinValue,
			B = 0,
			C = short.MaxValue
		}

		public enum TestEnum_U16 : ushort
		{
			A = ushort.MinValue,
			B = ushort.MaxValue / 2,
			C = ushort.MaxValue
		}

		public enum TestEnum_S32
		{
			A = int.MinValue,
			B = 0,
			C = int.MaxValue
		}

		public enum TestEnum_U32 : uint
		{
			A = uint.MinValue,
			B = uint.MaxValue / 2,
			C = uint.MaxValue
		}

		public enum TestEnum_S64 : long
		{
			A = long.MinValue,
			B = 0,
			C = long.MaxValue
		}

		public enum TestEnum_U64 : ulong
		{
			A = ulong.MinValue,
			B = ulong.MaxValue / 2,
			C = ulong.MaxValue
		}
	}

}
