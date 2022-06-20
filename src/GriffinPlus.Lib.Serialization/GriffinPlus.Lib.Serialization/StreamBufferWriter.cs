///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// A <see cref="IBufferWriter{T}"/> that works on top of a <see cref="Stream"/>.
	/// As soon as the buffer is full, it is flushed to the stream.
	/// </summary>
	sealed class StreamBufferWriter : IDisposable, IBufferWriter<byte>
	{
		private static readonly LogWriter       sLog        = LogWriter.Get<StreamBufferWriter>();
		private readonly        ArrayPool<byte> mPool       = ArrayPool<byte>.Shared;
		private readonly        int             mFlushLimit = Serializer.MaxChunkSize;
		private                 byte[]          mBuffer     = Array.Empty<byte>();
		private                 int             mIndex      = 0;
		private                 Stream          mStream     = null;

		/// <summary>
		/// Disposes the buffer writer returning the rented buffer to the array pool.
		/// </summary>
		public void Dispose()
		{
			if (mBuffer.Length > 0)
			{
				Flush();
				mPool.Return(mBuffer);
				mBuffer = Array.Empty<byte>();
				mIndex = 0;
			}
		}

		/// <summary>
		/// Gets or sets the stream backing the buffer writer.
		/// </summary>
		public Stream Stream
		{
			get => mStream;
			set
			{
				Flush();
				mStream = value;
			}
		}

		/// <summary>
		/// Gets the array backing the buffer writer.
		/// The number of elements that are actually valid is reflected by the <see cref="WrittenCount"/> property.
		/// </summary>
		public byte[] Buffer => mBuffer;

		/// <summary>
		/// Gets a <see cref="ReadOnlyMemory{T}"/> that contains the data written to the underlying buffer so far.
		/// </summary>
		/// <returns>The data written to the underlying buffer.</returns>
		public ReadOnlyMemory<byte> WrittenMemory => mBuffer.AsMemory(0, mIndex);

		/// <summary>
		/// Gets a <see cref="ReadOnlySpan{T}"/> that contains the data written to the underlying buffer so far.
		/// </summary>
		/// <returns>The data written to the underlying buffer.</returns>
		public ReadOnlySpan<byte> WrittenSpan => mBuffer.AsSpan(0, mIndex);

		/// <summary>
		/// Gets the amount of data written to the underlying buffer.
		/// </summary>
		/// <returns>The amount of data written to the underlying buffer.</returns>
		public int WrittenCount => mIndex;

		/// <summary>
		/// Gets the total amount of space within the underlying buffer.
		/// </summary>
		/// <returns>The total capacity of the underlying buffer.</returns>
		public int Capacity => mBuffer.Length;

		/// <summary>
		/// Gets the amount of available space that can be written to without forcing the underlying buffer to grow.
		/// </summary>
		/// <returns>The space available for writing without forcing the underlying buffer to grow.</returns>
		public int FreeCapacity => mBuffer.Length - mIndex;

		/// <summary>
		/// Clears the data written to the underlying buffer.
		/// </summary>
		public void Clear()
		{
			Flush();
			mBuffer.AsSpan(0, mIndex).Clear();
			mIndex = 0;
		}

		/// <summary>
		/// Flushes buffered data to the underlying stream.
		/// </summary>
		public void Flush()
		{
			if (mIndex > 0)
			{
				mStream.Write(mBuffer, 0, mIndex);
				mIndex = 0;
			}
		}

		/// <summary>
		/// Notifies the <see cref="IBufferWriter{T}"/> that <paramref name="count"/> items were written to the output <see cref="Span{T}"/>/
		/// <see cref="Memory{T}"/>.
		/// </summary>
		/// <param name="count">The number of items written.</param>
		/// <exception cref="ArgumentException"><paramref name="count"/> is negative.</exception>
		/// <exception cref="InvalidOperationException">The method call attempts to advance past the end of the underlying buffer.</exception>
		public void Advance(int count)
		{
			if (count < 0)
				throw new ArgumentException(null, nameof(count));
			if (mIndex > mBuffer.Length - count)
				throw new InvalidOperationException($"Advanced too far, exceeded the end of the buffer (buffer size: {mBuffer.Length}).");
			mIndex += count;
		}

		/// <summary>
		/// Returns a <see cref="Memory{T}"/> to write to that is at least the length specified by <paramref name="sizeHint"/>.
		/// </summary>
		/// <param name="sizeHint">The minimum requested length of the <see cref="Memory{T}"/>.</param>
		/// <exception cref="T:System.ArgumentException"><paramref name="sizeHint"/> is negative.</exception>
		/// <returns>
		/// A <see cref="Memory{T}"/> whose length is at least <paramref name="sizeHint"/>.
		/// If <paramref name="sizeHint"/> is not provided or is equal to 0, some non-empty buffer is returned.
		/// </returns>
		public Memory<byte> GetMemory(int sizeHint = 0)
		{
			PrepareBuffer(sizeHint);
			return mBuffer.AsMemory(mIndex);
		}

		/// <summary>
		/// Returns a <see cref="Span{T}"/> to write to that is at least a specified length.
		/// </summary>
		/// <param name="sizeHint">The minimum requested length of the <see cref="Span{T}"/>.</param>
		/// <exception cref="T:System.ArgumentException"><paramref name="sizeHint"/> is negative.</exception>
		/// <returns>
		/// A span of at least <paramref name="sizeHint"/> in length.
		/// If <paramref name="sizeHint"/> is not provided or is equal to 0, some non-empty buffer is returned.
		/// </returns>
		public Span<byte> GetSpan(int sizeHint = 0)
		{
			PrepareBuffer(sizeHint);
			return mBuffer.AsSpan(mIndex);
		}

		/// <summary>
		/// Checks the backing buffer and resizes it, if necessary.
		/// </summary>
		/// <param name="sizeHint">The minimum requested length.</param>
		private void PrepareBuffer(int sizeHint)
		{
			// ensure the request number of bytes is positive
			if (sizeHint < 0)
				throw new ArgumentException(nameof(sizeHint));

			// ensure to allocate at least 1 byte
			if (sizeHint == 0)
				sizeHint = 1;

			// abort if the backing buffer has enough free space to serve the request
			if (sizeHint <= FreeCapacity)
				return;

			// the backing buffer is not large enough to serve the request
			// => calculate the size of the new buffer
			int length = mBuffer.Length;
			int val1 = Math.Max(sizeHint, length);
			if (length == 0) val1 = Math.Max(val1, 256);
			int newBufferSize = length + val1;
			if ((uint)newBufferSize > int.MaxValue)
			{
				newBufferSize = length + sizeHint;
				if ((uint)newBufferSize > int.MaxValue)
					throw new OutOfMemoryException($"Allocating new backing buffer failed (requested size: {(uint)newBufferSize}).");
			}

			if (newBufferSize > mFlushLimit)
			{
				// the size of the new buffer would exceed the flush limit
				// => flush the buffer to the stream to gain free space
				Flush();

				// abort if there is enough free space now...
				if (sizeHint <= FreeCapacity)
					return;

				// the buffer has reached its maximum allowed size and it is empty now,
				// but it still does not have the requested amount of free space
				// => implementation issue, the serializer should never request such big buffers...
				sLog.Write(
					LogLevel.Error,
					"The buffer is not large enough to serve the request ({0}), even after flushing it to the backing stream.",
					newBufferSize);
			}

			// resize the buffer
			byte[] newBuffer = mPool.Rent(newBufferSize);
			Debug.WriteLine($"Reallocating: {newBufferSize} bytes, got: {newBuffer.Length} bytes");
			Array.Copy(mBuffer, newBuffer, mBuffer.Length);
			mPool.Return(mBuffer);
			mBuffer = newBuffer;
		}
	}

}
