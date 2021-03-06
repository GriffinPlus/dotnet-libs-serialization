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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;
using GriffinPlus.Lib.Logging;
using System.Linq.Expressions;
using System.Linq;
using GriffinPlus.Lib.Io;

namespace GriffinPlus.Lib.Serialization
{
	/// <summary>
	/// Serializer for serializing and deserializing various objects
	/// </summary>
	public partial class Serializer
	{
		#region Internal Data Types

		private struct TypeItem
		{
			public readonly string Name;
			public readonly Type Type;

			public static readonly TypeItem Empty = new TypeItem();

			public TypeItem(string name, Type type)
			{
				Name = name;
				Type = type;
			}
		}

		private delegate void SerializerDelegate(Serializer serializer, Stream stream, object obj, object context);
		private delegate object DeserializerDelegate(Serializer serializer, Stream stream, object context);
		private delegate object EnumCasterDelegate(long value);
		internal delegate void IosSerializeDelegate(IInternalObjectSerializer ios, SerializerArchive archive, uint version);

		#endregion

		#region Class Variables

		// logging
		private static readonly LogWriter                                     sLog                                    = Log.GetWriter<Serializer>();

		// initialization
		private static          bool                                          sInitializing                           = false;
		private static          bool                                          sInitialized                            = false;
		private static          object                                        sInitializationSync                     = new object();

		private static readonly object                                        sSync                                   = new object();
		private static readonly Type[]                                        sConstructorArgumentTypes               = new Type[] { typeof(SerializerArchive) };
		private static Dictionary<string, Assembly>                           sAssemblyTable                          = new Dictionary<string, Assembly>();
		private static object                                                 sAssemblyTableLock                      = new object();
		private static Dictionary<string, Type>                               sTypeTable                              = new Dictionary<string,Type>();
		private static readonly object                                        sTypeTableLock                          = new object();
		private static Dictionary<Type, SerializerDelegate>                   sSerializers                            = new Dictionary<Type,SerializerDelegate>();
		private static readonly Dictionary<Type, SerializerDelegate>          sMultidimensionalArraySerializers       = new Dictionary<Type,SerializerDelegate>();
		private static readonly Dictionary<PayloadType, DeserializerDelegate> sDeserializers                          = new Dictionary<PayloadType,DeserializerDelegate>();
		private static Dictionary<Type,ExternalObjectSerializerInfo>          sExternalObjectSerializersBySerializee  = new Dictionary<Type,ExternalObjectSerializerInfo>();
		private static Dictionary<Type,IosSerializeDelegate>                  sIosSerializeCallers                    = new Dictionary<Type,IosSerializeDelegate>();
		private static int                                                    sIosSerializeCallersId                  = -1;
		private static Dictionary<Type, EnumCasterDelegate>                   sEnumCasters                            = new Dictionary<Type,EnumCasterDelegate>();
		private static int                                                    sEnumCasterId                           = -1;
		private static SerializerCache                                        sCache                                  = null;
		private static bool                                                   sIsVersionTolerantDefault               = false;

		#endregion

		#region Member Variables

		// for serialization only
		private Type                         mCurrentSerializedType;
		private Dictionary<Type, uint>       mSerializedTypeIdTable;
		private SerializerVersionTable       mSerializedTypeVersionTable;
		private Dictionary<object, uint>     mSerializedObjectIdTable;
		private uint                         mNextSerializedTypeId;
		private uint                         mNextSerializedObjectId;

		// for deserialization only
		private TypeItem                       mCurrentDeserializedType;
		private Dictionary<uint,TypeItem>      mDeserializedTypeIdTable;
		private Dictionary<uint,object>        mDeserializedObjectIdTable;
		private uint                           mNextDeserializedTypeId;
		private uint                           mNextDeserializedObjectId;
		private bool                           mIsVersionTolerant;

		#endregion

		#region Class Initialization

		/// <summary>
		/// Initializes the serializer, if necessary.
		/// </summary>
		/// <param name="isVersionTolerant">
		/// true to allow resolving assemblies to an assembly with a different version, if necessary;
		/// false to abort deserialization if the full assembly name does not match.
		/// </param>
		public static void Init(bool isVersionTolerant = false)
		{
			if (!sInitialized) {
				lock (sInitializationSync) {
					if (!sInitialized && !sInitializing) {
						sInitializing = true;
						sIsVersionTolerantDefault = isVersionTolerant;
						sCache = SerializerCache.Instance;
						InitSerializers();
						InitDeserializers();
						RegisterExternalObjectSerializers();
						sInitialized = true;
						sInitializing = false;
					}
				}
			}
		}

		/// <summary>
		/// Triggers initializing the serializer asynchronously, if necessary.
		/// </summary>
		/// <param name="isVersionTolerant">
		/// true to allow resolving assemblies to an assembly with a different version, if necessary;
		/// false to abort deserialization if the full assembly name does not match.
		/// </param>
		public static void TriggerInit(bool isVersionTolerant = false)
		{
			if (!sInitialized && !sInitializing) {
				lock (sInitializationSync) {
					if (!sInitialized && !sInitializing) {
						sIsVersionTolerantDefault = isVersionTolerant;
						ThreadPool.QueueUserWorkItem(x => Init(isVersionTolerant));
					}
				}
			}
		}

		/// <summary>
		/// Adds serializers for types that are supported out of the box.
		/// </summary>
		private static void InitSerializers()
		{
			// simple types
			sSerializers.Add(typeof(SByte),    (serializer, stream, obj, context) => { serializer.WritePrimitive_SByte((sbyte)obj, stream);        });
			sSerializers.Add(typeof(Int16),    (serializer, stream, obj, context) => { serializer.WritePrimitive_Int16((short)obj, stream);        });
			sSerializers.Add(typeof(Int32),    (serializer, stream, obj, context) => { serializer.WritePrimitive_Int32((int)obj, stream);          });
			sSerializers.Add(typeof(Int64),    (serializer, stream, obj, context) => { serializer.WritePrimitive_Int64((long)obj, stream);         });
			sSerializers.Add(typeof(Byte),     (serializer, stream, obj, context) => { serializer.WritePrimitive_Byte((byte)obj, stream);          });
			sSerializers.Add(typeof(UInt16),   (serializer, stream, obj, context) => { serializer.WritePrimitive_UInt16((ushort)obj, stream);      });
			sSerializers.Add(typeof(UInt32),   (serializer, stream, obj, context) => { serializer.WritePrimitive_UInt32((uint)obj, stream);        });
			sSerializers.Add(typeof(UInt64),   (serializer, stream, obj, context) => { serializer.WritePrimitive_UInt64((ulong)obj, stream);       });
			sSerializers.Add(typeof(Single),   (serializer, stream, obj, context) => { serializer.WritePrimitive_Single((float)obj, stream);       });
			sSerializers.Add(typeof(Double),   (serializer, stream, obj, context) => { serializer.WritePrimitive_Double((double)obj, stream);      });
			sSerializers.Add(typeof(Boolean),  (serializer, stream, obj, context) => { serializer.WritePrimitive_Boolean((bool)obj, stream);       });
			sSerializers.Add(typeof(Char),     (serializer, stream, obj, context) => { serializer.WritePrimitive_Char((char)obj, stream);          });
			sSerializers.Add(typeof(Decimal),  (serializer, stream, obj, context) => { serializer.WritePrimitive_Decimal((decimal)obj, stream);    });
			sSerializers.Add(typeof(DateTime), (serializer, stream, obj, context) => { serializer.WritePrimitive_DateTime((DateTime)obj, stream);  });
			sSerializers.Add(typeof(String),   (serializer, stream, obj, context) => { serializer.WritePrimitive_String((string)obj, stream);      });
			sSerializers.Add(typeof(Object),   (serializer, stream, obj, context) => { serializer.WritePrimitive_Object(obj, stream);              });

			// arrays of simple types (one-dimensional, zero-based indexing)
			sSerializers.Add(typeof(SByte[]),    (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfSByte,   array as sbyte[], 1, stream);    });
			sSerializers.Add(typeof(Int16[]),    (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfInt16,   array as short[], 2, stream);    });
			sSerializers.Add(typeof(Int32[]),    (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfInt32,   array as int[],   4, stream);    });
			sSerializers.Add(typeof(Int64[]),    (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfInt64,   array as long[],  8, stream);    });
			sSerializers.Add(typeof(Byte[]),     (serializer, stream, array, context) => { serializer.WriteArrayOfByte(array as byte[], stream);                                          });
			sSerializers.Add(typeof(UInt16[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfUInt16,  array as ushort[], 2, stream);   });
			sSerializers.Add(typeof(UInt32[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfUInt32,  array as uint[],   4, stream);   });
			sSerializers.Add(typeof(UInt64[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfUInt64,  array as ulong[],  8, stream);   });
			sSerializers.Add(typeof(Single[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfSingle,  array as float[], 4, stream);    });
			sSerializers.Add(typeof(Double[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfDouble,  array as double[], 8, stream);   });
			sSerializers.Add(typeof(Boolean[]),  (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfBoolean, array as bool[], 1, stream);     });
			sSerializers.Add(typeof(Char[]),     (serializer, stream, array, context) => { serializer.WriteArrayOfPrimitives(PayloadType.ArrayOfChar,    array as char[], 2, stream);     });
			sSerializers.Add(typeof(Decimal[]),  (serializer, stream, array, context) => { serializer.WriteArrayOfDecimal(array as decimal[], stream);                                    });
			sSerializers.Add(typeof(DateTime[]), (serializer, stream, array, context) => { serializer.WriteArrayOfDateTime(array as DateTime[], stream);                                  });
			sSerializers.Add(typeof(String[]),   (serializer, stream, array, context) => { serializer.WriteArrayOfString(array as string[], stream);                                      });

			// multidimensional arrays
			sMultidimensionalArraySerializers.Add(typeof(SByte),    (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfSByte,   array as Array,  1, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Int16),    (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfInt16,   array as Array,  2, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Int32),    (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfInt32,   array as Array,  4, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Int64),    (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfInt64,   array as Array,  8, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Byte),     (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfByte,    array as Array,  1, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(UInt16),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfUInt16,  array as Array,  2, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(UInt32),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfUInt32,  array as Array,  4, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(UInt64),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfUInt64,  array as Array,  8, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Single),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfSingle,  array as Array,  4, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Double),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfDouble,  array as Array,  8, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Boolean),  (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfBoolean, array as Array,  1, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Char),     (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfPrimitives(PayloadType.MultidimensionalArrayOfChar,    array as Array,  2, stream);   });
			sMultidimensionalArraySerializers.Add(typeof(Decimal),  (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfDecimal(array as Array, stream);                                                      });
			sMultidimensionalArraySerializers.Add(typeof(DateTime), (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfDateTime(array as Array, stream);                                                     });
			sMultidimensionalArraySerializers.Add(typeof(String),   (serializer, stream, array, context) => { serializer.WriteMultidimensionalArrayOfString(array as Array, stream);                                                       });
		}

		/// <summary>
		/// Adds deserializers for types that are supported out of the box.
		/// </summary>
		private static void InitDeserializers()
		{
			// special types
			sDeserializers.Add(PayloadType.NullReference,     (serializer, stream, context) => { return null; });
			sDeserializers.Add(PayloadType.AlreadySerialized, (serializer, stream, context) => { return serializer.ReadAlreadySerializedObject(stream); });
			sDeserializers.Add(PayloadType.Enum,              (serializer, stream, context) => { return serializer.ReadEnum(stream);                    });
			sDeserializers.Add(PayloadType.ArchiveStart,      (serializer, stream, context) => { return serializer.ReadArchive(stream, context);        });

			// simple types
			sDeserializers.Add(PayloadType.SByte,      (serializer, stream, context) => { return serializer.ReadPrimitive_SByte(stream);       });
			sDeserializers.Add(PayloadType.Int16,      (serializer, stream, context) => { return serializer.ReadPrimitive_Int16(stream);       });
			sDeserializers.Add(PayloadType.Int32,      (serializer, stream, context) => { return serializer.ReadPrimitive_Int32(stream);       });
			sDeserializers.Add(PayloadType.Int64,      (serializer, stream, context) => { return serializer.ReadPrimitive_Int64(stream);       });
			sDeserializers.Add(PayloadType.Byte,       (serializer, stream, context) => { return serializer.ReadPrimitive_Byte(stream);        });
			sDeserializers.Add(PayloadType.UInt16,     (serializer, stream, context) => { return serializer.ReadPrimitive_UInt16(stream);      });
			sDeserializers.Add(PayloadType.UInt32,     (serializer, stream, context) => { return serializer.ReadPrimitive_UInt32(stream);      });
			sDeserializers.Add(PayloadType.UInt64,     (serializer, stream, context) => { return serializer.ReadPrimitive_UInt64(stream);      });
			sDeserializers.Add(PayloadType.Single,     (serializer, stream, context) => { return serializer.ReadPrimitive_Single(stream);      });
			sDeserializers.Add(PayloadType.Double,     (serializer, stream, context) => { return serializer.ReadPrimitive_Double(stream);      });
			sDeserializers.Add(PayloadType.Boolean,    (serializer, stream, context) => { return serializer.ReadPrimitive_Boolean(stream);     });
			sDeserializers.Add(PayloadType.Char,       (serializer, stream, context) => { return serializer.ReadPrimitive_Char(stream);        });
			sDeserializers.Add(PayloadType.Decimal,    (serializer, stream, context) => { return serializer.ReadPrimitive_Decimal(stream);     });
			sDeserializers.Add(PayloadType.DateTime,   (serializer, stream, context) => { return serializer.ReadPrimitive_DateTime(stream);    });
			sDeserializers.Add(PayloadType.String,     (serializer, stream, context) => { return serializer.ReadPrimitive_String(stream);      });
			sDeserializers.Add(PayloadType.Object,     (serializer, stream, context) => { return serializer.ReadPrimitive_Object();            });
			sDeserializers.Add(PayloadType.TypeObject, (serializer, stream, context) => { return serializer.ReadTypeObject(stream);            });

			// arrays of simple types (one-dimensional, zero-based indexing)
			sDeserializers.Add(PayloadType.ArrayOfSByte,    (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(sbyte),    1);  });
			sDeserializers.Add(PayloadType.ArrayOfInt16,    (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(short),    2);  });
			sDeserializers.Add(PayloadType.ArrayOfInt32,    (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(int),      4);  });
			sDeserializers.Add(PayloadType.ArrayOfInt64,    (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(long),     8);  });
			sDeserializers.Add(PayloadType.ArrayOfByte,     (serializer, stream, context) => { return serializer.ReadArrayOfByte(stream);                             });
			sDeserializers.Add(PayloadType.ArrayOfUInt16,   (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(ushort),   2);  });
			sDeserializers.Add(PayloadType.ArrayOfUInt32,   (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(uint),     4);  });
			sDeserializers.Add(PayloadType.ArrayOfUInt64,   (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(ulong),    8);  });
			sDeserializers.Add(PayloadType.ArrayOfSingle,   (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(float),    4);  });
			sDeserializers.Add(PayloadType.ArrayOfDouble,   (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(double),   8);  });
			sDeserializers.Add(PayloadType.ArrayOfBoolean,  (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(bool),     1);  });
			sDeserializers.Add(PayloadType.ArrayOfChar,     (serializer, stream, context) => { return serializer.ReadArrayOfPrimitives(stream, typeof(char),     2);  });
			sDeserializers.Add(PayloadType.ArrayOfDecimal,  (serializer, stream, context) => { return serializer.ReadArrayOfDecimal(stream);                          });
			sDeserializers.Add(PayloadType.ArrayOfDateTime, (serializer, stream, context) => { return serializer.ReadDateTimeArray(stream);                           });
			sDeserializers.Add(PayloadType.ArrayOfString,   (serializer, stream, context) => { return serializer.ReadStringArray(stream);                             });
			sDeserializers.Add(PayloadType.ArrayOfObjects,  (serializer, stream, context) => { return serializer.ReadArrayOfObjects(stream, context);                 });

			// multidimensional arrays
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfSByte,    (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(sbyte),    1);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfInt16,    (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(short),    2);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfInt32,    (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(int),      4);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfInt64,    (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(long),     8);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfByte,     (serializer, stream, context) => { return serializer.DeserializeMultidimensionalByteArray(stream);                        });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfUInt16,   (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(ushort),   2);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfUInt32,   (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(uint),     4);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfUInt64,   (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(ulong),    8);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfSingle,   (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(float),    4);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfDouble,   (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(double),   8);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfBoolean,  (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(bool),     1);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfChar,     (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfPrimitives(stream, typeof(char),     2);  });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfDecimal,  (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfDecimal(stream);                          });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfDateTime, (serializer, stream, context) => { return serializer.ReadMultidimensionalDateTimeArray(stream);                           });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfString,   (serializer, stream, context) => { return serializer.ReadMultidimensionalStringArray(stream);                             });
			sDeserializers.Add(PayloadType.MultidimensionalArrayOfObjects,  (serializer, stream, context) => { return serializer.ReadMultidimensionalArrayOfObjects(stream, context);                 });

			// type
			sDeserializers.Add(PayloadType.Type, (serializer, stream, context) => {
				serializer.ReadTypeMetadata(stream);
				return serializer.InnerDeserialize(stream, context);
			});


			// type id
			sDeserializers.Add(PayloadType.TypeId, (serializer, stream, context) => {
				serializer.ReadTypeId(stream);
				return serializer.InnerDeserialize(stream, context);
			});
		}

		/// <summary>
		/// Registers external object serializers provided along with the library.
		/// </summary>
		private static void RegisterExternalObjectSerializers()
		{
			RegisterExternalObjectSerializer(typeof(GuidSerializer));
			RegisterExternalObjectSerializer(typeof(ListTSerializer));
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

			// serializer specific
			mSerializedTypeIdTable = new Dictionary<Type, uint>();
			mSerializedTypeVersionTable = new SerializerVersionTable();
			mSerializedObjectIdTable = new Dictionary<object,uint>(new IdentityComparer<object>());

			// deserializer specific
			mIsVersionTolerant = sIsVersionTolerantDefault;
			mDeserializedTypeIdTable = new Dictionary<uint, TypeItem>();
			mDeserializedObjectIdTable = new Dictionary<uint,object>();

			// init the serializer/deserializer
			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Serializer"/> class.
		/// </summary>
		/// <param name="versions">Table defining requested serializer versions for specific types.</param>
		public Serializer(SerializerVersionTable versions)
		{
			// serializer specific
			mSerializedTypeIdTable = new Dictionary<Type, uint>();
			mSerializedTypeVersionTable = versions;
			mSerializedObjectIdTable = new Dictionary<object, uint>(new IdentityComparer<object>());

			// deserializer specific
			mIsVersionTolerant = sIsVersionTolerantDefault;
			mDeserializedTypeIdTable = new Dictionary<uint, TypeItem>();
			mDeserializedObjectIdTable = new Dictionary<uint, object>();

			// init the serializer/deserializer
			Reset();
		}

		#endregion

		#region Registering External Object Serializers

		/// <summary>
		/// Registers an external object serializer for use with the serializer.
		/// </summary>
		/// <param name="type">The external object serializer class.</param>
		/// <exception cref="ArgumentNullException">The specified type is null.</exception>
		/// <exception cref="ArgumentException">The specified type is not a valid external object serializer.</exception>
		/// <remarks>
		/// This method dynamically creates delegates that handle serialization and deserialization using the specified
		/// external object serializer.
		/// </remarks>
		public static void RegisterExternalObjectSerializer(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (!type.IsClass) throw new ArgumentException("An external object serializer must be a class.");

			Init();

			object[] attributes = type.GetCustomAttributes(typeof(ExternalObjectSerializerAttribute), false);
			bool attributeOk = attributes.Length > 0;
			bool interfaceOk = typeof(IExternalObjectSerializer).IsAssignableFrom(type);

			if (attributeOk && interfaceOk)
			{
				// class is annotated with the external object serializer attribute and implements the appropriate interface
				lock (sSync)
				{
					foreach (ExternalObjectSerializerAttribute attribute in attributes)
					{
						// create a copy of the serializer delegate dictionary and add a new serializer for type to register
						IExternalObjectSerializer eos = FastActivator.CreateInstance(type) as IExternalObjectSerializer;
						SerializerDelegate serializer = CreateExternalObjectSerializer(attribute.TypeToSerialize, eos, attribute.Version);
						Dictionary<Type,SerializerDelegate> serDictCopy = new Dictionary<Type,SerializerDelegate>(sSerializers);
						serDictCopy[attribute.TypeToSerialize] = serializer;

						// create a copy of the dictionary mapping types to serialize to external object serializers
						Dictionary<Type, ExternalObjectSerializerInfo> eosDictCopy = new Dictionary<Type,ExternalObjectSerializerInfo>(sExternalObjectSerializersBySerializee);
						eosDictCopy[attribute.TypeToSerialize] = new ExternalObjectSerializerInfo(eos, attribute.Version);

						Thread.MemoryBarrier();
						sSerializers = serDictCopy;
						sExternalObjectSerializersBySerializee = eosDictCopy;
					}
				}

				return;
			}

			if (!attributeOk)
			{
				// attribute is missing
				sLog.Write(
					LogLevel.Error,
					"Class '{0}' seems to be an external serializer class, but it is not annotated with the '{1}' attribute.",
					type.FullName, typeof(ExternalObjectSerializerAttribute).FullName);
			}

			if (!interfaceOk)
			{
				// interface is missing
				sLog.Write(
					LogLevel.Error,
					"Class '{0}' seems to be an external serializer class, but does not implement the '{1}' interface.",
					type.FullName, typeof(IExternalObjectSerializer).FullName);
			}

			throw new ArgumentException(string.Format(
				"The specified type ({0}) is not annotated with the '{1}' attribute or does not implement the '{2}' interface.",
				type.FullName, typeof(ExternalObjectSerializerAttribute).FullName, typeof(IExternalObjectSerializer).FullName));
		}

		#endregion

		#region Type Serialization

		/// <summary>
		/// Serializes metadata about a type.
		/// </summary>
		/// <param name="stream">Stream to serialize the type to.</param>
		/// <param name="type">Type to serialize.</param>
		internal void WriteTypeMetadata(Stream stream, Type type)
		{
			// abort if the type has not changed to avoid blowing the stream up
			if (type == mCurrentSerializedType) {
				return;
			}

			uint id;
			if (mSerializedTypeIdTable.TryGetValue(type, out id))
			{
				// the type was serialized before
				// => write type id only
				WriteTypeId(stream, id);
			}
			else
			{
				// its the first time this type is serialized
				// => write type metadata
				mSerializedTypeIdTable.Add(type, mNextSerializedTypeId++);
				string name = type.AssemblyQualifiedName;

				// convert type name
				int size = Encoding.UTF8.GetMaxByteCount(name.Length);
				if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
				int byteCount = Encoding.UTF8.GetBytes(name, 0, name.Length, mTempBuffer_BigBuffer, 0);

				// write header
				mTempBuffer_Buffer[0] = (byte)PayloadType.Type;
				int count = LEB128.Write(mTempBuffer_Buffer, 1, byteCount);
				stream.Write(mTempBuffer_Buffer, 0, 1+count);

				// write type name
				stream.Write(mTempBuffer_BigBuffer, 0, byteCount);
			}

			mCurrentSerializedType = type;
		}

		/// <summary>
		/// Writes a type id for already serialized type metadata.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="id">Type id that was assigned to the type.</param>
		private void WriteTypeId(Stream stream, uint id)
		{
			mTempBuffer_Buffer[0] = (byte)PayloadType.TypeId;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, id);
			stream.Write(mTempBuffer_Buffer, 0, 1+count);
		}

		/// <summary>
		/// Reads metadata about a type.
		/// </summary>
		/// <param name="stream">Stream to read type metadata from.</param>
		/// <exception cref="SerializationException">The stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Type specified in the stream could not be loaded.</exception>
		private void ReadTypeMetadata(Stream stream)
		{
			// read number of characters in the following string
			int length = LEB128.ReadInt32(stream);

			// read type full name
			byte[] array = new byte[length];
			int bytesRead = stream.Read(array, 0, length);
			if (bytesRead < length) throw new SerializationException("Unexpected end of stream.");
			string typename = Encoding.UTF8.GetString(array, 0, length);

			// try to get the type name from the type cache
			Type type;
			TypeItem typeItem;
			if (sTypeTable.TryGetValue(typename, out type))
			{
				// assign a type id
				typeItem = new TypeItem(typename, type);
				mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
			}
			else 
			{
				type = Type.GetType(typename, ResolveAssembly, null, false);
				if (type != null)
				{
					// remember the determined name-to-type mapping
					lock (sTypeTableLock)
					{
						if (!sTypeTable.ContainsKey(typename))
						{
							Dictionary<string, Type> copy = new Dictionary<string,Type>(sTypeTable);
							copy.Add(typename, type);
							Thread.MemoryBarrier();
							sTypeTable = copy;
						}
					}

					// assign a type id if the serializer uses assembly and type ids
					typeItem = new TypeItem(typename, type);
					mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
				}
				else
				{
					// type is unknown

					sLog.Write(
						LogLevel.Error,
						"Stream contains an unknown type (type: {0}). Aborting deserialization...",
						typename);

					throw new SerializationException("Unable to load type " + typename + ".");
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
			uint id = LEB128.ReadUInt32(stream);

			TypeItem item;
			if (mDeserializedTypeIdTable.TryGetValue(id, out item)) {
				mCurrentDeserializedType = item;
			} else {
				throw new SerializationException("Deserialized type id that does not match a previously deserialized type.");
			}
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
		/// This method resets the serializer clearing the mapping of assemblys and
		/// types to ids. The table used for detecting already serialized objects is
		/// cleared, too. The table containing information about requested serializer
		/// versions is NOT cleared.
		/// </remarks>
		public void ResetSerializer()
		{
			mCurrentSerializedType = null;
			mSerializedTypeIdTable.Clear();
			mNextSerializedTypeId = 0;
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
		/// This method resets the deserializer clearing the mapping of ids to assemblys and
		/// types. The table mapping object ids to already deserialiezd objects is cleared, too.
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
		/// Gets the default value indicating whether the serializer allows to resolve a full assembly name during deserialization
		/// to a different assembly with the same name, but a different version number.
		/// </summary>
		public static bool IsVersionTolerantDefault
		{
			get { return sIsVersionTolerantDefault; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the serializer allows to resolve a full assembly name during deserialization
		/// to a different assembly with the same name, but a different version number.
		/// </summary>
		public bool IsVersionTolerant
		{
			get { return mIsVersionTolerant; }
			set { mIsVersionTolerant = value; }
		}

		#endregion

		#region Serialization

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
			ResetSerializer();
			InnerSerialize(stream, obj, context);
		}

		/// <summary>
		/// Performs the actual serialization.
		/// </summary>
		/// <param name="stream">Stream to serialize to.</param>
		/// <param name="obj">Object to serialize.</param>
		/// <param name="context">Context object to pass to the serializer of the object that is serialized next.</param>
		internal void InnerSerialize(Stream stream, object obj, object context)
		{
			uint id;
			SerializerDelegate serializer;

			// a null reference?
			if (obj == null) {
				stream.WriteByte((byte)PayloadType.NullReference);
				return;
			}

			// get the type of the type to serialize
			Type type = obj.GetType();

			if (type.IsValueType)
			{
				// a value type
				// => every object is unique (checking for for already serialized objects doesn't make sense)

				// check whether the serializer knows how to serialize the object
				if (sSerializers.TryGetValue(type, out serializer))
				{
					serializer(this, stream, obj, context);
					return;
				}
				else
				{
					if (type.IsEnum)
					{
						// an enumeration value
						// => create a serializer delegate that handles it
						lock (sSync)
						{
							if (!sSerializers.TryGetValue(type, out serializer))
							{
								serializer = GetEnumSerializer(type);
								Dictionary<Type, SerializerDelegate> copy = new Dictionary<Type,SerializerDelegate>(sSerializers);
								copy[type] = serializer;
								Thread.MemoryBarrier();
								sSerializers = copy;
							}
						}

						serializer(this, stream, obj, context);
						return;
					}
				}
			}
			else
			{
				// a reference type

				if (mSerializedObjectIdTable.TryGetValue(obj, out id))
				{
					// the object is already serialized
					// => write object id only
					SerializeObjectId(stream, id);
					return;
				}
				else if (sSerializers.TryGetValue(type, out serializer))
				{
					serializer(this, stream, obj, context);
					return;
				}
				else if (type.IsArray)
				{
					Array array = obj as Array;
					if (type.GetArrayRank() == 1 && array.GetLowerBound(0) == 0)
					{
						// an SZARRAY
						WriteArrayOfObjects(array, stream);
						return;
					}
					else
					{
						// an MDARRAY
						if (sMultidimensionalArraySerializers.TryGetValue(type.GetElementType(), out serializer)) {
							serializer(this, stream, array, context);
						} else {
							WriteMultidimensionalArrayOfObjects(array, stream);
						}
						return;
					}
				}
				else if (type.IsInstanceOfType(typeof(Type)))
				{
					SerializeTypeObject(stream, obj as Type);
					mSerializedObjectIdTable.Add(obj, mNextSerializedObjectId++);
					return;
				}
				else if (type.IsGenericType)
				{
					Type genericTypeDefinition = type.GetGenericTypeDefinition();
					if (sSerializers.TryGetValue(genericTypeDefinition, out serializer))
					{
						ExternalObjectSerializerInfo eosi = sExternalObjectSerializersBySerializee[genericTypeDefinition];
						serializer = CreateExternalObjectSerializer(type, eosi.Serializer, eosi.Version);

						lock (sSync)
						{
							Dictionary<Type,SerializerDelegate> serDictCopy = new Dictionary<Type,SerializerDelegate>(sSerializers);
							serDictCopy[type] = serializer;

							// create a copy of the dictionary mapping types to serialize to external object serializers
							Dictionary<Type, ExternalObjectSerializerInfo> eosDictCopy = new Dictionary<Type,ExternalObjectSerializerInfo>(sExternalObjectSerializersBySerializee);
							eosDictCopy[type] = new ExternalObjectSerializerInfo(eosi.Serializer, eosi.Version);

							Thread.MemoryBarrier();
							sSerializers = serDictCopy;
							sExternalObjectSerializersBySerializee = eosDictCopy;
						}

						serializer(this, stream, obj, context);
						return;
					}
				}
			}

			// try to use an internal object serializer
			uint currentInternalSerializerVersion;
			IInternalObjectSerializer ios = GetInternalObjectSerializer(obj, out currentInternalSerializerVersion);
			if (ios != null)
			{
				// a struct that has an internal object serializer
				// => create a serializer delegate that handles it
				lock (sSync)
				{
					if (!sSerializers.TryGetValue(type, out serializer))
					{
						serializer = CreateInternalObjectSerializer(type);
						Dictionary<Type, SerializerDelegate> copy = new Dictionary<Type,SerializerDelegate>(sSerializers);
						copy[type] = serializer;
						Thread.MemoryBarrier();
						sSerializers = copy;
					}
				}

				serializer(this, stream, obj, context);
				return;
			}

			// object cannot be serialized
			string error = string.Format("Type '{0}' cannot be serialized, consider implementing an internal or external object serializer for this type.", obj.GetType().FullName);
			throw new SerializationException(error);
		}

		#endregion

		#region Serialization of Type Objects

		/// <summary>
		/// Serializes a type object.
		/// </summary>
		/// <param name="stream">Stream to serialize the type object to.</param>
		/// <param name="type">Type object to serialize.</param>
		private void SerializeTypeObject(Stream stream, Type type)
		{
			// write type metadata
			WriteTypeMetadata(stream, type.GetType());

			// tell the deserializer that an enum follows
			stream.WriteByte((byte)PayloadType.TypeObject);

			// write type
			// ----------------------------------------------------------------------------------------
			uint id;
			if (mSerializedTypeIdTable.TryGetValue(type, out id))
			{
				// the type was already serialized
				// => serialize type id only.
				mTempBuffer_Buffer[0] = (byte)PayloadType.TypeId;
				int count = LEB128.Write(mTempBuffer_Buffer, 1, id);
				stream.Write(mTempBuffer_Buffer, 0, 1+count);
			}
			else
			{
				// it's the first time this type is serialized
				// => serialize type metadata
				mSerializedTypeIdTable.Add(type, mNextSerializedTypeId++);
				string typeName = type.AssemblyQualifiedName;

				// convert type name
				int size = Encoding.UTF8.GetMaxByteCount(typeName.Length);
				if (mTempBuffer_BigBuffer.Length < size) mTempBuffer_BigBuffer = new byte[size];
				int byteCount = Encoding.UTF8.GetBytes(typeName, 0, typeName.Length, mTempBuffer_BigBuffer, 0);

				// write header
				mTempBuffer_Buffer[0] = (byte)PayloadType.Type;
				int count = LEB128.Write(mTempBuffer_Buffer, 1, byteCount);
				stream.Write(mTempBuffer_Buffer, 0, 1+count);

				// write assembly name
				stream.Write(mTempBuffer_BigBuffer, 0, byteCount);
			}
		}

		#endregion

		#region Serialization of Object Ids

		/// <summary>
		/// Serializes the id of an object that was already serialized.
		/// </summary>
		/// <param name="stream">Stream to serialize the object id to.</param>
		/// <param name="id">Object id.</param>
		private void SerializeObjectId(Stream stream, uint id)
		{
			mTempBuffer_Buffer[0] = (byte)PayloadType.AlreadySerialized;
			int count = LEB128.Write(mTempBuffer_Buffer, 1, id);
			stream.Write(mTempBuffer_Buffer, 0, 1+count);
		}

		#endregion

		#region Deserialization

		/// <summary>
		/// Deserializes an object from a stream.
		/// </summary>
		/// <param name="stream">Stream to deserialize the object from.</param>
		/// <param name="context">Context object to pass to the serializer of the expected object (may be null).</param>
		/// <returns>Deserialized object.</returns>
		/// <exception cref="VersionNotSupportedException">The serializer version of one of the objects in the specified stream is not supported.</exception>
		/// <exception cref="SerializationException">Deserializing the object failed (the exception object contains a message describing the reason why deserialization has failed).</exception>
		public object Deserialize(Stream stream, object context = null)
		{
			ResetDeserializer();
			return InnerDeserialize(stream, context);
		}

		/// <summary>
		/// Deserializes an object from a stream (internal).
		/// </summary>
		/// <param name="stream">Stream to deserialize an object from.</param>
		/// <param name="context">Context to pass to the next serializer via the serializer archive.</param>
		/// <returns>Deserialized object.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Deserialization failed due to an invalid stream of bytes.</exception>
		internal object InnerDeserialize(Stream stream, object context)
		{
			int byteRead = stream.ReadByte();
			if (byteRead < 0) throw new SerializationException("Unexpected end of stream.");
			PayloadType objType = (PayloadType)byteRead;

			// try to use a deserializer
			DeserializerDelegate deserializer;
			if (sDeserializers.TryGetValue(objType, out deserializer)) {
				return deserializer(this, stream, context);
			}

			// unknown object type
			Debug.Assert(false);
			return null;
		}

		#region Deserialization of Enumerations

		/// <summary>
		/// Reads an enumeration value.
		/// </summary>
		/// <param name="stream">Stream to read the enumeration value from.</param>
		/// <returns>The read enumeration value.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Deserialized value does not match the underlying type of the enumeration.</exception>
		private object ReadEnum(Stream stream)
		{
			// assembly and type metadata have been read already

			// get/create an enum caster that converts a 64bit integer (long) into a properly typed enumeration value
			EnumCasterDelegate caster;
			if (!sEnumCasters.TryGetValue(mCurrentDeserializedType.Type, out caster))
			{
				lock (sSync)
				{
					if (!sEnumCasters.TryGetValue(mCurrentDeserializedType.Type, out caster))
					{
						caster = CreateEnumCaster(mCurrentDeserializedType.Type);
						Dictionary<Type, EnumCasterDelegate> copy = new Dictionary<Type, EnumCasterDelegate>(sEnumCasters);
						copy[mCurrentDeserializedType.Type] = caster;
						Thread.MemoryBarrier();
						sEnumCasters = copy;
					}
				}
			}

			long value = LEB128.ReadInt64(stream);
			return caster(value);
		}

		/// <summary>
		/// Creates a method that casts an integer to the specified enumeration type.
		/// </summary>
		/// <param name="type">Enumeration type to cast an integer to.</param>
		/// <returns>A caster delegate.</returns>
		private static EnumCasterDelegate CreateEnumCaster(Type type)
		{
			// create a deserializer delegate that handles the specified type
			int enumCaster_id = Interlocked.Increment(ref sEnumCasterId);
			string name = "_enumCaster" + enumCaster_id;
			Type[] parameterTypes = { typeof(long) };
			ParameterExpression parameterExpression = parameterTypes.Select(x => Expression.Parameter(x)).First();
			Expression body = Expression.Convert(Expression.Convert(parameterExpression, type), typeof(object));
			LambdaExpression lambda = Expression.Lambda(typeof(EnumCasterDelegate), body, parameterExpression);
			return (EnumCasterDelegate)lambda.Compile();
		}

		#endregion

		#region Deserialization of Type Objects

		/// <summary>
		/// Deserializes a type object.
		/// </summary>
		/// <param name="stream">Stream to deserialize the type object from.</param>
		/// <returns>Type object.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		private object ReadTypeObject(Stream stream)
		{
			TypeItem typeItem;

			// read type
			// -----------------------------------------------------------------------------------------------

			int byteRead = stream.ReadByte();
			if (byteRead < 0) throw new SerializationException("Unexpected end of stream.");
			PayloadType objType = (PayloadType)byteRead;

			if (objType == PayloadType.Type)
			{
				// read number of characters in the following string
				int length = LEB128.ReadInt32(stream);

				// read type full name
				byte[] array = new byte[length];
				int bytesRead = stream.Read(array, 0, length);
				if (bytesRead < length) throw new SerializationException("Unexpected end of stream.");
				string typename = Encoding.UTF8.GetString(array, 0, length);

				// try to get the type name from the type cache
				Type type;
				if (sTypeTable.TryGetValue(typename, out type))
				{
					// assign a type id if the serializer uses assembly and type ids
					typeItem = new TypeItem(typename, type);
					mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
				}
				else
				{
					type = Type.GetType(typename, ResolveAssembly, null, false);

					if (type == null) {
						throw new SerializationException("Unable to load type " + typename + ".");
					}

					// remember the determined name-to-type mapping
					lock (sTypeTableLock)
					{
						Dictionary<string,Type> copy = new Dictionary<string,Type>(sTypeTable);
						copy[typename] = type;
						Thread.MemoryBarrier();
						sTypeTable = copy;
					}

					// assign a type id
					typeItem = new TypeItem(typename, type);
					mDeserializedTypeIdTable.Add(mNextDeserializedTypeId++, typeItem);
				}
			}
			else if (objType == PayloadType.TypeId)
			{
				uint id = LEB128.ReadUInt32(stream);
				if (!mDeserializedTypeIdTable.TryGetValue(id, out typeItem)) {
					throw new SerializationException("Deserialized type id that does not match a previously deserialized type.");
				}
			}
			else
			{
				string error = string.Format("Expected 'Type' or 'TypeId' in the stream, but got payload id '{0}'.", objType);
				throw new SerializationException(error);
			}

			mDeserializedObjectIdTable.Add(mNextDeserializedObjectId++, typeItem.Type);
			return typeItem.Type;
		}

		#endregion

		#region Deserialization of Already Serialized Objects

		/// <summary>
		/// Gets an already deserialized object from its object id stored in the stream.
		/// </summary>
		/// <param name="stream">Stream to deserialize the object from.</param>
		/// <returns>Deserialized object (from the deserialized object table).</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Deserialized object id does not correspond to a previously deserialized object.</exception>
		private object ReadAlreadySerializedObject(Stream stream)
		{
			object obj;
			uint id = LEB128.ReadUInt32(stream);
			if (mDeserializedObjectIdTable.TryGetValue(id, out obj)) {
				return obj;
			} else {
				string error = "Invalid object id detected.";
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}
		}

		#endregion

		#region Deserialization of Serialization Archives

		/// <summary>
		/// Deserializes an object from an archive.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <param name="context">Context to pass to the serializer via the serializer archive.</param>
		/// <returns>Deserialized object.</returns>
		/// <exception cref="SerializationException">Stream ended unexpectedly.</exception>
		/// <exception cref="SerializationException">Object serializer version of the object that is about to be deserialized is higher than the max. supported version of the available serializer.</exception>
		/// <exception cref="SerializationException">There is no internal/external object serializer for the object that is about to be deserialized.</exception>
		private object ReadArchive(Stream stream, object context)
		{
			uint deserializedVersion = LEB128.ReadUInt32(stream);
			uint currentVersion;
			string error;

			#region External Object Serializer
			// try to get an external object serializer for exactly the deserialized type, fallback to a serializer for a generic type, if available...
			ExternalObjectSerializerInfo eosi;
			if (!sExternalObjectSerializersBySerializee.TryGetValue(mCurrentDeserializedType.Type, out eosi)) {
				if (mCurrentDeserializedType.Type.IsGenericType) {
					sExternalObjectSerializersBySerializee.TryGetValue(mCurrentDeserializedType.Type.GetGenericTypeDefinition(), out eosi);
				}
			}

			if (eosi != null)
			{
				currentVersion = eosi.Version;

				if (deserializedVersion > currentVersion) {
					// version of the archive that is about to be deserialized is greater than
					// the version the internal object serializer supports
					error = string.Format("Deserializing type '{0}' failed due to a version conflict (got version: {1}, max. supported version: {2}).", mCurrentDeserializedType.Type.FullName, deserializedVersion, currentVersion);
					sLog.Write(LogLevel.Failure, error);
					throw new SerializationException(error);
				}

				// version is ok, deserialize...
				SerializerArchive archive = new SerializerArchive(this, stream, mCurrentDeserializedType.Type, deserializedVersion, context);
				object obj = eosi.Serializer.Deserialize(archive);
				archive.Close();

				// read and check archive end
				ReadAndCheckPayloadType(stream, PayloadType.ArchiveEnd);
				return obj;
			}
			#endregion

			#region Internal Object Serializer

			if (HasInternalObjectSerializer(mCurrentDeserializedType.Type, out currentVersion))
			{
				if (deserializedVersion > currentVersion) {
					// version of the archive that is about to be deserialized is greater than
					// the version the internal object serializer supports
					error = string.Format("Deserializing type '{0}' failed due to a version conflict (got version: {1}, max. supported version: {2}).", mCurrentDeserializedType.Type.FullName, deserializedVersion, currentVersion);
					sLog.Write(LogLevel.Failure, error);
					throw new SerializationException(error);
				}

				// version is ok, deserialize...
				SerializerArchive archive = new SerializerArchive(this, stream, mCurrentDeserializedType.Type, deserializedVersion, context);
				object obj = FastActivator.CreateInstance<SerializerArchive>(mCurrentDeserializedType.Type, archive);
				archive.Close();

				// read and check archive end
				ReadAndCheckPayloadType(stream, PayloadType.ArchiveEnd);
				return obj;
			}

			#endregion

			error = string.Format("Deserializing type '{0}' failed because it lacks an internal/external object serializer.", mCurrentDeserializedType.Type.FullName);
			sLog.Write(LogLevel.Failure, error);
			throw new SerializationException(error);
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
			int readbyte = stream.ReadByte();
			if (readbyte < 0) throw new SerializationException("Stream ended unexpectedly.");
			if (readbyte != (int)type) {
				StackTrace trace = new StackTrace();
				string error = string.Format("Unexpected payload type during deserialization. Stack Trace:\n{0}", trace.ToString());
				sLog.Write(LogLevel.Failure, error);
				throw new SerializationException(error);
			}
		}

		#endregion

		#region Assembly and Type Resolution (Version Tolerant Serialization)

		/// <summary>
		/// Is called to resolve the assembly name read during deserialization to the correct assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to resolve.</param>
		/// <returns>The resolved assembly.</returns>
		private Assembly ResolveAssembly(AssemblyName assemblyName)
		{
			// ----------------------------------------------------------------------------------------------------------------
			// check whether the assembly has been resolved before
			// ----------------------------------------------------------------------------------------------------------------
			Assembly assembly;
			if (sAssemblyTable.TryGetValue(assemblyName.FullName, out assembly))
			{
				if (!mIsVersionTolerant && assembly.FullName != assemblyName.FullName)
				{
					throw new SerializationException(
						"Resolving full name of assembly ({0}) failed. Resolution to assembly ({1}) exists, but the serializer is configured to be version-intolerant.",
						assemblyName.FullName, assembly.FullName);
				}

				return assembly;
			}

			sLog.Write(
				LogLevel.Developer,
				"Trying to load assembly by its full name ({0}).",
				assemblyName.FullName);

			// ----------------------------------------------------------------------------------------------------------------
			// try to load the assembly by its full name
			// (searches the application base directory, its private directories and the Global Assembly Cache (GAC))
			// ----------------------------------------------------------------------------------------------------------------
			try
			{
				assembly = Assembly.Load(assemblyName);

				sLog.Write(
					LogLevel.Developer,
					"Loading assembly by its full name ({0}) succeeded.",
					assemblyName.FullName);

				KeepAssemblyResolution(assemblyName.FullName, assembly);
				return assembly;
			}
			catch (Exception ex)
			{
				if (mIsVersionTolerant)
				{
					sLog.Write(
						LogLevel.Developer,
						"Loading assembly by its full name ({0}) failed. Trying to load assembly allowing a different version.\nException: {1}",
						assemblyName.FullName, ex.ToString());
				}
				else
				{
					sLog.Write(
						LogLevel.Error,
						"Loading assembly by its full name ({0}) failed.\nException: {1}",
						assemblyName.FullName, ex.ToString());

					throw;
				}
			}

			// ----------------------------------------------------------------------------------------------------------------
			// try to load assembly from the application's base directory
			// (ignores version information)
			// ----------------------------------------------------------------------------------------------------------------

			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".dll");

			sLog.Write(
				LogLevel.Developer,
				"Trying to load assembly ({0}) from file ({1}).",
				assemblyName.Name, path);

			try
			{
				assembly = Assembly.LoadFrom(path);

				sLog.Write(
					LogLevel.Developer,
					"Loading assembly ({0}) from file ({1}) succeeded.",
					assemblyName.Name, path);

				KeepAssemblyResolution(assemblyName.FullName, assembly);
				return assembly;
			}
			catch (FileNotFoundException)
			{
				// try next...
			}
			catch (Exception ex)
			{
				sLog.Write(
					LogLevel.Error,
					"Loading assembly ({0}) from file ({1}) failed.\nException: {2}",
					assemblyName.Name, path, ex.ToString());

				throw;
			}

			// ----------------------------------------------------------------------------------------------------------------
			// try to load assembly from the private bin path
			// (ignores version information)
			// ----------------------------------------------------------------------------------------------------------------

			if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath))
			{
				path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath, assemblyName.Name);

				sLog.Write(
					LogLevel.Developer,
					"Trying to load assembly ({0}) from ({1}).",
					assemblyName.Name, path);

				try
				{
					assembly = Assembly.LoadFrom(path);

					sLog.Write(
						LogLevel.Developer,
						"Loading assembly ({0}) from file ({1}) succeeded.",
						assemblyName.Name, path);

					KeepAssemblyResolution(assemblyName.FullName, assembly);
					return assembly;
				}
				catch (FileNotFoundException)
				{
					// try next...
				}
				catch (Exception ex)
				{
					sLog.Write(
						LogLevel.Error,
						"Loading assembly ({0}) from file ({1}) failed.\nException: {2}",
						assemblyName.FullName, path, ex.ToString());

					throw;
				}
			}

			sLog.Write(
				LogLevel.Error,
				"Resolving name of assembly ({0}) failed. File could not be found.",
				assemblyName.FullName, path);

			return null;
		}

		/// <summary>
		/// Keeps the result of an assembly resolution in the serializer's assembly cache.
		/// </summary>
		/// <param name="assemblyFullName">Full name of the assembly (as read during deserialization).</param>
		/// <param name="assembly">Assembly to use, if the specified assembly name is encountered.</param>
		private static void KeepAssemblyResolution(string assemblyFullName, Assembly assembly)
		{
			lock (sAssemblyTableLock)
			{
				if (!sAssemblyTable.ContainsKey(assemblyFullName))
				{
					Dictionary<string, Assembly> copy = new Dictionary<string,Assembly>(sAssemblyTable);
					copy.Add(assemblyFullName, assembly);
					Thread.MemoryBarrier();
					sAssemblyTable = copy;
				}
			}
		}

		/// <summary>
		/// Is called to resolve the type name read during deserialization to the correct <see cref="Type"/> object.
		/// </summary>
		/// <param name="assembly">Assembly that contains the type.</param>
		/// <param name="name">Name of the type to resolve.</param>
		/// <param name="caseInsensitive">
		/// true to perform a case-insensitive search;
		/// false to perform a case-sensitive search.
		/// </param>
		/// <returns>The resolved type; null, if the type could not be resolved.</returns>
		private static Type ResolveType(Assembly assembly, string name, bool caseInsensitive)
		{
			try
			{
				return assembly.GetType(name);
			}
			catch (Exception ex)
			{
				sLog.Write(
					LogLevel.Error,
					"Resolving type name ({0}) in assembly ({1}) failed. Exception:\n{2}",
					name, assembly.FullName, ex.ToString());

				return null;
			}
		}

		#endregion

		#endregion

		#region Info

		/// <summary>
		/// Gets the maximum version a serializer of the specified type supports.
		/// </summary>
		/// <param name="type">Type to check for.</param>
		/// <returns>Maximum version the serializer supports.</returns>
		/// <exception cref="ArgumentException">Type is not serializable.</exception>
		/// <remarks>
		/// The maximum version a serializer supports is taken from the attribute assigned to the serializer.
		/// </remarks>
		public static uint GetSerializerVersion(Type type)
		{
			Init();

			uint version;

			if (HasInternalObjectSerializer(type, out version)) {
				return version;
			}

			if (HasExternalObjectSerializer(type, out version)) {
				return version;
			}

			string error = string.Format("Specified type ({0}) is not serializable.", type.FullName);
			throw new ArgumentException(error, nameof(type));
		}

		#endregion

		#region Convenience: Copying Serializable Objects

		/// <summary>
		/// Copies a serializable object once.
		/// </summary>
		/// <param name="obj">Object to copy.</param>
		/// <returns>Copy of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static object CopySerializableObject(object obj)
		{
			return CopySerializableObject(obj, null, null);
		}

		/// <summary>
		/// Copies a serializable object once.
		/// </summary>
		/// <param name="obj">Object to copy.</param>
		/// <param name="serializationContext">Serialization context to use.</param>
		/// <param name="deserializationContext">Serialization context to use.</param>
		/// <returns>Copy of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static object CopySerializableObject(object obj, object serializationContext, object deserializationContext)
		{
			Init();

			// abort if the object to copy is null
			if (obj == null) {
				return null;
			}

			Type type = obj.GetType();
			if (type.IsPrimitive || type.IsEnum)
			{
				// primitive types and enums are value types and can be copied by assigning
				return obj;
			}
			else if (type == typeof(string) || type == typeof(DateTime))
			{
				// - strings are immutable and do not have to be copied
				// - System.DateTime is a struct and does not reference other objects
				return obj;
			}
			else
			{
				MemoryBlockStream mbs = new MemoryBlockStream();
				Serializer serializer = new Serializer();
				object copy;

				try
				{
					serializer.Serialize(mbs, obj, serializationContext);
					mbs.Position = 0;
					copy = serializer.Deserialize(mbs, deserializationContext);
				}
				finally
				{
					mbs.Dispose();
				}

				return copy;
			}
		}

		/// <summary>
		/// Copies a serializable object once.
		/// </summary>
		/// <typeparam name="T">Type of the value to copy.</typeparam>
		/// <param name="obj">Object to copy.</param>
		/// <returns>Copy of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static T CopySerializableObject<T>(T obj)
		{
			return CopySerializableObject<T>(obj, null, null);
		}

		/// <summary>
		/// Copies a serializable object once.
		/// </summary>
		/// <typeparam name="T">Type of the value to copy.</typeparam>
		/// <param name="obj">Object to copy.</param>
		/// <param name="serializationContext">Serialization context to use.</param>
		/// <param name="deserializationContext">Serialization context to use.</param>
		/// <returns>Copy of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static T CopySerializableObject<T>(T obj, object serializationContext, object deserializationContext)
		{
			Init();

			// abort if the object to copy is null
			if (obj == null) {
				return default(T);
			}

			Type type = typeof(T);
			if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime))
			{
				// - primitive types and enums are value types and can be copied by assigning
				// - strings are immutable and do not have to be copied
				// - System.DateTime is a struct and does not reference other objects
				return obj;
			}
			else
			{
				// other objects
				MemoryBlockStream mbs = new MemoryBlockStream();
				Serializer serializer = new Serializer();
				object copy;

				try
				{
					serializer.Serialize(mbs, obj, serializationContext);
					mbs.Position = 0;
					copy = serializer.Deserialize(mbs, deserializationContext);
				}
				finally
				{
					mbs.Dispose();
				}

				return (T)copy;
			}
		}

		/// <summary>
		/// Copies a serializable object multiple times.
		/// </summary>
		/// <typeparam name="T">Type of the value to copy.</typeparam>
		/// <param name="obj">Object to copy.</param>
		/// <param name="count">Number of copies to make.</param>
		/// <returns>Copies of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static T[] CopySerializableObject<T>(T obj, int count)
		{
			return CopySerializableObject<T>(obj, null, null, count);
		}

		/// <summary>
		/// Copies a serializable object multiple times.
		/// </summary>
		/// <typeparam name="T">Type of the value to copy.</typeparam>
		/// <param name="obj">Object to copy.</param>
		/// <param name="count">Number of copies to make.</param>
		/// <param name="serializationContext">Serialization context to use.</param>
		/// <param name="deserializationContext">Serialization context to use.</param>
		/// <returns>Copy of the specified object.</returns>
		/// <exception cref="SerializationException">Serializing/deserializing failed due to some reason (see exception message for further details).</exception>
		public static T[] CopySerializableObject<T>(T obj, object serializationContext, object deserializationContext, int count)
		{
			Init();

			// abort if the object to copy is null or if count is zero
			T[] copies = new T[count];
			if (obj == null || count == 0) {
				for (int i = 0; i < count; i++) {
					copies[i] = default(T);
				}
				return copies;
			}

			Type type = typeof(T);
			if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime))
			{
				// - primitive types and enums are value types and can be copied by assigning
				// - strings are immutable and do not have to be copied
				// - System.DateTime is a struct and does not reference other objects
				for (int i = 0; i < count; i++) {
					copies[i] = obj;
				}
				return copies;
			}
			else
			{
				// other objects
				MemoryBlockStream mbs = new MemoryBlockStream();
				Serializer serializer = new Serializer();

				try
				{
					serializer.Serialize(mbs, obj, serializationContext);
					for (int i = 0; i < count; i++) {
						mbs.Position = 0;
						copies[i] = (T )serializer.Deserialize(mbs, deserializationContext);
					}
				}
				finally
				{
					mbs.Dispose();
				}

				return copies;
			}
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
			Serializer serializer = new Serializer();
			using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)) {
				serializer.Serialize(fs, obj, context);
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
			Serializer serializer = new Serializer();
			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				return (T)serializer.Deserialize(fs, context);
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Checks whether the specified object is serializable.
		/// </summary>
		/// <param name="obj">Object to check.</param>
		/// <returns>true, if the object is serializable; otherwise false.</returns>
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
		/// <returns>true, if the type is serializable; otherwise false.</returns>
		public static bool IsSerializable(Type type)
		{
			Init();
			if (sSerializers.ContainsKey(type)) return true;
			if (type.IsArray && IsSerializable(type.GetElementType())) return true;
			if (type.IsGenericType) {
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				return sSerializers.ContainsKey(genericTypeDefinition);
			}

			// check whether the type has an internal object serializer and create a serializer delegate for it,
			// so it is found faster next time...
			uint version;
			if (sCache.HasInternalObjectSerializer(type, out version))
			{
				lock (sSync)
				{
					if (!sSerializers.ContainsKey(type))
					{
						SerializerDelegate serializer = CreateInternalObjectSerializer(type);
						Dictionary<Type, SerializerDelegate> copy = new Dictionary<Type,SerializerDelegate>(sSerializers);
						copy[type] = serializer;
						Thread.MemoryBarrier();
						sSerializers = copy;
					}
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks whether the specified type is serialized using a custom (internal/external) object serializer.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>true, if the type is serialized using a custom serializer; otherwise false.</returns>
		public static bool HasCustomSerializer(Type type)
		{
			uint version;
			return HasInternalObjectSerializer(type, out version) || HasExternalObjectSerializer(type, out version);
		}

		/// <summary>
		/// Checks whether the specified type provides an internal object serializer.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="version">Receives the current version of internal object serializer.</param>
		/// <returns>true if the specified type provides an internal object seriaizer, otherwise false.</returns>
		internal static bool HasInternalObjectSerializer(Type type, out uint version)
		{
			return sCache.HasInternalObjectSerializer(type, out version);
		}

		/// <summary>
		/// Gets the overridden version number of the serializer to use for the specified type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="version">Receives the version number to use when serializing the specified type.</param>
		/// <returns>true, if a serializer version override is available; otherwise false.</returns>
		internal bool GetSerializerVersionOverride(Type type, out uint version)
		{
			return mSerializedTypeVersionTable.TryGet(type, out version);
		}

		/// <summary>
		/// Checks whether the specified type provides an external object serializer.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="version">Receives the version of the serializer.</param>
		/// <returns>true if the specified type can be serialized using an external object serializer, otherwise false.</returns>
		internal static bool HasExternalObjectSerializer(Type type, out uint version)
		{
			// check whether a serializer for exactly the specified type is available
			ExternalObjectSerializerInfo eosi;
			if (sExternalObjectSerializersBySerializee.TryGetValue(type, out eosi)) {
				version = eosi.Version;
				return true;
			}

			// check, whether a serializer for the generic type definition is available
			if (type.IsGenericType) {
				if (sExternalObjectSerializersBySerializee.TryGetValue(type.GetGenericTypeDefinition(), out eosi)) {
					version = eosi.Version;
					return true;
				}
			}

			version = 0;
			return false;
		}

		/// <summary>
		/// Gets the external object serializer for the specified type.
		/// </summary>
		/// <param name="type">Type to get an external object serializer for.</param>
		/// <param name="version">Receives the version of the serializer.</param>
		/// <returns>
		/// The external object serializer for the specified type;
		/// null, if the type does not have an external object serializer.
		/// </returns>
		internal static IExternalObjectSerializer GetExternalObjectSerializer(Type type, out uint version)
		{
			// check whether a serializer for exactly the specified type is available
			ExternalObjectSerializerInfo eosi;
			if (sExternalObjectSerializersBySerializee.TryGetValue(type, out eosi)) {
				version = eosi.Version;
				return eosi.Serializer;
			}

			version = 0;
			return null;
		}

		/// <summary>
		/// Gets the internal object serializer interface of an object, if it implements it.
		/// </summary>
		/// <param name="obj">Object to retrieve the internal serializer interface for.</param>
		/// <param name="version">Receives the version of the serializer for the specified type.</param>
		/// <returns>
		/// Internal object serializer interface implemented by the type of the specified object;
		/// null, if the type of the specified object does not (properly) implement the internal object serializer.
		/// </returns>
		internal static IInternalObjectSerializer GetInternalObjectSerializer(object obj, out uint version)
		{
			if (sCache.HasInternalObjectSerializer(obj.GetType(), out version)) {
				return obj as IInternalObjectSerializer;
			}

			// type does not implement an internal object serializer, or not properly...
			return null;
		}

		/// <summary>
		/// Gets the internal object serializer interface of the specified object's base class, if it implements it.
		/// </summary>
		/// <param name="obj">Object to retrieve the internal serializer interface for.</param>
		/// <param name="type">Type of a base class the specified object's class derives from.</param>
		/// <param name="version">Receives the version of the serializer for the specified type.</param>
		/// <returns>
		/// Internal object serializer interface implemented by the specified base class of the specified object;
		/// null, if the specified base class of the specified object does not (properly) implement the internal object serializer.
		/// </returns>
		internal static IInternalObjectSerializer GetInternalObjectSerializer(object obj, Type type, out uint version)
		{
			if (sCache.HasInternalObjectSerializer(type, out version)) {
				return obj as IInternalObjectSerializer;
			}

			// type does not implement an internal object serializer, or not properly...
			return null;
		}

		#endregion

		#region Creating/Getting Serializer Delegates

		/// <summary>
		/// Creates a delegate that handles the serialization of the specified type using an internal object serializer
		/// implemented by the type itself.
		/// </summary>
		/// <param name="type">Type implementing an internal object serializer.</param>
		/// <returns>A delegate that handles the serialization of the specified type.</returns>
		private static SerializerDelegate CreateInternalObjectSerializer(Type type)
		{
			return (serializer, stream, obj, context) =>
			{
				// determine the serializer version to use
				uint version;
				if (!serializer.mSerializedTypeVersionTable.TryGet(type, out version)) {
					sCache.HasInternalObjectSerializer(type, out version);
				}

				serializer.WriteTypeMetadata(stream, type);
				serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.ArchiveStart;
				int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, version);
				stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				SerializerArchive archive = new SerializerArchive(serializer, stream, type, version, context);
				IosSerializeDelegate serialize = GetInternalObjectSerializerSerializeCaller(type);
				serialize(obj as IInternalObjectSerializer, archive, version);
				archive.Close();
				stream.WriteByte((byte)PayloadType.ArchiveEnd);
			};
		}

		/// <summary>
		/// Gets a delegate that refers to the Serialize() method of the specified type
		/// (needed during serialization of base classes implementing an internal object serializer).
		/// </summary>
		/// <param name="type">Type of class to retrieve the Serialize() method from.</param>
		/// <returns>Delegate referring to the Serialize() method of the specified class.</returns>
		internal static IosSerializeDelegate GetInternalObjectSerializerSerializeCaller(Type type)
		{
			IosSerializeDelegate serializeDelegate;
			if (!sIosSerializeCallers.TryGetValue(type, out serializeDelegate))
			{
				lock (sSync)
				{
					if (!sIosSerializeCallers.TryGetValue(type, out serializeDelegate))
					{
						serializeDelegate = CreateIosSerializeCaller(type);
						Dictionary<Type,IosSerializeDelegate> copy = new Dictionary<Type,IosSerializeDelegate>(sIosSerializeCallers);
						copy.Add(type, serializeDelegate);
						Thread.MemoryBarrier();
						sIosSerializeCallers = copy;
					}
				}
			}

			return serializeDelegate;
		}

		/// <summary>
		/// Creates a dynamic method that calls the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified type
		/// that may be implemented implicitly or explicitly.
		/// </summary>
		/// <param name="type">Type of a class implementing the <see cref="IInternalObjectSerializer"/> interface.</param>
		/// <returns>A delegate to a dynamic method that simply calls the <see cref="IInternalObjectSerializer.Serialize"/> method of the specified type.</returns>
		private static IosSerializeDelegate CreateIosSerializeCaller(Type type)
		{
			// try to get the publicly implemented 'Serialize' method...
			MethodInfo method = type.GetMethod(nameof(IInternalObjectSerializer.Serialize), new Type[] { typeof(SerializerArchive), typeof(uint) });
			if (method == null)
			{
				// the publicly implemented 'Serialize' method is not available
				// => try to get the explicitly implemented 'Serialize' method...
				method = type.GetMethod(
					typeof(IInternalObjectSerializer).FullName + "." + nameof(IInternalObjectSerializer.Serialize),
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
					null,
					new Type[] { typeof(SerializerArchive), typeof(uint) },
					null);
			}

			Debug.Assert(method != null);

			// create a delegate that simply calls the Serialize() method of the internal object serializer
			int method_id = Interlocked.Increment(ref sIosSerializeCallersId);
			string name = "_ios_serialize_caller" + method_id;
			Type[] parameterTypes = { typeof(IInternalObjectSerializer), typeof(SerializerArchive), typeof(uint) };
			
			ParameterExpression[] parameterExpressions = {
				Expression.Parameter(typeof(IInternalObjectSerializer), "object"),
				Expression.Parameter(typeof(SerializerArchive), "archive"),
				Expression.Parameter(typeof(uint), "version")
			};

			Expression body = Expression.Call(
				Expression.Convert(parameterExpressions[0], type),
				method,
				parameterExpressions[1], parameterExpressions[2]);

			LambdaExpression lambda = Expression.Lambda(typeof(IosSerializeDelegate), body, parameterExpressions);
			return (IosSerializeDelegate)lambda.Compile();
		}

		/// <summary>
		/// Creates a delegate that handles the serialization of the specified type using an external object serializer.
		/// </summary>
		/// <param name="typeToSerialize">Type the delegate will handle.</param>
		/// <param name="eos">Receives the created external object serializer.</param>
		/// <param name="serializerVersion">Max. supported version of the serializer (as specified in the <see cref="ExternalObjectSerializerAttribute"/> attached to the external object serializer).</param>
		/// <returns>A delegate that handles the serialization of the specified type.</returns>
		private static SerializerDelegate CreateExternalObjectSerializer(Type typeToSerialize, IExternalObjectSerializer eos, uint serializerVersion)
		{
			return (serializer, stream, obj, context) =>
			{
				// determine the serializer version to use
				uint version;
				if (!serializer.mSerializedTypeVersionTable.TryGet(typeToSerialize, out version)) {
					version = serializerVersion;
				}

				// write type metadata
				serializer.WriteTypeMetadata(stream, typeToSerialize);

				// write serializer archive
				serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.ArchiveStart;
				int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, version);
				stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				SerializerArchive archive = new SerializerArchive(serializer, stream, typeToSerialize, version, context);
				eos.Serialize(archive, version, obj);
				archive.Close();
				stream.WriteByte((byte)PayloadType.ArchiveEnd);
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
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (int)((sbyte)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(byte))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (int)((byte)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(short))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (int)((short)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(ushort))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (int)((ushort)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(int))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (int)obj);
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(uint))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (long)((uint)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(long))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (long)obj);
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else if (underlyingType == typeof(ulong))
			{
				return (serializer, stream, obj, context) =>
				{
					serializer.WriteTypeMetadata(stream, type);
					serializer.mTempBuffer_Buffer[0] = (byte)PayloadType.Enum;
					int count = LEB128.Write(serializer.mTempBuffer_Buffer, 1, (long)((ulong)obj));
					stream.Write(serializer.mTempBuffer_Buffer, 0, 1+count);
				};
			}
			else
			{
				string error = string.Format("The underlying type ({0}) of enumeration ({1}) is not supported.", underlyingType.FullName, type.FullName);
				throw new NotSupportedException(error);
			}
		}

		#endregion
	}
}
