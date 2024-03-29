﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Xunit;

namespace GriffinPlus.Lib.Serialization.Tests;

/// <summary>
/// Tests for the <see cref="Leb128EncodingHelper"/> utility class.
/// </summary>
public class Leb128EncodingHelperTests
{
	[Theory]
	[InlineData(unchecked((int)0xFFFFFFFF), 1)]
	[InlineData(unchecked((int)0xFFFFFFC0), 1)]
	[InlineData(unchecked((int)0xFFFFFFBF), 2)]
	[InlineData(unchecked((int)0xFFFFE000), 2)]
	[InlineData(unchecked((int)0xFFFFDFFF), 3)]
	[InlineData(unchecked((int)0xFFF00000), 3)]
	[InlineData(unchecked((int)0xFFEFFFFF), 4)]
	[InlineData(unchecked((int)0xF8000000), 4)]
	[InlineData(unchecked((int)0xF7FFFFFF), 5)]
	[InlineData(unchecked((int)0x80000000), 5)]
	[InlineData(0x0000003F, 1)]
	[InlineData(0x00000040, 2)]
	[InlineData(0x00001FFF, 2)]
	[InlineData(0x00002000, 3)]
	[InlineData(0x000FFFFF, 3)]
	[InlineData(0x00100000, 4)]
	[InlineData(0x07FFFFFF, 4)]
	[InlineData(0x08000000, 5)]
	[InlineData(0x7FFFFFFF, 5)]
	public void LEB128_Int32(int value, int byteCount)
	{
		// determine the number of bytes the encoding will take
		int encodedByteCount = Leb128EncodingHelper.GetByteCount(value);
		Assert.Equal(byteCount, encodedByteCount);

		// write to array
		byte[] buffer_array = new byte[encodedByteCount];
		int count_array = Leb128EncodingHelper.Write(buffer_array, 0, value);
		Assert.Equal(byteCount, count_array);

		// write to span
		byte[] buffer_span = new byte[encodedByteCount];
		int count_span = Leb128EncodingHelper.Write(buffer_span.AsSpan(), value);
		Assert.Equal(byteCount, count_span);

		// read from array
		int reverse_array = Leb128EncodingHelper.ReadInt32(buffer_array, 0, count_array, out int size_array);
		Assert.Equal(byteCount, size_array);
		Assert.Equal(value, reverse_array);

		// read from stream
		Stream ms = new MemoryStream(buffer_array);
		int reverse_stream = Leb128EncodingHelper.ReadInt32(ms);
		Assert.Equal(byteCount, ms.Position);
		Assert.Equal(value, reverse_stream);
	}

	[Theory]
	[InlineData((uint)0x00000000, 1)]
	[InlineData((uint)0x0000007F, 1)]
	[InlineData((uint)0x00000080, 2)]
	[InlineData((uint)0x00003FFF, 2)]
	[InlineData((uint)0x00004000, 3)]
	[InlineData((uint)0x001FFFFF, 3)]
	[InlineData((uint)0x00200000, 4)]
	[InlineData((uint)0x0FFFFFFF, 4)]
	[InlineData((uint)0x10000000, 5)]
	[InlineData(0xFFFFFFFF, 5)]
	public void LEB128_UInt32(uint value, int byteCount)
	{
		// determine the number of bytes the encoding will take
		int encodedByteCount = Leb128EncodingHelper.GetByteCount(value);
		Assert.Equal(byteCount, encodedByteCount);

		// write to array
		byte[] buffer_array = new byte[encodedByteCount];
		int count_array = Leb128EncodingHelper.Write(buffer_array, 0, value);
		Assert.Equal(byteCount, count_array);

		// write to span
		byte[] buffer_span = new byte[encodedByteCount];
		int count_span = Leb128EncodingHelper.Write(buffer_span.AsSpan(), value);
		Assert.Equal(byteCount, count_span);

		// read from array
		uint reverse_array = Leb128EncodingHelper.ReadUInt32(buffer_array, 0, count_array, out int size_array);
		Assert.Equal(byteCount, size_array);
		Assert.Equal(value, reverse_array);

		// read from stream
		Stream ms = new MemoryStream(buffer_array);
		uint reverse_stream = Leb128EncodingHelper.ReadUInt32(ms);
		Assert.Equal(byteCount, ms.Position);
		Assert.Equal(value, reverse_stream);
	}

	[Theory]
	[InlineData(unchecked((long)0xFFFFFFFFFFFFFFFF), 1)]
	[InlineData(unchecked((long)0xFFFFFFFFFFFFFFC0), 1)]
	[InlineData(unchecked((long)0xFFFFFFFFFFFFFFBF), 2)]
	[InlineData(unchecked((long)0xFFFFFFFFFFFFE000), 2)]
	[InlineData(unchecked((long)0xFFFFFFFFFFFFDFFF), 3)]
	[InlineData(unchecked((long)0xFFFFFFFFFFF00000), 3)]
	[InlineData(unchecked((long)0xFFFFFFFFFFEFFFFF), 4)]
	[InlineData(unchecked((long)0xFFFFFFFFF8000000), 4)]
	[InlineData(unchecked((long)0xFFFFFFFFF7FFFFFF), 5)]
	[InlineData(unchecked((long)0xFFFFFFFC00000000), 5)]
	[InlineData(unchecked((long)0xFFFFFFFBFFFFFFFF), 6)]
	[InlineData(unchecked((long)0xFFFFFE0000000000), 6)]
	[InlineData(unchecked((long)0xFFFFFDFFFFFFFFFF), 7)]
	[InlineData(unchecked((long)0xFFFF000000000000), 7)]
	[InlineData(unchecked((long)0xFFFEFFFFFFFFFFFF), 8)]
	[InlineData(unchecked((long)0xFF80000000000000), 8)]
	[InlineData(unchecked((long)0xFF7FFFFFFFFFFFFF), 9)]
	[InlineData(unchecked((long)0xC000000000000000), 9)]
	[InlineData(unchecked((long)0xBFFFFFFFFFFFFFFF), 10)]
	[InlineData(unchecked((long)0x8000000000000000), 10)]
	[InlineData((long)0x0000000000000000, 1)]
	[InlineData((long)0x000000000000003F, 1)]
	[InlineData((long)0x0000000000000040, 2)]
	[InlineData((long)0x0000000000001FFF, 2)]
	[InlineData((long)0x0000000000002000, 3)]
	[InlineData((long)0x00000000000FFFFF, 3)]
	[InlineData((long)0x0000000000100000, 4)]
	[InlineData((long)0x0000000007FFFFFF, 4)]
	[InlineData((long)0x0000000008000000, 5)]
	[InlineData(0x00000003FFFFFFFF, 5)]
	[InlineData(0x0000000400000000, 6)]
	[InlineData(0x000001FFFFFFFFFF, 6)]
	[InlineData(0x0000020000000000, 7)]
	[InlineData(0x0000FFFFFFFFFFFF, 7)]
	[InlineData(0x0001000000000000, 8)]
	[InlineData(0x007FFFFFFFFFFFFF, 8)]
	[InlineData(0x0080000000000000, 9)]
	[InlineData(0x3FFFFFFFFFFFFFFF, 9)]
	[InlineData(0x4000000000000000, 10)]
	[InlineData(0x7fffffffffffffff, 10)]
	public void LEB128_Int64(long value, int byteCount)
	{
		// determine the number of bytes the encoding will take
		int encodedByteCount = Leb128EncodingHelper.GetByteCount(value);
		Assert.Equal(byteCount, encodedByteCount);

		// write to array
		byte[] buffer_array = new byte[encodedByteCount];
		int count_array = Leb128EncodingHelper.Write(buffer_array, 0, value);
		Assert.Equal(byteCount, count_array);

		// write to span
		byte[] buffer_span = new byte[encodedByteCount];
		int count_span = Leb128EncodingHelper.Write(buffer_span.AsSpan(), value);
		Assert.Equal(byteCount, count_span);

		// read from array
		long reverse_array = Leb128EncodingHelper.ReadInt64(buffer_array, 0, count_array, out int size_array);
		Assert.Equal(byteCount, size_array);
		Assert.Equal(value, reverse_array);

		// read from stream
		Stream ms = new MemoryStream(buffer_array);
		long reverse_stream = Leb128EncodingHelper.ReadInt64(ms);
		Assert.Equal(byteCount, ms.Position);
		Assert.Equal(value, reverse_stream);
	}

	[Theory]
	[InlineData((ulong)0x0000000000000000, 1)]
	[InlineData((ulong)0x000000000000007F, 1)]
	[InlineData((ulong)0x0000000000000080, 2)]
	[InlineData((ulong)0x0000000000003FFF, 2)]
	[InlineData((ulong)0x0000000000004000, 3)]
	[InlineData((ulong)0x00000000001FFFFF, 3)]
	[InlineData((ulong)0x0000000000200000, 4)]
	[InlineData((ulong)0x000000000FFFFFFF, 4)]
	[InlineData((ulong)0x0000000010000000, 5)]
	[InlineData((ulong)0x00000007FFFFFFFF, 5)]
	[InlineData((ulong)0x0000000800000000, 6)]
	[InlineData((ulong)0x000003FFFFFFFFFF, 6)]
	[InlineData((ulong)0x0000040000000000, 7)]
	[InlineData((ulong)0x0001FFFFFFFFFFFF, 7)]
	[InlineData((ulong)0x0002000000000000, 8)]
	[InlineData((ulong)0x00FFFFFFFFFFFFFF, 8)]
	[InlineData((ulong)0x0100000000000000, 9)]
	[InlineData((ulong)0x7FFFFFFFFFFFFFFF, 9)]
	[InlineData(0x8000000000000000, 10)]
	[InlineData(0xFFFFFFFFFFFFFFFF, 10)]
	public void LEB128_UInt64(ulong value, int byteCount)
	{
		// determine the number of bytes the encoding will take
		int encodedByteCount = Leb128EncodingHelper.GetByteCount(value);
		Assert.Equal(byteCount, encodedByteCount);

		// write to array
		byte[] buffer_array = new byte[encodedByteCount];
		int count_array = Leb128EncodingHelper.Write(buffer_array, 0, value);
		Assert.Equal(byteCount, count_array);

		// write to span
		byte[] buffer_span = new byte[encodedByteCount];
		int count_span = Leb128EncodingHelper.Write(buffer_span.AsSpan(), value);
		Assert.Equal(byteCount, count_span);

		// read from array
		ulong reverse_array = Leb128EncodingHelper.ReadUInt64(buffer_array, 0, count_array, out int size_array);
		Assert.Equal(byteCount, size_array);
		Assert.Equal(value, reverse_array);

		// read from stream
		Stream ms = new MemoryStream(buffer_array);
		ulong reverse_stream = Leb128EncodingHelper.ReadUInt64(ms);
		Assert.Equal(byteCount, ms.Position);
		Assert.Equal(value, reverse_stream);
	}
}
