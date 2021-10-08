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
			mTempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfByte;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, array.Length);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
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
			int sizeByteCount = LEB128.GetByteCount(length);
			int size = 1 + sizeByteCount + length * elementSize;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			mTempBuffer_BigBuffer[0] = (byte)type;
			LEB128.Write(mTempBuffer_BigBuffer, 1, length);
			int index = 1 + sizeByteCount;
			Buffer.BlockCopy(array, 0, mTempBuffer_BigBuffer, index, length * elementSize);
			stream.Write(mTempBuffer_BigBuffer, 0, size);
			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> values (for arrays with zero-based indexing).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteArrayOfDecimal(decimal[] array, Stream stream)
		{
			int length = array.Length;
			int sizeByteCount = LEB128.GetByteCount(length);
			int size = 1 + sizeByteCount + length * 16;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			mTempBuffer_BigBuffer[0] = (byte)PayloadType.ArrayOfDecimal;
			int index = LEB128.Write(mTempBuffer_BigBuffer, 1, length) + 1;
			for (int i = 0; i < length; i++)
			{
				int[] bits = decimal.GetBits(array[i]);
				Buffer.BlockCopy(bits, 0, mTempBuffer_BigBuffer, index, 16);
				index += 16;
			}

			stream.Write(mTempBuffer_BigBuffer, 0, index);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfString;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, array.Length);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);

			// write array data
			for (int i = 0; i < array.Length; i++)
			{
				string s = array[i];
				uint id;
				if (s == null)
				{
					stream.WriteByte((byte)PayloadType.NullReference);
				}
				else if (mSerializedObjectIdTable.TryGetValue(s, out id))
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
			int size;
			int index = PrepareArrayBuffer(PayloadType.ArrayOfDateTime, array.Length, 8, out size);

			for (int i = 0; i < array.Length; i++)
			{
				mTempBuffer_Int64[0] = array[i].ToBinary();
				Buffer.BlockCopy(mTempBuffer_Int64, 0, mTempBuffer_BigBuffer, index + 8 * i, 8);
			}

			stream.Write(mTempBuffer_BigBuffer, 0, size);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.ArrayOfObjects;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, array.Length);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);

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
			int length = LEB128.ReadInt32(stream);

			// read array data
			byte[] array = new byte[length];
			stream.Read(array, 0, length);
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
			int length = LEB128.ReadInt32(stream);
			int size = length * elementSize;

			// read array data
			Array array = FastActivator.CreateArray(type, length);
			if (mTempBuffer_BigBuffer.Length < length) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);
			Buffer.BlockCopy(mTempBuffer_BigBuffer, 0, array, 0, size);

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
			// read array length
			int length = LEB128.ReadInt32(stream);
			int size = 16 * length;

			// read data from stream
			if (mTempBuffer_BigBuffer.Length < length) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);

			// read array data
			decimal[] array = new decimal[length];
			int index = 0;
			for (int i = 0; i < length; i++)
			{
				Buffer.BlockCopy(mTempBuffer_BigBuffer, index, mTempBuffer_Int32, 0, 16);
				array[i] = new decimal(mTempBuffer_Int32);
				index += 16;
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
			int length = LEB128.ReadInt32(stream);

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
			// read array length
			int length = LEB128.ReadInt32(stream);
			int size = 8 * length;

			// read array data
			DateTime[] array = new DateTime[length];
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);
			for (int i = 0; i < length; i++) array[i] = DateTime.FromBinary(BitConverter.ToInt64(mTempBuffer_BigBuffer, 8 * i));

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
		private object ReadArrayOfObjects(Stream stream, object context)
		{
			// assembly and type metadata has been read already

			Type t = mCurrentDeserializedType.Type;

			// read array length
			int length = LEB128.ReadInt32(stream);

			// read array elements
			Array array = FastActivator.CreateArray(t, length);
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
			stream.WriteByte((byte)type);     // payload type
			LEB128.Write(stream, array.Rank); // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				LEB128.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				LEB128.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
			}

			int size = totalCount * elementSize;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			Buffer.BlockCopy(array, 0, mTempBuffer_BigBuffer, 0, size);
			stream.Write(mTempBuffer_BigBuffer, 0, size);

			mSerializedObjectIdTable.Add(array, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Writes an array of <see cref="System.Decimal"/> (for arrays with non-zero-based indexing and/or multiple dimensions).
		/// </summary>
		/// <param name="array">Array to write.</param>
		/// <param name="stream">Stream to write the array to.</param>
		private void WriteMultidimensionalArrayOfDecimal(Array array, Stream stream)
		{
			//int totalCount = 1;
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfDecimal); // payload type
			LEB128.Write(stream, array.Rank);                                   // number of dimensions
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				indices[i] = array.GetLowerBound(i);
				LEB128.Write(stream, indices[i]); // lower bound of the dimension
				int count = array.GetLength(i);
				LEB128.Write(stream, count); // number of elements in the dimension
				//totalCount *= count;
			}

			// write array elements
			for (int i = 0; i < array.Length; i++)
			{
				decimal value = (decimal)array.GetValue(indices);
				int[] bits = decimal.GetBits(value);
				Buffer.BlockCopy(bits, 0, mTempBuffer_Buffer, 0, 16);
				stream.Write(mTempBuffer_Buffer, 0, 16);
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
			int totalCount = 1;
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfString); // payload type
			LEB128.Write(stream, array.Rank);                                  // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				LEB128.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				LEB128.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
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
					uint id;
					if (mSerializedObjectIdTable.TryGetValue(s, out id))
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
			int totalCount = 1;
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfDateTime); // payload type
			LEB128.Write(stream, array.Rank);                                    // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				LEB128.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				LEB128.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
			}

			// prepare indexing array
			int[] indices = new int[array.Rank];
			for (int i = 0; i < array.Rank; i++)
			{
				indices[i] = array.GetLowerBound(i);
			}

			// resize temporary buffer, if necessary
			int size = 8 * totalCount;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];

			// convert array elements
			for (int i = 0; i < array.Length; i++)
			{
				DateTime dt = (DateTime)array.GetValue(indices);
				mTempBuffer_Int64[0] = dt.ToBinary();
				Buffer.BlockCopy(mTempBuffer_Int64, 0, mTempBuffer_BigBuffer, 8 * i, 8);
				IncrementArrayIndices(indices, array);
			}

			// write to stream
			stream.Write(mTempBuffer_BigBuffer, 0, size);
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
			int totalCount = 1;
			stream.WriteByte((byte)PayloadType.MultidimensionalArrayOfObjects); // payload type
			LEB128.Write(stream, array.Rank);                                   // number of dimensions
			for (int i = 0; i < array.Rank; i++)
			{
				// ...dimension information...
				LEB128.Write(stream, array.GetLowerBound(i)); // lower bound of the dimension
				int count = array.GetLength(i);
				LEB128.Write(stream, count); // number of elements in the dimension
				totalCount *= count;
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
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array of bytes
			Array array = Array.CreateInstance(typeof(byte), lengths, lowerBounds);

			// read array data
			int size = totalCount;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);
			Buffer.BlockCopy(mTempBuffer_BigBuffer, 0, array, 0, size);

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
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				totalCount *= lengths[i];
			}

			// create an array of the specified type
			Array array = Array.CreateInstance(type, lengths, lowerBounds);

			// read array data
			int size = totalCount * elementSize;
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);
			Buffer.BlockCopy(mTempBuffer_BigBuffer, 0, array, 0, size);

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
			int totalCount = 1;
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create an array of strings
			Array array = Array.CreateInstance(typeof(string), lengths, lowerBounds);

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
			// read header
			int totalCount = 1;
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create an array of decimals
			Array array = Array.CreateInstance(typeof(decimal), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				stream.Read(mTempBuffer_Buffer, 0, 16);
				Buffer.BlockCopy(mTempBuffer_Buffer, 0, mTempBuffer_Int32, 0, 16);
				decimal value = new decimal(mTempBuffer_Int32);
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
			// read header
			int totalCount = 1;
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create an array of datetimes
			Array array = Array.CreateInstance(typeof(DateTime), lengths, lowerBounds);

			// read array elements
			for (int i = 0; i < array.Length; i++)
			{
				stream.Read(mTempBuffer_Buffer, 0, 8);
				long l = BitConverter.ToInt64(mTempBuffer_Buffer, 0);
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
		private object ReadMultidimensionalArrayOfObjects(Stream stream, object context)
		{
			Type type = mCurrentDeserializedType.Type;

			// read header
			int totalCount = 1;
			int ranks = LEB128.ReadInt32(stream);
			int[] lowerBounds = new int[ranks];
			int[] lengths = new int[ranks];
			int[] indices = new int[ranks];
			for (int i = 0; i < ranks; i++)
			{
				lowerBounds[i] = LEB128.ReadInt32(stream);
				lengths[i] = LEB128.ReadInt32(stream);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create an array with elements of the specified type
			Array array = Array.CreateInstance(type, lengths, lowerBounds);

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
		/// (inits payload type, the length field and reserves space for the value)
		/// </summary>
		/// <param name="type">Type of the payload.</param>
		/// <param name="length">Length of the array (in elements).</param>
		/// <param name="elementSize">Size of an element (in bytes).</param>
		/// <param name="size">Receives the number of valid bytes in <see cref="mTempBuffer_BigBuffer"/>.</param>
		/// <returns>Index in the returned buffer where the array data part begins.</returns>
		private int PrepareArrayBuffer(
			PayloadType type,
			int         length,
			int         elementSize,
			out int     size)
		{
			int sizeByteCount = LEB128.GetByteCount(length);
			size = 1 + sizeByteCount + length * elementSize;
			if (mTempBuffer_BigBuffer.Length < size)
			{
				mTempBuffer_BigBuffer = new byte[size];
			}

			mTempBuffer_BigBuffer[0] = (byte)type;
			LEB128.Write(mTempBuffer_BigBuffer, 1, length);
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
