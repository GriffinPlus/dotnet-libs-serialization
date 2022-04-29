///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace GriffinPlus.Lib.Serialization
{

	partial class Serializer
	{
		#region System.Boolean

		/// <summary>
		/// Writes a <see cref="System.Boolean"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Boolean(bool value, Stream stream)
		{
			stream.WriteByte((byte)PayloadType.Boolean);
			stream.WriteByte((byte)(value ? 1 : 0));
		}

		/// <summary>
		/// Reads a <see cref="System.Boolean"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal bool ReadPrimitive_Boolean(Stream stream)
		{
			int readByte = stream.ReadByte();
			if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
			return readByte != 0;
		}

		#endregion

		#region System.Char

		/// <summary>
		/// Writes a <see cref="System.Char"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Char(char value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.Char;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, (uint)value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Char"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal char ReadPrimitive_Char(Stream stream)
		{
			return (char)Leb128EncodingHelper.ReadUInt32(stream);
		}

		#endregion

		#region System.Decimal

		/// <summary>
		/// Writes a <see cref="System.Decimal"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Decimal(decimal value, Stream stream)
		{
			const int elementSize = 16;
			stream.WriteByte((byte)PayloadType.Decimal);
			int[] bits = decimal.GetBits(value);
			Buffer.BlockCopy(bits, 0, TempBuffer_Buffer, 0, elementSize);
			stream.Write(TempBuffer_Buffer, 0, elementSize);
		}

		/// <summary>
		/// Reads a <see cref="System.Decimal"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal decimal ReadPrimitive_Decimal(Stream stream)
		{
			const int elementSize = 16;
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
			if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
			Buffer.BlockCopy(TempBuffer_Buffer, 0, TempBuffer_Int32, 0, elementSize);
			return new decimal(TempBuffer_Int32);
		}

		#endregion

		#region System.Single

		/// <summary>
		/// Writes a <see cref="System.Single"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Single(float value, Stream stream)
		{
			const int elementSize = 4;
			stream.WriteByte((byte)PayloadType.Single);
			TempBuffer_Single[0] = value;
			Buffer.BlockCopy(TempBuffer_Single, 0, TempBuffer_Buffer, 0, elementSize);
			stream.Write(TempBuffer_Buffer, 0, elementSize);
		}

		/// <summary>
		/// Reads a <see cref="System.Single"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal float ReadPrimitive_Single(Stream stream)
		{
			const int elementSize = 4;
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
			if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
			return BitConverter.ToSingle(TempBuffer_Buffer, 0);
		}

		#endregion

		#region System.Double

		/// <summary>
		/// Writes a <see cref="System.Double"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Double(double value, Stream stream)
		{
			const int elementSize = 8;
			stream.WriteByte((byte)PayloadType.Double);
			TempBuffer_Double[0] = value;
			Buffer.BlockCopy(TempBuffer_Double, 0, TempBuffer_Buffer, 0, elementSize);
			stream.Write(TempBuffer_Buffer, 0, elementSize);
		}

		/// <summary>
		/// Reads a <see cref="System.Double"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal double ReadPrimitive_Double(Stream stream)
		{
			const int elementSize = 8;
			int bytesRead = stream.Read(TempBuffer_Buffer, 0, elementSize);
			if (bytesRead < elementSize) throw new SerializationException("Unexpected end of stream.");
			return BitConverter.ToDouble(TempBuffer_Buffer, 0);
		}

		#endregion

		#region System.SByte

		/// <summary>
		/// Writes a <see cref="System.SByte"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_SByte(sbyte value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.SByte;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.SByte"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal sbyte ReadPrimitive_SByte(Stream stream)
		{
			return (sbyte)Leb128EncodingHelper.ReadInt32(stream);
		}

		#endregion

		#region System.Int16

		/// <summary>
		/// Writes a <see cref="System.Int16"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Int16(short value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.Int16;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int16"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal short ReadPrimitive_Int16(Stream stream)
		{
			return (short)Leb128EncodingHelper.ReadInt32(stream);
		}

		#endregion

		#region System.Int32

		/// <summary>
		/// Writes a <see cref="System.Int32"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Int32(int value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.Int32;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int32"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal int ReadPrimitive_Int32(Stream stream)
		{
			return Leb128EncodingHelper.ReadInt32(stream);
		}

		#endregion

		#region System.Int64

		/// <summary>
		/// Writes a <see cref="System.Int64"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Int64(long value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.Int64;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int64"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal long ReadPrimitive_Int64(Stream stream)
		{
			return Leb128EncodingHelper.ReadInt64(stream);
		}

		#endregion

		#region System.Byte

		/// <summary>
		/// Writes a <see cref="System.Byte"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_Byte(byte value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.Byte;
			TempBuffer_Buffer[1] = value;
			stream.Write(TempBuffer_Buffer, 0, 2);
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
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_UInt16(ushort value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.UInt16;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, (uint)value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt16"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal ushort ReadPrimitive_UInt16(Stream stream)
		{
			return (ushort)Leb128EncodingHelper.ReadUInt32(stream);
		}

		#endregion

		#region System.UInt32

		/// <summary>
		/// Writes a <see cref="System.UInt32"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_UInt32(uint value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.UInt32;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt32"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal uint ReadPrimitive_UInt32(Stream stream)
		{
			return Leb128EncodingHelper.ReadUInt32(stream);
		}

		#endregion

		#region System.UInt64

		/// <summary>
		/// Writes a <see cref="System.UInt64"/> value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="stream">Stream to write the value to.</param>
		internal void WritePrimitive_UInt64(ulong value, Stream stream)
		{
			TempBuffer_Buffer[0] = (byte)PayloadType.UInt64;
			int count = Leb128EncodingHelper.Write(TempBuffer_Buffer, 1, value);
			stream.Write(TempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt64"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal ulong ReadPrimitive_UInt64(Stream stream)
		{
			return Leb128EncodingHelper.ReadUInt64(stream);
		}

		#endregion

		#region System.DateTime

		/// <summary>
		/// Writes a <see cref="System.DateTime"/> object.
		/// </summary>
		/// <param name="value">DateTime object to write.</param>
		/// <param name="stream">Stream to write the <see cref="System.DateTime"/> object to.</param>
		internal void WritePrimitive_DateTime(DateTime value, Stream stream)
		{
			const int elementSize = 8;
			stream.WriteByte((byte)PayloadType.DateTime);
			TempBuffer_Int64[0] = value.ToBinary();
			Buffer.BlockCopy(TempBuffer_Int64, 0, TempBuffer_Buffer, 0, elementSize);
			stream.Write(TempBuffer_Buffer, 0, elementSize);
		}

		/// <summary>
		/// read a <see cref="System.DateTime"/> object.
		/// </summary>
		/// <param name="stream">Stream to read the DateTime object from.</param>
		/// <returns>The read <see cref="System.DateTime"/> object.</returns>
		internal DateTime ReadPrimitive_DateTime(Stream stream)
		{
			const int elementSize = 8;
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
		/// <param name="stream">Stream to write the string to.</param>
		internal void WritePrimitive_String(string value, Stream stream)
		{
			// resize temporary buffer for the encoding the string
			// (reserve max. expected bytes for the string + max. 5 bytes of LEB128 encoded 32 bit integer + 1 byte for payload type)
			int size = Encoding.UTF8.GetMaxByteCount(value.Length) + 6;
			EnsureTemporaryByteBufferSize(size);

			// encode the string
			int valueByteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, TempBuffer_Buffer, 6);

			// put the header with the payload type and the size of the encoded string in front of the encoded string
			int headerLength = Leb128EncodingHelper.GetByteCount(valueByteCount) + 1;
			TempBuffer_Buffer[6 - headerLength] = (byte)PayloadType.String;
			Leb128EncodingHelper.Write(TempBuffer_Buffer, 7 - headerLength, valueByteCount);

			// write the sequence to the stream
			stream.Write(TempBuffer_Buffer, 6 - headerLength, headerLength + valueByteCount);
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
		/// <param name="stream">Stream to write the object to.</param>
		internal void WritePrimitive_Object(object obj, Stream stream)
		{
			stream.WriteByte((byte)PayloadType.Object);
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
