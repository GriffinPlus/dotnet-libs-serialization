///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// A stream that enables a serializer archive to get access to a byte buffer in the archive using a stream.
	/// </summary>
	class SerializerArchiveStream : Stream
	{
		private readonly Stream mStream;
		private readonly long   mOriginalPosition = -1;
		private          long   mPosition;
		private readonly long   mLength;
		private          bool   mClosed;

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerArchiveStream"/> class restricting the specified stream
		/// to a range starting at its current position with the specified length.
		/// </summary>
		/// <param name="stream">Stream to work on.</param>
		/// <param name="length">Length the restricted stream should have.</param>
		public SerializerArchiveStream(Stream stream, long length)
		{
			mStream = stream;
			mLength = length;
			if (mStream.CanSeek)
			{
				mOriginalPosition = mStream.Position;
				mLength = Math.Min(mLength, mStream.Length - mOriginalPosition);
			}
		}

		/// <summary>
		/// Disposes the stream causing any attempt to use it any further to fail.
		/// </summary>
		/// <param name="disposing">
		/// <c>true</c> if the stream is disposed explicitly;
		/// <c>false</c> if the stream is finalized.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;

			if (!mClosed)
			{
				// skip bytes if the archive stream has not been consumed entirely
				long bytesToSkip = Length - Position;
				if (bytesToSkip > 0)
				{
					if (mStream.CanSeek)
					{
						// deserialization stream supports seeking
						// => set stream position
						mStream.Position += bytesToSkip;
					}
					else
					{
						// stream does not support seeking
						// => read and discard bytes to skip
						byte[] buffer = ArrayPool<byte>.Shared.Rent(80000);
						try
						{
							while (bytesToSkip > 0)
							{
								int bytesToRead = (int)Math.Min(bytesToSkip, int.MaxValue);
								bytesToRead = Math.Min(bytesToRead, buffer.Length);
								bytesToSkip -= mStream.Read(buffer, 0, bytesToRead);
							}
						}
						finally
						{
							ArrayPool<byte>.Shared.Return(buffer);
						}
					}
				}

				mClosed = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the stream supports reading.
		/// </summary>
		public override bool CanRead => mStream.CanRead;

		/// <summary>
		/// Gets a value indicating whether the stream supports writing.
		/// </summary>
		public override bool CanWrite => false;

		/// <summary>
		/// Gets a value indicating whether the stream supports seeking.
		/// </summary>
		public override bool CanSeek => mStream.CanSeek;

		/// <summary>
		/// Gets the length of the current stream.
		/// </summary>
		public override long Length => mLength;

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		public override long Position
		{
			get
			{
				if (mClosed) throw new InvalidOperationException("The stream is closed.");
				if (!mStream.CanSeek) throw new NotSupportedException("Seeking is not supported.");
				return mPosition;
			}

			set
			{
				if (mClosed) throw new InvalidOperationException("The stream is closed.");
				if (!mStream.CanSeek) throw new NotSupportedException("Seeking is not supported.");
				if (mPosition < 0 || mPosition > mLength) throw new ArgumentException("The position is not within the stream.");
				mStream.Position = mOriginalPosition + value;
				mPosition = value;
			}
		}

		/// <summary>
		/// Sets the current position within the stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">Indicates the reference point used to obtain the new position.</param>
		/// <returns>The new position within the stream.</returns>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (mClosed) throw new InvalidOperationException("The stream is closed.");
			if (!mStream.CanSeek) throw new NotSupportedException("The stream does not support seeking.");

			if (origin == SeekOrigin.Begin)
			{
				if (offset < 0) throw new ArgumentException("Position must be positive when seeking from the beginning of the stream.");
				if (offset > mLength) throw new ArgumentException("Position exceeds the length of the stream.");
				long position = mOriginalPosition + offset;
				long positionAfterSeek = mStream.Seek(position, SeekOrigin.Begin);
				Debug.Assert(positionAfterSeek == position);
				mPosition = offset;
				return mPosition;
			}

			if (origin == SeekOrigin.Current)
			{
				if (offset < 0 && -offset > mPosition) throw new ArgumentException("The target position is before the start of the stream.");
				if (offset > 0 && offset > mLength - mPosition) throw new ArgumentException("The target position is after the end of the stream.");
				long position = mOriginalPosition + mPosition + offset;
				long positionAfterSeek = mStream.Seek(position, SeekOrigin.Begin);
				Debug.Assert(positionAfterSeek == position);
				mPosition += offset;
				return mPosition;
			}

			if (origin == SeekOrigin.End)
			{
				if (offset > 0) throw new ArgumentException("Position must be negative when seeking from the end of the stream.");
				if (offset < mLength) throw new ArgumentException("Position exceeds the start of the stream.");
				long position = mOriginalPosition + mLength - mPosition;
				long positionAfterSeek = mStream.Seek(position, SeekOrigin.Begin);
				Debug.Assert(positionAfterSeek == position);
				mPosition = mLength - offset;
				return mPosition;
			}

			throw new ArgumentException("The specified seek origin is invalid.");
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">Buffer receiving data from the stream.</param>
		/// <param name="offset">Offset in the buffer to start reading data to.</param>
		/// <param name="count">Number of bytes to read.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of requested bytes,
		/// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// buffer is null and count is not 0 -or-\n
		/// offset is less than 0 -or-\n
		/// count is less than 0 -or-\n
		/// offset + count is greater than the buffer's length.
		/// </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (mClosed) throw new InvalidOperationException("The stream is closed.");
			if (count == 0) return 0;
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentException("Offset must be greater than or equal to 0.", nameof(offset));
			if (count < 0) throw new ArgumentException("Count must be greater than or equal to 0.", nameof(count));
			if (offset + count > buffer.Length) throw new ArgumentException("The buffer's length is less than offset + count.");

			int bytesToRead = (int)Math.Min(mLength - mPosition, count);
			int bytesRead = mStream.Read(buffer, offset, bytesToRead);
			mPosition += bytesRead;
			return bytesRead;
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">Buffer containing data to write to the stream.</param>
		/// <param name="offset">Offset in the buffer to start writing data from.</param>
		/// <param name="count">Number of bytes to write.</param>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("The stream does not support writing.");
		}

		/// <summary>
		/// Flushes written data to the underlying device (for interface compatibility only).
		/// </summary>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		public override void Flush()
		{
			throw new NotSupportedException("The stream does not support writing.");
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <param name="length">The desired length of the current stream in bytes.</param>
		/// <exception cref="NotSupportedException">Setting the length of the stream is not supported.</exception>
		public override void SetLength(long length)
		{
			throw new NotSupportedException("Setting the length of the stream is not supported.");
		}
	}

}
