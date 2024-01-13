///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GriffinPlus.Lib.Serialization
{

	partial class Serializer
	{
		#region Serialization of SZARRAYs (One-Dimensional Arrays with Zero-Based Indexing)

		#region System.Boolean

		/// <summary>
		/// Writes an array with of <see cref="System.Boolean"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(bool[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(bool);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfBoolean_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<bool, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => compress 8 booleans into 1 byte

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfBoolean_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				byte byteToWrite = 0;
				for (int i = 0; i < array.Length; i++)
				{
					bool value = array[i];
					if (value) byteToWrite |= (byte)(1 << (i % 8));
					if (i % 8 == 7)
					{
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
						valueBuffer[0] = byteToWrite;
						writer.Advance(elementSize);
						byteToWrite = 0;
					}
				}

				if (array.Length % 8 != 0)
				{
					Span<byte> valueBuffer = writer.GetSpan(elementSize);
					valueBuffer[0] = byteToWrite;
					writer.Advance(elementSize);
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Boolean"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private bool[] ReadArrayOfBoolean_Native(Stream stream)
		{
			const int elementSize = sizeof(bool);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			bool[] array;
			if (length > 0)
			{
				array = new bool[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<bool, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
			}
			else
			{
				array = Array.Empty<bool>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Boolean"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private bool[] ReadArrayOfBoolean_Compact(Stream stream)
		{
			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read compacted boolean array
			int compactedArrayLength = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(compactedArrayLength);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, compactedArrayLength);
			if (bytesRead < compactedArrayLength) throw new SerializationException("Unexpected end of stream.");

			// build boolean array to return
			bool[] array;
			if (length > 0)
			{
				array = new bool[length];
				for (int i = 0; i < length; i++)
				{
					array[i] = (TempBuffer_Buffer[i / 8] & (byte)(1 << (i % 8))) != 0;
				}
			}
			else
			{
				array = Array.Empty<bool>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Char

		/// <summary>
		/// Writes an array with of <see cref="System.Char"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(char[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(char);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfChar_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<char, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element 

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					// pre-initialization indicates to use native encoding
					// => set bits that indicate LEB128 encoding
					if (IsLeb128EncodingMoreEfficient(array[i]))
					{
						// use LEB128 encoding for this element
						encoding[i / 8] |= (byte)(1 << (i % 8));
					}
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfChar_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (char c in array)
				{
					char value = c;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, (uint)value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Char"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private char[] ReadArrayOfChar_Native(Stream stream)
		{
			const int elementSize = sizeof(char);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			char[] array;
			if (length > 0)
			{
				array = new char[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<char, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<char>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Char"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private char[] ReadArrayOfChar_Compact(Stream stream)
		{
			const int elementSize = sizeof(char);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			char[] array;
			if (length > 0)
			{
				array = new char[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = (char)Leb128EncodingHelper.ReadUInt32(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						char value = BitConverter.ToChar(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<char>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.SByte

		/// <summary>
		/// Writes an array of <see cref="System.SByte"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(sbyte[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(sbyte);

			// write header with payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfSByte;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
				MemoryMarshal.Cast<sbyte, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
				writer.Advance(bytesToCopy);
				fromIndex += elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.SByte"/> from a stream (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private sbyte[] ReadArrayOfSByte(Stream stream)
		{
			const int elementSize = sizeof(sbyte); // 1 (used only for clarity)

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			sbyte[] array;
			if (length > 0)
			{
				array = new sbyte[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<sbyte, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
			}
			else
			{
				array = Array.Empty<sbyte>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Byte

		/// <summary>
		/// Writes an array of <see cref="System.Byte"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfByte(byte[] array, IBufferWriter<byte> writer)
		{
			// write header with payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfByte;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int bytesToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize);
				buffer = writer.GetSpan(bytesToCopy);
				array.AsSpan(fromIndex, bytesToCopy).CopyTo(buffer);
				writer.Advance(bytesToCopy);
				fromIndex += bytesToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Byte"/> from a stream (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private byte[] ReadArrayOfByte(Stream stream)
		{
			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read array elements
			byte[] array = new byte[length];
			int bytesRead = stream.Read(array, 0, length);
			if (bytesRead < length) throw new SerializationException("Unexpected end of stream.");
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Int16

		/// <summary>
		/// Writes an array with of <see cref="System.Int16"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(short[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(short);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt16_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<short, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt16_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (short x in array)
				{
					short value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int16"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private short[] ReadArrayOfInt16_Native(Stream stream)
		{
			const int elementSize = sizeof(short);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			short[] array;
			if (length > 0)
			{
				array = new short[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<short, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<short>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int16"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private short[] ReadArrayOfInt16_Compact(Stream stream)
		{
			const int elementSize = sizeof(short);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			short[] array;
			if (length > 0)
			{
				array = new short[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = (short)Leb128EncodingHelper.ReadInt32(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						short value = BitConverter.ToInt16(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<short>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.UInt16

		/// <summary>
		/// Writes an array with of <see cref="System.UInt16"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(ushort[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(ushort);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// prepare buffer for payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt16_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<ushort, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt16_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (ushort x in array)
				{
					ushort value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt16"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private ushort[] ReadArrayOfUInt16_Native(Stream stream)
		{
			const int elementSize = sizeof(ushort);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			ushort[] array;
			if (length > 0)
			{
				array = new ushort[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<ushort, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<ushort>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt16"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private ushort[] ReadArrayOfUInt16_Compact(Stream stream)
		{
			const int elementSize = sizeof(ushort);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			ushort[] array;
			if (length > 0)
			{
				array = new ushort[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = (ushort)Leb128EncodingHelper.ReadUInt32(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						ushort value = BitConverter.ToUInt16(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<ushort>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Int32

		/// <summary>
		/// Writes an array with of <see cref="System.Int32"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(int[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(int);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt32_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<int, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt32_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (int x in array)
				{
					int value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int32"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadArrayOfInt32_Native(Stream stream)
		{
			const int elementSize = sizeof(int);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			int[] array;
			if (length > 0)
			{
				array = new int[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<int, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<int>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int32"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadArrayOfInt32_Compact(Stream stream)
		{
			const int elementSize = sizeof(int);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			int[] array;
			if (length > 0)
			{
				array = new int[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = Leb128EncodingHelper.ReadInt32(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						int value = BitConverter.ToInt32(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<int>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.UInt32

		/// <summary>
		/// Writes an array with of <see cref="System.UInt32"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(uint[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(uint);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// prepare buffer for payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt32_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<uint, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt32_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (uint x in array)
				{
					uint value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt32"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private uint[] ReadArrayOfUInt32_Native(Stream stream)
		{
			const int elementSize = sizeof(uint);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			uint[] array;
			if (length > 0)
			{
				array = new uint[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<uint, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<uint>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt32"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private uint[] ReadArrayOfUInt32_Compact(Stream stream)
		{
			const int elementSize = sizeof(uint);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			uint[] array;
			if (length > 0)
			{
				array = new uint[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = Leb128EncodingHelper.ReadUInt32(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						uint value = BitConverter.ToUInt32(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<uint>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Int64

		/// <summary>
		/// Writes an array with of <see cref="System.Int64"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(long[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(long);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// write header with payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt64_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<long, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt64_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (long x in array)
				{
					long value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int64"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private long[] ReadArrayOfInt64_Native(Stream stream)
		{
			const int elementSize = sizeof(long);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			long[] array;
			if (length > 0)
			{
				array = new long[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<long, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<long>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int64"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private long[] ReadArrayOfInt64_Compact(Stream stream)
		{
			const int elementSize = sizeof(long);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			long[] array;
			if (length > 0)
			{
				array = new long[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = Leb128EncodingHelper.ReadInt64(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						long value = BitConverter.ToInt64(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<long>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.UInt64

		/// <summary>
		/// Writes an array with of <see cref="System.UInt64"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(ulong[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(ulong);

			if (SerializationOptimization == SerializationOptimization.Speed)
			{
				// optimization for speed
				// => all elements should be written using native encoding

				// prepare buffer for payload type and array length
				const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt64_Native;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					MemoryMarshal.Cast<ulong, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}
			}
			else
			{
				// optimization for size
				// => choose best encoding for every element

				// choose encoding and store the decision bitwise
				// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
				Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
				for (int i = 0; i < array.Length; i++)
				{
					if (IsLeb128EncodingMoreEfficient(array[i]))
						encoding[i / 8] |= (byte)(1 << (i % 8));
				}

				// write header with payload type, array length and encoding
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt64_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				foreach (ulong x in array)
				{
					ulong value = x;

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
						MemoryMarshal.Write(valueBuffer, in value);
#else
						MemoryMarshal.Write(valueBuffer, ref value);
#endif
						writer.Advance(elementSize);
					}
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt64"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private ulong[] ReadArrayOfUInt64_Native(Stream stream)
		{
			const int elementSize = sizeof(ulong);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			ulong[] array;
			if (length > 0)
			{
				array = new ulong[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<ulong, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<ulong>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt64"/> (for arrays with zero-based indexing, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private ulong[] ReadArrayOfUInt64_Compact(Stream stream)
		{
			const int elementSize = sizeof(ulong);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			ulong[] array;
			if (length > 0)
			{
				array = new ulong[length];
				for (int i = 0; i < length; i++)
				{
					bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;
					if (useLeb128Encoding)
					{
						// use LEB128 encoding
						array[i] = Leb128EncodingHelper.ReadUInt64(stream);
					}
					else
					{
						// use native encoding
						// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
						bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
						if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
						ulong value = BitConverter.ToUInt64(TempBuffer_Buffer, 0);

						// swap bytes if the endianness of the system that has serialized the value is different
						// from the current system
						if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
							EndiannessHelper.SwapBytes(ref value);

						// store value in array
						array[i] = value;
					}
				}
			}
			else
			{
				array = Array.Empty<ulong>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Single

		/// <summary>
		/// Writes an array with of <see cref="System.Single"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(float[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(float);

			// write header with payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfSingle;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
				MemoryMarshal.Cast<float, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
				writer.Advance(bytesToCopy);
				fromIndex += elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Single"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private float[] ReadArrayOfSingle(Stream stream)
		{
			const int elementSize = sizeof(float);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			float[] array;
			if (length > 0)
			{
				array = new float[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<float, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<float>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Double

		/// <summary>
		/// Writes an array with of <see cref="System.Double"/> (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(double[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(double);

			// write header with payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfDouble;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
				MemoryMarshal.Cast<double, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
				writer.Advance(bytesToCopy);
				fromIndex += elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Double"/> (for arrays with zero-based indexing, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private double[] ReadArrayOfDouble(Stream stream)
		{
			const int elementSize = sizeof(double);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			double[] array;
			if (length > 0)
			{
				array = new double[length];
#if NETSTANDARD2_0 || NET461
				EnsureTemporaryByteBufferSize(size);
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);
#elif NETSTANDARD2_1 || NET5_0_OR_GREATER
				int bytesRead = stream.Read(MemoryMarshal.Cast<double, byte>(array.AsSpan()));
				if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
#else
#error Unhandled .NET framework.
#endif
				// swap bytes if the endianness of the system that has serialized the array is different
				// from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < length; i++)
					{
						EndiannessHelper.SwapBytes(ref array[i]);
					}
				}
			}
			else
			{
				array = Array.Empty<double>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Decimal

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArray(IReadOnlyList<decimal> array, IBufferWriter<byte> writer)
		{
			const int elementSize = 16;

			// write payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfDecimal;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Count);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Count)
			{
				int elementsToCopy = Math.Min(array.Count - fromIndex, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
				for (int i = 0; i < elementsToCopy; i++)
				{
					int[] bits = decimal.GetBits(array[fromIndex++]);
					MemoryMarshal.Cast<int, byte>(bits.AsSpan()).CopyTo(buffer.Slice(i * elementSize));
				}
#elif NET5_0_OR_GREATER
				var intBuffer = MemoryMarshal.Cast<byte, int>(buffer);
				for (int i = 0; i < elementsToCopy; i++)
				{
					decimal.GetBits(array[fromIndex++], intBuffer.Slice(4 * i, 4));
				}
#else
				#error Unhandled .NET framework
#endif

				writer.Advance(bytesToCopy);
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Decimal"/> from a stream (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private decimal[] ReadArrayOfDecimal(Stream stream)
		{
			const int elementSize = 16;

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements into temporary buffer
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");

			// convert elements to decimal
			decimal[] array;
			if (length > 0)
			{
				array = new decimal[length];
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
				int index = 0;
				for (int i = 0; i < length; i++)
				{
					// copy bytes of one decimal value to int32 buffer
					Buffer.BlockCopy(TempBuffer_Buffer, index, TempBuffer_Int32, 0, elementSize);

					// swap bytes if the endianness of the system that has serialized the array is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
					{
						EndiannessHelper.SwapBytes(ref TempBuffer_Int32[0]);
						EndiannessHelper.SwapBytes(ref TempBuffer_Int32[1]);
						EndiannessHelper.SwapBytes(ref TempBuffer_Int32[2]);
						EndiannessHelper.SwapBytes(ref TempBuffer_Int32[3]);
					}

					array[i] = new decimal(TempBuffer_Int32);
					index += elementSize;
				}
#elif NET5_0_OR_GREATER
				var data = MemoryMarshal.Cast<byte, int>(TempBuffer_Buffer.AsSpan(0, size)); // temporary byte array should be aligned to a 4-byte boundary
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < data.Length; i++)
						EndiannessHelper.SwapBytes(ref data[i]);
				}

				for (int i = 0; i < length; i++)
				{
					array[i] = new decimal(data.Slice(4 * i, 4));
				}
#endif
			}
			else
			{
				array = Array.Empty<decimal>();
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region Other Objects

		/// <summary>
		/// Writes an array of objects (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfObjects(Array array, IBufferWriter<byte> writer)
		{
			// write element type
			WriteTypeMetadata(writer, array.GetType().GetElementType());

			// write payload type and array length
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfObjects;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// assign an object id to the array before serializing its elements
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);

			// write array elements
			for (int i = 0; i < array.Length; i++)
			{
				InnerSerialize(writer, array.GetValue(i), null);
			}
		}

		/// <summary>
		/// Reads an array of objects (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <param name="context">Context object to pass to an internal/external object serializer class.</param>
		/// <returns>The read array.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		private Array ReadArrayOfObjects(Stream stream, object context)
		{
			// assembly and type metadata has been read already

			Type t = mCurrentDeserializedType.Type;

			// read array length and create an uninitialized array
			int length = Leb128EncodingHelper.ReadInt32(stream);
			Array array = FastActivator.CreateArray(t, length);

			// assign an object id to the array before deserializing its elements
			// (elements may refer to the array)
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			// read array elements
			for (int i = 0; i < length; i++)
			{
				array.SetValue(InnerDeserialize(stream, context), i);
			}

			return array;
		}

		#endregion

		#endregion

		#region Serialization of MDARRAYs (Multiple Dimensions and/or Non-Zero-Based Indexing)

		#region System.Boolean

		/// <summary>
		/// Writes an array of <see cref="System.Boolean"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfBoolean(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(bool);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			bool* pArray = (bool*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfBoolean_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => use compact encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfBoolean_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					byte byteToWrite = 0;
					for (int i = 0; i < array.Length; i++)
					{
						bool value = pArray[i];

						if (value) byteToWrite |= (byte)(1 << (i % 8));
						if (i % 8 == 7)
						{
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
							valueBuffer[0] = byteToWrite;
							writer.Advance(elementSize);
							byteToWrite = 0;
						}
					}

					if (array.Length % 8 != 0)
					{
						Span<byte> valueBuffer = writer.GetSpan(elementSize);
						valueBuffer[0] = byteToWrite;
						writer.Advance(elementSize);
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Boolean"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfBoolean_Native(Stream stream)
		{
			const int elementSize = sizeof(bool);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(bool), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Boolean"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfBoolean_Compact(Stream stream)
		{
			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create array of the specified type
			var array = Array.CreateInstance(typeof(bool), lengths, lowerBounds);

			// read compacted boolean array
			int compactedArrayLength = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(compactedArrayLength);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, compactedArrayLength);
			if (bytesRead < compactedArrayLength) throw new SerializationException("Unexpected end of stream.");

			// build boolean array to return
			for (int i = 0; i < totalCount; i++)
			{
				bool value = (TempBuffer_Buffer[i / 8] & (byte)(1 << (i % 8))) != 0;
				array.SetValue(value, indices);
				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Char

		/// <summary>
		/// Writes an array of <see cref="System.Char"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfChar(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(char);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			char* pArray = (char*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfChar_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						char value = pArray[i];

						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding for this element
							encoding[i / 8] |= (byte)(1 << (i % 8));
						}
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfChar_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						char value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, (uint)value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Char"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfChar_Native(Stream stream)
		{
			const int elementSize = sizeof(char);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(char), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				char* pArray = (char*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Char"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfChar_Compact(Stream stream)
		{
			const int elementSize = sizeof(char);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(char), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue((char)Leb128EncodingHelper.ReadUInt32(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					char value = BitConverter.ToChar(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.SByte

		/// <summary>
		/// Writes an array of <see cref="System.SByte"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfSByte(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(sbyte);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			sbyte* pArray = (sbyte*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				// write payload type and array dimensions
				int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfSByte;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
				for (int i = 0; i < array.Rank; i++)
				{
					// ...dimension information...
					int lowerBound = array.GetLowerBound(i);
					int count = array.GetLength(i);
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
				}

				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}

				// assign an object id to the array to allow referencing it later on
				mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}
		}

		/// <summary>
		/// Reads an array of <see cref="System.SByte"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfSByte(Stream stream)
		{
			const int elementSize = sizeof(sbyte);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(sbyte), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Byte

		/// <summary>
		/// Writes an array of <see cref="System.Byte"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfByte(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(byte);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			byte* pArray = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				// write payload type and array dimensions
				int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfByte;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
				for (int i = 0; i < array.Rank; i++)
				{
					// ...dimension information...
					int lowerBound = array.GetLowerBound(i);
					int count = array.GetLength(i);
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
				}

				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}

				// assign an object id to the array to allow referencing it later on
				mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}
		}

		/// <summary>
		/// Reads an array of <see cref="System.Byte"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions)
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array DeserializeMultidimensionalArrayOfByte(Stream stream)
		{
			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array of bytes
			var array = Array.CreateInstance(typeof(byte), lengths, lowerBounds);

			// read array data
			int size = totalCount;
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Int16

		/// <summary>
		/// Writes an array of <see cref="System.Int16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfInt16(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(short);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			short* pArray = (short*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt16_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt16_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						short value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfInt16_Native(Stream stream)
		{
			const int elementSize = sizeof(short);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(short), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				short* pArray = (short*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfInt16_Compact(Stream stream)
		{
			const int elementSize = sizeof(short);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(short), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue((short)Leb128EncodingHelper.ReadInt32(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					short value = BitConverter.ToInt16(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.UInt16

		/// <summary>
		/// Writes an array of <see cref="System.UInt16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfUInt16(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(ushort);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			ushort* pArray = (ushort*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt16_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt16_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						ushort value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, (uint)value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfUInt16_Native(Stream stream)
		{
			const int elementSize = sizeof(ushort);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(ushort), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				ushort* pArray = (ushort*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt16"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfUInt16_Compact(Stream stream)
		{
			const int elementSize = sizeof(ushort);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(ushort), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue((ushort)Leb128EncodingHelper.ReadUInt32(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					ushort value = BitConverter.ToUInt16(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Int32

		/// <summary>
		/// Writes an array of <see cref="System.Int32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfInt32(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(int);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			int* pArray = (int*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt32_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt32_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						int value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfInt32_Native(Stream stream)
		{
			const int elementSize = sizeof(int);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(int), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				int* pArray = (int*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfInt32_Compact(Stream stream)
		{
			const int elementSize = sizeof(int);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(int), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue(Leb128EncodingHelper.ReadInt32(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					int value = BitConverter.ToInt32(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.UInt32

		/// <summary>
		/// Writes an array of <see cref="System.UInt32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfUInt32(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(uint);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			uint* pArray = (uint*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt32_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt32_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						uint value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfUInt32_Native(Stream stream)
		{
			const int elementSize = sizeof(uint);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(uint), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				uint* pArray = (uint*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt32"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfUInt32_Compact(Stream stream)
		{
			const int elementSize = sizeof(uint);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(uint), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue(Leb128EncodingHelper.ReadUInt32(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					uint value = BitConverter.ToUInt32(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Int64

		/// <summary>
		/// Writes an array of <see cref="System.Int64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfInt64(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(long);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			long* pArray = (long*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt64_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfInt64_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						long value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfInt64_Native(Stream stream)
		{
			const int elementSize = sizeof(long);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(long), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				long* pArray = (long*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.Int64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfInt64_Compact(Stream stream)
		{
			const int elementSize = sizeof(long);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(long), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue(Leb128EncodingHelper.ReadInt64(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					long value = BitConverter.ToInt64(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.UInt64

		/// <summary>
		/// Writes an array of <see cref="System.UInt64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfUInt64(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(ulong);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			ulong* pArray = (ulong*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				if (SerializationOptimization == SerializationOptimization.Speed)
				{
					// optimization for speed
					// => all elements should be written using native encoding

					// write payload type and array dimensions
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt64_Native;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					writer.Advance(bufferIndex);

					// write array elements
					int fromIndex = 0;
					while (fromIndex < array.Length)
					{
						int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
						int bytesToCopy = elementsToCopy * elementSize;
						buffer = writer.GetSpan(bytesToCopy);
						new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
						writer.Advance(bytesToCopy);
						fromIndex += elementsToCopy;
					}
				}
				else
				{
					// optimization for size
					// => choose best encoding for every element 

					// choose encoding and store the decision bitwise
					// (bitwise, 0 = native encoding, 1 = LEB128 encoding, C# initializes the memory to zero)
					Span<byte> encoding = stackalloc byte[(array.Length + 7) / 8];
					for (int i = 0; i < array.Length; i++)
					{
						// pre-initialization indicates to use native encoding
						// => set bits that indicate LEB128 encoding
						if (IsLeb128EncodingMoreEfficient(pArray[i]))
							encoding[i / 8] |= (byte)(1 << (i % 8));
					}

					// write payload type, array dimensions and encoding
					int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue + encoding.Length;
					Span<byte> buffer = writer.GetSpan(maxBufferSize);
					int bufferIndex = 0;
					buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfUInt64_Compact;
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
					for (int i = 0; i < array.Rank; i++)
					{
						// ...dimension information...
						int lowerBound = array.GetLowerBound(i);
						int count = array.GetLength(i);
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
						bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
					}

					encoding.CopyTo(buffer.Slice(bufferIndex));
					bufferIndex += encoding.Length;
					writer.Advance(bufferIndex);

					// write array elements
					for (int i = 0; i < array.Length; i++)
					{
						ulong value = pArray[i];

						if (IsLeb128EncodingMoreEfficient(value))
						{
							// use LEB128 encoding
							Span<byte> valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
							int count = Leb128EncodingHelper.Write(valueBuffer, value);
							writer.Advance(count);
						}
						else
						{
							// use native encoding
							Span<byte> valueBuffer = writer.GetSpan(elementSize);
#if NET8_0_OR_GREATER
							MemoryMarshal.Write(valueBuffer, in value);
#else
							MemoryMarshal.Write(valueBuffer, ref value);
#endif
							writer.Advance(elementSize);
						}
					}
				}
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfUInt64_Native(Stream stream)
		{
			const int elementSize = sizeof(ulong);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(ulong), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				ulong* pArray = (ulong*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.UInt64"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, compact encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfUInt64_Compact(Stream stream)
		{
			const int elementSize = sizeof(ulong);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// read encoding information
			int bytesToRead = (totalCount + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(ulong), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < totalCount; i++)
			{
				bool useLeb128Encoding = (TempBuffer_Buffer[elementSize + i / 8] & (1 << (i % 8))) != 0;

				if (useLeb128Encoding)
				{
					// use LEB128 encoding
					array.SetValue(Leb128EncodingHelper.ReadUInt64(stream), indices);
				}
				else
				{
					// use native encoding
					// EnsureTemporaryByteBufferSize(elementSize); // not necessary, the buffer has at least 256 bytes
					bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
					if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
					ulong value = BitConverter.ToUInt64(TempBuffer_Buffer, 0);

					// swap bytes if the endianness of the system that has serialized the value is different
					// from the current system
					if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
						EndiannessHelper.SwapBytes(ref value);

					// store value in array
					array.SetValue(value, indices);
				}

				IncrementArrayIndices(indices, array);
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Single

		/// <summary>
		/// Writes an array of <see cref="System.Single"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfSingle(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(float);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			float* pArray = (float*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				// write payload type and array dimensions
				int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfSingle;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
				for (int i = 0; i < array.Rank; i++)
				{
					// ...dimension information...
					int lowerBound = array.GetLowerBound(i);
					int count = array.GetLength(i);
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
				}

				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}

				// assign an object id to the array to allow referencing it later on
				mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}
		}

		/// <summary>
		/// Reads an array of <see cref="System.Single"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfSingle(Stream stream)
		{
			const int elementSize = sizeof(float);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(float), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				float* pArray = (float*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Double

		/// <summary>
		/// Writes an array of <see cref="System.Double"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfDouble(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = sizeof(double);

			// pin array in memory and get a pointer to it
			GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			double* pArray = (double*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
			Debug.Assert(pArray != null);

			try
			{
				// write payload type and array dimensions
				int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
				Span<byte> buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfDouble;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
				for (int i = 0; i < array.Rank; i++)
				{
					// ...dimension information...
					int lowerBound = array.GetLowerBound(i);
					int count = array.GetLength(i);
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), lowerBound); // lower bound of the dimension
					bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
				}

				writer.Advance(bufferIndex);

				// write array elements
				int fromIndex = 0;
				while (fromIndex < array.Length)
				{
					int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
					int bytesToCopy = elementsToCopy * elementSize;
					buffer = writer.GetSpan(bytesToCopy);
					new Span<byte>(pArray + fromIndex, bytesToCopy).CopyTo(buffer);
					writer.Advance(bytesToCopy);
					fromIndex += elementsToCopy;
				}

				// assign an object id to the array to allow referencing it later on
				mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
			}
			finally
			{
				// release GC handle to array
				arrayGcHandle.Free();
			}
		}

		/// <summary>
		/// Reads an array of <see cref="System.Double"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions, native encoding).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private unsafe Array ReadMultidimensionalArrayOfDouble(Stream stream)
		{
			const int elementSize = sizeof(double);

			// read header
			int totalCount = 1;
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array with the specified size
			var array = Array.CreateInstance(typeof(double), lengths, lowerBounds);

			// read array data into temporary buffer, then copy into the final array
			// => avoids pinning the array while reading from the stream
			//    (can have a negative impact on the performance of the garbage collector)
			int bytesToRead = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(bytesToRead);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, bytesToRead);

			// swap bytes if the endianness of the system that has serialized the array is different
			// from the current system
			if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
			{
				GCHandle arrayGcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				double* pArray = (double*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
				Debug.Assert(pArray != null);
				for (int i = 0; i < totalCount; i++) EndiannessHelper.SwapBytes(ref pArray[i]);
				arrayGcHandle.Free();
			}

			// assign an object id to the array to allow referencing it later on
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			return array;
		}

		#endregion

		#region System.Decimal

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteMultidimensionalArrayOfDecimal(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = 16;

			// write payload type and array dimensions
			int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfDecimal;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				indices[i] = array.GetLowerBound(i);
				int count = array.GetLength(i);
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), indices[i]); // lower bound of the dimension
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
			}

			writer.Advance(bufferIndex);

			// write array elements
			int remaining = array.Length;
			while (remaining > 0)
			{
				int elementsToCopy = Math.Min(remaining, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
				bufferIndex = 0;
				for (int i = 0; i < elementsToCopy; i++)
				{
					// ReSharper disable once PossibleNullReferenceException
					decimal value = (decimal)array.GetValue(indices);
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
					int[] bits = decimal.GetBits(value);
					MemoryMarshal.Cast<int, byte>(bits.AsSpan()).CopyTo(buffer.Slice(bufferIndex));
#elif NET5_0_OR_GREATER
					var intBuffer = MemoryMarshal.Cast<byte, int>(buffer.Slice(bufferIndex));
					decimal.GetBits(value, intBuffer);
#else
					#error Unhandled .NET framework
#endif
					IncrementArrayIndices(indices, array);
					bufferIndex += elementSize;
				}

				writer.Advance(bytesToCopy);
				remaining -= elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of <see cref="System.Decimal"/>
		/// (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfDecimal(Stream stream)
		{
			const int elementSize = 16;

			// read header
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
			}

			// create an array of decimals
			var array = Array.CreateInstance(typeof(decimal), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				// read four 32-bit integer values representing the decimal value
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
				if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, TempBuffer_Int32, 0, elementSize);

				// swap bytes if the endianness of the system that has serialized the array is different from the current system
				if (IsDeserializingLittleEndian != BitConverter.IsLittleEndian)
				{
					EndiannessHelper.SwapBytes(ref TempBuffer_Int32[0]);
					EndiannessHelper.SwapBytes(ref TempBuffer_Int32[1]);
					EndiannessHelper.SwapBytes(ref TempBuffer_Int32[2]);
					EndiannessHelper.SwapBytes(ref TempBuffer_Int32[3]);
				}

				// create decimal value and store it in the array
				decimal value = new decimal(TempBuffer_Int32);
				array.SetValue(value, indices);
				IncrementArrayIndices(indices, array);
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region Other Objects

		/// <summary>
		/// Writes a multidimensional array of objects (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to serialize.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteMultidimensionalArrayOfObjects(Array array, IBufferWriter<byte> writer)
		{
			// write type metadata
			WriteTypeMetadata(writer, array.GetType().GetElementType());

			// write payload type and array dimensions
			int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfObjects;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Rank); // number of dimensions
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				indices[i] = array.GetLowerBound(i);
				int count = array.GetLength(i);
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), indices[i]); // lower bound of the dimension
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), count);      // number of elements in the dimension
			}

			writer.Advance(bufferIndex);

			// assign an object id to the array before serializing its elements
			// (elements may refer to the array)
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);

			// write array elements
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array.GetValue(indices);
				InnerSerialize(writer, obj, null);
				IncrementArrayIndices(indices, array);
			}
		}

		/// <summary>
		/// Reads an array of objects (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <param name="context">Context object to pass to an internal/external object serializer class.</param>
		/// <returns>The read array.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		private Array ReadMultidimensionalArrayOfObjects(Stream stream, object context)
		{
			Type type = mCurrentDeserializedType.Type;

			// read header
			int ranks = Leb128EncodingHelper.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = Leb128EncodingHelper.ReadInt32(stream);
				lengths[i] = Leb128EncodingHelper.ReadInt32(stream);
				indices[i] = lowerBounds[i];
			}

			// create an array with elements of the specified type
			var array = Array.CreateInstance(type, lengths, lowerBounds);

			// assign an object id to the array before deserializing its elements
			// (elements may refer to the array)
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				object obj = InnerDeserialize(stream, context);
				array.SetValue(obj, indices);
				IncrementArrayIndices(indices, array);
			}

			return array;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Increments a multidimensional array indexing vector.
		/// </summary>
		/// <param name="indices">Indexing vector.</param>
		/// <param name="array">Array that is being accessed.</param>
		private static void IncrementArrayIndices(IList<int> indices, Array array)
		{
			for (int i = indices.Count; i > 0; i--)
			{
				if (indices[i - 1] == array.GetUpperBound(i - 1))
				{
					indices[i - 1] = array.GetLowerBound(i - 1);
					continue;
				}

				indices[i - 1]++;
				return;
			}
		}

		#endregion

		#endregion
	}

}
