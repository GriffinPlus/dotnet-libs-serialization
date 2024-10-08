///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using GriffinPlus.Lib.Collections;
using GriffinPlus.Lib.Io;
using GriffinPlus.Lib.Logging;

#pragma warning disable CS0618

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable InconsistentlySynchronizedField

namespace GriffinPlus.Lib.Serialization;

/// <summary>
/// Serializer for serializing and deserializing various objects.
/// </summary>
public partial class Serializer
{
	#region Constants

	/// <summary>
	/// Maximum buffer size to use (in bytes).
	/// When serializing writers will only write up to this number of bytes at once.
	/// </summary>
	internal const int MaxChunkSize = 256 * 1024;

	#endregion

	#region Class Variables

	private static readonly LogWriter    sLog          = LogWriter.Get<Serializer>();
	private static readonly UTF8Encoding sUtf8Encoding = new(false);
	private static readonly object       sSync         = new();

	#endregion

	#region Member Variables

	// for serialization only
	private          Type                                  mCurrentSerializedType      = default;
	private readonly TypeKeyedDictionary<uint>             mSerializedTypeIdTable      = new();
	private readonly SerializerVersionTable                mSerializedTypeVersionTable = new();
	private readonly IdentityKeyedDictionary<object, uint> mSerializedObjectIdTable    = new();
	private readonly HashSet<object>                       mObjectsUnderSerialization  = new(IdentityComparer<object>.Default);
	private readonly StreamBufferWriter                    mWriter                     = new();
	private          uint                                  mNextSerializedTypeId       = default;
	private          uint                                  mNextSerializedObjectId     = default;

	// for deserialization only
	private          TypeItem                   mCurrentDeserializedType   = default;
	private readonly Dictionary<uint, TypeItem> mDeserializedTypeIdTable   = new();
	private readonly Dictionary<uint, object>   mDeserializedObjectIdTable = new();
	private          uint                       mNextDeserializedTypeId    = default;
	private          uint                       mNextDeserializedObjectId  = default;

	#endregion

	#region Initialization

	private static          bool                                              sInitializing                        = false;
	private static          bool                                              sInitialized                         = false;
	private static readonly object                                            sInitializationSync                  = new();
	private static volatile bool                                              sUseTolerantDeserializationByDefault = false;
	private static          TypeKeyedDictionary<InternalObjectSerializerInfo> sInternalObjectSerializerInfoByType  = new();
	private static readonly Type[]                                            sConstructorArgumentTypes            = [typeof(DeserializationArchive)];

	/// <summary>
	/// Initializes the serializer, if necessary.
	/// </summary>
	public static void Init()
	{
		if (sInitialized)
			return;

		lock (sInitializationSync)
		{
			if (sInitialized || sInitializing)
				return;

			// initialize built-in serializers
			sInitializing = true;
			InitBuiltinSerializers();
			InitBuiltinDeserializers();

			// hook up the event that is raised when an assembly is loaded into the application domain
			// and run the event handler for already loaded assemblies to find custom serializers
			RuntimeMetadata.LoadAssembliesInApplicationBaseDirectory(true);
			RuntimeMetadata.AssemblyScanned += (_, args) => ScanAssemblyForCustomSerializers(args.Assembly);
			foreach (Assembly assembly in RuntimeMetadata.AssembliesByFullName.Select(x => x.Value))
			{
				ScanAssemblyForCustomSerializers(assembly);
			}

			// print the detected custom serializers to the log
			PrintToLog(LogLevel.Debug);
			sInitialized = true;
			sInitializing = false;
		}
	}

	/// <summary>
	/// Triggers initializing the serializer asynchronously, if necessary.
	/// </summary>
	public static void TriggerInit()
	{
		if (sInitialized || sInitializing)
			return;

		lock (sInitializationSync)
		{
			if (!sInitialized && !sInitializing)
			{
				ThreadPool.QueueUserWorkItem(_ => Init());
			}
		}
	}

	/// <summary>
	/// Adds serializers for types that are supported out of the box.
	/// </summary>
	private static void InitBuiltinSerializers()
	{
		// simple types
		sSerializers.Add(
			typeof(bool),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Boolean((bool)obj, writer);
			});

		sSerializers.Add(
			typeof(char),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Char((char)obj, writer);
			});

		sSerializers.Add(
			typeof(sbyte),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_SByte((sbyte)obj, writer);
			});

		sSerializers.Add(
			typeof(short),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Int16((short)obj, writer);
			});

		sSerializers.Add(
			typeof(int),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Int32((int)obj, writer);
			});

		sSerializers.Add(
			typeof(long),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Int64((long)obj, writer);
			});

		sSerializers.Add(
			typeof(byte),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Byte((byte)obj, writer);
			});

		sSerializers.Add(
			typeof(ushort),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_UInt16((ushort)obj, writer);
			});

		sSerializers.Add(
			typeof(uint),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_UInt32((uint)obj, writer);
			});

		sSerializers.Add(
			typeof(ulong),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_UInt64((ulong)obj, writer);
			});

		sSerializers.Add(
			typeof(float),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Single((float)obj, writer);
			});

		sSerializers.Add(
			typeof(double),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Double((double)obj, writer);
			});

		sSerializers.Add(
			typeof(decimal),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Decimal((decimal)obj, writer);
			});

		sSerializers.Add(
			typeof(string),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_String((string)obj, writer);
			});

		sSerializers.Add(
			typeof(DateTime),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_DateTime((DateTime)obj, writer);
			});

		sSerializers.Add(
			typeof(DateTimeOffset),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_DateTimeOffset((DateTimeOffset)obj, writer);
			});

#if NET6_0_OR_GREATER
		sSerializers.Add(
			typeof(DateOnly),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_DateOnly((DateOnly)obj, writer);
			});

		sSerializers.Add(
			typeof(TimeOnly),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_TimeOnly((TimeOnly)obj, writer);
			});
#endif

		sSerializers.Add(
			typeof(Guid),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Guid((Guid)obj, writer);
			});

		sSerializers.Add(
			typeof(object),
			(
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WritePrimitive_Object(obj, writer);
			});

		// arrays of simple types (one-dimensional, zero-based indexing)

		sSerializers.Add(
			typeof(bool[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as bool[], writer);
			});

		sSerializers.Add(
			typeof(char[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as char[], writer);
			});

		sSerializers.Add(
			typeof(sbyte[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as sbyte[], writer);
			});

		sSerializers.Add(
			typeof(short[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as short[], writer);
			});

		sSerializers.Add(
			typeof(int[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as int[], writer);
			});

		sSerializers.Add(
			typeof(long[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as long[], writer);
			});

		sSerializers.Add(
			typeof(byte[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArrayOfByte(array as byte[], writer);
			});

		sSerializers.Add(
			typeof(ushort[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as ushort[], writer);
			});

		sSerializers.Add(
			typeof(uint[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as uint[], writer);
			});

		sSerializers.Add(
			typeof(ulong[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as ulong[], writer);
			});

		sSerializers.Add(
			typeof(float[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as float[], writer);
			});

		sSerializers.Add(
			typeof(double[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as double[], writer);
			});

		sSerializers.Add(
			typeof(decimal[]),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteArray(array as decimal[], writer);
			});

		// multidimensional arrays

		sMultidimensionalArraySerializers.Add(
			typeof(bool),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfBoolean(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(char),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfChar(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(sbyte),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfSByte(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(short),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfInt16(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(int),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfInt32(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(long),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfInt64(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(byte),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfByte(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(ushort),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfUInt16(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(uint),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfUInt32(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(ulong),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfUInt64(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(float),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfSingle(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(double),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfDouble(array as Array, writer);
			});

		sMultidimensionalArraySerializers.Add(
			typeof(decimal),
			(
				serializer,
				writer,
				array,
				_) =>
			{
				serializer.WriteMultidimensionalArrayOfDecimal(array as Array, writer);
			});
	}

	/// <summary>
	/// Adds deserializers for types that are supported out of the box.
	/// </summary>
	private static void InitBuiltinDeserializers()
	{
		// special types
		sDeserializersByPayloadType[(int)PayloadType.NullReference] = (_,              _,      _) => null;
		sDeserializersByPayloadType[(int)PayloadType.AlreadySerialized] = (serializer, stream, _) => serializer.ReadAlreadySerializedObject(stream);
		sDeserializersByPayloadType[(int)PayloadType.Enum] = (serializer,              stream, _) => serializer.ReadEnum(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArchiveStart] = (serializer,      stream, context) => serializer.ReadArchive(stream, context);

		// built-in types
		sDeserializersByPayloadType[(int)PayloadType.BooleanFalse] = (_,            _,      _) => false;
		sDeserializersByPayloadType[(int)PayloadType.BooleanTrue] = (_,             _,      _) => true;
		sDeserializersByPayloadType[(int)PayloadType.Char_Native] = (serializer,    stream, _) => serializer.ReadPrimitive_Char_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.Char_LEB128] = (serializer,    stream, _) => serializer.ReadPrimitive_Char_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.SByte] = (serializer,          stream, _) => serializer.ReadPrimitive_SByte(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int16_Native] = (serializer,   stream, _) => serializer.ReadPrimitive_Int16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int16_LEB128] = (serializer,   stream, _) => serializer.ReadPrimitive_Int16_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int32_Native] = (serializer,   stream, _) => serializer.ReadPrimitive_Int32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int32_LEB128] = (serializer,   stream, _) => serializer.ReadPrimitive_Int32_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int64_Native] = (serializer,   stream, _) => serializer.ReadPrimitive_Int64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.Int64_LEB128] = (serializer,   stream, _) => serializer.ReadPrimitive_Int64_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.Byte] = (serializer,           stream, _) => serializer.ReadPrimitive_Byte(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt16_Native] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt16_LEB128] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt16_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt32_Native] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt32_LEB128] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt32_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt64_Native] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.UInt64_LEB128] = (serializer,  stream, _) => serializer.ReadPrimitive_UInt64_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.Single] = (serializer,         stream, _) => serializer.ReadPrimitive_Single(stream);
		sDeserializersByPayloadType[(int)PayloadType.Double] = (serializer,         stream, _) => serializer.ReadPrimitive_Double(stream);
		sDeserializersByPayloadType[(int)PayloadType.Decimal] = (serializer,        stream, _) => serializer.ReadPrimitive_Decimal(stream);
		sDeserializersByPayloadType[(int)PayloadType.String_UTF8] = (serializer,    stream, _) => serializer.ReadPrimitive_String_UTF8(stream);
		sDeserializersByPayloadType[(int)PayloadType.String_UTF16] = (serializer,   stream, _) => serializer.ReadPrimitive_String_UTF16(stream);
		sDeserializersByPayloadType[(int)PayloadType.DateTime] = (serializer,       stream, _) => serializer.ReadPrimitive_DateTime(stream);
		sDeserializersByPayloadType[(int)PayloadType.DateTimeOffset] = (serializer, stream, _) => serializer.ReadPrimitive_DateTimeOffset(stream);
#if NET6_0_OR_GREATER
		sDeserializersByPayloadType[(int)PayloadType.DateOnly_Native] = (serializer, stream, _) => serializer.ReadPrimitive_DateOnly_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.DateOnly_LEB128] = (serializer, stream, _) => serializer.ReadPrimitive_DateOnly_LEB128(stream);
		sDeserializersByPayloadType[(int)PayloadType.TimeOnly_Native] = (serializer, stream, _) => serializer.ReadPrimitive_TimeOnly_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.TimeOnly_LEB128] = (serializer, stream, _) => serializer.ReadPrimitive_TimeOnly_LEB128(stream);
#endif
		sDeserializersByPayloadType[(int)PayloadType.Guid] = (serializer,       stream, _) => serializer.ReadPrimitive_Guid(stream);
		sDeserializersByPayloadType[(int)PayloadType.Object] = (serializer,     _,      _) => serializer.ReadPrimitive_Object();
		sDeserializersByPayloadType[(int)PayloadType.TypeObject] = (serializer, stream, _) => serializer.ReadTypeObject(stream, true, out string _);

		// one-dimensional arrays with zero-based indexing
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfBoolean_Native] = (serializer,  stream, _) => serializer.ReadArrayOfBoolean_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfBoolean_Compact] = (serializer, stream, _) => serializer.ReadArrayOfBoolean_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfChar_Native] = (serializer,     stream, _) => serializer.ReadArrayOfChar_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfChar_Compact] = (serializer,    stream, _) => serializer.ReadArrayOfChar_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfSByte] = (serializer,           stream, _) => serializer.ReadArrayOfSByte(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt16_Native] = (serializer,    stream, _) => serializer.ReadArrayOfInt16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt16_Compact] = (serializer,   stream, _) => serializer.ReadArrayOfInt16_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt32_Native] = (serializer,    stream, _) => serializer.ReadArrayOfInt32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt32_Compact] = (serializer,   stream, _) => serializer.ReadArrayOfInt32_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt64_Native] = (serializer,    stream, _) => serializer.ReadArrayOfInt64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfInt64_Compact] = (serializer,   stream, _) => serializer.ReadArrayOfInt64_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfByte] = (serializer,            stream, _) => serializer.ReadArrayOfByte(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt16_Native] = (serializer,   stream, _) => serializer.ReadArrayOfUInt16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt16_Compact] = (serializer,  stream, _) => serializer.ReadArrayOfUInt16_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt32_Native] = (serializer,   stream, _) => serializer.ReadArrayOfUInt32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt32_Compact] = (serializer,  stream, _) => serializer.ReadArrayOfUInt32_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt64_Native] = (serializer,   stream, _) => serializer.ReadArrayOfUInt64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfUInt64_Compact] = (serializer,  stream, _) => serializer.ReadArrayOfUInt64_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfSingle] = (serializer,          stream, _) => serializer.ReadArrayOfSingle(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfDouble] = (serializer,          stream, _) => serializer.ReadArrayOfDouble(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfDecimal] = (serializer,         stream, _) => serializer.ReadArrayOfDecimal(stream);
		sDeserializersByPayloadType[(int)PayloadType.ArrayOfObjects] = (serializer,         stream, context) => serializer.ReadArrayOfObjects(stream, context);

		// multidimensional arrays
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfBoolean_Native] = (serializer,  stream, _) => serializer.ReadMultidimensionalArrayOfBoolean_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfBoolean_Compact] = (serializer, stream, _) => serializer.ReadMultidimensionalArrayOfBoolean_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfChar_Native] = (serializer,     stream, _) => serializer.ReadMultidimensionalArrayOfChar_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfChar_Compact] = (serializer,    stream, _) => serializer.ReadMultidimensionalArrayOfChar_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfSByte] = (serializer,           stream, _) => serializer.ReadMultidimensionalArrayOfSByte(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt16_Native] = (serializer,    stream, _) => serializer.ReadMultidimensionalArrayOfInt16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt16_Compact] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfInt16_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt32_Native] = (serializer,    stream, _) => serializer.ReadMultidimensionalArrayOfInt32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt32_Compact] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfInt32_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt64_Native] = (serializer,    stream, _) => serializer.ReadMultidimensionalArrayOfInt64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfInt64_Compact] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfInt64_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfByte] = (serializer,            stream, _) => serializer.DeserializeMultidimensionalArrayOfByte(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt16_Native] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfUInt16_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt16_Compact] = (serializer,  stream, _) => serializer.ReadMultidimensionalArrayOfUInt16_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt32_Native] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfUInt32_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt32_Compact] = (serializer,  stream, _) => serializer.ReadMultidimensionalArrayOfUInt32_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt64_Native] = (serializer,   stream, _) => serializer.ReadMultidimensionalArrayOfUInt64_Native(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfUInt64_Compact] = (serializer,  stream, _) => serializer.ReadMultidimensionalArrayOfUInt64_Compact(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfSingle] = (serializer,          stream, _) => serializer.ReadMultidimensionalArrayOfSingle(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfDouble] = (serializer,          stream, _) => serializer.ReadMultidimensionalArrayOfDouble(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfDecimal] = (serializer,         stream, _) => serializer.ReadMultidimensionalArrayOfDecimal(stream);
		sDeserializersByPayloadType[(int)PayloadType.MultidimensionalArrayOfObjects] = (serializer,         stream, context) => serializer.ReadMultidimensionalArrayOfObjects(stream, context);

		// type
		sDeserializersByPayloadType[(int)PayloadType.Type] = (serializer, stream, context) =>
		{
			serializer.ReadTypeMetadata(stream);
			return serializer.InnerDeserialize(stream, context);
		};

		// type id
		sDeserializersByPayloadType[(int)PayloadType.TypeId] = (serializer, stream, context) =>
		{
			serializer.ReadTypeId(stream);
			return serializer.InnerDeserialize(stream, context);
		};
	}

	/// <summary>
	/// Scans the specified assembly for custom serializers.
	/// </summary>
	/// <param name="assembly">Assembly to scan.</param>
	private static void ScanAssemblyForCustomSerializers(Assembly assembly)
	{
		// register types implementing an internal object serializer or
		// types that represent an external object serializer for some other type
		foreach (Type type in RuntimeMetadata.TypesByAssembly[assembly])
		{
			try
			{
				TryToAddInternalObjectSerializer(type);
				ExternalObjectSerializerFactory.TryRegisterExternalObjectSerializer(type);
			}
			catch (Exception ex)
			{
				sLog.Write(
					LogLevel.Debug,
					"Inspecting type ({0}) failed. Exception:\n{1}",
					type.AssemblyQualifiedName,
					ex);
			}
		}
	}

	/// <summary>
	/// Adds the specified type to the list of internal object serializers, if appropriate.
	/// </summary>
	/// <param name="type">Type to add to the list of internal object serializers.</param>
	private static void TryToAddInternalObjectSerializer(Type type)
	{
		// abort if the type is not a class and not a value type of primitive type
		if (!type.IsClass && (!type.IsValueType || type.IsPrimitive))
			return;

		// a class
		InternalObjectSerializerAttribute[] iosAttributes = type.GetCustomAttributes<InternalObjectSerializerAttribute>(false).ToArray();
		bool iosAttributeOk = iosAttributes.Length > 0;
		bool interfaceOk = typeof(IInternalObjectSerializer).IsAssignableFrom(type);
		bool constructorOk = type.GetConstructor(BindingFlags.ExactBinding | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, sConstructorArgumentTypes, null) != null;
		MethodInfo serializeMethodInfo = type.GetMethod("Serialize", [typeof(SerializationArchive), typeof(uint)]);
		bool virtualSerializeMethod = serializeMethodInfo != null && serializeMethodInfo.IsVirtual && !serializeMethodInfo.IsFinal;

		if (iosAttributeOk && interfaceOk && constructorOk && !virtualSerializeMethod)
		{
			// class is annotated with the internal object serializer attribute and implements the appropriate interface
			// => add a serializer delegate that handles it
			lock (sSync)
			{
				// abort, if the type is already registered
				if (sInternalObjectSerializerInfoByType.ContainsKey(type))
					return;

				// register the type
				var typeToInternalObjectSerializerInfo = new TypeKeyedDictionary<InternalObjectSerializerInfo>(sInternalObjectSerializerInfoByType)
				{
					{ type, new InternalObjectSerializerInfo(iosAttributes[0].Version) }
				};
				Thread.MemoryBarrier();
				sInternalObjectSerializerInfoByType = typeToInternalObjectSerializerInfo;
			}
		}
		else if (iosAttributeOk || interfaceOk) // || constructorOk <-- do not check this, since this will create false alarms for classes taking a SerializerArchive in the constructor
		{
			if (!iosAttributeOk)
			{
				// attribute is missing
				sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it is not annotated with the '{1}' attribute.", type.FullName, typeof(InternalObjectSerializerAttribute).FullName);
			}

			if (!interfaceOk)
			{
				// interface is missing
				sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it does not implement the '{1}' interface.", type.FullName, typeof(IInternalObjectSerializer).FullName);
			}

			if (!constructorOk)
			{
				// serialization constructor is missing
				sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it lacks a serialization constructor.", type.FullName);
			}

			if (virtualSerializeMethod)
			{
				// 'Serialize' method is virtual
				sLog.Write(
					LogLevel.Error,
					"Class '{0}' seems to be an internal serializer class, but its 'Serialize' method is virtual which will cause problems when serializing derived classes. You should overwrite the 'Serialize' method in derived classes instead.",
					type.FullName);
			}
		}
	}

	/// <summary>
	/// Prints information about internal object serializers and external object serializers to the log.
	/// </summary>
	private static void PrintToLog(LogLevel level)
	{
		var linesByTypeName = new List<Tuple<string, string>>();

		// internal object serializers
		foreach (KeyValuePair<Type, InternalObjectSerializerInfo> kvp in sInternalObjectSerializerInfoByType)
		{
			Debug.Assert(kvp.Key.FullName != null, "kvp.Key.FullName != null");
			linesByTypeName.Add(
				new Tuple<string, string>(
					kvp.Key.ToCSharpFormattedString(),
					$"-> {kvp.Key.ToCSharpFormattedString()}" + Environment.NewLine +
					$"   o Assembly: {ConditionAssemblyPath(kvp.Key.Assembly)}" + Environment.NewLine +
					"   o Serializer: Internal Object Serializer" + Environment.NewLine +
					$"   o Version: {kvp.Value.SerializerVersion}"));
		}

		// external object serializers
		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach (ExternalObjectSerializerFactory.SerializerInfo info in ExternalObjectSerializerFactory.RegisteredSerializers)
		{
			linesByTypeName.Add(
				new Tuple<string, string>(
					info.SerializeeType.ToCSharpFormattedString(),
					$"-> {info.SerializeeType.ToCSharpFormattedString()}" + Environment.NewLine +
					$"   o Assembly: {ConditionAssemblyPath(info.SerializeeType.Assembly)}" + Environment.NewLine +
					$"   o Serializer: External Object Serializer, {info.SerializerType.ToCSharpFormattedString()} ({ConditionAssemblyPath(info.SerializerType.Assembly)})" + Environment.NewLine +
					$"   o Version: {info.Version}"));
		}

		// sort type names
		linesByTypeName.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Item1, y.Item1));

		// put everything together and print to the log
		var builder = new StringBuilder();
		builder.AppendLine("Types with Custom Serializers:");
		foreach (Tuple<string, string> kvp in linesByTypeName) builder.AppendLine(kvp.Item2);
		sLog.Write(level, builder.ToString());
		return;

		static string ConditionAssemblyPath(Assembly assembly)
		{
			string fullPath = Path.GetFullPath(assembly.Location);
			string basePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
			return fullPath.StartsWith(basePath, StringComparison.InvariantCulture)
				       ? fullPath[basePath.Length..]
				       : fullPath;
		}
	}

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="Serializer"/> class.
	/// </summary>
	public Serializer()
	{
		// ensure static data is initialized appropriately
		Init();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Serializer"/> class.
	/// </summary>
	/// <param name="versions">Table defining requested serializer versions for specific types.</param>
	public Serializer(SerializerVersionTable versions)
	{
		// ensure static data is initialized appropriately
		Init();

		// use specified serializer version table
		mSerializedTypeVersionTable = versions;
	}

	#endregion

	#region Type Serialization

	private static          Dictionary<string, Type>    sTypeTable_Exact                  = new(); // assembly-qualified type name => type (exact type resolution)
	private static          Dictionary<string, Type>    sTypeTable_Tolerant               = new(); // assembly-qualified type name => type (tolerant type resolution)
	private static          TypeKeyedDictionary<byte[]> sSerializedTypeSnippetsByType     = new(); // type => UTF-8 encoded assembly-qualified type name
	private static readonly object                      sSerializedTypeSnippetsByTypeLock = new(); // lock protecting the type snippet table
	private static readonly Regex                       sExtractFullTypeNameRegex         = new(@"^([^[\[,]*)(\[[, ]*\])?(?:, )(.*)", RegexOptions.Compiled);

	/// <summary>
	/// Serializes metadata about a type.
	/// </summary>
	/// <param name="writer">Buffer writer to serialize the type to.</param>
	/// <param name="type">Type to serialize.</param>
	internal void WriteTypeMetadata(IBufferWriter<byte> writer, Type type)
	{
		// abort if the type has not changed to avoid bloating the stream
		if (type == mCurrentSerializedType)
			return;

		// decompose generic types and store types separately to allow the deserializer to
		// access assembly information of types used as generic arguments
		WriteDecomposedType(writer, type);

		// store currently serialized type to avoid bloating the stream when a bunch of objects
		// of the same type is serialized
		mCurrentSerializedType = type;
	}

	/// <summary>
	/// Serializes the specified decomposed type.
	/// </summary>
	/// <param name="writer">Buffer writer to serialize the type to.</param>
	/// <param name="type">Type to serialize.</param>
	private void WriteDecomposedType(IBufferWriter<byte> writer, Type type)
	{
		WriteDecomposedTypeInternal(writer, type);

		// write the number of type arguments that are following, if the type itself is a type definition
		// (in this case the number of following type arguments is always 0)
		if (!type.IsGenericTypeDefinition) return;
		Span<byte> buffer = writer.GetSpan(1);
		buffer[0] = 0; // LEB128 encoded '0'
		writer.Advance(1);
	}

	/// <summary>
	/// Serializes the specified decomposed type (for internal use).
	/// </summary>
	/// <param name="writer">Buffer writer to serialize the type to.</param>
	/// <param name="type">Type to serialize.</param>
	private void WriteDecomposedTypeInternal(IBufferWriter<byte> writer, Type type)
	{
		if (mSerializedTypeIdTable.TryGetValue(type, out uint id))
		{
			// the type was serialized before
			// => write type id only
			WriteTypeId(writer, id);
		}
		else
		{
			// it's the first time this type is serialized
			// => write the fully qualified assembly name of the outermost type and recurse in case of generic types to cover
			//    generic type arguments

			if (type.IsGenericType)
			{
				if (type.IsGenericTypeDefinition)
				{
					// a generic type definition

					// try to get a pre-serialized type snippet, add it, if necessary
					if (!sSerializedTypeSnippetsByType.TryGetValue(type, out byte[] serializedTypeName))
						serializedTypeName = AddSerializedTypeSnippet(type);

					// write pre-serialized type snippet
					Span<byte> buffer = writer.GetSpan(serializedTypeName.Length);
					serializedTypeName.CopyTo(buffer);
					writer.Advance(serializedTypeName.Length);
				}
				else if (!type.ContainsGenericParameters)
				{
					// a closed constructed generic type
					// => write generic type definition and generic type arguments
					WriteDecomposedTypeInternal(writer, type.GetGenericTypeDefinition());
					Span<byte> buffer = writer.GetSpan(Leb128EncodingHelper.MaxBytesFor32BitValue);
					int count = Leb128EncodingHelper.Write(buffer, type.GenericTypeArguments.Length);
					writer.Advance(count);
					for (int i = 0; i < type.GenericTypeArguments.Length; i++)
					{
						WriteDecomposedTypeInternal(writer, type.GenericTypeArguments[i]);
					}
				}
				else
				{
					// the type has unassigned generic type parameters, but it is not a type definition
					// => it's an open constructed generic type (the runtime supports this, but it is not supported by C#)
					throw new SerializationException(
						"Serializing type (namespace: {0}, name: {1}) is not supported (it is an open constructed generic type, i.e. it is not a generic type definition, but it contains generic parameters).",
						type.Namespace,
						type.Name);
				}
			}
			else
			{
				// try to get a pre-serialized type snippet, add it, if necessary
				if (!sSerializedTypeSnippetsByType.TryGetValue(type, out byte[] serializedTypeName))
					serializedTypeName = AddSerializedTypeSnippet(type);

				// write pre-serialized type snippet
				Span<byte> buffer = writer.GetSpan(serializedTypeName.Length);
				serializedTypeName.CopyTo(buffer);
				writer.Advance(serializedTypeName.Length);
			}

			// assign a type id to the type to allow referencing it later on
			mSerializedTypeIdTable.Add(type, mNextSerializedTypeId++);
		}
	}

	/// <summary>
	/// Adds a pre-serialized type snippet to the cache.
	/// </summary>
	/// <param name="type">Type to add a pre-serialized type snippet for.</param>
	/// <returns>The pre-serialized type snippet.</returns>
	[MethodImpl(MethodImplOptions.NoInlining)] // the method is rarely used, so do not inline it to keep the calling method short
	private static byte[] AddSerializedTypeSnippet(Type type)
	{
		lock (sSerializedTypeSnippetsByTypeLock)
		{
			// return cached snippet, if available...
			if (sSerializedTypeSnippetsByType.TryGetValue(type, out byte[] serializedTypeName))
				return serializedTypeName;

			// the type snippet is not cached, yet
			// => build a new type snippet...
			string name = type.AssemblyQualifiedName;
			Debug.Assert(name != null, nameof(name) + " != null");
			int utf8NameLength = Encoding.UTF8.GetByteCount(name);
			int leb128Length = Leb128EncodingHelper.GetByteCount(utf8NameLength);
			serializedTypeName = new byte[1 + leb128Length + utf8NameLength];
			serializedTypeName[0] = (byte)PayloadType.Type;
			Leb128EncodingHelper.Write(serializedTypeName, 1, utf8NameLength);
			Encoding.UTF8.GetBytes(name, 0, name.Length, serializedTypeName, 1 + leb128Length);

			// create a copy of the current dictionary and add the new snippet
			var newSnippets = new TypeKeyedDictionary<byte[]>(sSerializedTypeSnippetsByType) { [type] = serializedTypeName };

			// ensure that dictionary contents are committed to memory before publishing the reference of the new dictionary
			Thread.MemoryBarrier();

			// exchange dictionary with pre-serialized type snippets
			sSerializedTypeSnippetsByType = newSnippets;

			return serializedTypeName;
		}
	}

	/// <summary>
	/// Writes a type id for already serialized type metadata.
	/// </summary>
	/// <param name="writer">Buffer writer to write to.</param>
	/// <param name="id">Type id that was assigned to the type.</param>
	private static void WriteTypeId(IBufferWriter<byte> writer, uint id)
	{
		Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
		buffer[0] = (byte)PayloadType.TypeId;
		int count = Leb128EncodingHelper.Write(buffer[1..], id);
		writer.Advance(1 + count);
	}

	/// <summary>
	/// Reads metadata about a type.
	/// </summary>
	/// <param name="stream">Stream to read type metadata from.</param>
	/// <exception cref="SerializationException">The stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Type specified in the stream could not be loaded.</exception>
	private void ReadTypeMetadata(Stream stream)
	{
		// read number of utf-8 code units in the following string
		int length = Leb128EncodingHelper.ReadInt32(stream);

		// read type full name
		byte[] array = new byte[length];
		int bytesRead = stream.Read(array, 0, length);
		if (bytesRead < length) throw new SerializationException("Unexpected end of stream.");
		string typename = Encoding.UTF8.GetString(array, 0, length);

		// assign a type id if the serializer uses type ids
		var typeItem = new TypeItem(typename, ResolveType(typename));
		mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);

		if (typeItem.Type.IsGenericTypeDefinition)
		{
			// read number of type arguments following the type definition
			// (0, if the effective type is really a type definition and not a constructed generic type)
			int genericTypeArgumentCount = Leb128EncodingHelper.ReadInt32(stream);

			if (genericTypeArgumentCount > 0)
			{
				Type[] genericTypeParameters = typeItem.Type.GetTypeInfo().GenericTypeParameters;
				var genericTypeArgumentTypeItems = new TypeItem[genericTypeParameters.Length];
				for (int i = 0; i < genericTypeParameters.Length; i++)
				{
					Type genericTypeArgument = ReadTypeObject(stream, false, out string genericTypeArgumentSourceName);
					genericTypeArgumentTypeItems[i] = new TypeItem(genericTypeArgumentSourceName, genericTypeArgument);
				}

				// compose the generic type
				Type composedType = typeItem.Type.MakeGenericType(genericTypeArgumentTypeItems.Select(x => x.Type).ToArray());
				typeItem = new TypeItem(MakeGenericTypeName(typeItem.Name, genericTypeArgumentTypeItems.Select(x => x.Name)), composedType);

				// assign a type id if the serializer uses assembly and type ids
				typeItem = new TypeItem(typeItem.Name, composedType);
				mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
			}
		}

		mCurrentDeserializedType = typeItem;
	}

	/// <summary>
	/// Reads a type id from a stream.
	/// </summary>
	/// <param name="stream">Stream to read the type id from.</param>
	/// <exception cref="SerializationException">The stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Deserialized type id does not match a previously deserialized type.</exception>
	private void ReadTypeId(Stream stream)
	{
		uint id = Leb128EncodingHelper.ReadUInt32(stream);

		if (mDeserializedTypeIdTable.TryGetValue(id, out TypeItem item))
		{
			if (item.Type.IsGenericTypeDefinition)
			{
				// read number of type arguments following the type definition
				// (0, if the effective type is really a type definition and not a constructed generic type)
				int genericTypeArgumentCount = Leb128EncodingHelper.ReadInt32(stream);

				if (genericTypeArgumentCount > 0)
				{
					Type[] genericTypeParameters = item.Type.GetTypeInfo().GenericTypeParameters;
					var genericTypeArgumentTypeItems = new TypeItem[genericTypeParameters.Length];
					for (int i = 0; i < genericTypeParameters.Length; i++)
					{
						Type genericTypeArgument = ReadTypeObject(stream, false, out string genericTypeArgumentSourceName);
						genericTypeArgumentTypeItems[i] = new TypeItem(genericTypeArgumentSourceName, genericTypeArgument);
					}

					// compose the generic type
					Type composedType = item.Type.MakeGenericType(genericTypeArgumentTypeItems.Select(x => x.Type).ToArray());
					item = new TypeItem(MakeGenericTypeName(item.Name, genericTypeArgumentTypeItems.Select(x => x.Name)), composedType);

					// assign a type id if the serializer uses assembly and type ids
					item = new TypeItem(item.Name, composedType);
					mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, item);
				}
			}

			mCurrentDeserializedType = item;
		}
		else
		{
			throw new SerializationException("Deserialized type id that does not match a previously deserialized type.");
		}
	}

	/// <summary>
	/// Resolves the specified assembly qualified type name to the corresponding <see cref="Type"/> object
	/// taking resolution tolerance criteria into account.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name to resolve.</param>
	/// <returns>
	/// The <see cref="Type"/> object corresponding to the specified type name
	/// (can be some other type than it was originally, if tolerant deserialization is enabled).
	/// </returns>
	/// <exception cref="TypeResolutionException">The type could not be resolved.</exception>
	/// <exception cref="AmbiguousTypeResolutionException">The type could not be resolved unambiguously.</exception>
	/// <exception cref="SerializationException">Type could be resolved, but tolerant deserialization is disabled and the type was not resolved exactly.</exception>
	private Type ResolveType(string assemblyQualifiedTypeName)
	{
		// -----------------------------------------------------------------------------------------------------------------
		// try to resolve the type by looking up its fully qualified name
		// -----------------------------------------------------------------------------------------------------------------

		Dictionary<string, Type> typeTable = UseTolerantDeserialization ? sTypeTable_Tolerant : sTypeTable_Exact;
		if (typeTable.TryGetValue(assemblyQualifiedTypeName, out Type resolvedType))
			return resolvedType;

		// -----------------------------------------------------------------------------------------------------------------
		// split assembly name and type name, condition array types
		// -----------------------------------------------------------------------------------------------------------------

		Match match = sExtractFullTypeNameRegex.Match(assemblyQualifiedTypeName);
		if (!match.Success) throw new SerializationException($"Detected invalid type name ({assemblyQualifiedTypeName}) during deserialization.");
		string originalAssemblyQualifiedTypeName = assemblyQualifiedTypeName;
		string assemblyFullName = match.Groups[3].Value;
		string fullTypeName;
		int arrayRank = 0;

		if (match.Groups[2].Length > 0)
		{
			// type name identifies an array
			// => strip array brackets, otherwise they will break the look-up

			arrayRank = match.Groups[2].Value.Count(x => x == ',') + 1;
			fullTypeName = match.Groups[1].Value;
			assemblyQualifiedTypeName = fullTypeName + ", " + match.Groups[3].Value;

			sLog.Write(
				LogLevel.Debug,
				"Trying to look up {0}. It's an array type, looking up {1} instead...",
				originalAssemblyQualifiedTypeName,
				assemblyQualifiedTypeName);
		}
		else
		{
			Debug.Assert(match.Groups[2].Value.Length == 0);
			fullTypeName = match.Groups[1].Value;
			assemblyQualifiedTypeName = fullTypeName + ", " + match.Groups[3].Value;
		}

		// -----------------------------------------------------------------------------------------------------------------
		// try to resolve the assembly name exactly and load the assembly, if necessary
		// (updates the properties provided by the RuntimeMetadata class)
		// -----------------------------------------------------------------------------------------------------------------

		if (!RuntimeMetadata.AssembliesByFullName.TryGetValue(assemblyFullName, out Assembly assembly))
		{
			try
			{
				assembly = Assembly.Load(assemblyFullName);
				Debug.Assert(RuntimeMetadata.AssembliesByFullName.ContainsKey(assemblyFullName));
			}
			catch (Exception)
			{
				// swallow
			}
		}

		// -----------------------------------------------------------------------------------------------------------------
		// try to resolve the type within the assembly
		// => exact match of assembly qualified type name
		// -----------------------------------------------------------------------------------------------------------------

		if (assembly != null)
		{
			resolvedType = RuntimeMetadata.TypesByAssembly[assembly].FirstOrDefault(x => x.FullName == fullTypeName);
			if (resolvedType != null)
			{
				Debug.Assert(resolvedType.AssemblyQualifiedName != null, "resolvedType.AssemblyQualifiedName != null");
				goto proceed;
			}
		}

		// -----------------------------------------------------------------------------------------------------------------
		// determine types with the same full name (independent of the assembly the type is declared in)
		// -----------------------------------------------------------------------------------------------------------------

		// load all assemblies that are reachable via references
		RuntimeMetadata.LoadAllAssemblies(true);

		RuntimeMetadata.TypesByFullName.TryGetValue(fullTypeName, out IReadOnlyList<Type> matchingTypes);
		if (matchingTypes == null)
		{
			string message = $"Resolving type ({assemblyQualifiedTypeName}) failed. There is no assembly containing a type with that name ({fullTypeName}).";
			sLog.Write(LogLevel.Debug, message);
			throw new TypeResolutionException(message, assemblyQualifiedTypeName);
		}

		// -----------------------------------------------------------------------------------------------------------------
		// the type in the assembly matching exactly has been handled above, so there is no need to check this here again...
		// fall back to the type in an assembly with the same simple assembly name
		// (this does not take the version and the public key into account)
		// -----------------------------------------------------------------------------------------------------------------

		var assemblyName = new AssemblyName(assemblyFullName);

		sLog.Write(
			LogLevel.Debug,
			"Trying to tolerantly resolve type ({0}) using the full name of the type ({1}) and the simple name of the declaring assembly ({2})...",
			assemblyQualifiedTypeName,
			fullTypeName,
			assemblyName.Name);

		Type[] resolvedTypes = matchingTypes.Where(x => x.Assembly.GetName().Name == assemblyName.Name).ToArray();
		if (resolvedTypes.Length == 1)
		{
			resolvedType = resolvedTypes[0];
			goto proceed;
		}
		if (resolvedTypes.Length > 1)
		{
			string message = $"Resolving type ({assemblyQualifiedTypeName}) failed. The simple assembly name is ambiguous.";
			sLog.Write(LogLevel.Debug, message);
			throw new AmbiguousTypeResolutionException(
				message,
				assemblyQualifiedTypeName,
				resolvedTypes);
		}

		// -----------------------------------------------------------------------------------------------------------------
		// fall back to the type in an assembly with some other name
		// (type has probably migrated, maybe due to a different .NET version)
		// -----------------------------------------------------------------------------------------------------------------

		sLog.Write(
			LogLevel.Debug,
			"Trying to tolerantly resolve type ({0}) to some other assembly...",
			assemblyQualifiedTypeName);

		resolvedTypes = matchingTypes.ToArray();
		if (resolvedTypes.Length == 1)
		{
			resolvedType = resolvedTypes[0];
			goto proceed;
		}
		if (resolvedTypes.Length > 1)
		{
			string message = $"Resolving type ({assemblyQualifiedTypeName}) failed. There are multiple assemblies defining a type with that name ({fullTypeName}).";
			sLog.Write(LogLevel.Debug, message);
			throw new AmbiguousTypeResolutionException(
				message,
				assemblyQualifiedTypeName,
				resolvedTypes);
		}

		// -----------------------------------------------------------------------------------------------------------------

		proceed:

		Debug.Assert(resolvedType != null, nameof(resolvedType) + " != null");

		// determine whether the type has been resolved exactly
		bool isResolvedExactly = resolvedType.AssemblyQualifiedName == assemblyQualifiedTypeName;

		// reconstruct array type, if the type to lookup was an array
		// ReSharper disable once ConvertIfStatementToSwitchStatement
		if (arrayRank == 1) resolvedType = resolvedType.MakeArrayType();
		else if (arrayRank > 1) resolvedType = resolvedType.MakeArrayType(arrayRank);

		Debug.Assert(resolvedType != null, nameof(resolvedType) + " != null");
		Debug.Assert(resolvedType.AssemblyQualifiedName != null, "resolvedType.AssemblyQualifiedName != null");

		// store the resolved type
		lock (sSync)
		{
			if (isResolvedExactly)
			{
				sLog.Write(
					LogLevel.Debug,
					"The type ({0}) was resolved exactly.",
					assemblyQualifiedTypeName);

				if (!sTypeTable_Exact.ContainsKey(resolvedType.AssemblyQualifiedName))
				{
					var typeTable_exact = new Dictionary<string, Type>(sTypeTable_Exact)
					{
						{ resolvedType.AssemblyQualifiedName, resolvedType }
					};
					Thread.MemoryBarrier();
					sTypeTable_Exact = typeTable_exact;
				}

				if (sTypeTable_Tolerant.ContainsKey(resolvedType.AssemblyQualifiedName))
					return resolvedType;

				var typeTable_tolerant = new Dictionary<string, Type>(sTypeTable_Tolerant)
				{
					{ resolvedType.AssemblyQualifiedName, resolvedType }
				};
				Thread.MemoryBarrier();
				sTypeTable_Tolerant = typeTable_tolerant;
			}
			else
			{
				sLog.Write(
					LogLevel.Debug,
					"The type ({0}) was resolved tolerantly to ({1})",
					assemblyQualifiedTypeName,
					resolvedType.AssemblyQualifiedName);

				if (!sTypeTable_Tolerant.ContainsKey(resolvedType.AssemblyQualifiedName))
				{
					var typeTable_tolerant = new Dictionary<string, Type>(sTypeTable_Tolerant)
					{
						{ resolvedType.AssemblyQualifiedName, resolvedType }
					};
					Thread.MemoryBarrier();
					sTypeTable_Tolerant = typeTable_tolerant;
				}

				if (!UseTolerantDeserialization)
				{
					throw new SerializationException(
						"The type ({0}) could be resolved, but tolerant deserialization is disabled and the type was not resolved exactly.",
						assemblyQualifiedTypeName);
				}
			}
		}

		return resolvedType;
	}

	#endregion

	#region Resetting

	/// <summary>
	/// Resets the internal state of the serializer/deserializer.
	/// </summary>
	public void Reset()
	{
		ResetSerializer();
		ResetDeserializer();
	}

	/// <summary>
	/// Resets the internal state of the serializer.
	/// </summary>
	/// <remarks>
	/// This method resets the serializer clearing the mapping of assemblies and
	/// types to ids. The table used for detecting already serialized objects is
	/// cleared, too. The table containing information about requested serializer
	/// versions is NOT cleared.
	/// </remarks>
	public void ResetSerializer()
	{
		mCurrentSerializedType = null;
		mSerializedTypeIdTable.Clear();
		mNextSerializedTypeId = 0;
		mObjectsUnderSerialization.Clear();
		ResetSerializerObjectTable();
	}

	/// <summary>
	/// Resets the table used to detect objects that have already been serialized.
	/// </summary>
	private void ResetSerializerObjectTable()
	{
		mSerializedObjectIdTable.Clear();
		mNextSerializedObjectId = 0;
	}

	/// <summary>
	/// Resets the internal state of the deserializer.
	/// </summary>
	/// <remarks>
	/// This method resets the deserializer clearing the mapping of ids to assemblies and
	/// types. The table mapping object ids to already deserialized objects is cleared, too.
	/// </remarks>
	private void ResetDeserializer()
	{
		mCurrentDeserializedType = TypeItem.Empty;
		mDeserializedTypeIdTable.Clear();
		mNextDeserializedTypeId = 0;
		ResetDeserializerObjectTable();
	}

	/// <summary>
	/// Clears the table used to map object ids to already deserialized objects.
	/// </summary>
	private void ResetDeserializerObjectTable()
	{
		mDeserializedObjectIdTable.Clear();
		mNextDeserializedObjectId = 0;
	}

	#endregion

	#region Configuration

	/// <summary>
	/// Gets or sets the default value indicating whether the serializer is tolerant when deserializing.
	/// Enabling this allows to deserialize an object even if the assembly is strong-name signed and the version does not match.
	/// Furthermore, types may travel between different assemblies. In this case the full type name is used to find a certain type.
	/// This is strictly necessary when using .NET framework types and serialization and deserialization is done on different .NET
	/// versions. New serializer instances will use this setting to initialize their <see cref="UseTolerantDeserialization"/> property.
	/// </summary>
	public static bool UseTolerantDeserializationByDefault
	{
		get => sUseTolerantDeserializationByDefault;
		set => sUseTolerantDeserializationByDefault = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the serializer is tolerant when deserializing.
	/// Enabling this allows to deserialize an object even if the assembly is strong-name signed and the version does not match.
	/// Furthermore, types may travel between different assemblies. In this case the full type name is used to find a certain type.
	/// This is strictly necessary when using .NET framework types and serialization and deserialization is done on different .NET
	/// versions. New serializer instances will use this setting to initialize their <see cref="UseTolerantDeserialization"/> property.
	/// </summary>
	public bool UseTolerantDeserialization { get; set; } = sUseTolerantDeserializationByDefault;

	/// <summary>
	/// Gets or sets a value determining whether to optimize for speed or for size when serializing.
	/// </summary>
	public SerializationOptimization SerializationOptimization { get; set; } = SerializationOptimization.Size;

	#endregion

	#region Serialization

	private static          TypeKeyedDictionary<SerializerDelegate>   sSerializers                      = new();
	private static readonly TypeKeyedDictionary<SerializerDelegate>   sMultidimensionalArraySerializers = new();
	private static          TypeKeyedDictionary<IosSerializeDelegate> sIosSerializeCallers              = new();

	/// <summary>
	/// Serializes the specified object to a stream.
	/// </summary>
	/// <param name="stream">Stream to serialize the object to.</param>
	/// <param name="obj">Object to serialize.</param>
	/// <param name="context">Context object to pass to the serializer (can be <c>null</c>).</param>
	/// <param name="optimization">Optimization to apply when serializing.</param>
	public static void Serialize(
		Stream                    stream,
		object                    obj,
		object                    context,
		SerializationOptimization optimization)
	{
		Serializer serializer = null;

		try
		{
			serializer = GetPooledSerializer(true, optimization);
			serializer.Serialize(stream, obj, context);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
		}
	}

	/// <summary>
	/// Serializes an object to a stream.
	/// </summary>
	/// <param name="stream">Stream to serialize the object to.</param>
	/// <param name="obj">Object to serialize.</param>
	/// <param name="context">Context object to pass to the serializer of the specified object.</param>
	/// <exception cref="VersionNotSupportedException">The serializer version of one of the objects in the specified object graph is not supported.</exception>
	/// <exception cref="SerializationException">Serializing the object failed.</exception>
	public void Serialize(Stream stream, object obj, object context = null)
	{
		try
		{
			// prepare the serializer
			ResetSerializer();
			AllocateTemporaryBuffers();
			mWriter.Stream = stream;

			// write endianness indicator
			Span<byte> buffer = mWriter.GetSpan(1);
			buffer[0] = (byte)(BitConverter.IsLittleEndian ? 1 : 0);
			mWriter.Advance(1);

			// serialize the object
			InnerSerialize(mWriter, obj, context);
		}
		finally
		{
			mWriter.Stream = null; // flushes the buffer implicitly
			ReleaseTemporaryBuffers();
		}
	}

	/// <summary>
	/// Performs the actual serialization.
	/// </summary>
	/// <param name="writer">Buffer writer to write to.</param>
	/// <param name="obj">Object to serialize.</param>
	/// <param name="context">Context object to pass to the serializer of the object that is serialized next.</param>
	internal void InnerSerialize(IBufferWriter<byte> writer, object obj, object context)
	{
		// a null reference?
		if (ReferenceEquals(obj, null))
		{
			Span<byte> buffer = writer.GetSpan(1);
			buffer[0] = (byte)PayloadType.NullReference;
			writer.Advance(1);
			return;
		}

		// write object id, if the object was already serialized
		if (mSerializedObjectIdTable.TryGetValue(obj, out uint id))
		{
			// the object is already serialized
			// => write object id only
			SerializeObjectId(writer, id);
			return;
		}

		// get the type of the type to serialize
		Type type = obj.GetType();

		// use serialization handler that has been prepared before
		if (sSerializers.TryGetValue(type, out SerializerDelegate serializer))
		{
			serializer(this, writer, obj, context);
			return;
		}

		// handle arrays of types that do not have predefined serialization handlers
		// (these objects are serialized on demand, there is no serialization handler)
		if (type.IsArray)
		{
			// the type is an array
			// => differentiate one-dimensional arrays (SZARRAY) and multidimensional arrays (MDARRAY)

			var array = (Array)obj;
			if (type.GetArrayRank() == 1 && array.GetLowerBound(0) == 0)
			{
				// an SZARRAY
				WriteArrayOfObjects(array, writer);
				return;
			}

			// an MDARRAY
			Type elementType = type.GetElementType();
			Debug.Assert(elementType != null, nameof(elementType) + " != null");
			if (sMultidimensionalArraySerializers.TryGetValue(elementType, out serializer))
			{
				serializer(this, writer, array, context);
			}
			else
			{
				WriteMultidimensionalArrayOfObjects(array, writer);
			}

			return;
		}

		// handle type objects
		if (type.IsInstanceOfType(typeof(Type)))
		{
			SerializeTypeObject(writer, (Type)obj);
			return;
		}

		// there is no serialization handler for this type, yet
		// => analyze what this type is and prepare a serialization handler for the next time

		if (type.IsEnum)
		{
			// an enumeration value
			// => create a serializer delegate that handles it
			serializer = AddSerializerForType(type, () => GetEnumSerializer(type));
			serializer(this, writer, obj, context);
			return;
		}

		// try to use an internal object serializer
		IInternalObjectSerializer ios = GetInternalObjectSerializer(obj, out uint _);
		if (ios != null)
		{
			// the type has an internal object serializer
			// => create a serializer delegate that handles it and store it to speed up the serializer lookup for the next time
			serializer = AddSerializerForType(type, () => CreateInternalObjectSerializer(type));
			serializer(this, writer, obj, context);
			return;
		}

		// try to use an external object serializer
		IExternalObjectSerializer eos = ExternalObjectSerializerFactory.GetExternalObjectSerializer(type, out uint _);
		// ReSharper disable once InvertIf
		if (eos != null)
		{
			// the type has an external object serializer
			// => create a serializer delegate that handles it and store it to speed up the serializer lookup next time
			serializer = AddSerializerForType(type, () => CreateSerializerDelegateForExternalObjectSerializer(type, eos));
			serializer(this, writer, obj, context);
			return;
		}

		// object cannot be serialized
		throw new SerializationException($"Type '{obj.GetType().FullName}' cannot be serialized, consider implementing an internal or external object serializer for this type.");
	}

	/// <summary>
	/// Serializes a type object.
	/// </summary>
	/// <param name="writer">Buffer writer to write the type object to.</param>
	/// <param name="type">Type object to serialize.</param>
	private void SerializeTypeObject(IBufferWriter<byte> writer, Type type)
	{
		// tell the deserializer that a type object follows
		Span<byte> buffer = writer.GetSpan(1);
		buffer[0] = (byte)PayloadType.TypeObject;
		writer.Advance(1);

		// serialize the decomposed type
		WriteDecomposedType(writer, type);

		// assign an object id to the type object to allow referencing it later on
		mSerializedObjectIdTable.Add(type, mNextSerializedObjectId++);
	}

	/// <summary>
	/// Serializes the id of an object that was already serialized.
	/// </summary>
	/// <param name="writer">Buffer writer to write the object id to.</param>
	/// <param name="id">Object id.</param>
	private static void SerializeObjectId(IBufferWriter<byte> writer, uint id)
	{
		Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
		buffer[0] = (byte)PayloadType.AlreadySerialized;
		int count = Leb128EncodingHelper.Write(buffer[1..], id);
		writer.Advance(1 + count);
	}

	/// <summary>
	/// Adds a serializer for the specified type.
	/// </summary>
	/// <param name="type">Type to add a serializer for.</param>
	/// <param name="serializerFactory">Factory callback creating the serializer to add.</param>
	/// <returns>The added serializer.</returns>
	private static SerializerDelegate AddSerializerForType(Type type, Func<SerializerDelegate> serializerFactory)
	{
		lock (sSync)
		{
			// return cached serializer, if available...
			if (sSerializers.TryGetValue(type, out SerializerDelegate serializer))
				return serializer;

			// register serializer delegate produced by specified factory callback
			serializer = serializerFactory();
			var copy = new TypeKeyedDictionary<SerializerDelegate>(sSerializers) { [type] = serializer };
			Thread.MemoryBarrier();
			sSerializers = copy;

			return serializer;
		}
	}

	/// <summary>
	/// Creates a delegate that handles the serialization of the specified type using an internal object serializer
	/// implemented by the type itself.
	/// </summary>
	/// <param name="type">Type implementing an internal object serializer.</param>
	/// <returns>A delegate that handles the serialization of the specified type.</returns>
	private static SerializerDelegate CreateInternalObjectSerializer(Type type)
	{
		return (
			serializer,
			writer,
			obj,
			context) =>
		{
			// abort, if the object to serialize is under serialization itself
			if (serializer.mObjectsUnderSerialization.Contains(obj))
				throw new CyclicDependencyDetectedException($"Detected a cyclic dependency to the object to serialize (type: {obj.GetType().FullName}).");

			// determine the serializer version to use
			if (!serializer.mSerializedTypeVersionTable.TryGet(type, out uint version))
				HasInternalObjectSerializer(type, out version);

			// write the header of the archive
			serializer.WriteTypeMetadata(writer, type);
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArchiveStart;
			bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], version);
			writer.Advance(bufferIndex);

			// get the delegate invoking the internal object serializer
			IosSerializeDelegate serialize = GetInternalObjectSerializerSerializeCaller(type);

			// serialize the object using its internal object serializer
			var archive = new SerializationArchive(serializer, writer, type, obj, version, context);
			serializer.mObjectsUnderSerialization.Add(obj);
			try { serialize(obj as IInternalObjectSerializer, archive); }
			finally { serializer.mObjectsUnderSerialization.Remove(obj); }

			// assign an object id to the object to enable referencing it later on
			if (!type.IsValueType)
				serializer.mSerializedObjectIdTable.Add(obj, serializer.mNextSerializedObjectId++);

			// close the archive
			buffer = writer.GetSpan(1);
			buffer[0] = (byte)PayloadType.ArchiveEnd;
			writer.Advance(1);
		};
	}

	/// <summary>
	/// Gets a delegate that refers to the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified type
	/// (needed during serialization of base classes implementing an internal object serializer).
	/// </summary>
	/// <param name="type">Type of class to retrieve the Serialize() method from.</param>
	/// <returns>Delegate referring to the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified class.</returns>
	internal static IosSerializeDelegate GetInternalObjectSerializerSerializeCaller(Type type)
	{
		// return cached serializer, if available...
		if (sIosSerializeCallers.TryGetValue(type, out IosSerializeDelegate serializeDelegate))
			return serializeDelegate;

		lock (sSync)
		{
			// return cached serializer, if available...
			if (sIosSerializeCallers.TryGetValue(type, out serializeDelegate))
				return serializeDelegate;

			// create and register serializer delegate
			serializeDelegate = CreateIosSerializeCaller(type);
			var copy = new TypeKeyedDictionary<IosSerializeDelegate>(sIosSerializeCallers) { { type, serializeDelegate } };
			Thread.MemoryBarrier();
			sIosSerializeCallers = copy;
		}

		return serializeDelegate;
	}

	/// <summary>
	/// Creates a dynamic method that calls the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified type
	/// that may be implemented implicitly or explicitly.
	/// </summary>
	/// <param name="type">The type implementing the <see cref="IInternalObjectSerializer"/> interface.</param>
	/// <returns>A delegate to a dynamic method that simply calls the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified type.</returns>
	private static IosSerializeDelegate CreateIosSerializeCaller(Type type)
	{
		// try to get the publicly implemented 'Serialize' method...
		MethodInfo method = type.GetMethod(nameof(IInternalObjectSerializer.Serialize), [typeof(SerializationArchive)]);
		if (method == null)
		{
			// the publicly implemented 'Serialize' method is not available
			// => try to get the explicitly implemented 'Serialize' method...
			method = type.GetMethod(
				typeof(IInternalObjectSerializer).FullName + "." + nameof(IInternalObjectSerializer.Serialize),
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				null,
				[typeof(SerializationArchive)],
				null);
		}

		Debug.Assert(method != null);

		// create a delegate that simply calls the Serialize() method of the internal object serializer
		ParameterExpression[] parameterExpressions =
		[
			Expression.Parameter(typeof(IInternalObjectSerializer), "object"),
			Expression.Parameter(typeof(SerializationArchive), "archive")
		];

		Expression body = Expression.Call(
			Expression.Convert(parameterExpressions[0], type),
			method,
			parameterExpressions[1]);

		LambdaExpression lambda = Expression.Lambda(typeof(IosSerializeDelegate), body, parameterExpressions);
		return (IosSerializeDelegate)lambda.Compile();
	}

	/// <summary>
	/// Creates a delegate that handles the serialization of the specified type using an external object serializer.
	/// </summary>
	/// <param name="typeToSerialize">Type the delegate will handle.</param>
	/// <param name="eos">Receives the created external object serializer.</param>
	/// <returns>A delegate that handles the serialization of the specified type.</returns>
	private static SerializerDelegate CreateSerializerDelegateForExternalObjectSerializer(Type typeToSerialize, IExternalObjectSerializer eos)
	{
		return (
			serializer,
			writer,
			obj,
			context) =>
		{
			// abort, if the object to serialize is under serialization itself
			if (serializer.mObjectsUnderSerialization.Contains(obj))
				throw new CyclicDependencyDetectedException($"Detected a cyclic dependency to the object to serialize (type: {obj.GetType().FullName}).");

			// determine the serializer version to use
			if (!serializer.mSerializedTypeVersionTable.TryGet(typeToSerialize, out uint version))
				HasExternalObjectSerializer(typeToSerialize, out version);

			// write type metadata
			serializer.WriteTypeMetadata(writer, typeToSerialize);

			// write the header of the archive
			serializer.WriteTypeMetadata(writer, typeToSerialize);
			const int maxBufferSize = 1 + Leb128EncodingHelper.MaxBytesFor32BitValue;
			Span<byte> buffer = writer.GetSpan(maxBufferSize);
			int bufferIndex = 0;
			buffer[bufferIndex++] = (byte)PayloadType.ArchiveStart;
			bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], version);
			writer.Advance(bufferIndex);

			// serialize the object using the external object serializer
			var archive = new SerializationArchive(serializer, writer, typeToSerialize, obj, version, context);
			serializer.mObjectsUnderSerialization.Add(obj);
			try { eos.Serialize(archive, obj); }
			finally { serializer.mObjectsUnderSerialization.Remove(obj); }

			// assign an object id to the object to allow referencing it later on
			if (!typeToSerialize.IsValueType)
				serializer.mSerializedObjectIdTable.Add(obj, serializer.mNextSerializedObjectId++);

			// close the archive
			buffer = writer.GetSpan(1);
			buffer[0] = (byte)PayloadType.ArchiveEnd;
			writer.Advance(1);
		};
	}

	/// <summary>
	/// Gets a serialization delegate for the specified enumeration type.
	/// </summary>
	/// <param name="type">Enumeration type to get a serialization delegate for.</param>
	/// <returns>A delegate that handles the serialization of the specified enumeration type.</returns>
	private static SerializerDelegate GetEnumSerializer(Type type)
	{
		// determine the integer type the enumeration is built on top to return a serialization
		// delegate that is optimized for that type
		Type underlyingType = type.GetEnumUnderlyingType();

		if (underlyingType == typeof(sbyte))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (sbyte)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(byte))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (byte)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(short))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (short)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(ushort))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (ushort)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(int))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor32BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (int)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(uint))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (long)(uint)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(long))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (long)obj);
				writer.Advance(bufferIndex);
			};
		}

		if (underlyingType == typeof(ulong))
		{
			return (
				serializer,
				writer,
				obj,
				_) =>
			{
				serializer.WriteTypeMetadata(writer, type);
				Span<byte> buffer = writer.GetSpan(1 + Leb128EncodingHelper.MaxBytesFor64BitValue);
				int bufferIndex = 0;
				buffer[bufferIndex++] = (byte)PayloadType.Enum;
				bufferIndex += Leb128EncodingHelper.Write(buffer[bufferIndex..], (long)(ulong)obj);
				writer.Advance(bufferIndex);
			};
		}

		throw new NotSupportedException($"The underlying type ({underlyingType.FullName}) of enumeration ({type.FullName}) is not supported.");
	}

	#endregion

	#region Deserialization

	private static readonly DeserializerDelegate[]                                        sDeserializersByPayloadType        = new DeserializerDelegate[(int)PayloadType.Terminator];
	private static          TypeKeyedDictionary<EnumCasterDelegate>                       sEnumCasters                       = new();
	private static          TypeKeyedDictionary<DeserializationConstructorCallerDelegate> sDeserializationConstructorCallers = new();

	/// <summary>
	/// Gets a value indicating whether the serializer is currently deserializing an object that was serialized
	/// on a little endian system.
	/// </summary>
	public bool IsDeserializingLittleEndian { get; private set; } = false;

	/// <summary>
	/// Deserializes an object from the stream.
	/// </summary>
	/// <param name="stream">Stream to deserialize the object from.</param>
	/// <param name="context">Context object to pass to the serializer.</param>
	/// <param name="useTolerantDeserialization"><c>true</c> to enable tolerant deserialization; otherwise <c>false</c>.</param>
	/// <returns>The deserialized object.</returns>
	public static object Deserialize(Stream stream, object context, bool useTolerantDeserialization)
	{
		Serializer serializer = null;

		try
		{
			serializer = GetPooledSerializer(useTolerantDeserialization, SerializationOptimization.Speed);
			return serializer.Deserialize(stream, context);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
		}
	}

	/// <summary>
	/// Deserializes an object from a stream.
	/// </summary>
	/// <param name="stream">Stream to deserialize the object from.</param>
	/// <param name="context">Context object to pass to the serializer of the expected object (can be <c>null</c>).</param>
	/// <returns>Deserialized object.</returns>
	/// <exception cref="VersionNotSupportedException">The serializer version of one of the objects in the specified stream is not supported.</exception>
	/// <exception cref="SerializationException">
	/// Deserializing the object failed (the exception object contains a message describing the reason why
	/// deserialization has failed).
	/// </exception>
	public object Deserialize(Stream stream, object context = null)
	{
		try
		{
			// prepare the serializer
			ResetDeserializer();
			AllocateTemporaryBuffers();

			// read the endianness indicator
			int readByte = stream.ReadByte();
			if (readByte < 0) throw new SerializationException("Unexpected end of stream.");
			IsDeserializingLittleEndian = readByte != 0;

			// deserialize the object
			return InnerDeserialize(stream, context);
		}
		finally
		{
			ReleaseTemporaryBuffers();
		}
	}

	/// <summary>
	/// Deserializes an object from a stream (internal).
	/// </summary>
	/// <param name="stream">Stream to deserialize an object from.</param>
	/// <param name="context">Context to pass to the next serializer via the serialization archive.</param>
	/// <returns>Deserialized object.</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Deserialization failed due to an invalid stream of bytes.</exception>
	internal object InnerDeserialize(Stream stream, object context)
	{
		int byteRead = stream.ReadByte();
		if (byteRead < 0) throw new SerializationException("Unexpected end of stream.");

		// try to use a deserializer
		if (byteRead < sDeserializersByPayloadType.Length)
			return sDeserializersByPayloadType[byteRead](this, stream, context);

		// unknown object type
		throw new SerializationException($"Unknown payload type ({byteRead}).");
	}

	/// <summary>
	/// Reads an enumeration value.
	/// </summary>
	/// <param name="stream">Stream to read the enumeration value from.</param>
	/// <returns>The read enumeration value.</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Deserialized value does not match the underlying type of the enumeration.</exception>
	private Enum ReadEnum(Stream stream)
	{
		// assembly and type metadata have been read already

		// get/create an enum caster that converts a 64bit integer (long) into a properly typed enumeration value
		if (!sEnumCasters.TryGetValue(mCurrentDeserializedType.Type, out EnumCasterDelegate caster))
		{
			lock (sSync)
			{
				if (!sEnumCasters.TryGetValue(mCurrentDeserializedType.Type, out caster))
				{
					caster = CreateEnumCaster(mCurrentDeserializedType.Type);
					var copy = new TypeKeyedDictionary<EnumCasterDelegate>(sEnumCasters) { [mCurrentDeserializedType.Type] = caster };
					Thread.MemoryBarrier();
					sEnumCasters = copy;
				}
			}
		}

		long value = Leb128EncodingHelper.ReadInt64(stream);
		return caster(value);
	}

	/// <summary>
	/// Creates a method that casts an integer to the specified enumeration type.
	/// </summary>
	/// <param name="type">Enumeration type to cast an integer to.</param>
	/// <returns>A caster delegate.</returns>
	private static EnumCasterDelegate CreateEnumCaster(Type type)
	{
		Type[] parameterTypes = [typeof(long)];
		ParameterExpression parameterExpression = parameterTypes.Select(Expression.Parameter).First();
		Expression body = Expression.Convert(Expression.Convert(parameterExpression, type), typeof(Enum));
		LambdaExpression lambda = Expression.Lambda(typeof(EnumCasterDelegate), body, parameterExpression);
		return (EnumCasterDelegate)lambda.Compile();
	}

	/// <summary>
	/// Deserializes a type object.
	/// </summary>
	/// <param name="stream">Stream to deserialize the type object from.</param>
	/// <param name="assignObjectId"><c>true</c> to assign an object id; otherwise <c>false</c>.</param>
	/// <param name="sourceTypeName">Receives the assembly qualified name of the type as it was known when serializing.</param>
	/// <returns>Type object.</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	private Type ReadTypeObject(Stream stream, bool assignObjectId, out string sourceTypeName)
	{
		TypeItem typeItem;
		int byteRead = stream.ReadByte();
		if (byteRead < 0) throw new SerializationException("Unexpected end of stream.");
		var objType = (PayloadType)byteRead;
		if (objType == PayloadType.TypeId)
		{
			uint id = Leb128EncodingHelper.ReadUInt32(stream);
			if (!mDeserializedTypeIdTable.TryGetValue(id, out typeItem))
				throw new SerializationException("Deserialized type id that does not match a previously deserialized type.");
		}
		else if (objType == PayloadType.Type)
		{
			// read number of utf-8 code units in the following string
			int length = Leb128EncodingHelper.ReadInt32(stream);

			// read type full name
			byte[] array = new byte[length];
			int bytesRead = stream.Read(array, 0, length);
			if (bytesRead < length) throw new SerializationException("Unexpected end of stream.");
			string typename = Encoding.UTF8.GetString(array, 0, length);

			// assign a type id if the serializer uses type ids
			typeItem = new TypeItem(typename, ResolveType(typename));
			mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
		}
		else
		{
			var trace = new StackTrace();
			string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
			sLog.Write(LogLevel.Error, error);
			throw new SerializationException(error);
		}

		if (typeItem.Type.IsGenericTypeDefinition)
		{
			// read number of type arguments following the type definition
			int genericTypeArgumentCount = Leb128EncodingHelper.ReadInt32(stream);

			if (genericTypeArgumentCount > 0)
			{
				Type[] genericTypeParameters = typeItem.Type.GetTypeInfo().GenericTypeParameters;
				var genericTypeArgumentTypeItems = new TypeItem[genericTypeParameters.Length];
				for (int i = 0; i < genericTypeParameters.Length; i++)
				{
					Type genericTypeArgument = ReadTypeObject(stream, false, out string genericTypeArgumentSourceName);
					genericTypeArgumentTypeItems[i] = new TypeItem(genericTypeArgumentSourceName, genericTypeArgument);
				}

				// compose the generic type
				Type composedType = typeItem.Type.MakeGenericType(genericTypeArgumentTypeItems.Select(x => x.Type).ToArray());
				typeItem = new TypeItem(MakeGenericTypeName(typeItem.Name, genericTypeArgumentTypeItems.Select(x => x.Name)), composedType);

				// assign a type id if the serializer uses assembly and type ids
				typeItem = new TypeItem(typeItem.Name, composedType);
				mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
			}
		}

		// assign an object id, if requested
		if (assignObjectId)
			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, typeItem.Type);

		sourceTypeName = typeItem.Name;
		return typeItem.Type;
	}

	/// <summary>
	/// Composes an assembly qualified type name taking the assembly qualified type name of the generic type definition
	/// and assembly qualified generic type arguments.
	/// </summary>
	/// <param name="genericTypeDefinitionName">The assembly qualified type name of the generic type definition.</param>
	/// <param name="genericTypeArgumentNames">The assembly qualified type names of the generic type arguments.</param>
	/// <returns>The assembly qualified name of the composed generic type.</returns>
	private static string MakeGenericTypeName(string genericTypeDefinitionName, IEnumerable<string> genericTypeArgumentNames)
	{
		int index = genericTypeDefinitionName.IndexOf(',');
		if (index < 0 || index + 1 == genericTypeDefinitionName.Length) throw new SerializationException($"Detected invalid type name ({genericTypeDefinitionName}) during deserialization.");
		string typeName = genericTypeDefinitionName[..index].Trim();
		string assemblyName = genericTypeDefinitionName[(index + 1)..].Trim();
		var builder = new StringBuilder();
		builder.Append(typeName);
		builder.Append('[');
		bool first = true;
		foreach (string argumentTypeName in genericTypeArgumentNames)
		{
			if (!first) builder.Append(',');
			builder.Append('[');
			builder.Append(argumentTypeName);
			builder.Append(']');
			first = false;
		}

		builder.Append("], ");
		builder.Append(assemblyName);
		return builder.ToString();
	}

	/// <summary>
	/// Gets an already deserialized object from its object id stored in the stream.
	/// </summary>
	/// <param name="stream">Stream to deserialize the object from.</param>
	/// <returns>Deserialized object (from the deserialized object table).</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Deserialized object id does not correspond to a previously deserialized object.</exception>
	private object ReadAlreadySerializedObject(Stream stream)
	{
		uint id = Leb128EncodingHelper.ReadUInt32(stream);
		if (mDeserializedObjectIdTable.TryGetValue(id, out object obj))
			return obj;

		throw new SerializationException("Invalid object id detected.");
	}

	/// <summary>
	/// Deserializes an object from an archive.
	/// </summary>
	/// <param name="stream">Stream to read from.</param>
	/// <param name="context">Context to pass to the serializer via the deserialization archive.</param>
	/// <returns>Deserialized object.</returns>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">
	/// Object serializer version of the object that is about to be deserialized is higher than the max. supported
	/// version of the available serializer.
	/// </exception>
	/// <exception cref="SerializationException">There is no internal/external object serializer for the object that is about to be deserialized.</exception>
	private object ReadArchive(Stream stream, object context)
	{
		uint deserializedVersion = Leb128EncodingHelper.ReadUInt32(stream);
		string error;

		#region Internal Object Serializer

		if (HasInternalObjectSerializer(mCurrentDeserializedType.Type, out uint currentVersion))
		{
			if (deserializedVersion > currentVersion)
			{
				// version of the archive that is about to be deserialized is greater than
				// the version the internal object serializer supports
				error = $"Deserializing type '{mCurrentDeserializedType.Type.FullName}' failed due to a version conflict (got version: {deserializedVersion}, max. supported version: {currentVersion}).";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			// version is ok, deserialize...
			if (!sDeserializationConstructorCallers.TryGetValue(mCurrentDeserializedType.Type, out DeserializationConstructorCallerDelegate deserializationConstructorCaller))
			{
				lock (sSync)
				{
					if (!sDeserializationConstructorCallers.TryGetValue(mCurrentDeserializedType.Type, out deserializationConstructorCaller))
					{
						deserializationConstructorCaller = CreateDeserializationConstructorCaller(mCurrentDeserializedType.Type);
						var copy = new TypeKeyedDictionary<DeserializationConstructorCallerDelegate>(sDeserializationConstructorCallers) { [mCurrentDeserializedType.Type] = deserializationConstructorCaller };
						Thread.MemoryBarrier();
						sDeserializationConstructorCallers = copy;
					}
				}
			}

			bool isValueType = mCurrentDeserializedType.Type.IsValueType;
			var archive = new DeserializationArchive(this, stream, mCurrentDeserializedType.Type, deserializedVersion, context);
			object obj = deserializationConstructorCaller(ref archive);

			// assign an object id to the deserialized object, the serialization stream may refer to it later on
			if (!isValueType)
				mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, obj);

			// read and check archive end
			ReadAndCheckPayloadType(stream, PayloadType.ArchiveEnd);
			return obj;
		}

		#endregion

		#region External Object Serializer

		// try to get an external object serializer
		IExternalObjectSerializer eos = ExternalObjectSerializerFactory.GetExternalObjectSerializer(mCurrentDeserializedType.Type, out currentVersion);

		if (eos != null)
		{
			if (deserializedVersion > currentVersion)
			{
				// version of the archive that is about to be deserialized is greater than
				// the version the internal object serializer supports
				error = $"Deserializing type '{mCurrentDeserializedType.Type.FullName}' failed due to a version conflict (got version: {deserializedVersion}, max. supported version: {currentVersion}).";
				sLog.Write(LogLevel.Error, error);
				throw new SerializationException(error);
			}

			// version is ok, deserialize...
			bool isValueType = mCurrentDeserializedType.Type.IsValueType;
			var archive = new DeserializationArchive(this, stream, mCurrentDeserializedType.Type, deserializedVersion, context);
			object obj = eos.Deserialize(archive);

			// assign an object id to the deserialized object, the serialization stream may refer to it later on
			if (!isValueType)
				mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, obj);

			// read and check archive end
			ReadAndCheckPayloadType(stream, PayloadType.ArchiveEnd);
			return obj;
		}

		#endregion

		error = $"Deserializing type '{mCurrentDeserializedType.Type.FullName}' failed because it lacks an internal/external object serializer.";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	/// <summary>
	/// Creates a method that casts an integer to the specified enumeration type.
	/// </summary>
	/// <param name="type">Enumeration type to cast an integer to.</param>
	/// <returns>A caster delegate.</returns>
	private static DeserializationConstructorCallerDelegate CreateDeserializationConstructorCaller(Type type)
	{
		Type[] parameterTypes = [typeof(DeserializationArchive).MakeByRefType()];
		ParameterExpression parameterExpression = parameterTypes.Select(Expression.Parameter).First();
		ConstructorInfo constructor = type.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			Type.DefaultBinder,
			[typeof(DeserializationArchive)],
			null);
		Debug.Assert(constructor != null, nameof(constructor) + " != null");
		Expression body = Expression.New(constructor, parameterExpression);
		if (type.IsValueType) body = Expression.Convert(body, typeof(object)); // box value types
		LambdaExpression lambda = Expression.Lambda(typeof(DeserializationConstructorCallerDelegate), body, parameterExpression);
		return (DeserializationConstructorCallerDelegate)lambda.Compile();
	}

	/// <summary>
	/// Reads the payload type from the stream and checks whether it matches the expected payload type.
	/// </summary>
	/// <param name="stream">Stream to read from.</param>
	/// <param name="type">Type to check for.</param>
	/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
	/// <exception cref="SerializationException">Specified payload type does not match the received payload type.</exception>
	private static void ReadAndCheckPayloadType(Stream stream, PayloadType type)
	{
		int readByte = stream.ReadByte();
		if (readByte < 0) throw new SerializationException("Stream ended unexpectedly.");
		var payloadType = (PayloadType)readByte;
		if (payloadType == type) return;
		var trace = new StackTrace();
		string error = $"Unexpected payload type during deserialization. Stack Trace:\n{trace}";
		sLog.Write(LogLevel.Error, error);
		throw new SerializationException(error);
	}

	#endregion

	#region Info

	/// <summary>
	/// Gets the maximum version a serializer of the specified type supports.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>Maximum version the serializer supports.</returns>
	/// <exception cref="ArgumentException">Type is not serializable.</exception>
	/// <remarks>
	/// The maximum version a serializer supports is taken from the attribute assigned to the serializer.
	/// </remarks>
	public static uint GetSerializerVersion(Type type)
	{
		Init();

		if (HasInternalObjectSerializer(type, out uint version))
			return version;

		if (HasExternalObjectSerializer(type, out version))
			return version;

		throw new ArgumentException($"Specified type ({type.FullName}) is not serializable.", nameof(type));
	}

	/// <summary>
	/// Checks whether the specified object is serializable.
	/// </summary>
	/// <param name="obj">Object to check.</param>
	/// <returns>
	/// <c>true</c>, if the object is serializable;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool IsSerializable(object obj)
	{
		if (obj == null) return true;
		Init();
		Type type = obj.GetType();
		return IsSerializable(type);
	}

	/// <summary>
	/// Checks whether the specified type is serializable.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <c>true</c>, if the object is serializable;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool IsSerializable(Type type)
	{
		Init();
		if (sSerializers.ContainsKey(type)) return true;
		if (type.IsArray && IsSerializable(type.GetElementType())) return true;
		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return sSerializers.ContainsKey(genericTypeDefinition);
		}

		// check whether the type has an internal object serializer and create a serializer delegate for it,
		// so it is found faster next time...
		if (!HasInternalObjectSerializer(type, out uint _)) return false;
		AddSerializerForType(type, () => CreateInternalObjectSerializer(type));
		return true;
	}

	/// <summary>
	/// Checks whether the specified type is serialized using a custom (internal/external) object serializer.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <c>true</c>, if the type is serialized using a custom serializer;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool HasCustomSerializer(Type type)
	{
		return HasInternalObjectSerializer(type, out uint _) || HasExternalObjectSerializer(type, out uint _);
	}

	#endregion

	#region Convenience: Copying Serializable Objects

	/// <summary>
	/// Copies a serializable object once (considers immutability).
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <returns>
	/// Copy of the specified object;
	/// <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static object CopySerializableObject(object obj)
	{
		return CopySerializableObject(obj, null, null);
	}

	/// <summary>
	/// Copies a serializable object once (considers immutability).
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <param name="serializationContext">Serialization context to use.</param>
	/// <param name="deserializationContext">Serialization context to use.</param>
	/// <returns>
	/// Copy of the specified object;
	/// <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static object CopySerializableObject(object obj, object serializationContext, object deserializationContext)
	{
		Init();

		// abort if the object to copy is null
		if (obj == null) return null;

		if (Immutability.IsImmutable(obj.GetType()))
		{
			// the object is immutable and can be 'copied' by assigning
			return obj;
		}

		MemoryBlockStream stream = null;
		Serializer serializer = null;
		object copy;

		try
		{
			stream = GetPooledMemoryBlockStream();
			serializer = GetPooledSerializer(false, SerializationOptimization.Speed);
			serializer.Serialize(stream, obj, serializationContext);
			stream.Position = 0;
			copy = serializer.Deserialize(stream, deserializationContext);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
			ReturnMemoryBlockStreamToPool(stream);
		}

		return copy;
	}

	/// <summary>
	/// Copies a serializable object once (considers immutability).
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value to copy (the actual object passed via <paramref name="obj"/> may be derived from it).
	/// </typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <returns>
	/// Copy of the specified object;
	/// <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static T CopySerializableObject<T>(T obj)
	{
		return CopySerializableObject(obj, null, null);
	}

	/// <summary>
	/// Copies a serializable object once (considers immutability).
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value to copy (the actual object passed via <paramref name="obj"/> may be derived from it).
	/// </typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <param name="serializationContext">Serialization context to use.</param>
	/// <param name="deserializationContext">Serialization context to use.</param>
	/// <returns>
	/// Copy of the specified object;
	/// <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static T CopySerializableObject<T>(T obj, object serializationContext, object deserializationContext)
	{
		Init();

		// abort if the object to copy is null
		// ReSharper disable once CompareNonConstrainedGenericWithNull
		if (obj == null) return default;

		if (Immutability.IsImmutable(obj.GetType())) // do not use typeof(T) as obj might be of a derived type that is not immutable
		{
			// the object is immutable and can be 'copied' by assigning
			return obj;
		}

		MemoryBlockStream stream = null;
		Serializer serializer = null;
		object copy;

		try
		{
			stream = GetPooledMemoryBlockStream();
			serializer = GetPooledSerializer(false, SerializationOptimization.Speed);
			serializer.Serialize(stream, obj, serializationContext);
			stream.Position = 0;
			copy = serializer.Deserialize(stream, deserializationContext);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
			ReturnMemoryBlockStreamToPool(stream);
		}

		return (T)copy;
	}

	/// <summary>
	/// Copies a serializable object multiple times (considers immutability).
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value to copy (the actual object passed via <paramref name="obj"/> may be derived from it).
	/// </typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <param name="count">Number of copies to make.</param>
	/// <returns>
	/// An array of copies of the specified object;
	/// an array of <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static T[] CopySerializableObject<T>(T obj, int count)
	{
		return CopySerializableObject(obj, null, null, count);
	}

	/// <summary>
	/// Copies a serializable object multiple times (considers immutability).
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value to copy (the actual object passed via <paramref name="obj"/> may be derived from it).
	/// </typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <param name="count">Number of copies to make.</param>
	/// <param name="serializationContext">Serialization context to use.</param>
	/// <param name="deserializationContext">Serialization context to use.</param>
	/// <returns>
	/// An array of copies of the specified object;
	/// an array of <paramref name="obj"/> if the object is considered immutable by the <see cref="Immutability"/> class.
	/// </returns>
	/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
	public static T[] CopySerializableObject<T>(
		T      obj,
		object serializationContext,
		object deserializationContext,
		int    count)
	{
		Init();

		// abort if the object to copy is null or if count is zero
		T[] copies = count > 0 ? new T[count] : [];
		// ReSharper disable once CompareNonConstrainedGenericWithNull
		if (obj == null || count == 0)
			return copies;

		if (Immutability.IsImmutable(obj.GetType())) // do not use typeof(T) as obj might be of a derived type that is not immutable
		{
			// the object is immutable and can be 'copied' by assigning

#if NETSTANDARD2_0 || NET461
			for (int i = 0; i < count; i++)
			{
				copies[i] = obj;
			}
#else
			Array.Fill(copies, obj);
#endif

			return copies;
		}

		// other objects
		MemoryBlockStream stream = null;
		Serializer serializer = null;

		try
		{
			stream = GetPooledMemoryBlockStream();
			serializer = GetPooledSerializer(false, SerializationOptimization.Speed);
			serializer.Serialize(stream, obj, serializationContext);
			for (int i = 0; i < count; i++)
			{
				stream.Position = 0;
				copies[i] = (T)serializer.Deserialize(stream, deserializationContext);
			}
		}
		finally
		{
			ReturnSerializerToPool(serializer);
			ReturnMemoryBlockStreamToPool(stream);
		}

		return copies;
	}

	#endregion

	#region Convenience: Serialization/Deserialization to/from File

	/// <summary>
	/// Serializes the specified object to a file.
	/// </summary>
	/// <typeparam name="T">Type to serialize.</typeparam>
	/// <param name="filename">Name of the file to write.</param>
	/// <param name="obj">Object to serialize.</param>
	/// <param name="context">Context object to pass to the serializer.</param>
	public static void Serialize<T>(string filename, T obj, object context = null)
	{
		Serializer serializer = null;

		try
		{
			serializer = GetPooledSerializer(true, SerializationOptimization.Size);
			using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			serializer.Serialize(fs, obj, context);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
		}
	}

	/// <summary>
	/// Deserializes an object from the specified file.
	/// </summary>
	/// <typeparam name="T">Type to deserialize.</typeparam>
	/// <param name="filename">Name of the file to read.</param>
	/// <param name="context">Context object to pass to the serializer.</param>
	/// <returns>The deserialized object.</returns>
	public static T Deserialize<T>(string filename, object context = null)
	{
		Serializer serializer = null;

		try
		{
			serializer = GetPooledSerializer(true, SerializationOptimization.Size);
			using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			return (T)serializer.Deserialize(fs, context);
		}
		finally
		{
			ReturnSerializerToPool(serializer);
		}
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Checks whether the specified type provides an internal object serializer.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <param name="version">Receives the current version of the internal object serializer.</param>
	/// <returns>
	/// <c>true</c> if the specified type provides an internal object serializer;
	/// otherwise <c>false</c>.
	/// </returns>
	internal static bool HasInternalObjectSerializer(Type type, out uint version)
	{
		if (sInternalObjectSerializerInfoByType.TryGetValue(type, out InternalObjectSerializerInfo info))
		{
			version = info.SerializerVersion;
			return true;
		}

		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (sInternalObjectSerializerInfoByType.TryGetValue(genericTypeDefinition, out info))
			{
				version = info.SerializerVersion;
				return true;
			}
		}

		version = 0;
		return false;
	}

	/// <summary>
	/// Gets the overridden version number of the serializer to use for the specified type.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <param name="version">Receives the version number to use when serializing the specified type.</param>
	/// <returns>
	/// <c>true</c>, if a serializer version override is available;
	/// otherwise <c>false</c>.
	/// </returns>
	internal bool GetSerializerVersionOverride(Type type, out uint version)
	{
		return mSerializedTypeVersionTable.TryGet(type, out version);
	}

	/// <summary>
	/// Checks whether the specified type provides an external object serializer.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <param name="version">Receives the version of the serializer.</param>
	/// <returns>
	/// <c>true</c> if the specified type can be serialized using an external object serializer;
	/// otherwise <c>false</c>.
	/// </returns>
	internal static bool HasExternalObjectSerializer(Type type, out uint version)
	{
		return ExternalObjectSerializerFactory.GetExternalObjectSerializer(type, out version) != null;
	}

	/// <summary>
	/// Gets the internal object serializer interface of an object, if it implements it.
	/// </summary>
	/// <param name="obj">Object to retrieve the internal serializer interface for.</param>
	/// <param name="version">Receives the version of the serializer for the specified type.</param>
	/// <returns>
	/// Internal object serializer interface implemented by the type of the specified object;
	/// <c>null</c>, if the type of the specified object does not (properly) implement the internal object serializer.
	/// </returns>
	internal static IInternalObjectSerializer GetInternalObjectSerializer(object obj, out uint version)
	{
		if (HasInternalObjectSerializer(obj.GetType(), out version))
			return obj as IInternalObjectSerializer;

		// type does not implement an internal object serializer, or not properly...
		return null;
	}

	/// <summary>
	/// Gets the internal object serializer interface of the specified object's base class, if it implements it.
	/// </summary>
	/// <param name="obj">Object to retrieve the internal serializer interface for.</param>
	/// <param name="type">The base class the specified object's class derives from.</param>
	/// <param name="version">Receives the version of the serializer for the specified type.</param>
	/// <returns>
	/// Internal object serializer interface implemented by the specified base class of the specified object;
	/// <c>null</c>, if the specified base class of the specified object does not (properly) implement the internal object serializer.
	/// </returns>
	internal static IInternalObjectSerializer GetInternalObjectSerializer(object obj, Type type, out uint version)
	{
		if (HasInternalObjectSerializer(type, out version))
			return obj as IInternalObjectSerializer;

		// type does not implement an internal object serializer, or not properly...
		return null;
	}

	#endregion

	#region Serializer Pool

	private static readonly ConcurrentBag<Serializer> sSerializerPool = [];

	/// <summary>
	/// Gets a serializer from the pool.
	/// </summary>
	/// <param name="useTolerantDeserialization"><c>true</c> to enable tolerant deserialization; otherwise <c>false</c>.</param>
	/// <param name="optimization">Determines whether the serializer optimizes for size or for speed.</param>
	/// <returns>A serializer.</returns>
	private static Serializer GetPooledSerializer(
		bool                      useTolerantDeserialization,
		SerializationOptimization optimization)
	{
		// ReSharper disable once InvertIf
		if (sSerializerPool.TryTake(out Serializer serializer))
		{
			serializer.UseTolerantDeserialization = useTolerantDeserialization;
			serializer.SerializationOptimization = optimization;
			return serializer;
		}

		return new Serializer
		{
			UseTolerantDeserialization = useTolerantDeserialization,
			SerializationOptimization = optimization
		};
	}

	/// <summary>
	/// Returns a serializer to the pool.
	/// </summary>
	/// <param name="serializer">Serializer to return.</param>
	private static void ReturnSerializerToPool(Serializer serializer)
	{
		if (serializer == null) return;
		serializer.Reset();
		sSerializerPool.Add(serializer);
	}

	#endregion

	#region MemoryBlockStream Pool

	private static readonly ConcurrentBag<MemoryBlockStream> sMemoryBlockStreamPool = [];

	/// <summary>
	/// Gets a <see cref="MemoryBlockStream"/> from the pool.
	/// </summary>
	/// <returns>A <see cref="MemoryBlockStream"/>.</returns>
	private static MemoryBlockStream GetPooledMemoryBlockStream()
	{
		return sMemoryBlockStreamPool.TryTake(out MemoryBlockStream stream)
			       ? stream
			       : new MemoryBlockStream(ArrayPool<byte>.Shared);
	}

	/// <summary>
	/// Returns a <see cref="MemoryBlockStream"/> to the pool.
	/// </summary>
	/// <param name="stream"><see cref="MemoryBlockStream"/> to return.</param>
	private static void ReturnMemoryBlockStreamToPool(MemoryBlockStream stream)
	{
		if (stream == null) return;
		stream.SetLength(0);
		sMemoryBlockStreamPool.Add(stream);
	}

	#endregion
}
