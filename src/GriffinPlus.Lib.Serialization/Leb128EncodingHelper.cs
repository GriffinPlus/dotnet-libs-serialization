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
		/// Determines how many bytes are needed to encode the specified signed integer using the SLEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(int value)
		{
			unchecked
			{
				if (value < 0)
				{
					if (value >= (int)0xFFFFFFC0) return 1;
					if (value >= (int)0xFFFFE000) return 2;
					if (value >= (int)0xFFF00000) return 3;
					if (value >= (int)0xF8000000) return 4;
					return 5;
				}

				if (value <= 0x0000003F) return 1;
				if (value <= 0x00001FFF) return 2;
				if (value <= 0x000FFFFF) return 3;
				if (value <= 0x07FFFFFF) return 4;
				return 5;
			}
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
		/// Determines how many bytes are needed to encode a the specified unsigned integer using the ULEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(uint value)
		{
			if (value < 0x00000080) return 1; // 7 bits
			if (value < 0x00004000) return 2; // 14 bits
			if (value < 0x00200000) return 3; // 21 bits
			if (value < 0x10000000) return 4; // 28 bits
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
		/// Determines how many bytes are needed to encode a the specified signed integer using the SLEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(long value)
		{
			unchecked
			{
				if (value < 0)
				{
					if (value >= (long)0xFFFFFFFFFFFFFFC0) return 1;
					if (value >= (long)0xFFFFFFFFFFFFE000) return 2;
					if (value >= (long)0xFFFFFFFFFFF00000) return 3;
					if (value >= (long)0xFFFFFFFFF8000000) return 4;
					if (value >= (long)0xFFFFFFFC00000000) return 5;
					if (value >= (long)0xFFFFFE0000000000) return 6;
					if (value >= (long)0xFFFF000000000000) return 7;
					if (value >= (long)0xFF80000000000000) return 8;
					if (value >= (long)0xC000000000000000) return 9;
					return 10;
				}

				if (value <= 0x000000000000003F) return 1;
				if (value <= 0x0000000000001FFF) return 2;
				if (value <= 0x00000000000FFFFF) return 3;
				if (value <= 0x0000000007FFFFFF) return 4;
				if (value <= 0x00000003FFFFFFFF) return 5;
				if (value <= 0x000001FFFFFFFFFF) return 6;
				if (value <= 0x0000FFFFFFFFFFFF) return 7;
				if (value <= 0x007FFFFFFFFFFFFF) return 8;
				if (value <= 0x3FFFFFFFFFFFFFFF) return 9;
				return 10;
			}
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
					if (shift < 32 && (data & 0x40) != 0)
						result |= -((long)1 << shift);

					return result;
				}
			}

			throw new SerializationException("SLEB128 encoded integer did not stop after 10 bytes.");
		}

		#endregion

		#region 64-Bit Unsigned Integer

		/// <summary>
		/// Determines how many bytes are needed to encode a the specified unsigned integer using the ULEB128 encoding.
		/// </summary>
		/// <param name="value">Integer to encode.</param>
		/// <returns>Number of bytes needed to encode the specified integer.</returns>
		public static int GetByteCount(ulong value)
		{
			if (value < 0x0000000000000080) return 1; // 7 bits
			if (value < 0x0000000000004000) return 2; // 14 bits
			if (value < 0x0000000000200000) return 3; // 21 bits
			if (value < 0x0000000010000000) return 4; // 28 bits
			if (value < 0x0000000800000000) return 5; // 35 bits
			if (value < 0x0000040000000000) return 6; // 42 bits
			if (value < 0x0002000000000000) return 7; // 49 bits
			if (value < 0x0100000000000000) return 8; // 56 bits
			if (value < 0x8000000000000000) return 9; // 63 bits
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
