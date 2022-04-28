///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Buffers;
using System.Runtime.CompilerServices;

namespace GriffinPlus.Lib.Serialization
{

	partial class Serializer
	{
		internal byte[]    TempBuffer_UInt8;
		internal ushort[]  TempBuffer_UInt16;
		internal uint[]    TempBuffer_UInt32;
		internal ulong[]   TempBuffer_UInt64;
		internal sbyte[]   TempBuffer_Int8;
		internal short[]   TempBuffer_Int16;
		internal int[]     TempBuffer_Int32 = new int[4]; // must always be 4 ints to work for decimal conversion
		internal long[]    TempBuffer_Int64;
		internal char[]    TempBuffer_Char;
		internal float[]   TempBuffer_Single;
		internal double[]  TempBuffer_Double;
		internal decimal[] TempBuffer_Decimal;
		internal byte[]    TempBuffer_Buffer;

		/// <summary>
		/// Allocates temporary buffers.
		/// </summary>
		private void AllocateTemporaryBuffers()
		{
			TempBuffer_UInt8 = ArrayPool<byte>.Shared.Rent(1);
			TempBuffer_UInt16 = ArrayPool<ushort>.Shared.Rent(1);
			TempBuffer_UInt32 = ArrayPool<uint>.Shared.Rent(1);
			TempBuffer_UInt64 = ArrayPool<ulong>.Shared.Rent(1);
			TempBuffer_Int8 = ArrayPool<sbyte>.Shared.Rent(1);
			TempBuffer_Int16 = ArrayPool<short>.Shared.Rent(1);
			// TempBuffer_Int32 = new int[4]; 
			TempBuffer_Int64 = ArrayPool<long>.Shared.Rent(1);
			TempBuffer_Char = ArrayPool<char>.Shared.Rent(1);
			TempBuffer_Single = ArrayPool<float>.Shared.Rent(1);
			TempBuffer_Double = ArrayPool<double>.Shared.Rent(1);
			TempBuffer_Decimal = ArrayPool<decimal>.Shared.Rent(1);
			TempBuffer_Buffer = ArrayPool<byte>.Shared.Rent(256); // resized on demand
		}

		/// <summary>
		/// Releases temporary buffers.
		/// </summary>
		private void ReleaseTemporaryBuffers()
		{
			ReleaseBuffer(ref TempBuffer_UInt8);
			ReleaseBuffer(ref TempBuffer_UInt16);
			ReleaseBuffer(ref TempBuffer_UInt32);
			ReleaseBuffer(ref TempBuffer_UInt64);
			ReleaseBuffer(ref TempBuffer_Int8);
			ReleaseBuffer(ref TempBuffer_Int16);
			// ReleaseBuffer(ref TempBuffer_Int32);
			ReleaseBuffer(ref TempBuffer_Int64);
			ReleaseBuffer(ref TempBuffer_Char);
			ReleaseBuffer(ref TempBuffer_Single);
			ReleaseBuffer(ref TempBuffer_Double);
			ReleaseBuffer(ref TempBuffer_Decimal);
			ReleaseBuffer(ref TempBuffer_Buffer);
		}

		/// <summary>
		/// Returns the specified buffer to the corresponding array pool and resets the buffer reference.
		/// </summary>
		/// <typeparam name="T">Type of a buffer element.</typeparam>
		/// <param name="buffer">Buffer to return.</param>
		private static void ReleaseBuffer<T>(ref T[] buffer)
		{
			ArrayPool<T>.Shared.Return(buffer);
			buffer = null;
		}

		/// <summary>
		/// Ensures that <see cref="TempBuffer_Buffer"/> has at least the specified size.
		/// </summary>
		/// <param name="size">Size the buffer should have at least.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void EnsureTemporaryByteBufferSize(int size)
		{
			if (TempBuffer_Buffer.Length < size)
			{
				ArrayPool<byte>.Shared.Return(TempBuffer_Buffer);
				TempBuffer_Buffer = ArrayPool<byte>.Shared.Rent(size);
			}
		}
	}

}
