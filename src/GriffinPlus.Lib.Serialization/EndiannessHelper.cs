///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Helper methods for handling endianness conversions.
	/// </summary>
	static class EndianessHelper
	{
		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short SwapBytes(short x)
		{
			return (short)((x >> 8) | (x << 8)); // swap adjacent 8-bit blocks
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort SwapBytes(ushort x)
		{
			return (ushort)((x >> 8) | (x << 8)); // swap adjacent 8-bit blocks
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static char SwapBytes(char x)
		{
			return (char)((x >> 8) | (x << 8)); // swap adjacent 8-bit blocks
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SwapBytes(int x)
		{
			x = (x >> 16) | (x << 16);                                     // swap adjacent 16-bit blocks
			return (int)((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8); // swap adjacent 8-bit blocks
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint SwapBytes(uint x)
		{
			x = (x >> 16) | (x << 16);                                // swap adjacent 16-bit blocks
			return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8); // swap adjacent 8-bit blocks
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long SwapBytes(long x)
		{
			return (long)SwapBytes((ulong)x);
		}

		/// <summary>
		/// Swaps bytes to convert little-endian to big-endian and vice versa.
		/// </summary>
		/// <param name="x">Value to convert.</param>
		/// <returns>The converted value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong SwapBytes(ulong x)
		{
			x = (x >> 32) | (x << 32);                                                // swap adjacent 32-bit blocks
			x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);  // swap adjacent 16-bit blocks
			return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8); // swap adjacent 8-bit blocks
		}
	}

}
