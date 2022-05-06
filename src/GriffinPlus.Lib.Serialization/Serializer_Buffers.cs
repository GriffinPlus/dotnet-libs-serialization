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
		internal int[]  TempBuffer_Int32 = new int[4]; // must always be four 32-bit integers to work for decimal conversion
		internal byte[] TempBuffer_Buffer;

		/// <summary>
		/// Allocates temporary buffers.
		/// </summary>
		private void AllocateTemporaryBuffers()
		{
			// TempBuffer_Int32 = new int[4]; 
			TempBuffer_Buffer = ArrayPool<byte>.Shared.Rent(256); // resized on demand
		}

		/// <summary>
		/// Releases temporary buffers.
		/// </summary>
		private void ReleaseTemporaryBuffers()
		{
			// ReleaseBuffer(ref TempBuffer_Int32);
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
