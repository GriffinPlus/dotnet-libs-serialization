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
		#region Temporary Buffers

		internal byte[]    mTempBuffer_UInt8     = new byte[1];
		internal ushort[]  mTempBuffer_UInt16    = new ushort[1];
		internal uint[]    mTempBuffer_UInt32    = new uint[1];
		internal ulong[]   mTempBuffer_UInt64    = new ulong[1];
		internal sbyte[]   mTempBuffer_Int8      = new sbyte[1];
		internal short[]   mTempBuffer_Int16     = new short[1];
		internal int[]     mTempBuffer_Int32     = new int[4]; // intermediate buffer for decimal conversion as well
		internal long[]    mTempBuffer_Int64     = new long[1];
		internal char[]    mTempBuffer_Char      = new char[1];
		internal float[]   mTempBuffer_Single    = new float[1];
		internal double[]  mTempBuffer_Double    = new double[1];
		internal decimal[] mTempBuffer_Decimal   = new decimal[1];
		internal byte[]    mTempBuffer_Buffer    = new byte[16];
		internal byte[]    mTempBuffer_BigBuffer = new byte[256]; // resized on demand

		#endregion

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
			return stream.ReadByte() != 0;
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.Char;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, (uint)value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Char"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal char ReadPrimitive_Char(Stream stream)
		{
			return (char)LEB128.ReadUInt32(stream);
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
			stream.WriteByte((byte)PayloadType.Decimal);
			int[] bits = decimal.GetBits(value);
			Buffer.BlockCopy(bits, 0, mTempBuffer_Buffer, 0, 16);
			stream.Write(mTempBuffer_Buffer, 0, 16);
		}

		/// <summary>
		/// Reads a <see cref="System.Decimal"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal decimal ReadPrimitive_Decimal(Stream stream)
		{
			stream.Read(mTempBuffer_Buffer, 0, 16);
			Buffer.BlockCopy(mTempBuffer_Buffer, 0, mTempBuffer_Int32, 0, 16);
			return new decimal(mTempBuffer_Int32);
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
			stream.WriteByte((byte)PayloadType.Single);
			mTempBuffer_Single[0] = value;
			Buffer.BlockCopy(mTempBuffer_Single, 0, mTempBuffer_Buffer, 0, 4);
			stream.Write(mTempBuffer_Buffer, 0, 4);
		}

		/// <summary>
		/// Reads a <see cref="System.Single"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal float ReadPrimitive_Single(Stream stream)
		{
			stream.Read(mTempBuffer_Buffer, 0, 4);
			return BitConverter.ToSingle(mTempBuffer_Buffer, 0);
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
			stream.WriteByte((byte)PayloadType.Double);
			mTempBuffer_Double[0] = value;
			Buffer.BlockCopy(mTempBuffer_Double, 0, mTempBuffer_Buffer, 0, 8);
			stream.Write(mTempBuffer_Buffer, 0, 8);
		}

		/// <summary>
		/// Reads a <see cref="System.Double"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal double ReadPrimitive_Double(Stream stream)
		{
			stream.Read(mTempBuffer_Buffer, 0, 8);
			return BitConverter.ToDouble(mTempBuffer_Buffer, 0);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.SByte;
			mTempBuffer_Buffer[1] = (byte)value;
			stream.Write(mTempBuffer_Buffer, 0, 2);
		}

		/// <summary>
		/// Reads a <see cref="System.SByte"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal sbyte ReadPrimitive_SByte(Stream stream)
		{
			return (sbyte)stream.ReadByte();
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.Int16;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int16"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal short ReadPrimitive_Int16(Stream stream)
		{
			return (short)LEB128.ReadInt32(stream);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.Int32;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int32"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal int ReadPrimitive_Int32(Stream stream)
		{
			return LEB128.ReadInt32(stream);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.Int64;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.Int64"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal long ReadPrimitive_Int64(Stream stream)
		{
			return LEB128.ReadInt64(stream);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.Byte;
			mTempBuffer_Buffer[1] = value;
			stream.Write(mTempBuffer_Buffer, 0, 2);
		}

		/// <summary>
		/// Reads a <see cref="System.Byte"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal byte ReadPrimitive_Byte(Stream stream)
		{
			return (byte)stream.ReadByte();
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.UInt16;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, (uint)value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt16"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal ushort ReadPrimitive_UInt16(Stream stream)
		{
			return (ushort)LEB128.ReadUInt32(stream);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.UInt32;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt32"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal uint ReadPrimitive_UInt32(Stream stream)
		{
			return LEB128.ReadUInt32(stream);
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
			mTempBuffer_Buffer[0] = (byte)PayloadType.UInt64;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, value);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);
		}

		/// <summary>
		/// Reads a <see cref="System.UInt64"/> value.
		/// </summary>
		/// <param name="stream">Stream to read the value from.</param>
		/// <returns>The read value.</returns>
		internal ulong ReadPrimitive_UInt64(Stream stream)
		{
			return LEB128.ReadUInt64(stream);
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
			stream.WriteByte((byte)PayloadType.DateTime);
			mTempBuffer_Int64[0] = value.ToBinary();
			Buffer.BlockCopy(mTempBuffer_Int64, 0, mTempBuffer_Buffer, 0, 8);
			stream.Write(mTempBuffer_Buffer, 0, 8);
		}

		/// <summary>
		/// read a <see cref="System.DateTime"/> object.
		/// </summary>
		/// <param name="stream">Stream to read the DateTime object from.</param>
		/// <returns>The read <see cref="System.DateTime"/> object.</returns>
		internal DateTime ReadPrimitive_DateTime(Stream stream)
		{
			stream.Read(mTempBuffer_Buffer, 0, 8);
			long l = BitConverter.ToInt64(mTempBuffer_Buffer, 0);
			return DateTime.FromBinary(l);
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
			// convert string to utf-8
			int size = Encoding.UTF8.GetMaxByteCount(value.Length);
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			int valueByteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, mTempBuffer_BigBuffer, 0);

			// write header
			mTempBuffer_Buffer[0] = (byte)PayloadType.String;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, valueByteCount);
			stream.Write(mTempBuffer_Buffer, 0, 1 + count);

			// write string
			stream.Write(mTempBuffer_BigBuffer, 0, valueByteCount);
			mSerializedObjectIdTable.Add(value, mNextSerializedObjectId++);
		}

		/// <summary>
		/// Reads a <see cref="System.String"/> object.
		/// </summary>
		/// <param name="stream">Stream to read the string object from.</param>
		/// <returns>The read string.</returns>
		internal string ReadPrimitive_String(Stream stream)
		{
			int size = LEB128.ReadInt32(stream);

			// read encoded string
			if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
			stream.Read(mTempBuffer_BigBuffer, 0, size);

			// decode string
			string s = Encoding.UTF8.GetString(mTempBuffer_BigBuffer, 0, size);
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, s);
			return s;
		}

		#endregion

		#region System.Object

		/// <summary>
		/// Writes a <see cref="System.Object"/> object.
		/// </summary>
		/// <param name="obj">Object to write.</param>
		/// <param name="stream">Stream to write the string to.</param>
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
