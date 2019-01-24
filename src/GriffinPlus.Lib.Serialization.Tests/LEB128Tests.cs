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

using System;
using Xunit;

namespace GriffinPlus.Lib.Serialization.Tests
{
	/// <summary>
	/// Tests for the <see cref="LEB128"/> utility class.
	/// </summary>
	public class LEB128Tests
	{
		private byte[] mBuffer = new byte[20];

		[Theory]
		[InlineData(unchecked((int)0xFFFFFFC0), 1)]
		[InlineData(unchecked((int)0xFFFFFFBF), 2)]
		[InlineData(unchecked((int)0xFFFFE000), 2)]
		[InlineData(unchecked((int)0xFFFFDFFF), 3)]
		[InlineData(unchecked((int)0xFFF00000), 3)]
		[InlineData(unchecked((int)0xFFEFFFFF), 4)]
		[InlineData(unchecked((int)0xF8000000), 4)]
		[InlineData(unchecked((int)0xF7FFFFFF), 5)]
		[InlineData(unchecked((int)0x0000003F), 1)]
		[InlineData(unchecked((int)0x00000040), 2)]
		[InlineData(unchecked((int)0x00001FFF), 2)]
		[InlineData(unchecked((int)0x00002000), 3)]
		[InlineData(unchecked((int)0x000FFFFF), 3)]
		[InlineData(unchecked((int)0x00100000), 4)]
		[InlineData(unchecked((int)0x07FFFFFF), 4)]
		[InlineData(unchecked((int)0x08000000), 5)]
		public void LEB128_Int32(int value, int byteCount)
		{
			int size;
			int count1 = LEB128.GetByteCount(value);
			int count2 = LEB128.Write(mBuffer, 0, value);
			int reverse = LEB128.ReadInt32(mBuffer, 0, count2, out size);

			Assert.Equal(byteCount, count1);
			Assert.Equal(byteCount, count2);
			Assert.Equal(byteCount, size);
			Assert.Equal(value, reverse);
		}

		[Theory]
		[InlineData(unchecked((uint)0x0000007F), 1)]
		[InlineData(unchecked((uint)0x00000080), 2)]
		[InlineData(unchecked((uint)0x00003FFF), 2)]
		[InlineData(unchecked((uint)0x00004000), 3)]
		[InlineData(unchecked((uint)0x001FFFFF), 3)]
		[InlineData(unchecked((uint)0x00200000), 4)]
		[InlineData(unchecked((uint)0x0FFFFFFF), 4)]
		[InlineData(unchecked((uint)0x10000000), 5)]
		public void LEB128_UInt32(uint value, int byteCount)
		{
			int size;
			int count1 = LEB128.GetByteCount(value);
			int count2 = LEB128.Write(mBuffer, 0, value);
			uint reverse = LEB128.ReadUInt32(mBuffer, 0, count2, out size);

			Assert.Equal(byteCount, count1);
			Assert.Equal(byteCount, count2);
			Assert.Equal(byteCount, size);
			Assert.Equal(value, reverse);
		}

		[Theory]
		[InlineData(unchecked((long)0xFFFFFFFFFFFFFFC0),  1)]
		[InlineData(unchecked((long)0xFFFFFFFFFFFFFFBF),  2)]
		[InlineData(unchecked((long)0xFFFFFFFFFFFFE000),  2)]
		[InlineData(unchecked((long)0xFFFFFFFFFFFFDFFF),  3)]
		[InlineData(unchecked((long)0xFFFFFFFFFFF00000),  3)]
		[InlineData(unchecked((long)0xFFFFFFFFFFEFFFFF),  4)]
		[InlineData(unchecked((long)0xFFFFFFFFF8000000),  4)]
		[InlineData(unchecked((long)0xFFFFFFFFF7FFFFFF),  5)]
		[InlineData(unchecked((long)0xFFFFFFFC00000000),  5)]
		[InlineData(unchecked((long)0xFFFFFFFBFFFFFFFF),  6)]
		[InlineData(unchecked((long)0xFFFFFE0000000000),  6)]
		[InlineData(unchecked((long)0xFFFFFDFFFFFFFFFF),  7)]
		[InlineData(unchecked((long)0xFFFF000000000000),  7)]
		[InlineData(unchecked((long)0xFFFEFFFFFFFFFFFF),  8)]
		[InlineData(unchecked((long)0xFF80000000000000),  8)]
		[InlineData(unchecked((long)0xFF7FFFFFFFFFFFFF),  9)]
		[InlineData(unchecked((long)0xC000000000000000),  9)]
		[InlineData(unchecked((long)0xBFFFFFFFFFFFFFFF), 10)]
		[InlineData(unchecked((long)0x000000000000003F),  1)]
		[InlineData(unchecked((long)0x0000000000000040),  2)]
		[InlineData(unchecked((long)0x0000000000001FFF),  2)]
		[InlineData(unchecked((long)0x0000000000002000),  3)]
		[InlineData(unchecked((long)0x00000000000FFFFF),  3)]
		[InlineData(unchecked((long)0x0000000000100000),  4)]
		[InlineData(unchecked((long)0x0000000007FFFFFF),  4)]
		[InlineData(unchecked((long)0x0000000008000000),  5)]
		[InlineData(unchecked((long)0x00000003FFFFFFFF),  5)]
		[InlineData(unchecked((long)0x0000000400000000),  6)]
		[InlineData(unchecked((long)0x000001FFFFFFFFFF),  6)]
		[InlineData(unchecked((long)0x0000020000000000),  7)]
		[InlineData(unchecked((long)0x0000FFFFFFFFFFFF),  7)]
		[InlineData(unchecked((long)0x0001000000000000),  8)]
		[InlineData(unchecked((long)0x007FFFFFFFFFFFFF),  8)]
		[InlineData(unchecked((long)0x0080000000000000),  9)]
		[InlineData(unchecked((long)0x3FFFFFFFFFFFFFFF),  9)]
		[InlineData(unchecked((long)0x4000000000000000), 10)]
		public void LEB128_Int64(long value, int byteCount)
		{
			int size;
			int count1 = LEB128.GetByteCount(value);
			int count2 = LEB128.Write(mBuffer, 0, value);
			long reverse = LEB128.ReadInt64(mBuffer, 0, count2, out size);

			Assert.Equal(byteCount, count1);
			Assert.Equal(byteCount, count2);
			Assert.Equal(byteCount, size);
			Assert.Equal(value, reverse);
		}

		[Theory]
		[InlineData(unchecked((ulong)0x000000000000007F),  1)]
		[InlineData(unchecked((ulong)0x0000000000000080),  2)]
		[InlineData(unchecked((ulong)0x0000000000003FFF),  2)]
		[InlineData(unchecked((ulong)0x0000000000004000),  3)]
		[InlineData(unchecked((ulong)0x00000000001FFFFF),  3)]
		[InlineData(unchecked((ulong)0x0000000000200000),  4)]
		[InlineData(unchecked((ulong)0x000000000FFFFFFF),  4)]
		[InlineData(unchecked((ulong)0x0000000010000000),  5)]
		[InlineData(unchecked((ulong)0x00000007FFFFFFFF),  5)]
		[InlineData(unchecked((ulong)0x0000000800000000),  6)]
		[InlineData(unchecked((ulong)0x000003FFFFFFFFFF),  6)]
		[InlineData(unchecked((ulong)0x0000040000000000),  7)]
		[InlineData(unchecked((ulong)0x0001FFFFFFFFFFFF),  7)]
		[InlineData(unchecked((ulong)0x0002000000000000),  8)]
		[InlineData(unchecked((ulong)0x00FFFFFFFFFFFFFF),  8)]
		[InlineData(unchecked((ulong)0x0100000000000000),  9)]
		[InlineData(unchecked((ulong)0x7FFFFFFFFFFFFFFF),  9)]
		[InlineData(unchecked((ulong)0x8000000000000000), 10)]
		public void LEB128_UInt64(ulong value, int byteCount)
		{
			int size;
			int count1 = LEB128.GetByteCount(value);
			int count2 = LEB128.Write(mBuffer, 0, value);
			ulong reverse = LEB128.ReadUInt64(mBuffer, 0, count2, out size);

			Assert.Equal(byteCount, count1);
			Assert.Equal(byteCount, count2);
			Assert.Equal(byteCount, size);
			Assert.Equal(value, reverse);
		}

	}
}
