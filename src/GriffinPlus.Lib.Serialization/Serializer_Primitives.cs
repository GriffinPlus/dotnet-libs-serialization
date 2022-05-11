///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#if NET5_0_OR_GREATER
using System.Diagnostics;
#endif

namespace GriffinPlus.Lib.Serialization
{

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
			var buffer = writer.GetSpan(1);
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
			if (SerializationOptimization == SerializationOptimization.Speed || value > Leb128EncodingHelper.UInt32MaxValueEncodedWith1Byte)
			{
				// use native encoding
				var buffer = writer.GetSpan(3);
				buffer[0] = (byte)PayloadType.Char_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(3);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				buffer[0] = (byte)PayloadType.Char_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), (uint)value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			var buffer = writer.GetSpan(1 + elementSize);
			buffer[0] = (byte)PayloadType.Decimal;
#if NET5_0_OR_GREATER
			Span<int> temp = stackalloc int[4];
			decimal.TryGetBits(value, temp, out int valuesWritten);
			Debug.Assert(valuesWritten * sizeof(int) == elementSize);
			MemoryMarshal.Cast<int, byte>(temp).CopyTo(buffer.Slice(1));
#else
			int[] bits = decimal.GetBits(value);
			Buffer.BlockCopy(bits, 0, TempBuffer_Buffer, 0, elementSize);
			TempBuffer_Buffer.AsSpan(0, elementSize).CopyTo(buffer.Slice(1));
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
#if NET5_0_OR_GREATER
			var intBuffer = MemoryMarshal.Cast<byte, int>(TempBuffer_Buffer.AsSpan(0, elementSize));
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				EndiannessHelper.SwapBytes(ref intBuffer[0]);
				EndiannessHelper.SwapBytes(ref intBuffer[1]);
				EndiannessHelper.SwapBytes(ref intBuffer[2]);
				EndiannessHelper.SwapBytes(ref intBuffer[3]);
			}

			return new decimal(intBuffer);
#else
			Buffer.BlockCopy(TempBuffer_Buffer, 0, TempBuffer_Int32, 0, elementSize);
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				EndiannessHelper.SwapBytes(ref TempBuffer_Int32[0]);
				EndiannessHelper.SwapBytes(ref TempBuffer_Int32[1]);
				EndiannessHelper.SwapBytes(ref TempBuffer_Int32[2]);
				EndiannessHelper.SwapBytes(ref TempBuffer_Int32[3]);
			}
			return new decimal(TempBuffer_Int32);
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
			var buffer = writer.GetSpan(1 + elementSize);
			buffer[0] = (byte)PayloadType.Single;
			MemoryMarshal.Write(buffer.Slice(1), ref value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			var buffer = writer.GetSpan(1 + elementSize);
			buffer[0] = (byte)PayloadType.Double;
			MemoryMarshal.Write(buffer.Slice(1), ref value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			var buffer = writer.GetSpan(2);

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
			if (SerializationOptimization == SerializationOptimization.Speed ||
			    value < Leb128EncodingHelper.Int32MinValueEncodedWith1Byte ||
			    value > Leb128EncodingHelper.Int32MaxValueEncodedWith1Byte)
			{
				// use native encoding
				var buffer = writer.GetSpan(3);
				buffer[0] = (byte)PayloadType.Int16_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(3);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				buffer[0] = (byte)PayloadType.Int16_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			if (SerializationOptimization == SerializationOptimization.Speed ||
			    value < Leb128EncodingHelper.Int32MinValueEncodedWith3Bytes ||
			    value > Leb128EncodingHelper.Int32MaxValueEncodedWith3Bytes)
			{
				// use native encoding
				var buffer = writer.GetSpan(5);
				buffer[0] = (byte)PayloadType.Int32_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(5);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				buffer[0] = (byte)PayloadType.Int32_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			if (SerializationOptimization == SerializationOptimization.Speed ||
			    value < Leb128EncodingHelper.Int64MinValueEncodedWith7Bytes ||
			    value > Leb128EncodingHelper.Int64MaxValueEncodedWith7Bytes)
			{
				// use native encoding
				var buffer = writer.GetSpan(9);
				buffer[0] = (byte)PayloadType.Int64_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(9);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
				buffer[0] = (byte)PayloadType.Int64_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			var buffer = writer.GetSpan(2);
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
			if (SerializationOptimization == SerializationOptimization.Speed || value > Leb128EncodingHelper.UInt32MaxValueEncodedWith1Byte)
			{
				// use native encoding
				var buffer = writer.GetSpan(3);
				buffer[0] = (byte)PayloadType.UInt16_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(3);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				buffer[0] = (byte)PayloadType.UInt16_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), (uint)value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			if (SerializationOptimization == SerializationOptimization.Speed || value > Leb128EncodingHelper.UInt32MaxValueEncodedWith3Bytes)
			{
				// use native encoding
				var buffer = writer.GetSpan(5);
				buffer[0] = (byte)PayloadType.UInt32_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(5);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				buffer[0] = (byte)PayloadType.UInt32_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			if (SerializationOptimization == SerializationOptimization.Speed || value > Leb128EncodingHelper.UInt64MaxValueEncodedWith7Bytes)
			{
				// use native encoding
				var buffer = writer.GetSpan(9);
				buffer[0] = (byte)PayloadType.UInt64_Native;
				MemoryMarshal.Write(buffer.Slice(1), ref value);
				writer.Advance(9);
			}
			else
			{
				// use LEB128 encoding
				var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
				buffer[0] = (byte)PayloadType.UInt64_LEB128;
				int count = Leb128EncodingHelper.Write(buffer.Slice(1), value);
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
			if (mDeserializingLittleEndian != BitConverter.IsLittleEndian)
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
			const int elementSize = sizeof(long); // binary representation of a DateTime
			var buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
			buffer[0] = (byte)PayloadType.DateTime;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			BitConverter.TryWriteBytes(buffer.Slice(1), value.ToBinary());
#else
			Span<long> temp = stackalloc long[1];
			temp[0] = value.ToBinary();
			MemoryMarshal.Cast<long, byte>(temp).CopyTo(buffer.Slice(1));
#endif
			writer.Advance(1 + elementSize);
		}

		/// <summary>
		/// read a <see cref="System.DateTime"/> object.
		/// </summary>
		/// <param name="stream">Stream to read the DateTime object from.</param>
		/// <returns>The read <see cref="System.DateTime"/> object.</returns>
		internal DateTime ReadPrimitive_DateTime(Stream stream)
		{
			const int elementSize = sizeof(long); // binary representation of a DateTime
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
			if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
			long value = BitConverter.ToInt64(TempBuffer_Buffer, 0);
			return DateTime.FromBinary(value);
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
			// resize temporary buffer for the encoding the string
			int size = Encoding.UTF8.GetMaxByteCount(value.Length);
			EnsureTemporaryByteBufferSize(size);

			// encode the string
			int valueByteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, TempBuffer_Buffer, 0);

			// write the encoded string
			int maxSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + valueByteCount;
			var buffer = writer.GetSpan(maxSize);
			buffer[0] = (byte)PayloadType.String;
			int headerSize = 1 + Leb128EncodingHelper.Write(buffer.Slice(1), valueByteCount);
			TempBuffer_Buffer.AsSpan().Slice(0, valueByteCount).CopyTo(buffer.Slice(headerSize));
			writer.Advance(headerSize + valueByteCount);

			// assign an object id to the serialized string
			mSerializedObjectIdTable.Add(value, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads a <see cref="System.String"/> object.
		/// </summary>
		/// <param name="stream">Stream to read the string object from.</param>
		/// <returns>The read string.</returns>
		internal string ReadPrimitive_String(Stream stream)
		{
			int size = Leb128EncodingHelper.ReadInt32(stream);

			// read encoded string
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");

			// decode string
			string s = Encoding.UTF8.GetString(TempBuffer_Buffer, 0, size);
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
			var buffer = writer.GetSpan(1);
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
			object obj = new object();
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, obj);
			return obj;
		}

		#endregion
	}

}
