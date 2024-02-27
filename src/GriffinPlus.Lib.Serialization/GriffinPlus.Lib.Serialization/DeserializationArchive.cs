///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

using GriffinPlus.Lib.Io;
using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// Archive providing support for the deserialization of classes using the <see cref="Serializer"/> class.
/// </summary>
public ref struct DeserializationArchive
{
	#region Constants

	/// <summary>
	/// Maximum buffer size when resizing the internal temporary buffer for reading/writing streams and unmanaged buffers.
	/// </summary>
	internal const int TempBufferMaxSize = 80000; // Objects > 85000 bytes are allocated on the large object heap, so keep away from this limit!

	#endregion

	#region Class Variables

	private static readonly LogWriter sLog = LogWriter.Get(typeof(DeserializationArchive));

	#endregion

	#region Member Variables

	private SerializerArchiveStream mArchiveStream;

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
		Serializer = serializer;
		Stream = stream;
		mArchiveStream = null;
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

	/// <summary>
	/// Gets the <see cref="Serializer"/> the archive belongs to.
	/// </summary>
	public Serializer Serializer { get; }

	/// <summary>
	/// Gets the stream containing the serialized object.
	/// Usually it is more convenient to use the archive's Read() methods, but if you need more control over the deserialization
	/// you can directly read from this stream as well.
	/// </summary>
	public Stream Stream { get; }

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
		return Serializer.ReadPrimitive_SByte(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.Int16_Native, PayloadType.Int16_LEB128);
		return payloadType == PayloadType.Int16_Native
			       ? Serializer.ReadPrimitive_Int16_Native(Stream)
			       : Serializer.ReadPrimitive_Int16_LEB128(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.Int32_Native, PayloadType.Int32_LEB128);
		return payloadType == PayloadType.Int32_Native
			       ? Serializer.ReadPrimitive_Int32_Native(Stream)
			       : Serializer.ReadPrimitive_Int32_LEB128(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.Int64_Native, PayloadType.Int64_LEB128);
		return payloadType == PayloadType.Int64_Native
			       ? Serializer.ReadPrimitive_Int64_Native(Stream)
			       : Serializer.ReadPrimitive_Int64_LEB128(Stream);
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
		return Serializer.ReadPrimitive_Byte(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.UInt16_Native, PayloadType.UInt16_LEB128);
		return payloadType == PayloadType.UInt16_Native
			       ? Serializer.ReadPrimitive_UInt16_Native(Stream)
			       : Serializer.ReadPrimitive_UInt16_LEB128(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.UInt32_Native, PayloadType.UInt32_LEB128);
		return payloadType == PayloadType.UInt32_Native
			       ? Serializer.ReadPrimitive_UInt32_Native(Stream)
			       : Serializer.ReadPrimitive_UInt32_LEB128(Stream);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.UInt64_Native, PayloadType.UInt64_LEB128);
		return payloadType == PayloadType.UInt64_Native
			       ? Serializer.ReadPrimitive_UInt64_Native(Stream)
			       : Serializer.ReadPrimitive_UInt64_LEB128(Stream);
	}

	#endregion

	#region System.Enum

	/// <summary>
	/// Reads an enumeration value from the archive.
	/// </summary>
	/// <returns>Read value (must be cast to the concrete enumeration).</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public Enum ReadEnum()
	{
		CloseArchiveStream();
		object obj = Serializer.InnerDeserialize(Stream, null);

		if (obj.GetType().IsEnum) return (Enum)obj;
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
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
		object obj = Serializer.InnerDeserialize(Stream, null);

		if (obj != null && obj.GetType() == typeof(T)) return (T)obj;
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	#endregion

	#region System.Boolean

	/// <summary>
	/// Reads a <see cref="System.Boolean"/> value from the archive.
	/// </summary>
	/// <returns>The read value.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
	public bool ReadBoolean()
	{
		CloseArchiveStream();

		int readByte = Stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Stream ended unexpectedly.");
		var payloadType = (PayloadType)readByte;
		if (payloadType == PayloadType.BooleanFalse) return false;
		if (payloadType == PayloadType.BooleanTrue) return true;
		Debug.Fail("Unexpected payload type during deserialization.");
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
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
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.Char_Native, PayloadType.Char_LEB128);
		return payloadType == PayloadType.Char_Native
			       ? Serializer.ReadPrimitive_Char_Native(Stream)
			       : Serializer.ReadPrimitive_Char_LEB128(Stream);
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
		return Serializer.ReadPrimitive_Decimal(Stream);
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
		return Serializer.ReadPrimitive_Single(Stream);
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
		return Serializer.ReadPrimitive_Double(Stream);
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
		object obj = Serializer.InnerDeserialize(Stream, null);
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
		object obj = Serializer.InnerDeserialize(Stream, null);
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
		return Serializer.ReadPrimitive_DateTime(Stream);
	}

	#endregion

	#region System.DateTimeOffset

	/// <summary>
	/// Reads a <see cref="System.DateTimeOffset"/> value from the archive.
	/// </summary>
	/// <returns>The read value.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public DateTimeOffset ReadDateTimeOffset()
	{
		ReadAndCheckPayloadType(PayloadType.DateTimeOffset);
		return Serializer.ReadPrimitive_DateTimeOffset(Stream);
	}

	#endregion

	#region System.DateOnly (.NET 6+ only)

#if NET6_0_OR_GREATER
	/// <summary>
	/// Reads a <see cref="System.DateOnly"/> value from the archive.
	/// </summary>
	/// <returns>The read value.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public DateOnly ReadDateOnly()
	{
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.DateOnly_Native, PayloadType.DateOnly_LEB128);
		return payloadType == PayloadType.DateOnly_Native
			       ? Serializer.ReadPrimitive_DateOnly_Native(Stream)
			       : Serializer.ReadPrimitive_DateOnly_LEB128(Stream);
	}
#endif

	#endregion

	#region System.TimeOnly (.NET 6+ only)

#if NET6_0_OR_GREATER
	/// <summary>
	/// Reads a <see cref="System.TimeOnly"/> value from the archive.
	/// </summary>
	/// <returns>The read value.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public TimeOnly ReadTimeOnly()
	{
		PayloadType payloadType = ReadAndCheckPayloadType(PayloadType.TimeOnly_Native, PayloadType.TimeOnly_LEB128);
		return payloadType == PayloadType.TimeOnly_Native
			       ? Serializer.ReadPrimitive_TimeOnly_Native(Stream)
			       : Serializer.ReadPrimitive_TimeOnly_LEB128(Stream);
	}
#endif

	#endregion

	#region System.Guid

	/// <summary>
	/// Reads a <see cref="System.Guid"/> value from the archive.
	/// </summary>
	/// <returns>The read value.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public Guid ReadGuid()
	{
		ReadAndCheckPayloadType(PayloadType.Guid);
		return Serializer.ReadPrimitive_Guid(Stream);
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
		return Serializer.InnerDeserialize(Stream, context);
	}

	#endregion

	#region Class Hierarchies

	/// <summary>
	/// Prepares an archive for deserializing the base class of a serializable class.
	/// </summary>
	/// <returns>Deserialization archive for the base class.</returns>
	/// <exception cref="ArgumentException">Type does not have a base type, or it is not serializable.</exception>
	/// <exception cref="SerializationException">
	/// The serializer version in the archive is greater than the maximum supported version of the base class.
	/// </exception>
	public DeserializationArchive PrepareBaseArchive()
	{
		return PrepareBaseArchive(Context);
	}

	/// <summary>
	/// Prepares an archive for deserializing the base class of a serializable class.
	/// </summary>
	/// <param name="context">Context object to pass to the serializer of the base class.</param>
	/// <returns>Deserialization archive for the base class.</returns>
	/// <exception cref="ArgumentException">Type does not have a base type, or it is not serializable.</exception>
	/// <exception cref="SerializationException">
	/// The serializer version in the archive is greater than the maximum supported version of the base class.
	/// </exception>
	public DeserializationArchive PrepareBaseArchive(object context)
	{
		Type type = DataType.BaseType ?? throw new ArgumentException($"{DataType.FullName} does not have a base type.");

		// read payload type (expecting a base class archive)
		ReadAndCheckPayloadType(PayloadType.BaseArchiveStart);

		// read version number
		uint deserializedVersion = Leb128EncodingHelper.ReadUInt32(Stream);

		// check maximum supported version number
		uint currentVersion = Serializer.GetSerializerVersion(type); // throws ArgumentException if type is not serializable
		if (deserializedVersion <= currentVersion)                   // version is ok, create archive...
			return new DeserializationArchive(Serializer, Stream, type, deserializedVersion, context);

		// version of the archive that is about to be deserialized is greater than
		// the version the internal object serializer supports
		string error = $"Deserializing type '{type.FullName}' failed due to a version conflict (got version: {deserializedVersion}, max. supported version: {currentVersion}).";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
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
	public unsafe long ReadBuffer(void* p, long count)
	{
		// read payload type and size of the following buffer
		ReadAndCheckPayloadType(PayloadType.Buffer);
		long length = Leb128EncodingHelper.ReadInt64(Stream);

		// now we know how many bytes will be returned...
		long bytesReturned = Math.Min(length, count);

		if (Stream is MemoryBlockStream mbs)
		{
			// the MemoryBlockStream provides a direct way to read from the underlying buffer more efficiently
			// => let the stream directly copy data into the specified buffer
			while (count > 0 && length > 0)
			{
				int bytesToRead = (int)Math.Min(Math.Min(count, length), int.MaxValue);
				if (mbs.Read(new Span<byte>(p, bytesToRead)) < bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
				length -= bytesToRead;
				count -= bytesToRead;
				p = (byte*)p + bytesToRead;
			}
		}
		else
		{
			// some other stream
			// => copying data to a temporary buffer is needed before passing it to the stream
			Serializer.EnsureTemporaryByteBufferSize(TempBufferMaxSize);
			while (count > 0 && length > 0)
			{
				int bytesToRead = (int)Math.Min(count, Serializer.TempBuffer_Buffer.Length);
				if (Stream.Read(Serializer.TempBuffer_Buffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
				Marshal.Copy(Serializer.TempBuffer_Buffer, 0, (nint)p, bytesToRead);
				length -= bytesToRead;
				count -= bytesToRead;
				p = (byte*)p + bytesToRead;
			}
		}

		// skip bytes that have not been read to ensure the stream can be read any further
		// (just for the case that fewer bytes were requested)
		// ReSharper disable once InvertIf
		if (length > 0)
		{
			if (Stream.CanSeek)
			{
				if (Stream.Position + length >= Stream.Length) throw new SerializationException("Stream ended unexpectedly.");
				Stream.Position += length;
			}
			else
			{
				Serializer.EnsureTemporaryByteBufferSize(TempBufferMaxSize);
				while (length > 0)
				{
					int bytesToRead = (int)Math.Min(length, Serializer.TempBuffer_Buffer.Length);
					if (Stream.Read(Serializer.TempBuffer_Buffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
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
	/// Consume the returned stream before reading any other serialized data as doing so will skip the rest of the buffer.
	/// Dispose the returned stream at the end to ensure that unread data in the deserialization stream is skipped properly.
	/// </summary>
	/// <returns>Stream containing data to read.</returns>
	/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
	public Stream ReadStream()
	{
		// read payload type and size of the following buffer
		ReadAndCheckPayloadType(PayloadType.Buffer);
		long length = Leb128EncodingHelper.ReadInt64(Stream);

		mArchiveStream = new SerializerArchiveStream(Stream, length);
		return mArchiveStream;
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Reads the payload type from the stream and checks whether it matches the expected payload type.
	/// </summary>
	/// <param name="type">The expected payload type.</param>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Specified payload type does not match the received payload type.</exception>
	private void ReadAndCheckPayloadType(PayloadType type)
	{
		CloseArchiveStream();

		int readByte = Stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Stream ended unexpectedly.");
		var payloadType = (PayloadType)readByte;
		if (payloadType == type) return;
		Debug.Fail("Unexpected payload type during deserialization.");
		// ReSharper disable once HeuristicUnreachableCode
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	/// <summary>
	/// Reads the payload type from the stream and checks whether it matches one of the expected payload types.
	/// </summary>
	/// <param name="type1">The first expected payload type.</param>
	/// <param name="type2">The second expected payload type.</param>
	/// <returns>The read payload type.</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Specified payload type does not match the received payload type.</exception>
	private PayloadType ReadAndCheckPayloadType(PayloadType type1, PayloadType type2)
	{
		CloseArchiveStream();

		int readByte = Stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Stream ended unexpectedly.");
		var payloadType = (PayloadType)readByte;
		if (payloadType == type1 || payloadType == type2) return payloadType;
		Debug.Fail("Unexpected payload type during deserialization.");
		// ReSharper disable once HeuristicUnreachableCode
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	/// <summary>
	/// Checks whether the specified object is of the specified type and throws a <see cref="SerializationException"/>
	/// if the type of the specified object does not match the specified type.
	/// </summary>
	/// <param name="obj">Object to check.</param>
	/// <param name="type">Type to check the object for.</param>
	/// <exception cref="SerializationException">Specified object is not of the specified type.</exception>
	private static void CheckExpectedType(object obj, Type type)
	{
		StackTrace trace;
		string error;

		if (obj == null)
		{
			if (!type.IsValueType) return; // null is valid for reference types
			Debug.Fail("Unexpected type during deserialization.");
			trace = new StackTrace();
			error = $"Unexpected type during deserialization (expected: '{type.FullName}', got: null). Stack Trace:\n{trace}";
			sLog.Write(LogLevel.Error, error);
			throw new SerializationException(error);
		}

		if (type.IsInstanceOfType(obj))
			return;

		Debug.Fail("Unexpected type during deserialization.");
		// ReSharper disable once HeuristicUnreachableCode
		trace = new StackTrace();
		error = $"Unexpected type during deserialization (expected: '{type.FullName}', got: '{obj.GetType().FullName}'). Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	/// <summary>
	/// Closes the current archive stream, if necessary.
	/// </summary>
	private void CloseArchiveStream()
	{
		if (mArchiveStream == null) return;
		mArchiveStream.Dispose();
		mArchiveStream = null;
	}

	#endregion
}
