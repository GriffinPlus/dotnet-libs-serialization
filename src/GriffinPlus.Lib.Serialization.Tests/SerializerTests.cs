///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Xunit;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		#region Boolean

		/// <summary>
		/// Tests serializing and deserializing a boolean value.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SerializeAndDeserialize_Boolean(bool value)
		{
			object copy = SerializeAndDeserializeObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of boolean values (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_OneDimensionalArrayOfBoolean()
		{
			bool[] array = { false, true, true, false };
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.NotNull(copy);
			Assert.IsType<bool[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of boolean values (multi-dimensional).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_MultiDimensionalArrayOfBoolean()
		{
			// create a multi-dimensional array of boolean
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(typeof(bool), lengths, lowerBounds);

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
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Other Primitives (Except Boolean)

		/// <summary>
		/// Tests serializing and deserializing primitive types (all kinds of integers, floats, decimal and char).
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
		public void SerializeAndDeserialize_Primitives(Type type)
		{
			var minValueField = type.GetField("MinValue");
			var maxValueField = type.GetField("MaxValue");
			object min = minValueField.GetValue(null);
			object max = maxValueField.GetValue(null);

			object minCopy = SerializeAndDeserializeObject(min);
			object maxCopy = SerializeAndDeserializeObject(max);

			Assert.Equal(min, minCopy);
			Assert.Equal(max, maxCopy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array containing the minimum and the maximum of primitive types
		/// (one-dimensional, zero-based indexing).
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
		public void SerializeAndDeserialize_OneDimensionalArrayOfPrimitives(Type type)
		{
			var minValueField = type.GetField("MinValue");
			var maxValueField = type.GetField("MaxValue");
			dynamic min = minValueField.GetValue(null);
			dynamic max = maxValueField.GetValue(null);

			dynamic mid = Convert.ChangeType((max + min) / 2, type);

			dynamic array = Array.CreateInstance(type, 3);
			array[0] = min;
			array[1] = max;
			array[2] = mid;

			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.NotNull(copy);
			Assert.IsType(type.MakeArrayType(), copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array containing the minimum and the maximum of primitive types
		/// (one-dimensional, zero-based indexing).
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
		public void SerializeAndDeserialize_MultiDimensionalArrayOfPrimitives(Type type)
		{
			var minValueField = type.GetField("MinValue");
			var maxValueField = type.GetField("MaxValue");
			dynamic min = minValueField.GetValue(null);
			dynamic max = maxValueField.GetValue(null);
			dynamic mid = Convert.ChangeType((max + min) / 2, type);

			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(type, lengths, lowerBounds);

			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					array.SetValue(min, x, y, lowerBounds[2] + 0);
					array.SetValue(max, x, y, lowerBounds[2] + 1);
					array.SetValue(mid, x, y, lowerBounds[2] + 2);
				}
			}

			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region String

		/// <summary>
		/// Tests serializing and deserializing a string.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_String()
		{
			string value = "The quick brown fox jumps over the lazy dog";
			object copy = SerializeAndDeserializeObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of strings (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_OneDimensionalArrayOfString()
		{
			string[] array = { "The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.NotNull(copy);
			Assert.IsType<string[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of strings (multi-dimensional).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_MultiDimensionalArrayOfString()
		{
			// create a multi-dimensional array of string
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(typeof(string), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z < array.GetUpperBound(2); z++)
					{
						// last element is a null reference
						string value = $"x = {x}, y = {y}, z = {z}";
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region DateTime

		/// <summary>
		/// Tests serializing and deserializing a <see cref="DateTime"/> value.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_DateTime()
		{
			var value = DateTime.Now;
			object copy = SerializeAndDeserializeObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of <see cref="DateTime"/> values (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_OneDimensionalArrayOfDateTime()
		{
			var now = DateTime.Now;
			DateTime[] array = { now.AddMinutes(1), now.AddMinutes(2), now.AddMinutes(3), now.AddMinutes(4) };
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.NotNull(copy);
			Assert.IsType<DateTime[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of <see cref="DateTime"/> values (multi-dimensional).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_MultiDimensionalArrayOfDateTime()
		{
			// create a multi-dimensional array of datetime
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(typeof(DateTime), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						var value = DateTime.Now.AddMinutes(x + y + z);
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region DateTimeOffset

		/// <summary>
		/// Tests serializing and deserializing a <see cref="DateTimeOffset"/> value.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_DateTimeOffset()
		{
			var value = DateTimeOffset.Now;
			object copy = SerializeAndDeserializeObject(value);
			Assert.Equal(value, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of <see cref="DateTimeOffset"/> values (one-dimensional, zero-based indexing).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_OneDimensionalArrayOfDateTimeOffset()
		{
			var now = DateTimeOffset.Now;
			DateTimeOffset[] array = { now.AddMinutes(1), now.AddMinutes(2), now.AddMinutes(3), now.AddMinutes(4) };
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.NotNull(copy);
			Assert.IsType<DateTimeOffset[]>(copy);
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of <see cref="DateTimeOffset"/> values (multi-dimensional).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_MultiDimensionalArrayOfDateTimeOffset()
		{
			// create a multi-dimensional array of datetime
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(typeof(DateTimeOffset), lengths, lowerBounds);

			// populate array with some test data
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						var value = DateTimeOffset.Now.AddMinutes(x + y + z);
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Type Objects

		/// <summary>
		/// Tests serializing and deserializing a type object.
		/// </summary>
		[Theory]
		[InlineData(typeof(int))]                     // non-generic type
		[InlineData(typeof(Dictionary<int, string>))] // closed constructed generic type
		[InlineData(typeof(Dictionary<,>))]           // generic type definition
		public void SerializeAndDeserialize_Type(Type type)
		{
			object copy = SerializeAndDeserializeObject(type);
			Assert.Equal(type, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of type objects (one-dimensional, zero-based indexing).
		/// </summary>
		[Theory]
		[InlineData(typeof(int))]                                  // 1 element, non-generic type
		[InlineData(typeof(Dictionary<int, string>))]              // 1 element, closed constructed generic type
		[InlineData(typeof(Dictionary<,>))]                        // 1 element, generic type definition
		[InlineData(typeof(int), typeof(uint))]                    // 2 elements, non-generic types only
		[InlineData(typeof(int), typeof(Dictionary<int, string>))] // 2 elements, non-generic type and closed constructed generic type
		[InlineData(typeof(int), typeof(Dictionary<,>))]           // 2 elements, non-generic type and generic type definition
		public void SerializeAndDeserialize_OneDimensionalArrayOfType(params Type[] types)
		{
			dynamic copy = SerializeAndDeserializeObject(types);
			Assert.NotNull(copy);
			Assert.IsType<Type[]>(copy);
			Assert.Equal(types, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of type objects (multi-dimensional).
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_MultiDimensionalArrayOfType()
		{
			// create a multi-dimensional array of type objects
			int[] lengths = { 5, 4, 3 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(typeof(Type), lengths, lowerBounds);

			// populate array with some test data
			var types = Assembly.GetExecutingAssembly().GetTypes();
			for (int x = lowerBounds[0]; x <= array.GetUpperBound(0); x++)
			{
				for (int y = lowerBounds[1]; y <= array.GetUpperBound(1); y++)
				{
					for (int z = lowerBounds[2]; z <= array.GetUpperBound(2); z++)
					{
						int index = x - lowerBounds[0] + y - lowerBounds[1] + z - lowerBounds[2];
						var value = types[index];
						array.SetValue(value, x, y, z);
					}
				}
			}

			// check whether the copies array equals the original one
			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Enumerations

		/// <summary>
		/// Tests serializing and deserializing enumeration values.
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
		public void SerializeAndDeserialize_Enum(Type type)
		{
			foreach (object obj in Enum.GetValues(type))
			{
				object copy = SerializeAndDeserializeObject(obj);
				Assert.Equal(obj, copy);
			}
		}

		/// <summary>
		/// Tests serializing and deserializing an array of enumeration values (one-dimensional, zero-based indexing).
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
		public void SerializeAndDeserialize_OneDimensionalArrayOfEnum(Type type)
		{
			var array = Enum.GetValues(type);
			var copy = SerializeAndDeserializeObject(array) as Array;
			Assert.Equal(array, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an array of enumeration values (multi-dimensional).
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
		public void SerializeAndDeserialize_MultiDimensionalArrayOfEnum(Type type)
		{
			int[] lengths = { 5, 6, 7 };
			int[] lowerBounds = { 10, 20, 30 };
			var array = Array.CreateInstance(type, lengths, lowerBounds);

			var enums = Enum.GetValues(type);
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

			dynamic copy = SerializeAndDeserializeObject(array);
			Assert.Equal(array, copy);
		}

		#endregion

		#region Null Reference

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

		#region Instance of the System.Object Class

		/// <summary>
		/// Tests serializing and deserializing an instance of the <see cref="System.Object"/> class.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_Object()
		{
			object value = new object();
			object copy = SerializeAndDeserializeObject(value);
			Assert.NotNull(copy);
			Assert.IsType<object>(copy);
		}

		#endregion

		#region Internal Object Serializer

		/// <summary>
		/// Tests serializing and deserializing an instance of a class implementing an internal object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfClassUsingInternalObjectSerializer()
		{
			var obj = new TestClassWithInternalObjectSerializer();
			var copy = SerializeAndDeserializeObject(obj) as TestClassWithInternalObjectSerializer;
			Assert.Equal(obj, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an instance of a struct implementing an internal object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfStructUsingInternalObjectSerializer()
		{
			var obj = new TestStructWithInternalObjectSerializer();
			var copy = SerializeAndDeserializeObject(obj) as TestStructWithInternalObjectSerializer;
			Assert.Equal(obj, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an instance of a class using an external object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfDerivedClassUsingInternalObjectSerializer()
		{
			var obj = new TestClassWithInternalObjectSerializer_Derived();
			var copy = SerializeAndDeserializeObject(obj) as TestClassWithInternalObjectSerializer_Derived;
			Assert.Equal(obj, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an instance of a generic class implementing an internal object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfGenericClassUsingInternalObjectSerializer()
		{
			var obj = new GenericTestClassWithInternalObjectSerializer<int, uint>();
			var copy = SerializeAndDeserializeObject(obj) as GenericTestClassWithInternalObjectSerializer<int, uint>;
			Assert.Equal(obj, copy);
		}

		#endregion

		#region External Object Serializer

		/// <summary>
		/// Tests serializing and deserializing an instance of a class using an external object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfClassUsingExternalObjectSerializer()
		{
			var obj = new TestClassWithExternalObjectSerializer();
			var copy = SerializeAndDeserializeObject(obj) as TestClassWithExternalObjectSerializer;
			Assert.Equal(obj, copy);
		}

		/// <summary>
		/// Tests serializing and deserializing an instance of a struct using an external object serializer.
		/// </summary>
		[Fact]
		public void SerializeAndDeserialize_InstanceOfStructUsingExternalObjectSerializer()
		{
			var obj = new TestStructWithExternalObjectSerializer();
			var copy = SerializeAndDeserializeObject(obj) as TestStructWithExternalObjectSerializer;
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
		/// Serializes the specified object into a memory stream and deserializes the resulting byte stream creating a
		/// deep copy of the specified object (does not take any shortcuts for primitive or immutable types).
		/// </summary>
		/// <param name="obj">Object to copy.</param>
		/// <returns>Copy of the specified object.</returns>
		private static object SerializeAndDeserializeObject(object obj)
		{
			var stream = new MemoryStream();
			var serializer = new Serializer();
			serializer.Serialize(stream, obj, null);
			stream.Position = 0;
			return serializer.Deserialize(stream, null);
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
