///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

#pragma warning disable CS0618
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace GriffinPlus.Lib.Serialization.Tests
{

	public abstract partial class SerializerTests_Base
	{
		#region Creating Serializer to Test

		/// <summary>
		/// Creates an instance of the <see cref="Serializer"/> class and configures it for the test.
		/// </summary>
		/// <returns>The <see cref="Serializer"/> instance to test.</returns>
		protected abstract Serializer CreateSerializer();

		#endregion

		#region Serializing/Deserializing: Null Reference

		/// <summary>
		/// Tests serializing and deserializing a null reference.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_NullReference()
		{
			object copy = SerializeAndDeserializeObject(null);
			Assert.Null(copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Object

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Object
		{
			get
			{
				object[] data = { null, new object() };
				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Object"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Object))]
		public void SerializeAndDeserialize_Object(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			CheckEquals(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Boolean

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Boolean
		{
			get
			{
				// value: speed/size optimized: 1 byte (native encoding)
				// array: speed optimized: 1 byte (native encoding)
				//        size optimized: compact encoding (8 booleans in a byte)
				bool[] data = { false, true };

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Boolean"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Boolean))]
		public void SerializeAndDeserialize_Boolean(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Char

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Char
		{
			get
			{
				// speed optimized: 2 bytes (native encoding)
				// size optimized: encoding chosen per value
				char[] data =
				{
					(char)0x0000, // size optimized: 1 byte (LEB128 encoding)
					(char)0x007F, // size optimized: 1 byte (LEB128 encoding)
					(char)0x0080, // size optimized: 2 bytes (native encoding)
					(char)0xFFFF  // size optimized: 2 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Char"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Char))]
		public void SerializeAndDeserialize_Char(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.SByte

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_SByte
		{
			get
			{
				// speed/size optimized: 1 byte (native encoding)
				sbyte[] data =
				{
					-128,
					0,
					127
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}


		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Char"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_SByte))]
		public void SerializeAndDeserialize_SByte(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Byte

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Byte
		{
			get
			{
				// speed/size optimized: 1 byte (native encoding)
				byte[] data =
				{
					0,
					255
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Byte"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Byte))]
		public void SerializeAndDeserialize_Byte(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Int16

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Int16
		{
			get
			{
				// speed optimized: 2 bytes (native encoding)
				// size optimized: encoding chosen per value
				short[] data =
				{
					unchecked((short)0x8000), // -32768: size optimized: 2 bytes (native encoding)
					unchecked((short)0xFFBF), // -65: size optimized: 2 bytes (native encoding)
					unchecked((short)0xFFC0), // -64: size optimized: 1 byte (LEB128  encoding)
					0,                        // 0: size optimized: 1 byte (LEB128 encoding)
					0x003F,                   // 63: size optimized: 1 byte (LEB128 encoding)
					0x0040,                   // 64: size optimized: 2 bytes (native encoding)
					0x7FFF                    // 32767: size optimized: 2 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Int16"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Int16))]
		public void SerializeAndDeserialize_Int16(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.UInt16

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_UInt16
		{
			get
			{
				// speed optimized: 2 bytes (native encoding)
				// size optimized: encoding chosen per value
				ushort[] data =
				{
					0,      // 0, size optimized: 1 byte (LEB128 encoding)
					0x007F, // 127, size optimized: 1 byte (LEB128 encoding)
					0x0080, // 128, size optimized: 2 bytes (native encoding)
					0xFFFF  // 65535, size optimized: 2 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.UInt16"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_UInt16))]
		public void SerializeAndDeserialize_UInt16(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Int32

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Int32
		{
			get
			{
				// speed optimized: 4 bytes (native encoding)
				// size optimized: encoding chosen per value
				int[] data =
				{
					unchecked((int)0x80000000), // -2147483648: size optimized: 4 bytes (native encoding)
					unchecked((int)0xFFEFFFFF), // -1048577: size optimized: 4 bytes (native encoding)
					unchecked((int)0xFFF00000), // -1048576: size optimized: 3 bytes (LEB128 encoding)
					unchecked((int)0xFFFFDFFF), // -8193: size optimized: 3 bytes (LEB128 encoding)
					unchecked((int)0xFFFFE000), // -8192: size optimized: 2 bytes (LEB128 encoding)
					unchecked((int)0xFFFFFFBF), // -65: size optimized: 2 bytes (LEB128 encoding)
					unchecked((int)0xFFFFFFC0), // -64: size optimized: 1 byte (LEB128  encoding)
					0,                          // 0: size optimized: 1 byte (LEB128 encoding)
					0x0000003F,                 // 63: size optimized: 1 byte (LEB128 encoding)
					0x00000040,                 // 64: size optimized: 2 bytes (LEB128 encoding)
					0x00001FFF,                 // 8191: size optimized: 2 bytes (LEB128 encoding)
					0x00002000,                 // 8192: size optimized: 3 bytes (LEB128 encoding)
					0x000FFFFF,                 // 1048575: size optimized: 3 bytes (LEB128 encoding)
					0x00800000,                 // 1048576: size optimized: 4 bytes (native encoding)
					0x7FFFFFFF                  // 2147483647: size optimized: 4 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Int32"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Int32))]
		public void SerializeAndDeserialize_Int32(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.UInt32

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_UInt32
		{
			get
			{
				// speed optimized: 4 bytes (native encoding)
				// size optimized: encoding chosen per value
				uint[] data =
				{
					0x00000000U, // 0, size optimized: 1 byte (LEB128 encoding)
					0x0000007FU, // 127, size optimized: 1 byte (LEB128 encoding)
					0x00000080U, // 128, size optimized: 2 bytes (LEB128 encoding)
					0x00003FFFU, // 16383, size optimized: 2 bytes (LEB128 encoding)
					0x00004000U, // 16384, size optimized: 3 bytes (LEB128 encoding)
					0x001FFFFFU, // 2097151, size optimized: 3 bytes (LEB128 encoding)
					0x00200000U, // 2097152, size optimized: 4 bytes (native encoding)
					0xFFFFFFFFU  // 4294967295, size optimized: 4 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.UInt32"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_UInt32))]
		public void SerializeAndDeserialize_UInt32(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Int64

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Int64
		{
			get
			{
				// speed optimized: 8 bytes (native encoding)
				// size optimized: encoding chosen per value
				long[] data =
				{
					unchecked((long)0x8000000000000000L), // -9223372036854775808: size optimized: 8 bytes (native encoding)
					unchecked((long)0xFFFEFFFFFFFFFFFFL), // -281474976710657: size optimized: 8 bytes (native encoding)
					unchecked((long)0xFFFF000000000000L), // -281474976710656: size optimized: 7 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFDFFFFFFFFFFL), // -2199023255553: size optimized: 7 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFE0000000000L), // -2199023255552: size optimized: 6 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFBFFFFFFFFL), // -17179869185: size optimized: 6 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFC00000000L), // -17179869184: size optimized: 5 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFF7FFFFFFL), // -134217729: size optimized: 5 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFF8000000L), // -134217728: size optimized: 4 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFEFFFFFL), // -1048577: size optimized: 4 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFF00000L), // -1048576: size optimized: 3 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFFFDFFFL), // -8193: size optimized: 3 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFFFE000L), // -8192: size optimized: 2 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFFFFFBFL), // -65: size optimized: 2 bytes (LEB128 encoding)
					unchecked((long)0xFFFFFFFFFFFFFFC0L), // -64: size optimized: 1 byte (LEB128  encoding)
					0L,                                   // 0: size optimized: 1 byte (LEB128 encoding)
					0x000000000000003FL,                  // 63: size optimized: 1 byte (LEB128 encoding)
					0x0000000000000040L,                  // 64: size optimized: 2 bytes (LEB128 encoding)
					0x0000000000001FFFL,                  // 8191: size optimized: 2 bytes (LEB128 encoding)
					0x0000000000002000L,                  // 8192: size optimized: 3 bytes (LEB128 encoding)
					0x00000000000FFFFFL,                  // 1048575: size optimized: 3 bytes (LEB128 encoding)
					0x0000000000800000L,                  // 1048576: size optimized: 4 bytes (LEB128 encoding)
					0x0000000007FFFFFFL,                  // 134217727: size optimized: 4 bytes (LEB128 encoding)
					0x0000000008000000L,                  // 134217728: size optimized: 5 bytes (LEB128 encoding)
					0x00000003FFFFFFFFL,                  // 17179869183: size optimized: 5 bytes (LEB128 encoding)
					0x0000000400000000L,                  // 17179869184: size optimized: 5 bytes (LEB128 encoding)
					0x000001FFFFFFFFFFL,                  // 2199023255551: size optimized: 6 bytes (LEB128 encoding)
					0x0000020000000000L,                  // 2199023255552: size optimized: 7 bytes (LEB128 encoding)
					0x0000FFFFFFFFFFFFL,                  // 281474976710655: size optimized: 7 bytes (LEB128 encoding)
					0x0001000000000000L,                  // 281474976710656: size optimized: 8 bytes (native encoding)
					0x7FFFFFFFFFFFFFFFL                   // 9223372036854775807: size optimized: 8 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Int64"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Int64))]
		public void SerializeAndDeserialize_Int64(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.UInt64

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_UInt64
		{
			get
			{
				// speed optimized: 8 bytes (native encoding)
				// size optimized: encoding chosen per value
				ulong[] data =
				{
					0x0000000000000000UL, // 0, size optimized: 1 byte (LEB128 encoding)
					0x000000000000007FUL, // 127, size optimized: 1 byte (LEB128 encoding)
					0x0000000000000080UL, // 128, size optimized: 2 bytes (LEB128 encoding)
					0x0000000000003FFFUL, // 16383, size optimized: 2 bytes (LEB128 encoding)
					0x0000000000004000UL, // 16384, size optimized: 3 bytes (LEB128 encoding)
					0x00000000001FFFFFUL, // 2097151, size optimized: 3 bytes (LEB128 encoding)
					0x0000000000200000UL, // 2097152, size optimized: 4 bytes (LEB128 encoding)
					0x000000000FFFFFFFUL, // 268435455, size optimized: 4 bytes (LEB128 encoding)
					0x0000000010000000UL, // 268435456, size optimized: 5 bytes (LEB128 encoding)
					0x00000007FFFFFFFFUL, // 34359738367, size optimized: 5 bytes (LEB128 encoding)
					0x0000000800000000UL, // 34359738368, size optimized: 6 bytes (LEB128 encoding)
					0x000003FFFFFFFFFFUL, // 4398046511103, size optimized: 6 bytes (LEB128 encoding)
					0x0000040000000000UL, // 4398046511104, size optimized: 7 bytes (LEB128 encoding)
					0x0001FFFFFFFFFFFFUL, // 562949953421311, size optimized: 7 bytes (LEB128 encoding)
					0x0002000000000000UL, // 562949953421312, size optimized: 8 bytes (native encoding)
					0xFFFFFFFFFFFFFFFFUL  // 18446744073709551615, size optimized: 8 bytes (native encoding)
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}


		/// <summary>
		/// Tests serializing and deserializing <see cref="System.UInt64"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_UInt64))]
		public void SerializeAndDeserialize_UInt64(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Single

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Single
		{
			get
			{
				// speed/size optimized: 4 byte (native encoding)
				float[] data =
				{
					0.0f,
					float.MinValue,
					float.MaxValue,
					float.Epsilon,
					float.NaN,
					float.NegativeInfinity,
					float.PositiveInfinity
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Single"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Single))]
		public void SerializeAndDeserialize_Single(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Double

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Double
		{
			get
			{
				// speed/size optimized: 4 byte (native encoding)
				double[] data =
				{
					0.0d,
					double.MinValue,
					double.MaxValue,
					double.Epsilon,
					double.NaN,
					double.NegativeInfinity,
					double.PositiveInfinity
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Double"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Single))]
		public void SerializeAndDeserialize_Double(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Decimal

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Decimal
		{
			get
			{
				// speed/size optimized: 16 byte (native encoding)
				decimal[] data =
				{
					0.0m,
					decimal.MinValue,
					decimal.MaxValue
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Decimal"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Decimal))]
		public void SerializeAndDeserialize_Decimal(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.String

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_String
		{
			get
			{
				string[] data =
				{
					"The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"
				};

				// simple values
				yield return new object[] { "Value", typeof(string), string.Join(" ", data) };

				// multi-dimensional arrays
				foreach (object[] record in GenerateArrayTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.String"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_String))]
		public void SerializeAndDeserialize_String(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.DateTime

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_DateTime
		{
			get
			{
				DateTime[] data =
				{
					DateTime.MinValue,
					DateTime.Now,
					DateTime.UtcNow,
					DateTime.MaxValue
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.DateTime"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_DateTime))]
		public void SerializeAndDeserialize_DateTime(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.DateTimeOffset

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_DateTimeOffset
		{
			get
			{
				DateTimeOffset[] data =
				{
					DateTimeOffset.MinValue,
					DateTimeOffset.Now,
					DateTimeOffset.UtcNow,
					DateTimeOffset.MaxValue
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.DateTimeOffset"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_DateTimeOffset))]
		public void SerializeAndDeserialize_DateTimeOffset(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Guid

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Guid
		{
			get
			{
				Guid[] data =
				{
					Guid.Empty,
					Guid.NewGuid()
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Guid"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Guid))]
		public void SerializeAndDeserialize_Guid(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Type

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Type
		{
			get
			{
				Type[] data =
				{
					typeof(int),                     // non-generic type
					typeof(int[]),                   // szarray type
					typeof(int[,]),                  // mdarray type
					typeof(Dictionary<int, string>), // closed constructed generic type
					typeof(Dictionary<,>)            // generic type definition
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="System.Type"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Type))]
		public void SerializeAndDeserialize_Type(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsAssignableFrom(type, obj); // may also be System.RuntimeType deriving from System.Type on .NET Framework
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.SByte)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_S8
		{
			get
			{
				TestEnum_S8[] data =
				{
					TestEnum_S8.A,
					TestEnum_S8.B,
					TestEnum_S8.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.SByte"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_S8))]
		public void SerializeAndDeserialize_Enum_S8(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.Byte)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_U8
		{
			get
			{
				TestEnum_U8[] data =
				{
					TestEnum_U8.A,
					TestEnum_U8.B,
					TestEnum_U8.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.Byte"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_U8))]
		public void SerializeAndDeserialize_Enum_U8(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.Int16)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_S16
		{
			get
			{
				TestEnum_S16[] data =
				{
					TestEnum_S16.A,
					TestEnum_S16.B,
					TestEnum_S16.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.Int16"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_S16))]
		public void SerializeAndDeserialize_Enum_S16(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.UInt16)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_U16
		{
			get
			{
				TestEnum_U16[] data =
				{
					TestEnum_U16.A,
					TestEnum_U16.B,
					TestEnum_U16.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.UInt16"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_U16))]
		public void SerializeAndDeserialize_Enum_U16(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.Int32)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_S32
		{
			get
			{
				TestEnum_S32[] data =
				{
					TestEnum_S32.A,
					TestEnum_S32.B,
					TestEnum_S32.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}


		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.Int32"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_S32))]
		public void SerializeAndDeserialize_Enum_S32(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.UInt32)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_U32
		{
			get
			{
				TestEnum_U32[] data =
				{
					TestEnum_U32.A,
					TestEnum_U32.B,
					TestEnum_U32.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.UInt32"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_U32))]
		public void SerializeAndDeserialize_Enum_U32(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.Int64)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_S64
		{
			get
			{
				TestEnum_S64[] data =
				{
					TestEnum_S64.A,
					TestEnum_S64.B,
					TestEnum_S64.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.Int64"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_S64))]
		public void SerializeAndDeserialize_Enum_S64(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Enumerations (backed by System.UInt64)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_Enum_U64
		{
			get
			{
				TestEnum_U64[] data =
				{
					TestEnum_U64.A,
					TestEnum_U64.B,
					TestEnum_U64.C
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an enumeration type backed by <see cref="System.UInt64"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_Enum_U64))]
		public void SerializeAndDeserialize_Enum_U64(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: Types with Custom Serializers

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_CustomSerializers
		{
			get
			{
				object[] data =
				{
					new TestClassWithInternalObjectSerializer(),                   // class with internal object serializer
					new TestStructWithInternalObjectSerializer().Init(),           // struct with internal object serializer
					new TestClassWithInternalObjectSerializer_Derived(),           // class deriving from another serializable class with internal object serializer
					new GenericTestClassWithInternalObjectSerializer<int, uint>(), // generic class with internal object serializer
					new object[]
					{
						// instances of a generic class with internal object serializer
						// (should reuse already serialized generic type definition, but not the closed generic type)
						new GenericTestClassWithInternalObjectSerializer<int, uint>(),
						new GenericTestClassWithInternalObjectSerializer<int, string>()
					},
					new TestClassWithExternalObjectSerializer(),        // class with external object serializer
					new TestStructWithExternalObjectSerializer().Init() // struct with external object serializer
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing types with custom serializers.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_CustomSerializers))]
		public void SerializeAndDeserialize_CustomSerializers(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.Dictionary<TKey,TValue> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_DictionaryT
		{
			get
			{
				Dictionary<char, string>[] data =
				{
					new Dictionary<char, string>
					{
						['0'] = "Value 0",
						['1'] = "Value 1",
						['2'] = "Value 2",
						['3'] = "Value 3",
						['4'] = "Value 4"
					},
					new Dictionary<char, string>
					{
						['a'] = "Value a",
						['b'] = "Value b",
						['c'] = "Value c",
						['d'] = "Value d",
						['e'] = "Value e"
					},
					new Dictionary<char, string>
					{
						['A'] = "Value A",
						['B'] = "Value B",
						['C'] = "Value C",
						['D'] = "Value D",
						['E'] = "Value E"
					}
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="Dictionary{TKey,TValue}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_DictionaryT))]
		public void SerializeAndDeserialize_DictionaryT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.HashSet<T> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_HashSetT
		{
			get
			{
				HashSet<string>[] data =
				{
					new HashSet<string>
					{
						"Value 0",
						"Value 1",
						"Value 2",
						"Value 3",
						"Value 4"
					},
					new HashSet<string>
					{
						"Value a",
						"Value b",
						"Value c",
						"Value d",
						"Value e"
					},
					new HashSet<string>
					{
						"Value A",
						"Value B",
						"Value C",
						"Value D",
						"Value E"
					}
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="HashSet{T}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_HashSetT))]
		public void SerializeAndDeserialize_HashSetT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.Queue<T> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_QueueT
		{
			get
			{
				Queue<string>[] data =
				{
					new Queue<string>(
						new[]
						{
							"Value 0",
							"Value 1",
							"Value 2",
							"Value 3",
							"Value 4"
						}),
					new Queue<string>(
						new[]
						{
							"Value a",
							"Value b",
							"Value c",
							"Value d",
							"Value e"
						}),
					new Queue<string>(
						new[]
						{
							"Value A",
							"Value B",
							"Value C",
							"Value D",
							"Value E"
						})
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="Queue{T}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_QueueT))]
		public void SerializeAndDeserialize_QueueT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.SortedDictionary<TKey,TValue> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_SortedDictionaryT
		{
			get
			{
				SortedDictionary<char, string>[] data =
				{
					new SortedDictionary<char, string>
					{
						['0'] = "Value 0",
						['1'] = "Value 1",
						['2'] = "Value 2",
						['3'] = "Value 3",
						['4'] = "Value 4"
					},
					new SortedDictionary<char, string>
					{
						['a'] = "Value a",
						['b'] = "Value b",
						['c'] = "Value c",
						['d'] = "Value d",
						['e'] = "Value e"
					},
					new SortedDictionary<char, string>
					{
						['A'] = "Value A",
						['B'] = "Value B",
						['C'] = "Value C",
						['D'] = "Value D",
						['E'] = "Value E"
					}
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="SortedDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_SortedDictionaryT))]
		public void SerializeAndDeserialize_SortedDictionaryT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.SortedList<TKey,TValue> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_SortedListT
		{
			get
			{
				SortedList<char, string>[] data =
				{
					new SortedList<char, string>
					{
						['0'] = "Value 0",
						['1'] = "Value 1",
						['2'] = "Value 2",
						['3'] = "Value 3",
						['4'] = "Value 4"
					},
					new SortedList<char, string>
					{
						['a'] = "Value a",
						['b'] = "Value b",
						['c'] = "Value c",
						['d'] = "Value d",
						['e'] = "Value e"
					},
					new SortedList<char, string>
					{
						['A'] = "Value A",
						['B'] = "Value B",
						['C'] = "Value C",
						['D'] = "Value D",
						['E'] = "Value E"
					}
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="SortedList{TKey,TValue}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_SortedListT))]
		public void SerializeAndDeserialize_SortedListT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.SortedSet<T> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_SortedSetT
		{
			get
			{
				SortedSet<string>[] data =
				{
					new SortedSet<string>
					{
						"Value 0",
						"Value 1",
						"Value 2",
						"Value 3",
						"Value 4"
					},
					new SortedSet<string>
					{
						"Value a",
						"Value b",
						"Value c",
						"Value d",
						"Value e"
					},
					new SortedSet<string>
					{
						"Value A",
						"Value B",
						"Value C",
						"Value D",
						"Value E"
					}
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="SortedSet{T}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_SortedSetT))]
		public void SerializeAndDeserialize_SortedSetT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Serializing/Deserializing: System.Collections.Generic.Stack<T> (Specific External Object Serializer)

		public static IEnumerable<object[]> SerializeAndDeserializeTestData_StackT
		{
			get
			{
				Stack<string>[] data =
				{
					new Stack<string>(
						new[]
						{
							"Value 0",
							"Value 1",
							"Value 2",
							"Value 3",
							"Value 4"
						}),
					new Stack<string>(
						new[]
						{
							"Value a",
							"Value b",
							"Value c",
							"Value d",
							"Value e"
						}),
					new Stack<string>(
						new[]
						{
							"Value A",
							"Value B",
							"Value C",
							"Value D",
							"Value E"
						})
				};

				foreach (object[] record in GenerateTestData(data))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Tests serializing and deserializing <see cref="Stack{T}"/>.
		/// </summary>
		/// <param name="description">Test case description (for documentation purposes).</param>
		/// <param name="type">Type of the object to test (for documentation purposes).</param>
		/// <param name="obj">Object to test with.</param>
		[Theory]
		[MemberData(nameof(SerializeAndDeserializeTestData_StackT))]
		public void SerializeAndDeserialize_StackT(string description, Type type, object obj)
		{
			object copy = SerializeAndDeserializeObject(obj);
			Assert.IsType(type, obj);
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Cyclic References

		/// <summary>
		/// Tests serializing and deserializing an array referencing itself.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_CyclicReference_ObjectArray()
		{
			object[] objects = new object[1];
			objects[0] = objects;
			object[] copy = (object[])SerializeAndDeserializeObject(objects);
			Assert.NotNull(copy);
			Assert.Equal(objects.Length, copy.Length);
			Assert.Same(copy, copy[0]);
		}

		/// <summary>
		/// Tests detecting cyclic references when serializing using an internal object serializer.
		/// The serializer should throw a <see cref="CyclicDependencyDetectedException"/> in this case.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_CyclicReference_InternalObjectSerializer()
		{
			// build object graph:
			// 0 -> 1 -> 2
			// ^---------+
			var node0 = new GraphNodeWithInternalObjectSerializer("0");
			var node1 = new GraphNodeWithInternalObjectSerializer("1");
			var node2 = new GraphNodeWithInternalObjectSerializer("2");

			node0.Next.Add(node1);
			node1.Next.Add(node2);
			node2.Next.Add(node0);

			var stream = new MemoryStream();
			var serializer = new Serializer();
			Assert.Throws<CyclicDependencyDetectedException>(() => { serializer.Serialize(stream, node0, null); });
		}

		/// <summary>
		/// Tests detecting cyclic references when serializing using an external object serializer.
		/// The serializer should throw a <see cref="CyclicDependencyDetectedException"/> in this case.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_CyclicReference_ExternalObjectSerializer()
		{
			// build object graph:
			// 0 -> 1 -> 2
			// ^---------+
			var node0 = new GraphNode("0");
			var node1 = new GraphNode("1");
			var node2 = new GraphNode("2");

			node0.Next.Add(node1);
			node1.Next.Add(node2);
			node2.Next.Add(node0);

			var stream = new MemoryStream();
			var serializer = new Serializer();
			Assert.Throws<CyclicDependencyDetectedException>(() => { serializer.Serialize(stream, node0, null); });
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Checks whether the specified objects are equal after a serializer copy.
		/// </summary>
		/// <param name="expected">The original object.</param>
		/// <param name="actual">The deserialized object.</param>
		private void CheckEquals(object expected, object actual)
		{
			Assert.True(expected != null == (actual != null));
			if (expected == null)
			{
				Assert.Null(expected);
				Assert.Null(actual);
			}
			else
			{
				Assert.Equal(expected.GetType(), actual.GetType());
				if (expected.GetType().IsArray)
				{
					var expectedArray = (Array)expected;
					var actualArray = (Array)actual;
					Assert.Equal(expectedArray.Rank, actualArray.Rank);
					Assert.Equal(expectedArray.Length, actualArray.Length);
					Assert.Equal(expectedArray.LongLength, actualArray.LongLength);
					for (int dimension = 0; dimension < expectedArray.Rank; dimension++)
					{
						Assert.Equal(expectedArray.GetLowerBound(dimension), actualArray.GetLowerBound(dimension));
						Assert.Equal(expectedArray.GetUpperBound(dimension), actualArray.GetUpperBound(dimension));
					}

					if (expectedArray.GetType().GetElementType() == typeof(object))
					{
						Assert.Equal(expectedArray, actualArray, (x, y) => x?.GetType() == y?.GetType());
					}
					else
					{
						Assert.Equal(expectedArray, actualArray);
					}
				}
				else
				{
					if (expected.GetType() == typeof(object))
					{
						Assert.Equal(expected, actual, (x, y) => x?.GetType() == y?.GetType());
					}
					else
					{
						Assert.Equal(expected, actual);
					}
				}
			}
		}

		/// <summary>
		/// Generates test data for the specified type containing a mix of the specified elements.
		/// </summary>
		/// <typeparam name="T">Type of the elements.</typeparam>
		/// <param name="elements">Some elements to test with.</param>
		/// <returns>Test data.</returns>
		private static IEnumerable<object[]> GenerateTestData<T>(params T[] elements)
		{
			// simple values
			foreach (T value in elements)
			{
				yield return new object[] { "Value", value?.GetType(), value };
			}

			// arrays
			foreach (object[] data in GenerateArrayTestData(elements))
			{
				yield return data;
			}
		}

		/// <summary>
		/// Generates test data for arrays of the specified type containing a mix of the specified elements.
		/// </summary>
		/// <typeparam name="T">Type of the elements.</typeparam>
		/// <param name="elements">Some elements to test with.</param>
		/// <returns>Test data.</returns>
		private static IEnumerable<object[]> GenerateArrayTestData<T>(params T[] elements)
		{
			// one-dimensional, zero-based array, empty
			yield return new object[] { "SZARRAY", typeof(T[]), Array.Empty<T>() };

			// one-dimensional, zero-based array, non-empty
			yield return new object[] { "SZARRAY", typeof(T[]), elements };

			// multi-dimensional array, empty, 1 dimension
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(1),
				CreateMultidimensionalArray(1, 0, elements)
			};

			// multi-dimensional array, empty, 2 dimensions
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(2),
				CreateMultidimensionalArray(2, 0, elements)
			};

			// multi-dimensional array, empty, 3 dimensions
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(3),
				CreateMultidimensionalArray(3, 0, elements)
			};

			// multi-dimensional array, non-empty, 1 dimension
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(1),
				CreateMultidimensionalArray(1, 5, elements)
			};

			// multi-dimensional array, non-empty, 2 dimensions
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(2),
				CreateMultidimensionalArray(2, 5, elements)
			};

			// multi-dimensional array, non-empty, 3 dimensions
			yield return new object[]
			{
				"MDARRAY",
				typeof(T).MakeArrayType(3),
				CreateMultidimensionalArray(3, 5, elements)
			};
		}

		/// <summary>
		/// Creates a multidimensional array with the specified number of dimensions and populates it with the specified elements.
		/// The first dimension will have its lower bound at 10, all other dimensions will start at a multiple of 10 (10, 20, 30).
		/// The length of a dimension is the specified length of the first dimension plus 10 times the dimension
		/// (first, first + 10, first + 20, first + 30, ...).
		/// </summary>
		/// <typeparam name="T">Array element type.</typeparam>
		/// <param name="dimensionCount">Number of dimensions of the array.</param>
		/// <param name="firstDimensionLength">Length of the first dimension of the array.</param>
		/// <param name="elements">Elements to put into the array.</param>
		/// <returns>The created array.</returns>
		private static Array CreateMultidimensionalArray<T>(
			int        dimensionCount,
			int        firstDimensionLength,
			params T[] elements)
		{
			// calculate the lengths and lower bounds of the array dimensions
			int[] lengths = new int[dimensionCount];
			int[] lowerBounds = new int[dimensionCount];
			int[] indices = new int[dimensionCount];
			int totalCount = 1;
			for (int i = 0; i < dimensionCount; i++)
			{
				lengths[i] = firstDimensionLength + 10 * i;
				lowerBounds[i] = 10 * (i + 1);
				indices[i] = lowerBounds[i];
				totalCount *= lengths[i];
			}

			// create and populate the array
			var array = Array.CreateInstance(typeof(T), lengths, lowerBounds);
			for (int i = 0; i < totalCount; i++)
			{
				array.SetValue(elements[i % elements.Length], indices);
				IncrementArrayIndices(indices, array);
			}

			return array;
		}

		/// <summary>
		/// Serializes the specified object into a memory stream and deserializes the resulting byte stream creating a
		/// deep copy of the specified object (does not take any shortcuts for primitive or immutable types).
		/// </summary>
		/// <param name="obj">Object to copy.</param>
		/// <returns>Copy of the specified object.</returns>
		private object SerializeAndDeserializeObject(object obj)
		{
			// create a copy of the object by serializing it to a stream
			// and deserializing it from the stream
			var stream = new MemoryStream();
			Serializer serializer = CreateSerializer();
			serializer.Serialize(stream, obj, null);
			long positionAfterSerialization = stream.Position;
			stream.Position = 0;
			object copy = serializer.Deserialize(stream, null);

			// check whether the stream has been consumed entirely
			Assert.Equal(positionAfterSerialization, stream.Position);

			return copy;
		}

		/// <summary>
		/// Increments a multidimensional array indexing vector.
		/// </summary>
		/// <param name="indices">Indexing vector.</param>
		/// <param name="array">Array that is being accessed.</param>
		private static void IncrementArrayIndices(IList<int> indices, Array array)
		{
			for (int i = indices.Count; i > 0; i--)
			{
				if (indices[i - 1] == array.GetUpperBound(i - 1))
				{
					indices[i - 1] = array.GetLowerBound(i - 1);
					continue;
				}

				indices[i - 1]++;
				return;
			}
		}

		#endregion
	}

}
