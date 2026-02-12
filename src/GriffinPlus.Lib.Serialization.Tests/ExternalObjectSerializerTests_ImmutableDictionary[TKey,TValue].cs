///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Xunit;

#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace GriffinPlus.Lib.Serialization.Tests;

public abstract class ExternalObjectSerializerTests_ImmutableDictionaryT
{
	protected abstract Serializer CreateSerializer();

	public static IEnumerable<object[]> TestData
	{
		get
		{
			// string values (reference type, built-in serialization support)
			yield return ["ImmutableDictionary<char,string>, empty", ImmutableDictionary<char, string>.Empty];
			yield return
			[
				"ImmutableDictionary<char,string>, 3 items",
				ImmutableDictionary<char, string>.Empty
					.Add('A', "Alpha")
					.Add('B', "Beta")
					.Add('C', "Gamma")
			];

			// int values (value type, built-in serialization support)
			yield return ["ImmutableDictionary<string,int>, empty", ImmutableDictionary<string, int>.Empty];
			yield return
			[
				"ImmutableDictionary<string,int>, 3 items",
				ImmutableDictionary<string, int>.Empty
					.Add("one", 1)
					.Add("two", 2)
					.Add("three", 3)
			];

			// complex object values (requires custom external object serializer)
			yield return
			[
				"ImmutableDictionary<int,TestClassWithExternalObjectSerializer>, 2 items",
				ImmutableDictionary<int, SerializerTests_Base.TestClassWithExternalObjectSerializer>.Empty
					.Add(1, new SerializerTests_Base.TestClassWithExternalObjectSerializer())
					.Add(2, new SerializerTests_Base.TestClassWithExternalObjectSerializer())
			];
		}
	}

	[Theory]
	[MemberData(nameof(TestData))]
	public void SerializeAndDeserialize(string description, object obj)
	{
		// Arrange
		var stream = new MemoryStream();
		Serializer serializer = CreateSerializer();

		// Act
		serializer.Serialize(stream, obj, null);
		long positionAfterSerialization = stream.Position;
		stream.Position = 0;
		object copy = serializer.Deserialize(stream, null);

		// Assert
		Assert.Equal(positionAfterSerialization, stream.Position);
		Assert.Equal(obj.GetType(), copy.GetType());
		Assert.Equal(obj, copy);
	}

	public class SpeedOptimized : ExternalObjectSerializerTests_ImmutableDictionaryT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Speed };
		}
	}

	public class SizeOptimized : ExternalObjectSerializerTests_ImmutableDictionaryT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Size };
		}
	}
}
