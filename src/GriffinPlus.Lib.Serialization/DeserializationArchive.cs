///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using GriffinPlus.Lib.Io;
using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Archive providing support for the deserialization of classes using the <see cref="Serializer"/> class.
	/// </summary>
	public class DeserializationArchive
	{
		#region Constants

		/// <summary>
		/// Maximum buffer size when resizing <see cref="Serializer.TempBuffer_Buffer"/> for reading/writing
		/// streams and unmanaged buffers.
		/// </summary>
		internal const int TempBufferMaxSize = 80000; // Objects > 85000 bytes are allocated on the large object heap, so keep away from this limit!

		#endregion

		#region Class Variables

		private static readonly LogWriter sLog = LogWriter.Get<DeserializationArchive>();

		#endregion

		#region Member Variables

		private readonly Serializer              mSerializer;
		private readonly Stream                  mStream;
		private          SerializerArchiveStream mArchiveStream;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationArchive"/> class.
		/// </summary>
		/// <param name="serializer">The executing serializer.</param>
		/// <param name="stream">Stream containing the serialized archive.</param>
		/// <param name="type">Type the archive stores data from (used to instantiate the right class during deserialization).</param>
		/// <param name="version">Version of the type the archive contains data from.</param>
		/// <param name="context">User-specific context object.</param>
		internal DeserializationArchive(
			Serializer serializer,
			Stream     stream,
			Type       type,
			uint       version,
			object     context)
		{
			mSerializer = serializer;
			mStream = stream;
			DataType = type;
			Version = version;
			Context = context;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the version of the data in the archive.
		/// </summary>
		public uint Version { get; }

		/// <summary>
		/// Gets the type the archive contains data from.
		/// </summary>
		public Type DataType { get; }

		/// <summary>
		/// User-defined context object.
		/// </summary>
		/// <remarks>
		/// If you use the context object, you must consider the case that the context object may be <c>null</c>, even if you are sure
		/// that you always specify a valid context object. This is necessary to enable the serializer to skip unknown types properly.
		/// </remarks>
		public object Context { get; }

		#endregion

		#region System.SByte

		/// <summary>
		/// Reads a <see cref="System.SByte"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public sbyte ReadSByte()
		{
			ReadAndCheckPayloadType(PayloadType.SByte);
			return mSerializer.ReadPrimitive_SByte(mStream);
		}

		#endregion

		#region System.Int16

		/// <summary>
		/// Reads a<see cref="System.Int16"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public short ReadInt16()
		{
			ReadAndCheckPayloadType(PayloadType.Int16);
			return mSerializer.ReadPrimitive_Int16(mStream);
		}

		#endregion

		#region System.Int32

		/// <summary>
		/// Reads a <see cref="System.Int32"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public int ReadInt32()
		{
			ReadAndCheckPayloadType(PayloadType.Int32);
			return mSerializer.ReadPrimitive_Int32(mStream);
		}

		#endregion

		#region System.Int64

		/// <summary>
		/// Reads a <see cref="System.Int64"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public long ReadInt64()
		{
			ReadAndCheckPayloadType(PayloadType.Int64);
			return mSerializer.ReadPrimitive_Int64(mStream);
		}

		#endregion

		#region System.Byte

		/// <summary>
		/// Reads a <see cref="System.Byte"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public byte ReadByte()
		{
			ReadAndCheckPayloadType(PayloadType.Byte);
			return mSerializer.ReadPrimitive_Byte(mStream);
		}

		#endregion

		#region System.UInt16

		/// <summary>
		/// Reads a <see cref="System.UInt16"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public ushort ReadUInt16()
		{
			ReadAndCheckPayloadType(PayloadType.UInt16);
			return mSerializer.ReadPrimitive_UInt16(mStream);
		}

		#endregion

		#region System.UInt32

		/// <summary>
		/// Reads a <see cref="System.UInt32"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public uint ReadUInt32()
		{
			ReadAndCheckPayloadType(PayloadType.UInt32);
			return mSerializer.ReadPrimitive_UInt32(mStream);
		}

		#endregion

		#region System.UInt64

		/// <summary>
		/// Reads a <see cref="System.UInt64"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public ulong ReadUInt64()
		{
			ReadAndCheckPayloadType(PayloadType.UInt64);
			return mSerializer.ReadPrimitive_UInt64(mStream);
		}

		#endregion

		#region System.Enum

		/// <summary>
		/// Reads an enumeration value from the archive.
		/// </summary>
		/// <returns>Read value (must be casted to the concrete enumeration).</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public Enum ReadEnum()
		{
			CloseArchiveStream();
			object obj = mSerializer.InnerDeserialize(mStream, null);

			if (!obj.GetType().IsEnum)
			{
				var trace = new StackTrace();
				string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			return (Enum)obj;
		}

		/// <summary>
		/// Reads an enumeration value from the archive.
		/// </summary>
		/// <typeparam name="T">Enumeration type to read.</typeparam>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public T ReadEnum<T>() where T : Enum
		{
			CloseArchiveStream();
			object obj = mSerializer.InnerDeserialize(mStream, null);

			if (obj == null || obj.GetType() != typeof(T))
			{
				var trace = new StackTrace();
				string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			return (T)obj;
		}

		#endregion

		#region System.Boolean

		/// <summary>
		/// Reads a <see cref="System.Boolean"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public bool ReadBoolean()
		{
			ReadAndCheckPayloadType(PayloadType.Boolean);
			return mSerializer.ReadPrimitive_Boolean(mStream);
		}

		#endregion

		#region System.Char

		/// <summary>
		/// Reads a <see cref="System.Char"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public char ReadChar()
		{
			ReadAndCheckPayloadType(PayloadType.Char);
			return mSerializer.ReadPrimitive_Char(mStream);
		}

		#endregion

		#region System.Decimal

		/// <summary>
		/// Reads a <see cref="System.Decimal"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public decimal ReadDecimal()
		{
			ReadAndCheckPayloadType(PayloadType.Decimal);
			return mSerializer.ReadPrimitive_Decimal(mStream);
		}

		#endregion

		#region System.Single

		/// <summary>
		/// Reads a <see cref="System.Single"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public float ReadSingle()
		{
			ReadAndCheckPayloadType(PayloadType.Single);
			return mSerializer.ReadPrimitive_Single(mStream);
		}

		#endregion

		#region System.Double

		/// <summary>
		/// Reads a <see cref="System.Double"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public double ReadDouble()
		{
			ReadAndCheckPayloadType(PayloadType.Double);
			return mSerializer.ReadPrimitive_Double(mStream);
		}

		#endregion

		#region System.String

		/// <summary>
		/// Reads a <see cref="System.String"/> object from the archive.
		/// </summary>
		/// <returns>Read string.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public string ReadString()
		{
			CloseArchiveStream();
			object obj = mSerializer.InnerDeserialize(mStream, null);
			CheckExpectedType(obj, typeof(string));
			return obj as string;
		}

		#endregion

		#region System.Type

		/// <summary>
		/// Reads a <see cref="System.Type"/> object from the archive.
		/// </summary>
		/// <returns>Read type.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public Type ReadType()
		{
			CloseArchiveStream();
			object obj = mSerializer.InnerDeserialize(mStream, null);
			CheckExpectedType(obj, typeof(Type));
			return obj as Type;
		}

		#endregion

		#region System.DateTime

		/// <summary>
		/// Reads a <see cref="System.DateTime"/> value from the archive.
		/// </summary>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public DateTime ReadDateTime()
		{
			ReadAndCheckPayloadType(PayloadType.DateTime);
			return mSerializer.ReadPrimitive_DateTime(mStream);
		}

		#endregion

		#region System.Object

		/// <summary>
		/// Reads an object from the archive.
		/// </summary>
		/// <param name="context">Context object that is passed to the serializer processing the object.</param>
		/// <returns>The read object.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public object ReadObject(object context = null)
		{
			CloseArchiveStream();
			return mSerializer.InnerDeserialize(mStream, context);
		}

		#endregion

		#region Class Hierarchies

		/// <summary>
		/// Prepares an archive for deserializing the base class of a serializable class.
		/// </summary>
		/// <param name="type">Base class type.</param>
		/// <returns>Deserialization archive for the base class.</returns>
		/// <exception cref="ArgumentException">Type is not serializable.</exception>
		/// <exception cref="SerializationException">Archive does not contain an archive for the specified class.</exception>
		public DeserializationArchive PrepareBaseArchive(Type type)
		{
			return PrepareBaseArchive(type, Context);
		}

		/// <summary>
		/// Prepares an archive for deserializing the base class of a serializable class.
		/// </summary>
		/// <param name="type">Base class type.</param>
		/// <param name="context">Context object to pass to the serializer of the base class.</param>
		/// <returns>Deserialization archive for the base class.</returns>
		/// <exception cref="ArgumentException">Type is not serializable.</exception>
		/// <exception cref="SerializationException">Archive does not contain an archive for the specified class.</exception>
		public DeserializationArchive PrepareBaseArchive(Type type, object context)
		{
			// read payload type (expecting a base class archive)
			ReadAndCheckPayloadType(PayloadType.BaseArchiveStart);

			// read version number
			uint deserializedVersion = Leb128EncodingHelper.ReadUInt32(mStream);

			// check maximum supported version number
			uint currentVersion = Serializer.GetSerializerVersion(type); // throws ArgumentException if type is not serializable
			if (deserializedVersion > currentVersion)
			{
				// version of the archive that is about to be deserialized is greater than
				// the version the internal object serializer supports
				string error = $"Deserializing type '{type.FullName}' failed due to a version conflict (got version: {deserializedVersion}, max. supported version: {currentVersion}).";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			// version is ok, create archive...
			return new DeserializationArchive(mSerializer, mStream, type, deserializedVersion, context);
		}

		#endregion

		#region Buffer

		/// <summary>
		/// Reads a buffer from the archive.
		/// </summary>
		/// <param name="p">Pointer to the beginning of the buffer to fill.</param>
		/// <param name="count">Size of the buffer to fill.</param>
		/// <returns>Number of bytes actually read.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public unsafe long ReadBuffer(IntPtr p, long count)
		{
			// read payload type and size of the following buffer
			ReadAndCheckPayloadType(PayloadType.Buffer);
			long length = Leb128EncodingHelper.ReadInt64(mStream);

			// now we know how much bytes will be returned...
			long bytesReturned = Math.Min(length, count);

			if (mStream is MemoryBlockStream mbs)
			{
				// the MemoryBlockStream provides a direct way to read from the underlying buffer more efficiently
				// => let the stream directly copy data into the specified buffer
				while (count > 0 && length > 0)
				{
					int bytesToRead = (int)Math.Min(Math.Min(count, length), int.MaxValue);
					if (mbs.Read(new Span<byte>(p.ToPointer(), bytesToRead)) < bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
					length -= bytesToRead;
					count -= bytesToRead;
					p += bytesToRead;
				}
			}
			else
			{
				// some other stream
				// => copying data to a temporary buffer is needed before passing it to the stream
				mSerializer.EnsureTemporaryByteBufferSize(TempBufferMaxSize);
				while (count > 0 && length > 0)
				{
					int bytesToRead = (int)Math.Min(count, mSerializer.TempBuffer_Buffer.Length);
					if (mStream.Read(mSerializer.TempBuffer_Buffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
					Marshal.Copy(mSerializer.TempBuffer_Buffer, 0, p, bytesToRead);
					length -= bytesToRead;
					count -= bytesToRead;
					p += bytesToRead;
				}
			}

			// skip bytes that have not been read to ensure the stream can be read any further
			// (just for the case that that less bytes were requested)
			if (length > 0)
			{
				if (mStream.CanSeek)
				{
					if (mStream.Position + length >= mStream.Length) throw new SerializationException("Stream ended unexpectedly.");
					mStream.Position += length;
				}
				else
				{
					mSerializer.EnsureTemporaryByteBufferSize(TempBufferMaxSize);
					while (length > 0)
					{
						int bytesToRead = (int)Math.Min(length, mSerializer.TempBuffer_Buffer.Length);
						if (mStream.Read(mSerializer.TempBuffer_Buffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
						length -= bytesToRead;
					}
				}
			}

			return bytesReturned;
		}

		#endregion

		#region Stream

		/// <summary>
		/// Reads a byte buffer using a stream.
		/// </summary>
		/// <returns>Stream containing data to read.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public Stream ReadStream()
		{
			// read payload type and size of the following buffer
			ReadAndCheckPayloadType(PayloadType.Buffer);
			long length = Leb128EncodingHelper.ReadInt64(mStream);

			mArchiveStream = new SerializerArchiveStream(mStream, length);
			return mArchiveStream;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Reads the payload type from the stream and checks whether it matches the expected payload type.
		/// </summary>
		/// <param name="type">Type to check for.</param>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Specified payload type does not match the received payload type.</exception>
		private void ReadAndCheckPayloadType(PayloadType type)
		{
			CloseArchiveStream();

			int readByte = mStream.ReadByte();
			if (readByte < 0) throw new SerializationException("Stream ended unexpectedly.");
			if (readByte != (int)type)
			{
				Debug.Fail("Unexpected payload type during deserialization.");
				var trace = new StackTrace();
				string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}
		}

		/// <summary>
		/// Checks whether the specified object is of the specified type and throws a <see cref="SerializationException"/>
		/// if the type of the specified object does not match the specified type.
		/// </summary>
		/// <param name="obj">Object to check.</param>
		/// <param name="type">Type to check the object for.</param>
		/// <exception cref="SerializationException">Specified object is not of the specified type.</exception>
		private void CheckExpectedType(object obj, Type type)
		{
			if (obj == null)
			{
				if (!type.IsValueType) return; // null is valid for reference types
				Debug.Fail("Unexpected type during deserialization.");
				var trace = new StackTrace();
				string error = $"Unexpected type during deserialization (expected: '{type.FullName}', got: null). Stack Trace:\n{trace}";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			if (!type.IsInstanceOfType(obj))
			{
				Debug.Fail("Unexpected type during deserialization.");
				var trace = new StackTrace();
				string error = $"Unexpected type during deserialization (expected: '{type.FullName}', got: '{obj.GetType().FullName}'). Stack Trace:\n{trace}";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}
		}

		/// <summary>
		/// Closes the current archive stream, if necessary.
		/// </summary>
		private void CloseArchiveStream()
		{
			if (mArchiveStream != null)
			{
				long bytesToSkip = mArchiveStream.Length - mArchiveStream.Position;
				if (mStream.CanSeek)
				{
					// stream supports seeking
					// => set stream position
					mStream.Position += bytesToSkip;
				}
				else
				{
					// stream does not support seeking
					// => read and discard bytes to skip
					byte[] buffer = mSerializer.TempBuffer_Buffer;
					while (bytesToSkip > 0)
					{
						int bytesToRead = (int)Math.Min(bytesToSkip, int.MaxValue);
						bytesToRead = Math.Min(bytesToRead, buffer.Length);
						bytesToSkip -= mStream.Read(buffer, 0, bytesToRead);
					}
				}

				mArchiveStream.Dispose();
				mArchiveStream = null;
			}
		}

		/// <summary>
		/// Closes the archive (for internal use only).
		/// </summary>
		internal void Close()
		{
			CloseArchiveStream();
		}

		#endregion
	}

}
