///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Diagnostics;
#endif

namespace GriffinPlus.Lib.Serialization;

partial class Serializer
{
	#region System.Boolean

	/// <summary>
	/// Writes a <see cref="System.Boolean"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Boolean(bool value, IBufferWriter<byte> writer)
	{
		Span<byte> buffer = writer.GetSpan(1);
		buffer[0] = (byte)(value ? PayloadType.BooleanTrue : PayloadType.BooleanFalse);
		writer.Advance(1);
	}

	#endregion

	#region System.Char

	/// <summary>
	/// Writes a <see cref="System.Char"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Char(char value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(3);
			buffer[0] = (byte)PayloadType.Char_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(3);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.Char_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], (uint)value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.Char"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal char ReadPrimitive_Char_Native(Stream stream)
	{
		const int bytesToRead = 2;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		char value = MemoryMarshal.Read<char>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.Char"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal char ReadPrimitive_Char_LEB128(Stream stream)
	{
		return (char)Leb128EncodingHelper.ReadUInt32(stream);
	}

	#endregion

	#region System.Decimal

	/// <summary>
	/// Writes a <see cref="System.Decimal"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Decimal(decimal value, IBufferWriter<byte> writer)
	{
		const int elementSize = sizeof(decimal);
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.Decimal;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
		int[] bits = decimal.GetBits(value);
		Buffer.BlockCopy(bits, 0, TempBuffer_Buffer, 0, elementSize);
		TempBuffer_Buffer.AsSpan(0, elementSize).CopyTo(buffer[1..]);
#elif NET5_0_OR_GREATER
		Span<int> temp = stackalloc int[4];
		decimal.TryGetBits(value, temp, out int valuesWritten);
		Debug.Assert(valuesWritten * sizeof(int) == elementSize);
		MemoryMarshal.Cast<int, byte>(temp).CopyTo(buffer[1..]);
#else
#error Unhandled .NET framework
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.Decimal"/> value.
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal decimal ReadPrimitive_Decimal(Stream stream)
	{
		const int elementSize = sizeof(decimal);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
		Buffer.BlockCopy(TempBuffer_Buffer, 0, TempBuffer_Int32, 0, elementSize);

		if (IsDeserializingLittleEndian == BitConverter.IsLittleEndian)
			return new decimal(TempBuffer_Int32);

		EndiannessHelper.SwapBytes(ref TempBuffer_Int32[0]);
		EndiannessHelper.SwapBytes(ref TempBuffer_Int32[1]);
		EndiannessHelper.SwapBytes(ref TempBuffer_Int32[2]);
		EndiannessHelper.SwapBytes(ref TempBuffer_Int32[3]);

		return new decimal(TempBuffer_Int32);
#elif NET5_0_OR_GREATER
		Span<int> intBuffer = MemoryMarshal.Cast<byte, int>(TempBuffer_Buffer.AsSpan(0, elementSize));

		if (IsDeserializingLittleEndian == BitConverter.IsLittleEndian)
			return new decimal(intBuffer);

		EndiannessHelper.SwapBytes(ref intBuffer[0]);
		EndiannessHelper.SwapBytes(ref intBuffer[1]);
		EndiannessHelper.SwapBytes(ref intBuffer[2]);
		EndiannessHelper.SwapBytes(ref intBuffer[3]);

		return new decimal(intBuffer);
#else
#error Unhandled .NET framework
#endif
	}

	#endregion

	#region System.Single

	/// <summary>
	/// Writes a <see cref="System.Single"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Single(float value, IBufferWriter<byte> writer)
	{
		const int elementSize = sizeof(float);
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.Single;
#if NET8_0_OR_GREATER
		MemoryMarshal.Write(buffer[1..], in value);
#else
		MemoryMarshal.Write(buffer[1..], ref value);
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.Single"/> value.
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal float ReadPrimitive_Single(Stream stream)
	{
		const int elementSize = sizeof(float);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		float value = MemoryMarshal.Read<float>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	#endregion

	#region System.Double

	/// <summary>
	/// Writes a <see cref="System.Double"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Double(double value, IBufferWriter<byte> writer)
	{
		const int elementSize = sizeof(double);
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.Double;
#if NET8_0_OR_GREATER
		MemoryMarshal.Write(buffer[1..], in value);
#else
		MemoryMarshal.Write(buffer[1..], ref value);
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.Double"/> value.
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal double ReadPrimitive_Double(Stream stream)
	{
		const int elementSize = sizeof(double);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		double value = MemoryMarshal.Read<double>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	#endregion

	#region System.SByte

	/// <summary>
	/// Writes a <see cref="System.SByte"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_SByte(sbyte value, IBufferWriter<byte> writer)
	{
		Span<byte> buffer = writer.GetSpan(2);

		unchecked
		{
			buffer[0] = (byte)PayloadType.SByte;
			buffer[1] = (byte)value;
		}

		writer.Advance(2);
	}

	/// <summary>
	/// Reads a <see cref="System.SByte"/> value.
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal sbyte ReadPrimitive_SByte(Stream stream)
	{
		int readByte = stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
		unchecked { return (sbyte)(byte)readByte; }
	}

	#endregion

	#region System.Int16

	/// <summary>
	/// Writes a <see cref="System.Int16"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Int16(short value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(3);
			buffer[0] = (byte)PayloadType.Int16_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(3);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.Int16_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.Int16"/> value (native encoding)
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal short ReadPrimitive_Int16_Native(Stream stream)
	{
		const int bytesToRead = 2;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		short value = MemoryMarshal.Read<short>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.Int16"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal short ReadPrimitive_Int16_LEB128(Stream stream)
	{
		return (short)Leb128EncodingHelper.ReadInt32(stream);
	}

	#endregion

	#region System.Int32

	/// <summary>
	/// Writes a <see cref="System.Int32"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Int32(int value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(5);
			buffer[0] = (byte)PayloadType.Int32_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(5);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.Int32_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.Int32"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal int ReadPrimitive_Int32_Native(Stream stream)
	{
		const int bytesToRead = 4;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		int value = MemoryMarshal.Read<int>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.Int32"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal int ReadPrimitive_Int32_LEB128(Stream stream)
	{
		return Leb128EncodingHelper.ReadInt32(stream);
	}

	#endregion

	#region System.Int64

	/// <summary>
	/// Writes a <see cref="System.Int64"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Int64(long value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(9);
			buffer[0] = (byte)PayloadType.Int64_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(9);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
			buffer[0] = (byte)PayloadType.Int64_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.Int64"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal long ReadPrimitive_Int64_Native(Stream stream)
	{
		const int bytesToRead = 8;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		long value = MemoryMarshal.Read<long>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.Int64"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal long ReadPrimitive_Int64_LEB128(Stream stream)
	{
		return Leb128EncodingHelper.ReadInt64(stream);
	}

	#endregion

	#region System.Byte

	/// <summary>
	/// Writes a <see cref="System.Byte"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_Byte(byte value, IBufferWriter<byte> writer)
	{
		Span<byte> buffer = writer.GetSpan(2);
		buffer[0] = (byte)PayloadType.Byte;
		buffer[1] = value;
		writer.Advance(2);
	}

	/// <summary>
	/// Reads a <see cref="System.Byte"/> value.
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal byte ReadPrimitive_Byte(Stream stream)
	{
		int readByte = stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
		return (byte)readByte;
	}

	#endregion

	#region System.UInt16

	/// <summary>
	/// Writes a <see cref="System.UInt16"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_UInt16(ushort value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(3);
			buffer[0] = (byte)PayloadType.UInt16_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(3);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.UInt16_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], (uint)value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.UInt16"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal ushort ReadPrimitive_UInt16_Native(Stream stream)
	{
		const int bytesToRead = 2;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		ushort value = MemoryMarshal.Read<ushort>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.UInt16"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal ushort ReadPrimitive_UInt16_LEB128(Stream stream)
	{
		return (ushort)Leb128EncodingHelper.ReadUInt32(stream);
	}

	#endregion

	#region System.UInt32

	/// <summary>
	/// Writes a <see cref="System.UInt32"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_UInt32(uint value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(5);
			buffer[0] = (byte)PayloadType.UInt32_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(5);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.UInt32_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.UInt32"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal uint ReadPrimitive_UInt32_Native(Stream stream)
	{
		const int bytesToRead = 4;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		uint value = MemoryMarshal.Read<uint>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.UInt32"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal uint ReadPrimitive_UInt32_LEB128(Stream stream)
	{
		return Leb128EncodingHelper.ReadUInt32(stream);
	}

	#endregion

	#region System.UInt64

	/// <summary>
	/// Writes a <see cref="System.UInt64"/> value.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="writer">Buffer writer to write the value to.</param>
	internal void WritePrimitive_UInt64(ulong value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(value))
		{
			// use native encoding
			Span<byte> buffer = writer.GetSpan(9);
			buffer[0] = (byte)PayloadType.UInt64_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in value);
#else
			MemoryMarshal.Write(buffer[1..], ref value);
#endif
			writer.Advance(9);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
			buffer[0] = (byte)PayloadType.UInt64_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], value);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.UInt64"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal ulong ReadPrimitive_UInt64_Native(Stream stream)
	{
		const int bytesToRead = 8;
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
		if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
		ulong value = MemoryMarshal.Read<ulong>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return value;
	}

	/// <summary>
	/// Reads a <see cref="System.UInt64"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal ulong ReadPrimitive_UInt64_LEB128(Stream stream)
	{
		return Leb128EncodingHelper.ReadUInt64(stream);
	}

	#endregion

	#region System.DateTime

	/// <summary>
	/// Writes a <see cref="System.DateTime"/> object.
	/// </summary>
	/// <param name="value">DateTime object to write.</param>
	/// <param name="writer">Buffer writer to write the <see cref="System.DateTime"/> object to.</param>
	internal void WritePrimitive_DateTime(DateTime value, IBufferWriter<byte> writer)
	{
		// always use native encoding as the serialized value encodes both ticks and
		// datetime kind always resulting in a value that is too great to be encoded using
		// LEB128 with 7 bytes or fewer
		const int elementSize = sizeof(long); // binary representation of a DateTime
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.DateTime;
		long binaryValue = value.ToBinary();
#if NET8_0_OR_GREATER
		MemoryMarshal.Write(buffer[1..], in binaryValue);
#else
		MemoryMarshal.Write(buffer[1..], ref binaryValue);
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.DateTime"/> object.
	/// </summary>
	/// <param name="stream">Stream to read the DateTime object from.</param>
	/// <returns>The read <see cref="System.DateTime"/> object.</returns>
	internal DateTime ReadPrimitive_DateTime(Stream stream)
	{
		const int elementSize = sizeof(long); // binary representation of a DateTime
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		long value = MemoryMarshal.Read<long>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return DateTime.FromBinary(value);
	}

	#endregion

	#region System.DateTimeOffset

	/// <summary>
	/// Writes a <see cref="System.DateTimeOffset"/> object.
	/// </summary>
	/// <param name="value">DateTimeOffset object to write.</param>
	/// <param name="writer">Buffer writer to write the <see cref="System.DateTimeOffset"/> object to.</param>
	internal void WritePrimitive_DateTimeOffset(DateTimeOffset value, IBufferWriter<byte> writer)
	{
		// always use native encoding as the serialized ticks are usually too great to be encoded using
		// LEB128 with 7 bytes or fewer (using LEB128 encoding for the timezone offset could save some bytes,
		// but the benefit is marginal and does not outweigh the overhead that comes with it)
		const int elementSize = 2 * sizeof(long);
		long dateTimeTicks = value.Ticks;
		long timezoneOffsetTicks = value.Offset.Ticks;
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.DateTimeOffset;
#if NET8_0_OR_GREATER
		MemoryMarshal.Write(buffer[1..], in dateTimeTicks);
		MemoryMarshal.Write(buffer[9..], in timezoneOffsetTicks);
#else
		MemoryMarshal.Write(buffer[1..], ref dateTimeTicks);
		MemoryMarshal.Write(buffer[9..], ref timezoneOffsetTicks);
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.DateTimeOffset"/> object.
	/// </summary>
	/// <param name="stream">Stream to read the DateTimeOffset object from.</param>
	/// <returns>The read <see cref="System.DateTimeOffset"/> object.</returns>
	internal DateTimeOffset ReadPrimitive_DateTimeOffset(Stream stream)
	{
		const int elementSize = 2 * sizeof(long);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		long dateTimeTicks = MemoryMarshal.Read<long>(TempBuffer_Buffer);
		long timezoneOffsetTicks = MemoryMarshal.Read<long>(TempBuffer_Buffer.AsSpan()[8..]);

		// ReSharper disable once InvertIf
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
		{
			EndiannessHelper.SwapBytes(ref dateTimeTicks);
			EndiannessHelper.SwapBytes(ref timezoneOffsetTicks);
		}

		return new DateTimeOffset(dateTimeTicks, new TimeSpan(timezoneOffsetTicks));
	}

	#endregion

	#region System.DateOnly (.NET 6+ only)

#if NET6_0_OR_GREATER
	/// <summary>
	/// Writes a <see cref="System.DateOnly"/> object.
	/// </summary>
	/// <param name="value">Object to write.</param>
	/// <param name="writer">Buffer writer to write the object to.</param>
	internal void WritePrimitive_DateOnly(DateOnly value, IBufferWriter<byte> writer)
	{
		int dayNumber = value.DayNumber;
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(dayNumber))
		{
			// use native encoding
			const int elementSize = sizeof(int);
			Span<byte> buffer = writer.GetSpan(1 + elementSize);
			buffer[0] = (byte)PayloadType.DateOnly_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in dayNumber);
#else
			MemoryMarshal.Write(buffer[1..], ref dayNumber);
#endif
			writer.Advance(1 + elementSize);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer[0] = (byte)PayloadType.DateOnly_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], dayNumber);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.DateOnly"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal DateOnly ReadPrimitive_DateOnly_Native(Stream stream)
	{
		const int elementSize = sizeof(int); // binary representation of a DateOnly
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		int value = MemoryMarshal.Read<int>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref value);
		return DateOnly.FromDayNumber(value);
	}

	/// <summary>
	/// Reads a <see cref="System.DateOnly"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal DateOnly ReadPrimitive_DateOnly_LEB128(Stream stream)
	{
		int dayNumber = Leb128EncodingHelper.ReadInt32(stream);
		return DateOnly.FromDayNumber(dayNumber);
	}
#endif

	#endregion

	#region System.TimeOnly (.NET 6+ only)

#if NET6_0_OR_GREATER
	/// <summary>
	/// Writes a <see cref="System.TimeOnly"/> object.
	/// </summary>
	/// <param name="value">Object to write.</param>
	/// <param name="writer">Buffer writer to write the object to.</param>
	internal void WritePrimitive_TimeOnly(TimeOnly value, IBufferWriter<byte> writer)
	{
		long ticks = value.ToTimeSpan().Ticks;
		if (SerializationOptimization == SerializationOptimization.Speed || !IsLeb128EncodingMoreEfficient(ticks))
		{
			// use native encoding
			const int elementSize = sizeof(long);
			Span<byte> buffer = writer.GetSpan(1 + elementSize);
			buffer[0] = (byte)PayloadType.TimeOnly_Native;
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buffer[1..], in ticks);
#else
			MemoryMarshal.Write(buffer[1..], ref ticks);
#endif
			writer.Advance(1 + elementSize);
		}
		else
		{
			// use LEB128 encoding
			Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
			buffer[0] = (byte)PayloadType.TimeOnly_LEB128;
			int count = Leb128EncodingHelper.Write(buffer[1..], ticks);
			writer.Advance(1 + count);
		}
	}

	/// <summary>
	/// Reads a <see cref="System.TimeOnly"/> value (native encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal TimeOnly ReadPrimitive_TimeOnly_Native(Stream stream)
	{
		const int elementSize = sizeof(long); // binary representation of a TimeOnly
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		long ticks = MemoryMarshal.Read<long>(TempBuffer_Buffer);
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			EndiannessHelper.SwapBytes(ref ticks);
		return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(ticks));
	}

	/// <summary>
	/// Reads a <see cref="System.TimeOnly"/> value (LEB128 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the value from.</param>
	/// <returns>The read value.</returns>
	internal TimeOnly ReadPrimitive_TimeOnly_LEB128(Stream stream)
	{
		long ticks = Leb128EncodingHelper.ReadInt64(stream);
		return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(ticks));
	}
#endif

	#endregion

	#region System.Guid

	/// <summary>
	/// Writes a <see cref="System.Guid"/> object.
	/// </summary>
	/// <param name="value">Guid object to write.</param>
	/// <param name="writer">Buffer writer to write the <see cref="System.Guid"/> object to.</param>
	internal void WritePrimitive_Guid(Guid value, IBufferWriter<byte> writer)
	{
		const int elementSize = 16;
		Span<byte> buffer = writer.GetSpan(1 + elementSize);
		buffer[0] = (byte)PayloadType.Guid;
#if NETSTANDARD2_0 || NET461
		value.ToByteArray().AsSpan().CopyTo(buffer[1..]);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
		value.TryWriteBytes(buffer[1..]);
#else
#error Unhandled .NET framework
#endif
		writer.Advance(1 + elementSize);
	}

	/// <summary>
	/// Reads a <see cref="System.Guid"/> object.
	/// </summary>
	/// <param name="stream">Stream to read the Guid object from.</param>
	/// <returns>The read <see cref="System.Guid"/> object.</returns>
	internal Guid ReadPrimitive_Guid(Stream stream)
	{
		const int elementSize = 16;
#if NETSTANDARD2_0 || NET461
		byte[] buffer = new byte[elementSize];
		int bytesRead = stream.Read(buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		return new Guid(buffer);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
		if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
		return new Guid(TempBuffer_Buffer.AsSpan(0, 16));
#else
#error Unhandled .NET framework
#endif
	}

	#endregion

	#region System.String

	/// <summary>
	/// Writes a <see cref="System.String"/> object.
	/// </summary>
	/// <param name="value">String to write.</param>
	/// <param name="writer">Buffer writer to write the string to.</param>
	internal void WritePrimitive_String(string value, IBufferWriter<byte> writer)
	{
		if (SerializationOptimization == SerializationOptimization.Speed)
		{
			// use UTF-16 encoding
			// => .NET strings are always UTF-16 encoded itself, so no further encoding steps are needed...

			// write the encoded string
			int valueByteCount = value.Length * sizeof(char);
			int maxSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + valueByteCount;
			Span<byte> buffer = writer.GetSpan(maxSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.String_UTF16;
			bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], value.Length);
			MemoryMarshal.AsBytes(value.AsSpan()).CopyTo(buffer[bufferIndex..]);
			bufferIndex += valueByteCount;
			writer.Advance(bufferIndex);
		}
		else
		{
			// use UTF-8 encoding

			// resize temporary buffer for the encoding the string
			int size = sUtf8Encoding.GetMaxByteCount(value.Length);
			EnsureTemporaryByteBufferSize(size);

			// encode the string
			int valueByteCount = sUtf8Encoding.GetBytes(value, 0, value.Length, TempBuffer_Buffer, 0);

			// write the encoded string
			int maxSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + valueByteCount;
			Span<byte> buffer = writer.GetSpan(maxSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.String_UTF8;
			bufferIndex += Leb128EncodingHelper.Write(buffer[1..], valueByteCount);
			TempBuffer_Buffer.AsSpan()[..valueByteCount].CopyTo(buffer[bufferIndex..]);
			bufferIndex += valueByteCount;
			writer.Advance(bufferIndex);
		}

		// assign an object id to the serialized string
		mSerializedObjectIdTable.Add(value, mNextSerializedObjectId++);
	}

	/// <summary>
	/// Reads a <see cref="System.String"/> object (UTF-8 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the string object from.</param>
	/// <returns>The read string.</returns>
	internal string ReadPrimitive_String_UTF8(Stream stream)
	{
		// read the number of UTF-8 code units
		int codeUnitCount = Leb128EncodingHelper.ReadInt32(stream);
		int size = codeUnitCount * sizeof(byte);

		// read encoded string
		EnsureTemporaryByteBufferSize(size);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
		if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");

		// decode string
		string s = sUtf8Encoding.GetString(TempBuffer_Buffer, 0, size);
		mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, s);
		return s;
	}

	/// <summary>
	/// Reads a <see cref="System.String"/> object (UTF-16 encoding).
	/// </summary>
	/// <param name="stream">Stream to read the string object from.</param>
	/// <returns>The read string.</returns>
	internal
#if NETSTANDARD2_0 || NET461
		unsafe
#endif
		string ReadPrimitive_String_UTF16(Stream stream)
	{
		// read the number of UTF-16 code units
		int codeUnitCount = Leb128EncodingHelper.ReadInt32(stream);
		int size = codeUnitCount * sizeof(char);

		// read encoded string
		EnsureTemporaryByteBufferSize(size);
		int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
		if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");

		// swap bytes to fix endianness issues, if necessary
		Span<char> buffer = MemoryMarshal.Cast<byte, char>(TempBuffer_Buffer.AsSpan(0, bytesRead));
		if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				EndiannessHelper.SwapBytes(ref buffer[i]);
			}
		}

		// create a string from the buffer
#if NETSTANDARD2_0 || NET461
		string s;
		fixed (char* p = buffer) { s = new string(p, 0, buffer.Length); }
#elif NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		string s = new(buffer);
#else
#error Unhandled .NET framework
#endif
		mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, s);
		return s;
	}

	#endregion

	#region System.Object

	/// <summary>
	/// Writes a <see cref="System.Object"/> object.
	/// </summary>
	/// <param name="obj">Object to write.</param>
	/// <param name="writer">Buffer writer to write the object to.</param>
	internal void WritePrimitive_Object(object obj, IBufferWriter<byte> writer)
	{
		Span<byte> buffer = writer.GetSpan(1);
		buffer[0] = (byte)PayloadType.Object;
		writer.Advance(1);
		mSerializedObjectIdTable.Add(obj, mNextSerializedObjectId++);
	}

	/// <summary>
	/// Reads a <see cref="System.Object"/> object.
	/// </summary>
	/// <returns>The object.</returns>
	internal object ReadPrimitive_Object()
	{
		object obj = new();
		mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, obj);
		return obj;
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(char value)
	{
		return value <= Leb128EncodingHelper.UInt32MaxValueEncodedWith1Byte;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(short value)
	{
		return value is >= Leb128EncodingHelper.Int32MinValueEncodedWith1Byte and <= Leb128EncodingHelper.Int32MaxValueEncodedWith1Byte;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(ushort value)
	{
		return value <= Leb128EncodingHelper.UInt32MaxValueEncodedWith1Byte;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(int value)
	{
		return value is >= Leb128EncodingHelper.Int32MinValueEncodedWith3Bytes and <= Leb128EncodingHelper.Int32MaxValueEncodedWith3Bytes;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(uint value)
	{
		return value <= Leb128EncodingHelper.UInt32MaxValueEncodedWith3Bytes;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(long value)
	{
		return value is >= Leb128EncodingHelper.Int64MinValueEncodedWith7Bytes and <= Leb128EncodingHelper.Int64MaxValueEncodedWith7Bytes;
	}

	/// <summary>
	/// Determines whether the specified value can be encoded more efficiently using LEB128 encoding or native encoding.
	/// </summary>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <c>true</c> if encoding with LEB128 is more efficient;
	/// <c>false</c> if native encoding is more efficient.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLeb128EncodingMoreEfficient(ulong value)
	{
		return value <= Leb128EncodingHelper.UInt64MaxValueEncodedWith7Bytes;
	}

	#endregion
}
