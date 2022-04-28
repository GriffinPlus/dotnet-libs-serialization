///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace GriffinPlus.Lib.Serialization
{

	partial class Serializer
	{
		#region Serialization of SZARRAYs (One-Dimensional Arrays with Zero-Based Indexing)

		/// <summary>
		/// Writes an array of <see cref="System.Byte"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfByte(byte[] array, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfByte;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, array.Length);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
			stream.Write(array, 0, array.Length);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of primitive value types (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="type">Payload type corresponding to the type of the array.</param>
		/// <param name="array">Array to write.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfPrimitives(
			PayloadType type,
			Array       array,
			int         elementSize,
			Stream      stream)
		{
			int length = array.Length;
			int sizeByteCount = Leb128EncodingHelper.GetByteCount(length);
			int size = 1 + sizeByteCount + length * elementSize;
			EnsureTemporaryByteBufferSize(size);
			TempBuffer_Buffer[0] = (byte)type;
			Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, length);
			int index = 1 + sizeByteCount;
			Buffer.BlockCopy(array, 0, TempBuffer_Buffer, index, length * elementSize);
			stream.Write(TempBuffer_Buffer, 0, size);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfDecimal(decimal[] array, Stream stream)
		{
			const int elementSize = 16;

			int length = array.Length;
			int sizeByteCount = Leb128EncodingHelper.GetByteCount(length);
			int size = 1 + sizeByteCount + length * elementSize;
			EnsureTemporaryByteBufferSize(size);
			TempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfDecimal;
			int index = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, length) + 1;
			for (int i = 0; i < length; i++)
			{
				int[] bits = decimal.GetBits(array[i]);
				Buffer.BlockCopy(bits, 0, TempBuffer_Buffer, index, elementSize);
				index += elementSize;
			}

			stream.Write(TempBuffer_Buffer, 0, index);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.String"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfString(string[] array, Stream stream)
		{
			// write type and array length
			TempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfString;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, array.Length);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);

			// write array data
			foreach (string s in array)
			{
				if (s == null)
				{
					stream.WriteByte((byte)PayloadType.NullReference);
				}
				else if (mSerializedObjectIdTable.TryGetValue(s, out uint id))
				{
					SerializeObjectId(stream, id);
				}
				else
				{
					WritePrimitive_String(s, stream);
				}
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.DateTime"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfDateTime(DateTime[] array, Stream stream)
		{
			const int elementSize = 8;

			int index = PrepareArrayBuffer(PayloadType.ArrayOfDateTime, array.Length, elementSize, out int size);

			foreach (var dt in array)
			{
				TempBuffer_Int64[0] = dt.ToBinary();
				Buffer.BlockCopy(TempBuffer_Int64, 0, TempBuffer_Buffer, index, elementSize);
				index += elementSize;
			}

			stream.Write(TempBuffer_Buffer, 0, size);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of objects (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfObjects(Array array, Stream stream)
		{
			// write type metadata
			WriteTypeMetadata(stream, array.GetType().GetElementType());

			// write type and array length
			TempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfObjects;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, array.Length);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);

			// write array data
			for (int i = 0; i < array.Length; i++)
			{
				InnerSerialize(stream, array.GetValue(i), null);
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
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

			// read array data
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

			// read array data
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

			// read data from stream
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");

			// read array data
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
			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read array data
			string[] array = new string[length];
			for (int i = 0; i < length; i++)
			{
				object obj = InnerDeserialize(stream, null);
				array[i] = (string)obj;
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		/// <summary>
		/// Reads an array of <see cref="System.DateTime"/> from a stream (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private DateTime[] ReadDateTimeArray(Stream stream)
		{
			const int elementSize = 8;

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);
			int size = length * elementSize;

			// read array data
			var array = new DateTime[length];
			EnsureTemporaryByteBufferSize(size);
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, size);
			if (bytesRead < size) throw new SerializationException("Unexpected end of stream.");
			for (int i = 0; i < length; i++) array[i] = DateTime.FromBinary(BitConverter.ToInt64(TempBuffer_Buffer, i * elementSize));

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
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

			// read array length
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read array elements
			var array = FastActivator.CreateArray(t, length);
			for (int i = 0; i < length; i++)
			{
				array.SetValue(InnerDeserialize(stream, context), i);
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region Serialization of MDARRAYs (Multiple Dimensions and/or Non-Zero-Based Indexing)

		/// <summary>
		/// Writes an array of primitive value types (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="type">Payload type corresponding to the type of the array.</param>
		/// <param name="array">Array to write.</param>
		/// <param name="elementSize">Size of an array element.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteMultidimensionalArrayOfPrimitives(
			PayloadType type,
			Array       array,
			int         elementSize,
			Stream      stream)
		{
			int totalCount = 1;
			stream.WriteByte((byte)type);                   // payload type
			Leb128EncodingHelper.Write(stream, array.Rank); // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				Leb128EncodingHelper.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				Leb128EncodingHelper.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
			}

			int size = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(size);
			Buffer.BlockCopy(array, 0, TempBuffer_Buffer, 0, size);
			stream.Write(TempBuffer_Buffer, 0, size);

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteMultidimensionalArrayOfDecimal(Array array, Stream stream)
		{
			const int elementSize = 16;

			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfDecimal); // payload type
			Leb128EncodingHelper.Write(stream, array.Rank);                     // number of dimensions
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				indices[i] = array.GetLowerBound(i);
				Leb128EncodingHelper.Write(stream, indices[i]); // lower bound of the dimension
				int count = array.GetLength(i);
				Leb128EncodingHelper.Write(stream, count); // number of elements in the dimension
			}

			// write array elements
			for (int i = 0; i < array.Length; i++)
			{
				decimal value = (decimal)array.GetValue(indices);
				int[] bits = decimal.GetBits(value);
				Buffer.BlockCopy(bits, 0, TempBuffer_Buffer, 0, elementSize);
				stream.Write(TempBuffer_Buffer, 0, elementSize);
				IncrementArrayIndices(indices, array);
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes a multidimensional array of <see cref="System.String"/> to a stream  (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to serialize.</param>
		/// <param name="stream">Stream to serialize the array to.</param>
		private void WriteMultidimensionalArrayOfString(Array array, Stream stream)
		{
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfString); // payload type
			Leb128EncodingHelper.Write(stream, array.Rank);                    // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				Leb128EncodingHelper.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				Leb128EncodingHelper.Write(stream, count); // number of elements in the dimension
			}

			// prepare indexing array
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				indices[i] = array.GetLowerBound(i);
			}

			// write array elements
			for (int i = 0; i < array.Length; i++)
			{
				string s = (string)array.GetValue(indices);

				if (s != null)
				{
					if (mSerializedObjectIdTable.TryGetValue(s, out uint id))
					{
						SerializeObjectId(stream, id);
					}
					else
					{
						WritePrimitive_String(s, stream);
					}
				}
				else
				{
					stream.WriteByte((byte)PayloadType.NullReference);
				}

				IncrementArrayIndices(indices, array);
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes a multidimensional array of <see cref="System.DateTime"/> to a stream (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to serialize.</param>
		/// <param name="stream">Stream to serialize the array to.</param>
		private void WriteMultidimensionalArrayOfDateTime(Array array, Stream stream)
		{
			const int elementSize = 8;

			int totalCount = 1;
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfDateTime); // payload type
			Leb128EncodingHelper.Write(stream, array.Rank);                      // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				Leb128EncodingHelper.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				Leb128EncodingHelper.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
			}

			// prepare indexing array
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				indices[i] = array.GetLowerBound(i);
			}

			// resize temporary buffer, if necessary
			int size = totalCount * elementSize;
			EnsureTemporaryByteBufferSize(size);

			// convert array elements
			for (int i = 0; i < array.Length; i++)
			{
				var dt = (DateTime)array.GetValue(indices);
				TempBuffer_Int64[0] = dt.ToBinary();
				Buffer.BlockCopy(TempBuffer_Int64, 0, TempBuffer_Buffer, i * elementSize, elementSize);
				IncrementArrayIndices(indices, array);
			}

			// write to stream
			stream.Write(TempBuffer_Buffer, 0, size);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes a multidimensional array of objects (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to serialize.</param>
		/// <param name="stream">Stream to serialize the array to.</param>
		private void WriteMultidimensionalArrayOfObjects(Array array, Stream stream)
		{
			// write type metadata
			WriteTypeMetadata(stream, array.GetType().GetElementType());

			// write header
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfObjects); // payload type
			Leb128EncodingHelper.Write(stream, array.Rank);                     // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				Leb128EncodingHelper.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				Leb128EncodingHelper.Write(stream, count); // number of elements in the dimension
			}

			// prepare indexing array
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				indices[i] = array.GetLowerBound(i);
			}

			// write array elements
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array.GetValue(indices);
				InnerSerialize(stream, obj, null);
				IncrementArrayIndices(indices, array);
			}

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
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

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				object obj = InnerDeserialize(stream, null);
				array.SetValue(obj, indices);
				IncrementArrayIndices(indices, array);
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
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
		/// Reads an array of <see cref="System.DateTime"/> from a stream (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="stream">Stream to read the array from.</param>
		/// <returns>The read array.</returns>
		private Array ReadMultidimensionalDateTimeArray(Stream stream)
		{
			const int elementSize = 8;

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

			// create an array of DateTime
			var array = Array.CreateInstance(typeof(DateTime), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
				if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
				long l = BitConverter.ToInt64(TempBuffer_Buffer, 0);
				array.SetValue(DateTime.FromBinary(l), indices);
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

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				object obj = InnerDeserialize(stream, null);
				array.SetValue(obj, indices);
				IncrementArrayIndices(indices, array);
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, array);
			return array;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Prepares a buffer for array serialization
		/// (initializes the payload type, the length field and reserves space for the value).
		/// </summary>
		/// <param name="type">Type of the payload.</param>
		/// <param name="length">Length of the array (in elements).</param>
		/// <param name="elementSize">Size of an element (in bytes).</param>
		/// <param name="size">Receives the number of valid bytes in <see cref="TempBuffer_Buffer"/>.</param>
		/// <returns>Index in the returned buffer where the array data part begins.</returns>
		private int PrepareArrayBuffer(
			PayloadType type,
			int         length,
			int         elementSize,
			out int     size)
		{
			int sizeByteCount = Leb128EncodingHelper.GetByteCount(length);
			size = 1 + sizeByteCount + length * elementSize;
			EnsureTemporaryByteBufferSize(size);
			TempBuffer_Buffer[0] = (byte)type;
			Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, length);
			return 1 + sizeByteCount;
		}

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
