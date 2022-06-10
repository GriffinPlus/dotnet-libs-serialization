///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
						var valueBuffer = writer.GetSpan(elementSize);
						valueBuffer[0] = byteToWrite;
						writer.Advance(elementSize);
						byteToWrite = 0;
					}
				}

				if (array.Length % 8 != 0)
				{
					var valueBuffer = writer.GetSpan(elementSize);
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
			bool[] array = new bool[length];
#if NET461 || NETSTANDARD2_0
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
			bool[] array = new bool[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = (TempBuffer_Buffer[i / 8] & (byte)(1 << (i % 8))) != 0;
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfChar_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					char value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, (uint)value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			char[] array = new char[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(char);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			char[] array = new char[length];
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
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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
			sbyte[] array = new sbyte[length];
#if NET461 || NETSTANDARD2_0
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
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt16_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					short value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			short[] array = new short[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(short);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			short[] array = new short[length];
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt16_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					ushort value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			ushort[] array = new ushort[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(ushort);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			ushort[] array = new ushort[length];
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt32_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					int value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			int[] array = new int[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(int);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			int[] array = new int[length];
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt32_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					uint value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			uint[] array = new uint[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(uint);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			uint[] array = new uint[length];
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfInt64_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					long value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			long[] array = new long[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(long);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			long[] array = new long[length];
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
				int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
				var buffer = writer.GetSpan(maxBufferSize);
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
				var buffer = writer.GetSpan(maxBufferSize);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.ArrayOfUInt64_Compact;
				bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
				encoding.CopyTo(buffer.Slice(bufferIndex));
				bufferIndex += encoding.Length;
				writer.Advance(bufferIndex);

				// write array elements
				for (int i = 0; i < array.Length; i++)
				{
					ulong value = array[i];

					if (IsLeb128EncodingMoreEfficient(value))
					{
						// use LEB128 encoding
						var valueBuffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor64BitValue);
						int count = Leb128EncodingHelper.Write(valueBuffer, value);
						writer.Advance(count);
					}
					else
					{
						// use native encoding
						var valueBuffer = writer.GetSpan(elementSize);
						MemoryMarshal.Write(valueBuffer, ref value);
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
			ulong[] array = new ulong[length];
#if NET461 || NETSTANDARD2_0
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
			int elementSize = sizeof(ulong);

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read encoding information
			int bytesToRead = (length + 7) / 8;
			EnsureTemporaryByteBufferSize(bytesToRead + elementSize);
			int bytesRead = stream.Read(TempBuffer_Buffer, elementSize, bytesToRead);
			if (bytesRead < bytesToRead) throw new SerializationException("Unexpected end of stream.");

			// read array elements
			ulong[] array = new ulong[length];
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
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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
			float[] array = new float[length];
#if NET461 || NETSTANDARD2_0
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
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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
			double[] array = new double[length];
#if NET461 || NETSTANDARD2_0
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
		private void WriteArray(decimal[] array, IBufferWriter<byte> writer)
		{
			const int elementSize = 16;

			// write payload type and array length
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfDecimal;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
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
			decimal[] array = new decimal[length];
#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
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
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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

			var t = mCurrentDeserializedType.Type;

			// read array length and create an uninitialized array
			int length = Leb128EncodingHelper.ReadInt32(stream);
			var array = FastActivator.CreateArray(t, length);

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

		#region System.Byte

		/// <summary>
		/// Reads an array of <see cref="System.Byte"/> from a stream (for arrays with non-zero-based indexing and/or multiple dimensions)
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array DeserializeMultidimensionalByteArray(Stream stream)
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

		#region Primitive Types

		/// <summary>
		/// Writes an array of primitive value types (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="payloadType">Payload type corresponding to the type of the array.</param>
		/// <param name="array">Array to write.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private unsafe void WriteMultidimensionalArrayOfPrimitives(
			PayloadType         payloadType,
			Array               array,
			int                 elementSize,
			IBufferWriter<byte> writer)
		{
			// write payload type and array dimensions
			int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)payloadType;
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
				var arrayGcHandle = GCHandle.Alloc(array);
				var pSource = Marshal.UnsafeAddrOfPinnedArrayElement(array, fromIndex);
				new Span<byte>(pSource.ToPointer(), bytesToCopy).CopyTo(buffer);
				arrayGcHandle.Free();
				writer.Advance(bytesToCopy);
				fromIndex += elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads an array of primitive value types (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <param name="type">Type of an array element.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalArrayOfPrimitives(Stream stream, Type type, int elementSize)
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

			// create an array of the specified type
			var array = Array.CreateInstance(type, lengths, lowerBounds);

			// read array data
			int size = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region System.Decimal

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteMultidimensionalArrayOfDecimal(Array array, IBufferWriter<byte> writer)
		{
			const int elementSize = 16;

			// write payload type and array dimensions
			int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
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
					MemoryMarshal.Cast<int, byte>(bits).CopyTo(buffer.Slice(bufferIndex));
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
		/// Reads an array of <see cref="System.Decimal"/> from a stream (for arrays with non-zero-based indexing and/or multiple dimensions).
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
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
				if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
				Buffer.BlockCopy(TempBuffer_Buffer, 0, TempBuffer_Int32, 0, elementSize);
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
			var buffer = writer.GetSpan(maxBufferSize);
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
			var type = mCurrentDeserializedType.Type;

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

		#endregion

		#region Helpers

		/// <summary>
		/// Increments a multidimensional array indexing vector.
		/// </summary>
		/// <param name="indices">Indexing vector.</param>
		/// <param name="array">Array that is being accessed.</param>
		private static void IncrementArrayIndices(int[] indices, Array array)
		{
			for (int i = indices.Length; i > 0; i--)
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
	}

}
