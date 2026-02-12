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

public abstract class ExternalObjectSerializerTests_ImmutableArrayT
{
	protected abstract Serializer CreateSerializer();

	public static IEnumerable<object[]> TestData
	{
		get
		{
			// string elements (reference type, built-in serialization support)
			yield return ["ImmutableArray<string>, empty", ImmutableArray<string>.Empty];
			yield return ["ImmutableArray<string>, 3 items", ImmutableArray.Create("Alpha", "Beta", "Gamma")];

			// int elements (value type, built-in serialization support)
			yield return ["ImmutableArray<int>, empty", ImmutableArray<int>.Empty];
			yield return ["ImmutableArray<int>, 5 items", ImmutableArray.Create(1, 2, 3, 4, 5)];

			// complex object elements (requires custom external object serializer)
			yield return
			[
				"ImmutableArray<TestClassWithExternalObjectSerializer>, 2 items",
				ImmutableArray.Create(
					new SerializerTests_Base.TestClassWithExternalObjectSerializer(),
					new SerializerTests_Base.TestClassWithExternalObjectSerializer())
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

	public class SpeedOptimized : ExternalObjectSerializerTests_ImmutableArrayT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Speed };
		}
	}

	public class SizeOptimized : ExternalObjectSerializerTests_ImmutableArrayT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Size };
		}
	}
}
