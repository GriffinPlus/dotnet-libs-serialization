///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using Xunit;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public class SerializerTests
	{
		#region Test Types/Data: Enumerations

		private enum TestEnum_S8 : sbyte
		{
			A = sbyte.MinValue,
			B = 0,
			C = sbyte.MaxValue
		}

		private enum TestEnum_U8 : byte
		{
			A = byte.MinValue,
			B = byte.MaxValue / 2,
			C = byte.MaxValue
		}

		private enum TestEnum_S16 : short
		{
			A = short.MinValue,
			B = 0,
			C = short.MaxValue
		}

		private enum TestEnum_U16 : ushort
		{
			A = ushort.MinValue,
			B = ushort.MaxValue / 2,
			C = ushort.MaxValue
		}

		private enum TestEnum_S32
		{
			A = int.MinValue,
			B = 0,
			C = int.MaxValue
		}

		private enum TestEnum_U32 : uint
		{
			A = uint.MinValue,
			B = uint.MaxValue / 2,
			C = uint.MaxValue
		}

		private enum TestEnum_S64 : long
		{
			A = long.MinValue,
			B = 0,
			C = long.MaxValue
		}

		private enum TestEnum_U64 : ulong
		{
			A = ulong.MinValue,
			B = ulong.MaxValue / 2,
			C = ulong.MaxValue
		}

		#endregion

		#region Test Type: Class with Internal Object Serializer

		[InternalObjectSerializer(1)]
		public class TestClass1 : IInternalObjectSerializer
		{
			public sbyte   mSByte;
			public byte    mByte;
			public short   mInt16;
			public ushort  mUInt16;
			public int     mInt32;
			public uint    mUInt32;
			public long    mInt64;
			public ulong   mUInt64;
			public float   mSingle;
			public double  mDouble;
			public decimal mDecimal;
			public char    mChar;

			public TestClass1()
			{
				mSByte = 1;
				mByte = 2;
				mInt16 = 3;
				mUInt16 = 4;
				mInt32 = 5;
				mUInt32 = 6;
				mInt64 = 7;
				mUInt64 = 8;
				mSingle = 9.0f;
				mDouble = 10.0;
				mDecimal = 11;
				mChar = 'X';
			}

			public TestClass1(SerializerArchive archive)
			{
				if (archive.Version == 1)
				{
					mSByte = archive.ReadSByte();
					mByte = archive.ReadByte();
					mInt16 = archive.ReadInt16();
					mUInt16 = archive.ReadUInt16();
					mInt32 = archive.ReadInt32();
					mUInt32 = archive.ReadUInt32();
					mInt64 = archive.ReadInt64();
					mUInt64 = archive.ReadUInt64();
					mSingle = archive.ReadSingle();
					mDouble = archive.ReadDouble();
					mDecimal = archive.ReadDecimal();
					mChar = archive.ReadChar();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}
			}

			public void Serialize(SerializerArchive archive, uint version)
			{
				if (version == 1)
				{
					archive.Write(mSByte);
					archive.Write(mByte);
					archive.Write(mInt16);
					archive.Write(mUInt16);
					archive.Write(mInt32);
					archive.Write(mUInt32);
					archive.Write(mInt64);
					archive.Write(mUInt64);
					archive.Write(mSingle);
					archive.Write(mDouble);
					archive.Write(mDecimal);
					archive.Write(mChar);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClass1), version);
				}
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				TestClass1 other = obj as TestClass1;
				if (mSByte != other.mSByte) return false;
				if (mByte != other.mByte) return false;
				if (mInt16 != other.mInt16) return false;
				if (mUInt16 != other.mUInt16) return false;
				if (mInt32 != other.mInt32) return false;
				if (mUInt32 != other.mUInt32) return false;
				if (mInt64 != other.mInt64) return false;
				if (mUInt64 != other.mUInt64) return false;
				if (mSingle != other.mSingle) return false;
				if (mDouble != other.mDouble) return false;
				if (mDecimal != other.mDecimal) return false;
				if (mChar != other.mChar) return false;
				return true;
			}
		}

		[InternalObjectSerializer(1)]
		public class TestClass1_Derived : TestClass1
		{
			public string   mString;
			public DateTime mDateTime;

			public TestClass1_Derived()
			{
				mString = "The quick brown fox jumps over the lazy dog";
				mDateTime = DateTime.Now;
			}

			public TestClass1_Derived(SerializerArchive archive) :
				base(archive.PrepareBaseArchive(typeof(TestClass1)))
			{
				if (archive.Version == 1)
				{
					mString = archive.ReadString();
					mDateTime = archive.ReadDateTime();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}
			}

			public new void Serialize(SerializerArchive archive, uint version)
			{
				archive.WriteBaseArchive(this, typeof(TestClass1), null);

				if (version == 1)
				{
					archive.Write(mString);
					archive.Write(mDateTime);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClass1_Derived), version);
				}
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				TestClass1_Derived other = obj as TestClass1_Derived;
				if (!base.Equals(obj)) return false;
				if (mString != other.mString) return false;
				if (mDateTime != other.mDateTime) return false;
				return true;
			}
		}

		#endregion

		#region Test Type: Class with External Object Serializer

		public class TestClass2
		{
			public sbyte   mSByte;
			public byte    mByte;
			public short   mInt16;
			public ushort  mUInt16;
			public int     mInt32;
			public uint    mUInt32;
			public long    mInt64;
			public ulong   mUInt64;
			public float   mSingle;
			public double  mDouble;
			public decimal mDecimal;
			public char    mChar;

			public TestClass2()
			{
				mSByte = 1;
				mByte = 2;
				mInt16 = 3;
				mUInt16 = 4;
				mInt32 = 5;
				mUInt32 = 6;
				mInt64 = 7;
				mUInt64 = 8;
				mSingle = 9.0f;
				mDouble = 10.0;
				mDecimal = 11;
				mChar = 'X';
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				TestClass2 other = obj as TestClass2;
				if (mSByte != other.mSByte) return false;
				if (mByte != other.mByte) return false;
				if (mInt16 != other.mInt16) return false;
				if (mUInt16 != other.mUInt16) return false;
				if (mInt32 != other.mInt32) return false;
				if (mUInt32 != other.mUInt32) return false;
				if (mInt64 != other.mInt64) return false;
				if (mUInt64 != other.mUInt64) return false;
				if (mSingle != other.mSingle) return false;
				if (mDouble != other.mDouble) return false;
				if (mDecimal != other.mDecimal) return false;
				if (mChar != other.mChar) return false;
				return true;
			}
		}

		[ExternalObjectSerializer(typeof(TestClass2), 1)]
		public class TestClass2_ExternalObjectSerializer : IExternalObjectSerializer
		{
			public void Serialize(SerializerArchive archive, uint version, object obj)
			{
				TestClass2 other = obj as TestClass2;

				if (version == 1)
				{
					archive.Write(other.mSByte);
					archive.Write(other.mByte);
					archive.Write(other.mInt16);
					archive.Write(other.mUInt16);
					archive.Write(other.mInt32);
					archive.Write(other.mUInt32);
					archive.Write(other.mInt64);
					archive.Write(other.mUInt64);
					archive.Write(other.mSingle);
					archive.Write(other.mDouble);
					archive.Write(other.mDecimal);
					archive.Write(other.mChar);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClass2), version);
				}
			}

			public object Deserialize(SerializerArchive archive)
			{
				TestClass2 obj = new TestClass2();

				if (archive.Version == 1)
				{
					obj.mSByte = archive.ReadSByte();
					obj.mByte = archive.ReadByte();
					obj.mInt16 = archive.ReadInt16();
					obj.mUInt16 = archive.ReadUInt16();
					obj.mInt32 = archive.ReadInt32();
					obj.mUInt32 = archive.ReadUInt32();
					obj.mInt64 = archive.ReadInt64();
					obj.mUInt64 = archive.ReadUInt64();
					obj.mSingle = archive.ReadSingle();
					obj.mDouble = archive.ReadDouble();
					obj.mDecimal = archive.ReadDecimal();
					obj.mChar = archive.ReadChar();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}

				return obj;
			}
		}

		#endregion

		#region Test: Boolean

		/// <summary>
		/// Tests copying a boolean value.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Copy_Boolean(bool value)
		{
			object copy = CopySerializableObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests copying an array of boolean values (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void Copy_One_Dimensional_Array_Of_Boolean()
		{
			bool[] array = { false, true, true, false };
			dynamic copy = CopySerializableObject(array);
			Assert.NotNull(copy);
			Assert.IsType<bool[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array of boolean values (multi-dimensional).
		/// </summary>
		[Fact]
		public void Copy_Multi_Dimensional_Array_Of_Boolean()
		{
			// create a multi-dimensional array of boolean
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(typeof(bool), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						bool value = (x + y + z) % 2 != 0;
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: Other Primitives (Except Boolean)

		/// <summary>
		/// Tests copying primitive types (all kinds of integers, floats, decimal and char).
		/// </summary>
		[Theory]
		[InlineData(typeof(sbyte))]
		[InlineData(typeof(short))]
		[InlineData(typeof(int))]
		[InlineData(typeof(long))]
		[InlineData(typeof(byte))]
		[InlineData(typeof(ushort))]
		[InlineData(typeof(uint))]
		[InlineData(typeof(ulong))]
		[InlineData(typeof(float))]
		[InlineData(typeof(double))]
		[InlineData(typeof(decimal))]
		[InlineData(typeof(char))]
		public void Copy_Primitives(Type type)
		{
			object min = type.GetField("MinValue").GetValue(null);
			object max = type.GetField("MaxValue").GetValue(null);

			object minCopy = CopySerializableObject(min);
			object maxCopy = CopySerializableObject(max);

			Assert.Equal(min, minCopy);
			Assert.Equal(max, maxCopy);
		}

		/// <summary>
		/// Tests copying an array containing the minimum and the maximum of primitive types (one-dimensional, zero-based indexing).
		/// </summary>
		[Theory]
		[InlineData(typeof(sbyte))]
		[InlineData(typeof(short))]
		[InlineData(typeof(int))]
		[InlineData(typeof(long))]
		[InlineData(typeof(byte))]
		[InlineData(typeof(ushort))]
		[InlineData(typeof(uint))]
		[InlineData(typeof(ulong))]
		[InlineData(typeof(float))]
		[InlineData(typeof(double))]
		[InlineData(typeof(decimal))]
		[InlineData(typeof(char))]
		public void Copy_One_Dimensional_Array_Of_Primitives(Type type)
		{
			dynamic min = type.GetField("MinValue").GetValue(null);
			dynamic max = type.GetField("MaxValue").GetValue(null);
			dynamic mid = Convert.ChangeType((max + min) / 2, type);

			dynamic array = Array.CreateInstance(type, 3);
			array[0] = min;
			array[1] = max;
			array[2] = mid;

			dynamic copy = CopySerializableObject(array);
			Assert.NotNull(copy);
			Assert.IsType(type.MakeArrayType(), copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array containing the minimum and the maximum of primitive types (one-dimensional, zero-based indexing).
		/// </summary>
		[Theory]
		[InlineData(typeof(sbyte))]
		[InlineData(typeof(short))]
		[InlineData(typeof(int))]
		[InlineData(typeof(long))]
		[InlineData(typeof(byte))]
		[InlineData(typeof(ushort))]
		[InlineData(typeof(uint))]
		[InlineData(typeof(ulong))]
		[InlineData(typeof(float))]
		[InlineData(typeof(double))]
		[InlineData(typeof(decimal))]
		[InlineData(typeof(char))]
		public void Copy_Multi_Dimensional_Array_Of_Primitives(Type type)
		{
			dynamic min = type.GetField("MinValue").GetValue(null);
			dynamic max = type.GetField("MaxValue").GetValue(null);
			dynamic mid = Convert.ChangeType((max + min) / 2, type);

			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(type, lengths, lowerBounds);

			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					array.SetValue(min, x, y, lowerBounds[2] + 0);
					array.SetValue(max, x, y, lowerBounds[2] + 1);
					array.SetValue(mid, x, y, lowerBounds[2] + 2);
				}
			}

			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: String

		/// <summary>
		/// Tests copying a string.
		/// </summary>
		[Fact]
		public void Copy_String()
		{
			string value = "The quick brown fox jumps over the lazy dog";
			object copy = CopySerializableObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests copying an array of strings (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void Copy_One_Dimensional_Array_Of_String()
		{
			string[] array = { "The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
			dynamic copy = CopySerializableObject(array);
			Assert.NotNull(copy);
			Assert.IsType<string[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array of strings (multi-dimensional).
		/// </summary>
		[Fact]
		public void Copy_Multi_Dimensional_Array_Of_String()
		{
			// create a multi-dimensional array of string
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(typeof(string), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z < array.GetUpperBound(2); z++)
					{
						// last element is a null reference
						string value = string.Format("x = {0}, y = {1}, z = {2}", x, y, z);
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: DateTime

		/// <summary>
		/// Tests copying a <see cref="System.DateTime"/> value.
		/// </summary>
		[Fact]
		public void Copy_DateTime()
		{
			DateTime value = DateTime.Now;
			object copy = CopySerializableObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests copying an array of <see cref="System.DateTime"/> values (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void Copy_One_Dimensional_Array_Of_DateTime()
		{
			DateTime now = DateTime.Now;
			DateTime[] array = { now.AddMinutes(1), now.AddMinutes(2), now.AddMinutes(3), now.AddMinutes(4) };
			dynamic copy = CopySerializableObject(array);
			Assert.NotNull(copy);
			Assert.IsType<DateTime[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array of <see cref="System.DateTime"/> values (multi-dimensional).
		/// </summary>
		[Fact]
		public void Copy_Multi_Dimensional_Array_Of_DateTime()
		{
			// create a multi-dimensional array of datetime
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(typeof(DateTime), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						DateTime value = DateTime.Now.AddMinutes(x + y + z);
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: Type Objects

		/// <summary>
		/// Tests copying a type object.
		/// </summary>
		[Fact]
		public void Copy_Type()
		{
			Type value = typeof(SerializerTests);
			object copy = CopySerializableObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests copying an array of type objects (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void Copy_One_Dimensional_Array_Of_Type()
		{
			Type[] array = { typeof(Serializer), typeof(SerializerArchive), typeof(SerializerTests), typeof(SerializerVersionTable) };
			dynamic copy = CopySerializableObject(array);
			Assert.NotNull(copy);
			Assert.IsType<Type[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array of type objects (multi-dimensional).
		/// </summary>
		[Fact]
		public void Copy_Multi_Dimensional_Array_Of_Type()
		{
			// create a multi-dimensional array of type objects
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(typeof(Type), lengths, lowerBounds);

			// populate array with some test data
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						int index = x - lowerBounds[0] + y - lowerBounds[1] + z - lowerBounds[2];
						Type value = types[index];
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: Enumerations

		/// <summary>
		/// Tests copying enumeration values.
		/// </summary>
		[Theory]
		[InlineData(typeof(TestEnum_S8))]
		[InlineData(typeof(TestEnum_U8))]
		[InlineData(typeof(TestEnum_S16))]
		[InlineData(typeof(TestEnum_U16))]
		[InlineData(typeof(TestEnum_S32))]
		[InlineData(typeof(TestEnum_U32))]
		[InlineData(typeof(TestEnum_S64))]
		[InlineData(typeof(TestEnum_U64))]
		public void Copy_Enum(Type type)
		{
			foreach (object obj in Enum.GetValues(type))
			{
				object copy = CopySerializableObject(obj);
				Assert.Equal(obj, copy);
			}
		}

		/// <summary>
		/// Tests copying an array of enumeration values (one-dimensional, zero-based indexing).
		/// </summary>
		[Theory]
		[InlineData(typeof(TestEnum_S8))]
		[InlineData(typeof(TestEnum_U8))]
		[InlineData(typeof(TestEnum_S16))]
		[InlineData(typeof(TestEnum_U16))]
		[InlineData(typeof(TestEnum_S32))]
		[InlineData(typeof(TestEnum_U32))]
		[InlineData(typeof(TestEnum_S64))]
		[InlineData(typeof(TestEnum_U64))]
		public void Copy_One_Dimensional_Array_Of_Enum(Type type)
		{
			Array array = Enum.GetValues(type);
			Array copy = CopySerializableObject(array) as Array;
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests copying an array of enumeration values (multi-dimensional).
		/// </summary>
		[Theory]
		[InlineData(typeof(TestEnum_S8))]
		[InlineData(typeof(TestEnum_U8))]
		[InlineData(typeof(TestEnum_S16))]
		[InlineData(typeof(TestEnum_U16))]
		[InlineData(typeof(TestEnum_S32))]
		[InlineData(typeof(TestEnum_U32))]
		[InlineData(typeof(TestEnum_S64))]
		[InlineData(typeof(TestEnum_U64))]
		public void Copy_Multi_Dimensional_Array_Of_Enum(Type type)
		{
			int[] lengths = { 5, 6, 7 };
			int[] lowerBounds = { 10, 20, 30 };
			Array array = Array.CreateInstance(type, lengths, lowerBounds);

			Array enums = Enum.GetValues(type);
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						dynamic value = enums.GetValue((x + y + z) % enums.Length);
						array.SetValue(value, x, y, z);
					}
				}
			}

			dynamic copy = CopySerializableObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Test: Null Reference

		/// <summary>
		/// Tests copying a null reference.
		/// </summary>
		[Fact]
		public void Copy_Null_Reference()
		{
			Type value = null;
			object copy = CopySerializableObject(value);
			Assert.Null(copy);
		}

		#endregion

		#region Test: Instance of the System.Object Class

		/// <summary>
		/// Tests copying an instance of the <see cref="System.Object"/> class.
		/// </summary>
		[Fact]
		public void Copy_Object()
		{
			object value = new object();
			object copy = CopySerializableObject(value);
			Assert.NotNull(copy);
			Assert.IsType<object>(copy);
		}

		#endregion

		#region Test: Internal Object Serializer

		/// <summary>
		/// Tests copying an instance of a class implementing an internal object serializer.
		/// </summary>
		[Fact]
		public void Copy_Object_Using_InternalObjectSerializer()
		{
			TestClass1 obj = new TestClass1();
			TestClass1 copy = CopySerializableObject(obj) as TestClass1;
			Assert.Equal(obj, copy);
		}

		/// <summary>
		/// Tests copying an instance of a class using an external object serializer.
		/// </summary>
		[Fact]
		public void Copy_Object_Of_Derived_Class_Using_InternalObjectSerializer()
		{
			TestClass1_Derived obj = new TestClass1_Derived();
			TestClass1_Derived copy = CopySerializableObject(obj) as TestClass1_Derived;
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Test: External Object Serializer

		/// <summary>
		/// Tests copying an instance of a class using an external object serializer.
		/// </summary>
		[Fact]
		public void Copy_Object_Of_Class_Using_ExternalObjectSerializer()
		{
			Serializer.RegisterExternalObjectSerializer(typeof(TestClass2_ExternalObjectSerializer));

			TestClass2 obj = new TestClass2();
			TestClass2 copy = CopySerializableObject(obj) as TestClass2;
			Assert.Equal(obj, copy);
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Serializes the specified object into a memory stream and deserializes the resulting byte stream creating a deep copy of the specified object
		/// (does not take any shortcuts for primitive or immutable types).
		/// </summary>
		/// <param name="obj">Object to copy.</param>
		/// <returns>Copy of the specified object.</returns>
		private static object CopySerializableObject(object obj)
		{
			MemoryStream mbs = new MemoryStream();
			Serializer serializer = new Serializer();
			serializer.Serialize(mbs, obj, null);
			mbs.Position = 0;
			return serializer.Deserialize(mbs, null);
		}

		/// <summary>
		/// Increments a multidimensional array indexing vector.
		/// </summary>
		/// <param name="indices">Indexing vector.</param>
		/// <param name="array">Array that is being accessed.</param>
		private static void IncrementArrayIndices(int[] indices, Array array)
		{
			for (int i = indices.Length; i > 0; i--)
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
