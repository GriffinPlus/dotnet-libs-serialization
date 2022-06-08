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

		/// <summary>
		/// Writes an array of <see cref="System.Byte"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfByte(byte[] array, IBufferWriter<byte> writer)
		{
			// write payload type and array length
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
		/// Writes an array of primitive value types (for arrays with zero-based indexing).
		/// </summary>
		/// <typeparam name="TElement">Type of an array element.</typeparam>
		/// <param name="payloadType">Payload type corresponding to the type of the array.</param>
		/// <param name="array">Array to write.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfPrimitives<TElement>(
			PayloadType         payloadType,
			TElement[]          array,
			int                 elementSize,
			IBufferWriter<byte> writer) where TElement : struct
		{
			// write payload type and array length
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)payloadType;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// write array elements
			int fromIndex = 0;
			while (fromIndex < array.Length)
			{
				int elementsToCopy = Math.Min(array.Length - fromIndex, MaxChunkSize / elementSize);
				int bytesToCopy = elementsToCopy * elementSize;
				buffer = writer.GetSpan(bytesToCopy);
				MemoryMarshal.Cast<TElement, byte>(array.AsSpan(fromIndex, elementsToCopy)).CopyTo(buffer);
				writer.Advance(bytesToCopy);
				fromIndex += elementsToCopy;
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfDecimal(decimal[] array, IBufferWriter<byte> writer)
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
		/// Writes an array of <see cref="System.String"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteArrayOfString(string[] array, IBufferWriter<byte> writer)
		{
			// write payload type and array length
			int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArrayOfString;
			bufferIndex += Leb128EncodingHelper.Write(buffer.Slice(bufferIndex), array.Length);
			writer.Advance(bufferIndex);

			// assign an object id to the array before serializing its elements
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);

			// write array elements
			foreach (string s in array)
			{
				if (s == null)
				{
					buffer = writer.GetSpan(1);
					buffer[0] = (byte)PayloadType.NullReference;
					writer.Advance(1);
				}
				else if (mSerializedObjectIdTable.TryGetValue(s, out uint id))
				{
					SerializeObjectId(writer, id);
				}
				else
				{
					WritePrimitive_String(s, writer);
				}
			}
		}

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

		#endregion

		#region Deserialization of SZARRAYs (One-Dimensional Arrays with Zero-Based Indexing)

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

		/// <summary>
		/// Reads an array of primitive value types (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <param name="type">Type of an array element.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <returns>The read array.</returns>
		private Array ReadArrayOfPrimitives(Stream stream, Type type, int elementSize)
		{
			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array elements
			var array = FastActivator.CreateArray(type, length);
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, array, 0, size);

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
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
			int index = 0;
			for (int i = 0; i < length; i++)
			{
				Buffer.BlockCopy(TempBuffer_Buffer, index, TempBuffer_Int32, 0, elementSize);
				array[i] = new decimal(TempBuffer_Int32);
				index += elementSize;
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.String"/> from a stream (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private string[] ReadStringArray(Stream stream)
		{
			// read array length and create an uninitialized array
			int length = Leb128EncodingHelper.ReadInt32(stream);
			string[] array = new string[length];

			// assign an object id to the array before deserializing its elements
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			// deserialize array elements
			for (int i = 0; i < length; i++)
			{
				object obj = InnerDeserialize(stream, null);
				array[i] = (string)obj;
			}

			return array;
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

		#region Serialization of MDARRAYs (Multiple Dimensions and/or Non-Zero-Based Indexing)

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
		/// Writes a multidimensional array of <see cref="System.String"/> to a stream (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to serialize.</param>
		/// <param name="writer">Buffer writer to write the array to.</param>
		private void WriteMultidimensionalArrayOfString(Array array, IBufferWriter<byte> writer)
		{
			// write payload type and array dimensions
			int maxBufferSize = 1 + (1 + 2 * array.Rank) * Leb128EncodingHelper.MaxBytesFor32BitValue;
			var buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.MultidimensionalArrayOfString;
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
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);

			// serialize array elements
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < array.Length; i++)
			{
				string s = (string)array.GetValue(indices);

				if (s != null)
				{
					if (mSerializedObjectIdTable.TryGetValue(s, out uint id))
					{
						SerializeObjectId(writer, id);
					}
					else
					{
						WritePrimitive_String(s, writer);
					}
				}
				else
				{
					buffer = writer.GetSpan(1);
					buffer[0] = (byte)PayloadType.NullReference;
					writer.Advance(1);
				}

				IncrementArrayIndices(indices, array);
			}
		}

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

		#endregion

		#region Deserialization of MDARRAYs (Multiple Dimensions and/or Non-Zero-Based Indexing)

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

		/// <summary>
		/// Reads an array of <see cref="System.String"/> from a stream (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalStringArray(Stream stream)
		{
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

			// create an array of strings
			var array = Array.CreateInstance(typeof(string), lengths, lowerBounds);

			// assign an object id to the array before deserializing its elements
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				object obj = InnerDeserialize(stream, null);
				array.SetValue(obj, indices);
				IncrementArrayIndices(indices, array);
			}

			return array;
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
