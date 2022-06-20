///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Helper methods for encoding/decoding integers using the SLEB128/ULEB128 encoding.
	/// </summary>
	static class Leb128EncodingHelper
	{
		/// <summary>
		/// Maximum number of bytes for encoding a signed or unsigned 32 bit integer value.
		/// </summary>
		public const int MaxBytesFor32BitValue = 5;

		/// <summary>
		/// Maximum number of bytes for encoding a signed or unsigned 64 bit integer value.
		/// </summary>
		public const int MaxBytesFor64BitValue = 10;

		#region 32-Bit Signed Integer

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 1 byte.
		/// </summary>
		public const int Int32MinValueEncodedWith1Byte = -64;

		/// <summary>
		/// Maximum <see cref="System.Int32"/> value that can be encoded using 1 byte.
		/// </summary>
		public const int Int32MaxValueEncodedWith1Byte = 63;

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const int Int32MinValueEncodedWith2Bytes = -8192;

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const int Int32MaxValueEncodedWith2Bytes = 8191;

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const int Int32MinValueEncodedWith3Bytes = -1048576;

		/// <summary>
		/// Maximum <see cref="System.Int32"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const int Int32MaxValueEncodedWith3Bytes = 1048575;

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const int Int32MinValueEncodedWith4Bytes = -134217728;

		/// <summary>
		/// Maximum <see cref="System.Int32"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const int Int32MaxValueEncodedWith4Bytes = 134217727;

		/// <summary>
		/// Minimum <see cref="System.Int32"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const int Int32MinValueEncodedWith5Bytes = int.MinValue;

		/// <summary>
		/// Maximum <see cref="System.Int32"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const int Int32MaxValueEncodedWith5Bytes = int.MaxValue;

		/// <summary>
		/// Determines how many bytes are needed to encode the specified signed integer using the SLEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(int value)
		{
			if (value < 0)
			{
				if (value >= Int32MinValueEncodedWith1Byte) return 1;
				if (value >= Int32MinValueEncodedWith2Bytes) return 2;
				if (value >= Int32MinValueEncodedWith3Bytes) return 3;
				if (value >= Int32MinValueEncodedWith4Bytes) return 4;
				return 5;
			}

			if (value <= Int32MaxValueEncodedWith1Byte) return 1;
			if (value <= Int32MaxValueEncodedWith2Bytes) return 2;
			if (value <= Int32MaxValueEncodedWith3Bytes) return 3;
			if (value <= Int32MaxValueEncodedWith4Bytes) return 4;
			return 5;
		}

		/// <summary>
		/// Writes a signed integer into the specified byte array using the SLEB128 encoding.
		/// </summary>
		/// <param name="array">Array to write the integer to.</param>
		/// <param name="offset">Offset in the array to start writing at.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(byte[] array, int offset, int value)
		{
			bool more = true;
			int count = 0;
			bool negative = value < 0;

			while (more)
			{
				int data = value & 0x7F;
				value >>= 7;
				if (negative)
				{
					value |= -(1 << (32 - 7)); // sign extend
				}

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				array[offset++] = (byte)data;
				count++;
			}

			return count;
		}

		/// <summary>
		/// Writes a signed integer into the specified buffer using the SLEB128 encoding.
		/// </summary>
		/// <param name="buffer">Buffer to write the integer to.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Span<byte> buffer, int value)
		{
			bool more = true;
			int offset = 0;
			bool negative = value < 0;

			while (more)
			{
				int data = value & 0x7F;
				value >>= 7;
				if (negative)
				{
					value |= -(1 << (32 - 7)); // sign extend
				}

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				buffer[offset++] = (byte)data;
			}

			return offset;
		}

		/// <summary>
		/// Reads a SLEB128 encoded signed integer from the specified byte array (max. 32 bit).
		/// </summary>
		/// <param name="array">Array containing a SLEB128 encoded integer.</param>
		/// <param name="offset">Offset in the array to start reading at.</param>
		/// <param name="count">Maximum number of bytes to read.</param>
		/// <param name="size">Receives the number of bytes read from the array.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The SLEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after five bytes (the maximum for SLEB128 encoded 32-bit signed integers).
		/// </exception>
		public static int ReadInt32(
			byte[]  array,
			int     offset,
			int     count,
			out int size)
		{
			int result = 0;
			int shift = 0;
			size = 0;

			for (int i = 0; i < 5; i++)
			{
				if (count-- == 0) throw new SerializationException("Incomplete SLEB128 encoded integer.");
				size++;
				int data = array[offset++];
				result |= (data & 0x7F) << shift;
				shift += 7;
				if ((data & 0x80) == 0)
				{
					// sign extend, if the integer is negative
					if (shift < 32 && (data & 0x40) != 0)
						result |= -(1 << shift);

					return result;
				}
			}

			throw new SerializationException("SLEB128 encoded integer did not stop after 5 bytes.");
		}

		/// <summary>
		/// Writes a signed integer to the specified stream using the SLEB128 encoding.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="value">Integer to write to the stream.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Stream stream, int value)
		{
			bool more = true;
			int count = 0;
			bool negative = value < 0;

			while (more)
			{
				int data = value & 0x7F;
				value >>= 7;
				if (negative) value |= -(1 << (32 - 7)); // sign extend

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				stream.WriteByte((byte)data);
				count++;
			}

			return count;
		}

		/// <summary>
		/// Reads a SLEB128 encoded signed integer from the specified stream (max. 32 bit).
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The SLEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after five bytes (the maximum for SLEB128 encoded 32-bit signed integers).
		/// </exception>
		public static int ReadInt32(Stream stream)
		{
			int result = 0;
			int shift = 0;

			for (int i = 0; i < 5; i++)
			{
				int data = stream.ReadByte();
				if (data < 0) throw new SerializationException("Incomplete SLEB128 encoded integer.");
				result |= (data & 0x7F) << shift;
				shift += 7;
				if ((data & 0x80) == 0)
				{
					// sign extend, if the integer is negative
					if (shift < 32 && (data & 0x40) != 0)
						result |= -(1 << shift);

					return result;
				}
			}

			throw new SerializationException("SLEB128 encoded integer did not stop after 5 bytes.");
		}

		#endregion

		#region 32-Bit Unsigned Integer

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 1 byte.
		/// </summary>
		public const uint UInt32MaxValueEncodedWith1Byte = 0x0000007F;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const uint UInt32MaxValueEncodedWith2Bytes = 0x00003FFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const uint UInt32MaxValueEncodedWith3Bytes = 0x001FFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const uint UInt32MaxValueEncodedWith4Bytes = 0x0FFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const uint UInt32MaxValueEncodedWith5Bytes = uint.MaxValue;

		/// <summary>
		/// Determines how many bytes are needed to encode a the specified unsigned integer using the ULEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(uint value)
		{
			if (value <= UInt32MaxValueEncodedWith1Byte) return 1;  // 7 bits
			if (value <= UInt32MaxValueEncodedWith2Bytes) return 2; // 14 bits
			if (value <= UInt32MaxValueEncodedWith3Bytes) return 3; // 21 bits
			if (value <= UInt32MaxValueEncodedWith4Bytes) return 4; // 28 bits
			return 5;
		}

		/// <summary>
		/// Writes an unsigned integer into the specified byte array using the ULEB128 encoding.
		/// </summary>
		/// <param name="array">Array to write the integer to.</param>
		/// <param name="offset">Offset in the array to start writing at.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(byte[] array, int offset, uint value)
		{
			int count = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				array[offset] = value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite;
				value >>= 7;
				offset++;
				count++;
			} while (value != 0);

			return count;
		}

		/// <summary>
		/// Writes an unsigned integer into the specified buffer using the ULEB128 encoding.
		/// </summary>
		/// <param name="buffer">Buffer to write the integer to.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Span<byte> buffer, uint value)
		{
			int offset = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				buffer[offset] = value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite;
				value >>= 7;
				offset++;
			} while (value != 0);

			return offset;
		}

		/// <summary>
		/// Reads an ULEB128 encoded unsigned integer from the specified byte array (max. 32 bit).
		/// </summary>
		/// <param name="array">Array containing an ULEB128 encoded integer.</param>
		/// <param name="offset">Offset in the array to start reading at.</param>
		/// <param name="count">Maximum number of bytes to read.</param>
		/// <param name="size">Receives the number of bytes read from the array.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The ULEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after five bytes (the maximum for ULEB128 encoded 32-bit unsigned integers).
		/// </exception>
		public static uint ReadUInt32(
			byte[]  array,
			int     offset,
			int     count,
			out int size)
		{
			uint value = 0;
			size = 0;

			for (int i = 0; i < 5; i++)
			{
				if (count-- == 0) throw new SerializationException("Incomplete ULEB128 encoded integer.");
				size++;
				uint readByte = array[offset++];
				value |= (readByte & 0x7F) << (7 * i);
				if ((readByte & 0x80) == 0) return value;
			}

			throw new SerializationException("ULEB128 encoded integer did not stop after 5 bytes.");
		}

		/// <summary>
		/// Writes an unsigned integer to the specified stream using the ULEB128 encoding.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="value">Integer to write to the stream.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Stream stream, uint value)
		{
			int count = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				stream.WriteByte(value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite);
				value >>= 7;
				count++;
			} while (value != 0);

			return count;
		}

		/// <summary>
		/// Reads an ULEB128 encoded unsigned integer from the specified stream (max. 32 bit).
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The ULEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after five bytes (the maximum for ULEB128 encoded 32-bit unsigned integers).
		/// </exception>
		public static uint ReadUInt32(Stream stream)
		{
			uint value = 0;
			for (int i = 0; i < 5; i++)
			{
				int readByte = stream.ReadByte();
				if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
				value |= ((uint)readByte & 0x7F) << (7 * i);
				if ((readByte & 0x80) == 0) return value;
			}

			throw new SerializationException("ULEB128 encoded integer did not stop after 5 bytes.");
		}

		#endregion

		#region 64-Bit Signed Integer

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 1 byte.
		/// </summary>
		public const long Int64MinValueEncodedWith1Byte = -64;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 1 byte.
		/// </summary>
		public const long Int64MaxValueEncodedWith1Byte = 63;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith2Bytes = -8192;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith2Bytes = 8191;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith3Bytes = -1048576;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith3Bytes = 1048575;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith4Bytes = -134217728;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith4Bytes = 134217727;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith5Bytes = -17179869184;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith5Bytes = 17179869183;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 6 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith6Bytes = -2199023255552;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 6 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith6Bytes = 2199023255551;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 7 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith7Bytes = -281474976710656;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 7 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith7Bytes = 281474976710655;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 8 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith8Bytes = -36028797018963968;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 8 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith8Bytes = 36028797018963967;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 9 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith9Bytes = -4611686018427387904;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 9 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith9Bytes = 4611686018427387903;

		/// <summary>
		/// Minimum <see cref="System.Int64"/> value that can be encoded using 10 bytes.
		/// </summary>
		public const long Int64MinValueEncodedWith10Bytes = long.MinValue;

		/// <summary>
		/// Maximum <see cref="System.Int64"/> value that can be encoded using 10 bytes.
		/// </summary>
		public const long Int64MaxValueEncodedWith10Bytes = long.MaxValue;


		/// <summary>
		/// Determines how many bytes are needed to encode a the specified signed integer using the SLEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(long value)
		{
			if (value < 0)
			{
				if (value >= Int64MinValueEncodedWith1Byte) return 1;
				if (value >= Int64MinValueEncodedWith2Bytes) return 2;
				if (value >= Int64MinValueEncodedWith3Bytes) return 3;
				if (value >= Int64MinValueEncodedWith4Bytes) return 4;
				if (value >= Int64MinValueEncodedWith5Bytes) return 5;
				if (value >= Int64MinValueEncodedWith6Bytes) return 6;
				if (value >= Int64MinValueEncodedWith7Bytes) return 7;
				if (value >= Int64MinValueEncodedWith8Bytes) return 8;
				if (value >= Int64MinValueEncodedWith9Bytes) return 9;
				return 10;
			}

			if (value <= Int64MaxValueEncodedWith1Byte) return 1;
			if (value <= Int64MaxValueEncodedWith2Bytes) return 2;
			if (value <= Int64MaxValueEncodedWith3Bytes) return 3;
			if (value <= Int64MaxValueEncodedWith4Bytes) return 4;
			if (value <= Int64MaxValueEncodedWith5Bytes) return 5;
			if (value <= Int64MaxValueEncodedWith6Bytes) return 6;
			if (value <= Int64MaxValueEncodedWith7Bytes) return 7;
			if (value <= Int64MaxValueEncodedWith8Bytes) return 8;
			if (value <= Int64MaxValueEncodedWith9Bytes) return 9;
			return 10;
		}

		/// <summary>
		/// Writes a signed integer into the specified byte array using the SLEB128 encoding.
		/// </summary>
		/// <param name="array">Array to write the integer to.</param>
		/// <param name="offset">Offset in the array to start writing at.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(byte[] array, int offset, long value)
		{
			bool more = true;
			bool negative = value < 0;
			int count = 0;

			while (more)
			{
				long data = value & 0x7F;
				value >>= 7;
				if (negative) value |= -((long)1 << (64 - 7)); // sign extend

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				array[offset++] = (byte)data;
				count++;
			}

			return count;
		}

		/// <summary>
		/// Writes a signed integer into the specified buffer using the SLEB128 encoding.
		/// </summary>
		/// <param name="buffer">Buffer to write the integer to.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Span<byte> buffer, long value)
		{
			bool more = true;
			int offset = 0;
			bool negative = value < 0;

			while (more)
			{
				long data = value & 0x7F;
				value >>= 7;
				if (negative) value |= -((long)1 << (64 - 7)); // sign extend

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				buffer[offset++] = (byte)data;
			}

			return offset;
		}

		/// <summary>
		/// Reads a SLEB128 encoded signed integer from the specified byte array (max. 64 bit).
		/// </summary>
		/// <param name="array">Array containing a SLEB128 encoded integer.</param>
		/// <param name="offset">Offset in the array to start reading at.</param>
		/// <param name="count">Maximum number of bytes to read.</param>
		/// <param name="size">Receives the number of bytes read from the array.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The SLEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after ten bytes (the maximum for SLEB128 encoded 64-bit signed integers).
		/// </exception>
		public static long ReadInt64(
			byte[]  array,
			int     offset,
			int     count,
			out int size)
		{
			long result = 0;
			int shift = 0;
			size = 0;

			for (int i = 0; i < 10; i++)
			{
				if (count-- == 0) throw new SerializationException("Incomplete SLEB128 encoded integer.");
				size++;
				long data = array[offset++];
				result |= (data & 0x7F) << shift;
				shift += 7;
				if ((data & 0x80) == 0)
				{
					// sign extend, if the integer is negative
					if (shift < 64 && (data & 0x40) != 0)
						result |= -((long)1 << shift);

					return result;
				}
			}

			throw new SerializationException("SLEB128 encoded integer did not stop after 10 bytes.");
		}

		/// <summary>
		/// Writes a signed integer to the specified stream using the SLEB128 encoding.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="value">Integer to write to the stream.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Stream stream, long value)
		{
			bool more = true;
			bool negative = value < 0;
			int count = 0;

			while (more)
			{
				long data = value & 0x7F;
				value >>= 7;
				if (negative) value |= -((long)1 << (64 - 7)); // sign extend

				if ((value == 0 && (data & 0x40) == 0) || (value == -1 && (data & 0x40) == 0x40))
				{
					more = false;
				}
				else
				{
					data |= 0x80;
				}

				stream.WriteByte((byte)data);
				count++;
			}

			return count;
		}

		/// <summary>
		/// Reads a SLEB128 encoded signed integer from the specified stream (max. 64 bit).
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The SLEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after ten bytes (the maximum for SLEB128 encoded 64-bit signed integers).
		/// </exception>
		public static long ReadInt64(Stream stream)
		{
			long result = 0;
			int shift = 0;

			for (int i = 0; i < 10; i++)
			{
				long data = stream.ReadByte();
				if (data < 0) throw new SerializationException("Incomplete SLEB128 encoded integer.");
				result |= (data & 0x7F) << shift;
				shift += 7;
				if ((data & 0x80) == 0)
				{
					// sign extend, if the integer is negative
					if (shift < 64 && (data & 0x40) != 0)
						result |= -((long)1 << shift);

					return result;
				}
			}

			throw new SerializationException("SLEB128 encoded integer did not stop after 10 bytes.");
		}

		#endregion

		#region 64-Bit Unsigned Integer

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 1 byte.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith1Byte = 0x000000000000007F;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 2 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith2Bytes = 0x0000000000003FFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 3 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith3Bytes = 0x00000000001FFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 4 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith4Bytes = 0x000000000FFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 5 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith5Bytes = 0x00000007FFFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 6 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith6Bytes = 0x000003FFFFFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 7 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith7Bytes = 0x0001FFFFFFFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 8 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith8Bytes = 0x00FFFFFFFFFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 9 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith9Bytes = 0x7FFFFFFFFFFFFFFF;

		/// <summary>
		/// Maximum <see cref="System.UInt64"/> value that can be encoded using 10 bytes.
		/// </summary>
		public const ulong UInt64MaxValueEncodedWith10Bytes = 0xFFFFFFFFFFFFFFFF;

		/// <summary>
		/// Determines how many bytes are needed to encode a the specified unsigned integer using the ULEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(ulong value)
		{
			if (value <= UInt64MaxValueEncodedWith1Byte) return 1;  // 7 bits
			if (value <= UInt64MaxValueEncodedWith2Bytes) return 2; // 14 bits
			if (value <= UInt64MaxValueEncodedWith3Bytes) return 3; // 21 bits
			if (value <= UInt64MaxValueEncodedWith4Bytes) return 4; // 28 bits
			if (value <= UInt64MaxValueEncodedWith5Bytes) return 5; // 35 bits
			if (value <= UInt64MaxValueEncodedWith6Bytes) return 6; // 42 bits
			if (value <= UInt64MaxValueEncodedWith7Bytes) return 7; // 49 bits
			if (value <= UInt64MaxValueEncodedWith8Bytes) return 8; // 56 bits
			if (value <= UInt64MaxValueEncodedWith9Bytes) return 9; // 63 bits
			return 10;
		}

		/// <summary>
		/// Writes an unsigned integer into the specified byte array using the ULEB128 encoding.
		/// </summary>
		/// <param name="array">Array to write the integer to.</param>
		/// <param name="offset">Offset in the array to start writing at.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(byte[] array, int offset, ulong value)
		{
			int count = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				array[offset] = value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite;
				value >>= 7;
				offset++;
				count++;
			} while (value != 0);

			return count;
		}

		/// <summary>
		/// Writes an unsigned integer into the specified buffer using the ULEB128 encoding.
		/// </summary>
		/// <param name="buffer">Buffer to write the integer to.</param>
		/// <param name="value">Integer to write.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Span<byte> buffer, ulong value)
		{
			int offset = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				buffer[offset] = value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite;
				value >>= 7;
				offset++;
			} while (value != 0);

			return offset;
		}

		/// <summary>
		/// Reads a ULEB128 encoded unsigned integer from the specified byte array (max. 64 bit).
		/// </summary>
		/// <param name="array">Array containing an ULEB128 encoded integer.</param>
		/// <param name="offset">Offset in the array to start reading at.</param>
		/// <param name="count">Maximum number of bytes to read.</param>
		/// <param name="size">Receives the number of bytes read from the array.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The ULEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after ten bytes (the maximum for ULEB128 encoded 64-bit unsigned integers).
		/// </exception>
		public static ulong ReadUInt64(
			byte[]  array,
			int     offset,
			int     count,
			out int size)
		{
			ulong value = 0;
			size = 0;

			for (int i = 0; i < 10; i++)
			{
				if (count-- == 0) throw new SerializationException("Incomplete ULEB128 encoded integer.");
				size++;
				uint readByte = array[offset++];
				value |= ((ulong)readByte & 0x7F) << (7 * i);
				if ((readByte & 0x80) == 0) return value;
			}

			throw new SerializationException("ULEB128 encoded integer did not stop after 10 bytes.");
		}

		/// <summary>
		/// Writes an unsigned integer to the specified stream using the ULEB128 encoding.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="value">Integer to write to the stream.</param>
		/// <returns>Number of bytes written.</returns>
		public static int Write(Stream stream, ulong value)
		{
			int count = 0;

			do
			{
				byte byteToWrite = (byte)(value & 0x7F);
				stream.WriteByte(value > 0x7F ? (byte)(byteToWrite | 0x80) : byteToWrite);
				value >>= 7;
				count++;
			} while (value != 0);

			return count;
		}


		/// <summary>
		/// Reads a ULEB128 encoded unsigned integer from the specified stream (max. 64 bit).
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>The read integer.</returns>
		/// <exception cref="SerializationException">
		/// The ULEB128 encoded integer is incomplete -or-
		/// the encoded integer did not stop after ten bytes (the maximum for ULEB128 encoded 64-bit unsigned integers).
		/// </exception>
		public static ulong ReadUInt64(Stream stream)
		{
			ulong value = 0;
			for (int i = 0; i < 10; i++)
			{
				int readByte = stream.ReadByte();
				if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
				value |= ((ulong)readByte & 0x7F) << (7 * i);
				if ((readByte & 0x80) == 0) return value;
			}

			throw new SerializationException("ULEB128 encoded integer did not stop after 10 bytes.");
		}

		#endregion
	}

}
