///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using GriffinPlus.Lib.Logging;
using System.Runtime.InteropServices;
using GriffinPlus.Lib.Io;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Archive providing support for serialization/deserialization of classes using the <see cref="Serializer"/> class.
	/// </summary>
	public class SerializerArchive
	{
		#region Class Variables

		static LogWriter sLog = Log.GetWriter(typeof(SerializerArchive));

		#endregion

		#region Member Variables

		private Serializer              mSerializer;
		private Stream                  mStream;
		private Type                    mType;
		private uint                    mVersion;
		private object                  mContext;
		private SerializerArchiveStream mArchiveStream;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerArchive"/> class.
		/// </summary>
		/// <param name="serializer">The executing serializer.</param>
		/// <param name="stream">Stream to write to/read from.</param>
		/// <param name="type">Type the archive stores data from (used to instantiate the right class during deserialization).</param>
		/// <param name="version">Version of the type the archive contains data from.</param>
		/// <param name="context">User-specific context object.</param>
		internal SerializerArchive(Serializer serializer, Stream stream, Type type, uint version, object context)
		{
			mSerializer = serializer;
			mStream = stream;
			mType = type;
			mVersion = version;
			mContext = context;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the version of the data in the archive.
		/// </summary>
		public uint Version
		{
			get { return mVersion; }
		}

		/// <summary>
		/// Gets the type of the struct/class the archive contains data from.
		/// </summary>
		public Type DataType
		{
			get { return mType; }
		}

		/// <summary>
		/// User-defined context object.
		/// </summary>
		/// <remarks>
		/// If you do use the context object, you must consider the case that the context object may be null, even if you are sure
		/// that you always specify a valid context object. This is necessary to enable the serializer to skip unknown types properly.
		/// </remarks>
		public object Context
		{
			get { return mContext; }
		}

		#endregion

		#region Internal Properties

		/// <summary>
		/// Gets the type the current archive contains data from.
		/// </summary>
		internal Type Type {
			get {
				return mType;
			}
		}

		#endregion

		#region System.SByte

		/// <summary>
		/// Writes a <see cref="System.SByte"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(sbyte value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_SByte(value, mStream);
		}

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
		/// Writes a <see cref="System.Int16"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(short value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Int16(value, mStream);
		}

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
		/// Writes a <see cref="System.Int32"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(int value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Int32(value, mStream);
		}

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
		/// Writes a <see cref="System.Int64"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(long value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Int64(value, mStream);
		}

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
		/// Writes a <see cref="System.Byte"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(byte value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Byte(value, mStream);
		}

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
		/// Writes a <see cref="System.UInt16"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(ushort value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_UInt16(value, mStream);
		}

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
		/// Writes a <see cref="System.UInt32"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(uint value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_UInt32(value, mStream);
		}

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
		/// Writes a <see cref="System.UInt64"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(ulong value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_UInt64(value, mStream);
		}

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
		/// Writes an enumeration value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(Enum value)
		{
			CloseArchiveStream();
			mSerializer.InnerSerialize(mStream, value, null);
		}

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
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected payload type during deserialization. Stack Trace:\n{0}", trace.ToString());
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}

			return (Enum )obj;
		}

		/// <summary>
		/// Reads an enumeration value from the archive.
		/// </summary>
		/// <typeparam name="T">Enumeration type to read.</typeparam>
		/// <returns>The read value.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public T ReadEnum<T>()
		{
			CloseArchiveStream();
			object obj = mSerializer.InnerDeserialize(mStream, null);

			if (obj == null || obj.GetType() != typeof(T))
			{
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected payload type during deserialization. Stack Trace:\n{0}", trace.ToString());
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}

			return (T )obj;
		}

		#endregion

		#region System.Boolean

		/// <summary>
		/// Writes a <see cref="System.Boolean"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(bool value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Boolean(value, mStream);
		}

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
		/// Writes a <see cref="System.Char"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(char value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Char(value, mStream);
		}

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
		/// Writes a <see cref="System.Decimal"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(decimal value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Decimal(value, mStream);
		}

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
		/// Writes a <see cref="System.Single"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(float value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Single(value, mStream);
		}

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
		/// Writes a <see cref="System.Double"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(double value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_Double(value, mStream);
		}

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
		/// Writes a <see cref="System.String"/> object to the archive.
		/// </summary>
		/// <param name="value">String to write to the archive.</param>
		public void Write(string value)
		{
			// use the serializer to ensure already serialized strings are handled properly
			CloseArchiveStream();
			mSerializer.InnerSerialize(mStream, value, null);
		}

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
		/// Writes a <see cref="System.Type"/> object to the archive.
		/// </summary>
		/// <param name="value">Type to write to the archive.</param>
		public void Write(Type value)
		{
			// use the serializer to ensure already serialized types are handled properly
			CloseArchiveStream();
			mSerializer.InnerSerialize(mStream, value, null);
		}

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
		/// Writes a <see cref="System.DateTime"/> value to the archive.
		/// </summary>
		/// <param name="value">Value to write to the archive.</param>
		public void Write(DateTime value)
		{
			CloseArchiveStream();
			mSerializer.WritePrimitive_DateTime(value, mStream);
		}

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
		/// Writes an object to the archive.
		/// </summary>
		/// <param name="obj">Object to write to the archive.</param>
		public void Write(object obj)
		{
			CloseArchiveStream();
			mSerializer.InnerSerialize(mStream, obj, null);
		}

		/// <summary>
		/// Writes an object to the archive.
		/// </summary>
		/// <param name="obj">Object to write to the archive.</param>
		/// <param name="context">Context object to pass to the serializer via the serializer archive.</param>
		public void Write(object obj, object context)
		{
			CloseArchiveStream();
			mSerializer.InnerSerialize(mStream, obj, context);
		}

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
		/// <returns>Serializer archive for the base class.</returns>
		/// <exception cref="ArgumentException">Type is not serializable.</exception>
		/// <exception cref="SerializationException">Archive does not contain an archive for the specified class.</exception>
		public SerializerArchive PrepareBaseArchive(Type type)
		{
			return PrepareBaseArchive(type, mContext);
		}

		/// <summary>
		/// Prepares an archive for deserializing the base class of a serializable class.
		/// </summary>
		/// <param name="type">Base class type.</param>
		/// <param name="context">Context object to pass to the serializer of the base class.</param>
		/// <returns>Serializer archive for the base class.</returns>
		/// <exception cref="ArgumentException">Type is not serializable.</exception>
		/// <exception cref="SerializationException">Archive does not contain an archive for the specified class.</exception>
		public SerializerArchive PrepareBaseArchive(Type type, object context)
		{
			// read payload type (expecting a base class archive)
			ReadAndCheckPayloadType(PayloadType.BaseArchiveStart);

			// read version number
			uint deserializedVersion = LEB128.ReadUInt32(mStream);

			// check maximum supported version number
			uint currentVersion = Serializer.GetSerializerVersion(type); // throws ArgumentException if type is not serializable
			if (deserializedVersion > currentVersion)
			{
				// version of the archive that is about to be deserialized is greater than
				// the version the internal object serializer supports
				string error = string.Format("Deserializing type '{0}' failed due to a version conflict (got version: {1}, max. supported version: {2}).", type.FullName, deserializedVersion, currentVersion);
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}

			// version is ok, create archive...
			return new SerializerArchive(mSerializer, mStream, type, deserializedVersion, context);
		}

		/// <summary>
		/// Openes a base archive and calls the serializer of the specified type (for base class serialization).
		/// </summary>
		/// <param name="obj">Object to serialize.</param>
		/// <param name="type">Type of the base class to serialize.</param>
		/// <param name="context">Context object to pass to the serializer of the base class.</param>
		/// <exception cref="ArgumentException">Specified type is not serializable.</exception>
		public void WriteBaseArchive(object obj, Type type, object context = null)
		{
			CloseArchiveStream();

			// ensure the specified type is the type of the specified object or the type of one of its base classes
			if (obj.GetType().IsAssignableFrom(type)) {
				throw new ArgumentException("Specified type is neither the type of the specified object nor the type of one of its base classes.", "obj,type");
			}

			// try external object serializer
			uint version;
			IExternalObjectSerializer eos = Serializer.GetExternalObjectSerializer(type, out version);
			if (eos != null)
			{
				// consider serializer version overrides...
				uint versionOverride;
				if (mSerializer.GetSerializerVersionOverride(type, out versionOverride)) {
					version = versionOverride;
				}

				// write base archive header
				byte[] buffer = mSerializer.mTempBuffer_Buffer;
				buffer[0] = (byte)PayloadType.BaseArchiveStart;
				int count = LEB128.Write(buffer, 1, version);
				mStream.Write(buffer, 0, 1 + count);

				// serialize object
				SerializerArchive archive = new SerializerArchive(mSerializer, mStream, type, version, context);
				eos.Serialize(archive, version, obj);
				archive.Close();
				return;
			}

			// try internal object serializer
			IInternalObjectSerializer ios = Serializer.GetInternalObjectSerializer(obj, type, out version);
			if (ios != null)
			{
				// consider serializer version overrides...
				uint versionOverride;
				if (mSerializer.GetSerializerVersionOverride(type, out versionOverride)) {
					version = versionOverride;
				}

				// write base archive header
				byte[] buffer = mSerializer.mTempBuffer_Buffer;
				buffer[0] = (byte)PayloadType.BaseArchiveStart;
				int count = LEB128.Write(buffer, 1, version);
				mStream.Write(buffer, 0, 1 + count);

				// call the Serialize() method of the base class
				SerializerArchive archive = new SerializerArchive(mSerializer, mStream, type, version, context);
				Serializer.IosSerializeDelegate serializeDelegate = Serializer.GetInternalObjectSerializerSerializeCaller(type);
				serializeDelegate(ios, archive, version);
				archive.Close();
				return;
			}

			// specified type is not serializable...
			string error = string.Format("Specified type ({0}) is not serializable.", type.FullName);
			throw new ArgumentException(error, nameof(type));
		}

		#endregion

		#region Buffer

		/// <summary>
		/// Writes a buffer to the archive.
		/// </summary>
		/// <param name="p">Pointer to the beginning of the buffer.</param>
		/// <param name="count">Number of bytes to write.</param>
		public unsafe void Write(IntPtr p, long count)
		{
			CloseArchiveStream();

			// write payload type and size of the following buffer
			mSerializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Buffer;
			int writtenBytes = LEB128.Write(mSerializer.mTempBuffer_Buffer, 1, count);
			mStream.Write(mSerializer.mTempBuffer_Buffer, 0, writtenBytes + 1);

			if (mStream is MemoryBlockStream)
			{
				// the MemoryBlockStream provides a direct way to write to the underlying buffer more effiently
				MemoryBlockStream mbs = mStream as MemoryBlockStream;
				while (count > 0) {
					int bytesToCopy = (int)Math.Min(count, int.MaxValue);
					mbs.Write(new ReadOnlySpan<byte>(p.ToPointer(), bytesToCopy));
					count -= bytesToCopy;
					p += bytesToCopy;
				}
			}
			else
			{
				// some other stream
				// => copying data to a temporary buffer is needed before passing it to the stream
				if (mSerializer.mTempBuffer_BigBuffer.Length < 256*1024) mSerializer.mTempBuffer_BigBuffer = new byte[256*1024];
				while (count > 0) {
					int bytesToCopy = (int)Math.Min(count, mSerializer.mTempBuffer_BigBuffer.Length);
					Marshal.Copy(p, mSerializer.mTempBuffer_BigBuffer, 0, bytesToCopy);
					mStream.Write(mSerializer.mTempBuffer_BigBuffer, 0, bytesToCopy);
					count -= bytesToCopy;
					p += bytesToCopy;
				}
			}
		}

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
			long length = LEB128.ReadInt64(mStream);

			// now we know how much bytes will be returned...
			long bytesReturned = Math.Min(length, count);

			if (mStream is MemoryBlockStream)
			{
				// the MemoryBlockStream provides a direct way to read from the underlying buffer more effiently
				// => let the stream directly copy data into the specified buffer
				MemoryBlockStream mbs = mStream as MemoryBlockStream;
				while (count > 0 && length > 0) {
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

				if (mSerializer.mTempBuffer_BigBuffer.Length < 256*1024) {
					mSerializer.mTempBuffer_BigBuffer = new byte[256*1024];
				}

				while (count > 0 && length > 0) {
					int bytesToRead = (int)Math.Min(count, mSerializer.mTempBuffer_BigBuffer.Length);
					if (mStream.Read(mSerializer.mTempBuffer_BigBuffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
					Marshal.Copy(mSerializer.mTempBuffer_BigBuffer, 0, p, bytesToRead);
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
					if (mSerializer.mTempBuffer_BigBuffer.Length < 256*1024) mSerializer.mTempBuffer_BigBuffer = new byte[256*1024];
					while (length > 0) {
						int bytesToRead = (int)Math.Min(length, mSerializer.mTempBuffer_BigBuffer.Length);
						if (mStream.Read(mSerializer.mTempBuffer_BigBuffer, 0, bytesToRead) != bytesToRead) throw new SerializationException("Stream ended unexpectedly.");
						length -= bytesToRead;
					}
				}
			}

			return bytesReturned;
		}

		#endregion

		#region Stream

		/// <summary>
		/// Writes the contents of a stream to the archive (from the current position up to the end of the stream).
		/// </summary>
		/// <param name="s">Stream to write.</param>
		public void Write(Stream s)
		{
			CloseArchiveStream();

			if (s.CanSeek)
			{
				// stream can seek
				// => stream is usable without additional preparation
				WriteInternal(s);
			}
			else
			{
				// stream cannot seek
				// => read data into memory to make it seekable
				using (MemoryBlockStream bufferStream = new MemoryBlockStream())
				{
					// read stream into temporary buffer
					if (mSerializer.mTempBuffer_BigBuffer.Length < 256*1024) mSerializer.mTempBuffer_BigBuffer = new byte[256*1024];
					while (true) {
						int bytesRead = s.Read(mSerializer.mTempBuffer_BigBuffer, 0, mSerializer.mTempBuffer_BigBuffer.Length);
						if (bytesRead == 0) break;
						bufferStream.Write(mSerializer.mTempBuffer_BigBuffer, 0, bytesRead);
					}

					bufferStream.Position = 0;
					WriteInternal(bufferStream);
				}
			}
		}

		/// <summary>
		/// Internal method handling writing the content of a stream.
		/// </summary>
		/// <param name="stream">Stream containing data to write (must be seekable).</param>
		private void WriteInternal(Stream stream)
		{
			CloseArchiveStream();

			// write payload type and size of the following buffer
			long count = stream.Length;
			mSerializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Buffer;
			int writtenBytes = LEB128.Write(mSerializer.mTempBuffer_Buffer, 1, count);
			mStream.Write(mSerializer.mTempBuffer_Buffer, 0, writtenBytes+1);

			if (mStream is MemoryBlockStream)
			{
				// the MemoryBlockStream provides a direct way to write to the underlying buffer more effiently
				MemoryBlockStream mbs = mStream as MemoryBlockStream;
				mbs.Write(stream);
			}
			else
			{
				// some other stream
				// => copying data to a temporary buffer is needed before passing it to the stream
				if (mSerializer.mTempBuffer_BigBuffer.Length < 256*1024) mSerializer.mTempBuffer_BigBuffer = new byte[256*1024];
				while (true) {
					int bytesRead = stream.Read(mSerializer.mTempBuffer_BigBuffer, 0, mSerializer.mTempBuffer_BigBuffer.Length);
					if (bytesRead == 0) break;
					mStream.Write(mSerializer.mTempBuffer_BigBuffer, 0, bytesRead);
				}
			}
		}

		/// <summary>
		/// Reads a byte buffer using a stream.
		/// </summary>
		/// <returns>Stream containing data to read.</returns>
		/// <exception cref="SerializationException">Thrown if deserialization fails due to some reason.</exception>
		public Stream ReadStream()
		{
			// read payload type and size of the following buffer
			ReadAndCheckPayloadType(PayloadType.Buffer);
			long length = LEB128.ReadInt64(mStream);

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

			int readbyte = mStream.ReadByte();
			if (readbyte < 0) throw new SerializationException("Stream ended unexpectedly.");
			if (readbyte != (int)type) {
				Debug.Fail("Unexpected payload type during deserialization.");
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected payload type during deserialization. Stack Trace:\n{0}", trace.ToString());
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}
		}

		/// <summary>
		/// Checks whether the specfied object is of the specified type and throws a SerializationException
		/// if the type of the specified object does not match the specified type.
		/// </summary>
		/// <param name="obj">Object to check.</param>
		/// <param name="type">Type to check the object for.</param>
		/// <exception cref="SerializationException">Specified object is not of the specified type.</exception>
		private void CheckExpectedType(object obj, Type type)
		{
			if (obj == null) {
				if (!type.IsValueType) return; // null is valid for reference types
				Debug.Fail("Unexpected type during deserialization.");
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected type during deserialization (expected: '{0}', got: null). Stack Trace:\n{1}", type.FullName, trace.ToString());
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}

			if (!type.IsInstanceOfType(obj)) {
				Debug.Fail("Unexpected type during deserialization.");
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected type during deserialization (expected: '{0}', got: '{1}'). Stack Trace:\n{2}", type.FullName, obj.GetType().FullName, trace.ToString());
				sLog.Write(LogLevel.Failure, error);
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
					byte[] buffer = mSerializer.mTempBuffer_BigBuffer;
					while (bytesToSkip > 0) {
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
