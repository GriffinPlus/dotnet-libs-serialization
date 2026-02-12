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

public abstract class ExternalObjectSerializerTests_ImmutableQueueT
{
	protected abstract Serializer CreateSerializer();

	public static IEnumerable<object[]> TestData
	{
		get
		{
			// string elements (reference type, built-in serialization support)
			yield return ["ImmutableQueue<string>, empty", ImmutableQueue<string>.Empty];
			yield return ["ImmutableQueue<string>, 3 items", ImmutableQueue.Create("Alpha", "Beta", "Gamma")];

			// int elements (value type, built-in serialization support)
			yield return ["ImmutableQueue<int>, empty", ImmutableQueue<int>.Empty];
			yield return ["ImmutableQueue<int>, 5 items", ImmutableQueue.Create(1, 2, 3, 4, 5)];

			// complex object elements (requires custom external object serializer)
			yield return
			[
				"ImmutableQueue<TestClassWithExternalObjectSerializer>, 2 items",
				ImmutableQueue.Create(
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

	public class SpeedOptimized : ExternalObjectSerializerTests_ImmutableQueueT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Speed };
		}
	}

	public class SizeOptimized : ExternalObjectSerializerTests_ImmutableQueueT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Size };
		}
	}
}
