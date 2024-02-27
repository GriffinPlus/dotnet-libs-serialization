///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// Archive providing support for the serialization of classes using the <see cref="Serializer"/> class.
/// </summary>
public readonly ref struct SerializationArchive
{
	#region Member Variables

	private readonly object mObjectToSerialize;

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="SerializationArchive"/> class.
	/// </summary>
	/// <param name="serializer">The executing serializer.</param>
	/// <param name="writer">Writer to use for writing.</param>
	/// <param name="type">Type to serialize.</param>
	/// <param name="objectToSerialize">Object to serialize.</param>
	/// <param name="version">Version of the type the archive contains data from.</param>
	/// <param name="context">User-specific context object.</param>
	internal SerializationArchive(
		Serializer          serializer,
		IBufferWriter<byte> writer,
		Type                type,
		object              objectToSerialize,
		uint                version,
		object              context)
	{
		Serializer = serializer;
		Writer = writer;
		mObjectToSerialize = objectToSerialize;
		DataType = type;
		Version = version;
		Context = context;
	}

	#endregion

	#region Public Properties

	/// <summary>
	/// Gets the version of the serializer to use.
	/// </summary>
	public uint Version { get; }

	/// <summary>
	/// Gets the type to serialize into the archive.
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
	/// Gets the <see cref="IBufferWriter{T}"/> that is used to write to the stream.
	/// Usually it is more convenient to use the archive's Write() methods to write to the stream,
	/// but if you need more control the <see cref="IBufferWriter{T}"/> allows to write byte-wise.
	/// </summary>
	public IBufferWriter<byte> Writer { get; }

	#endregion

	#region System.SByte

	/// <summary>
	/// Writes a <see cref="System.SByte"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(sbyte value)
	{
		Serializer.WritePrimitive_SByte(value, Writer);
	}

	#endregion

	#region System.Int16

	/// <summary>
	/// Writes a <see cref="System.Int16"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(short value)
	{
		Serializer.WritePrimitive_Int16(value, Writer);
	}

	#endregion

	#region System.Int32

	/// <summary>
	/// Writes a <see cref="System.Int32"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(int value)
	{
		Serializer.WritePrimitive_Int32(value, Writer);
	}

	#endregion

	#region System.Int64

	/// <summary>
	/// Writes a <see cref="System.Int64"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(long value)
	{
		Serializer.WritePrimitive_Int64(value, Writer);
	}

	#endregion

	#region System.Byte

	/// <summary>
	/// Writes a <see cref="System.Byte"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(byte value)
	{
		Serializer.WritePrimitive_Byte(value, Writer);
	}

	#endregion

	#region System.UInt16

	/// <summary>
	/// Writes a <see cref="System.UInt16"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(ushort value)
	{
		Serializer.WritePrimitive_UInt16(value, Writer);
	}

	#endregion

	#region System.UInt32

	/// <summary>
	/// Writes a <see cref="System.UInt32"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(uint value)
	{
		Serializer.WritePrimitive_UInt32(value, Writer);
	}

	#endregion

	#region System.UInt64

	/// <summary>
	/// Writes a <see cref="System.UInt64"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(ulong value)
	{
		Serializer.WritePrimitive_UInt64(value, Writer);
	}

	#endregion

	#region System.Enum

	/// <summary>
	/// Writes an enumeration value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(Enum value)
	{
		Serializer.InnerSerialize(Writer, value, null);
	}

	#endregion

	#region System.Boolean

	/// <summary>
	/// Writes a <see cref="System.Boolean"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(bool value)
	{
		Serializer.WritePrimitive_Boolean(value, Writer);
	}

	#endregion

	#region System.Char

	/// <summary>
	/// Writes a <see cref="System.Char"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(char value)
	{
		Serializer.WritePrimitive_Char(value, Writer);
	}

	#endregion

	#region System.Decimal

	/// <summary>
	/// Writes a <see cref="System.Decimal"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(decimal value)
	{
		Serializer.WritePrimitive_Decimal(value, Writer);
	}

	#endregion

	#region System.Single

	/// <summary>
	/// Writes a <see cref="System.Single"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(float value)
	{
		Serializer.WritePrimitive_Single(value, Writer);
	}

	#endregion

	#region System.Double

	/// <summary>
	/// Writes a <see cref="System.Double"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(double value)
	{
		Serializer.WritePrimitive_Double(value, Writer);
	}

	#endregion

	#region System.String

	/// <summary>
	/// Writes a <see cref="System.String"/> object to the archive.
	/// </summary>
	/// <param name="value">String to write to the archive.</param>
	public void Write(string value)
	{
		// use the serializer to ensure already serialized strings are handled properly
		Serializer.InnerSerialize(Writer, value, null);
	}

	#endregion

	#region System.Type

	/// <summary>
	/// Writes a <see cref="System.Type"/> object to the archive.
	/// </summary>
	/// <param name="value">Type to write to the archive.</param>
	public void Write(Type value)
	{
		// use the serializer to ensure already serialized types are handled properly
		Serializer.InnerSerialize(Writer, value, null);
	}

	#endregion

	#region System.DateTime

	/// <summary>
	/// Writes a <see cref="System.DateTime"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(DateTime value)
	{
		Serializer.WritePrimitive_DateTime(value, Writer);
	}

	#endregion

	#region System.DateTimeOffset

	/// <summary>
	/// Writes a <see cref="System.DateTimeOffset"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(DateTimeOffset value)
	{
		Serializer.WritePrimitive_DateTimeOffset(value, Writer);
	}

	#endregion

	#region System.Guid

	/// <summary>
	/// Writes a <see cref="System.Guid"/> value to the archive.
	/// </summary>
	/// <param name="value">Value to write to the archive.</param>
	public void Write(Guid value)
	{
		Serializer.WritePrimitive_Guid(value, Writer);
	}

	#endregion

	#region System.Object

	/// <summary>
	/// Writes an object to the archive.
	/// </summary>
	/// <param name="obj">Object to write to the archive.</param>
	public void Write(object obj)
	{
		Serializer.InnerSerialize(Writer, obj, null);
	}

	/// <summary>
	/// Writes an object to the archive.
	/// </summary>
	/// <param name="obj">Object to write to the archive.</param>
	/// <param name="context">Context object to pass to the serializer via the serializer archive.</param>
	public void Write(object obj, object context)
	{
		Serializer.InnerSerialize(Writer, obj, context);
	}

	#endregion

	#region Class Hierarchies

	/// <summary>
	/// Opens a base archive and calls the serializer of the derived type (for base class serialization).
	/// </summary>
	/// <param name="context">Context object to pass to the serializer of the base class.</param>
	/// <exception cref="ArgumentException">Specified type is not serializable.</exception>
	public void WriteBaseArchive(object context = null)
	{
		Type baseClassType = DataType.BaseType ?? throw new ArgumentException($"{DataType.FullName} does not have a base class.");

		// try internal object serializer
		IInternalObjectSerializer ios = Serializer.GetInternalObjectSerializer(mObjectToSerialize, baseClassType, out uint version);
		if (ios != null)
		{
			// consider serializer version overrides...
			if (Serializer.GetSerializerVersionOverride(baseClassType, out uint versionOverride))
				version = versionOverride;

			// write base archive header
			Span<byte> buffer = Writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.BaseArchiveStart;
			bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], version);
			Writer.Advance(bufferIndex);

			// call the Serialize() method of the base class
			var archive = new SerializationArchive(Serializer, Writer, baseClassType, mObjectToSerialize, version, context);
			Serializer.IosSerializeDelegate serializeDelegate = Serializer.GetInternalObjectSerializerSerializeCaller(baseClassType);
			serializeDelegate(ios, archive);
			return;
		}

		// try external object serializer
		IExternalObjectSerializer eos = ExternalObjectSerializerFactory.GetExternalObjectSerializer(baseClassType, out version);
		// ReSharper disable once InvertIf
		if (eos != null)
		{
			// consider serializer version overrides...
			if (Serializer.GetSerializerVersionOverride(baseClassType, out uint versionOverride))
				version = versionOverride;

			// write base archive header
			Span<byte> buffer = Writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.BaseArchiveStart;
			bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], version);
			Writer.Advance(bufferIndex);

			// serialize object
			var archive = new SerializationArchive(Serializer, Writer, baseClassType, mObjectToSerialize, version, context);
			eos.Serialize(archive, mObjectToSerialize);
			return;
		}

		// specified type is not serializable...
		throw new ArgumentException($"Specified type ({baseClassType.FullName}) is not serializable.", nameof(baseClassType));
	}

	#endregion

	#region Buffer

	/// <summary>
	/// Writes a buffer to the archive.
	/// </summary>
	/// <param name="p">Pointer to the beginning of the buffer.</param>
	/// <param name="count">Number of bytes to write.</param>
	public unsafe void Write(void* p, long count)
	{
		// write payload type
		Span<byte> buffer = Writer.GetSpan(1);
		buffer[0] = (byte)PayloadType.Buffer;
		Writer.Advance(1);

		// write data chunkwise
		byte* pFrom = (byte*)p;
		byte* pTo = pFrom + count;
		while (pFrom != pTo)
		{
			Debug.Assert(Serializer.MaxChunkSize > Leb128EncodingHelper.MaxBytesFor32BitValue);
			buffer = Writer.GetSpan(Serializer.MaxChunkSize);
			int bytesToCopy = Math.Min((int)(pTo - pFrom), buffer.Length - Leb128EncodingHelper.MaxBytesFor32BitValue);
			int bufferIndex = Leb128EncodingHelper.Write(buffer, bytesToCopy);
			new Span<byte>(pFrom, bytesToCopy).CopyTo(buffer[bufferIndex..]);
			bufferIndex += bytesToCopy;
			pFrom += bytesToCopy;
			Writer.Advance(bufferIndex);
		}
	}

	#endregion

	#region Stream

	/// <summary>
	/// Writes the contents of a stream to the archive (from the current position up to the end of the stream).
	/// </summary>
	/// <param name="s">Stream to write.</param>
	public void Write(Stream s)
	{
		// write payload type
		Span<byte> buffer = Writer.GetSpan(1);
		buffer[0] = (byte)PayloadType.Buffer;
		Writer.Advance(1);

		// write data chunkwise
		while (true)
		{
			// read a chunk of data
			const int maxBytesToRead = Serializer.MaxChunkSize - Leb128EncodingHelper.MaxBytesFor32BitValue;
			Serializer.EnsureTemporaryByteBufferSize(maxBytesToRead);
			int bytesRead = s.Read(Serializer.TempBuffer_Buffer, 0, maxBytesToRead);
			if (bytesRead == 0) break;

			// write the chunk into the archive
			buffer = Writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue + bytesRead);
			int bufferIndex = Leb128EncodingHelper.Write(buffer, bytesRead);
			Serializer.TempBuffer_Buffer.AsSpan()[..bytesRead].CopyTo(buffer[bufferIndex..]);
			bufferIndex += bytesRead;
			Writer.Advance(bufferIndex);
		}
	}

	#endregion
}
