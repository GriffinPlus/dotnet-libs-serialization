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

public abstract class ExternalObjectSerializerTests_ImmutableSortedSetT
{
	protected abstract Serializer CreateSerializer();

	public static IEnumerable<object[]> TestData
	{
		get
		{
			// string elements (reference type, built-in serialization support)
			yield return ["ImmutableSortedSet<string>, empty", ImmutableSortedSet<string>.Empty];
			yield return ["ImmutableSortedSet<string>, 3 items", ImmutableSortedSet.Create("Alpha", "Beta", "Gamma")];

			// int elements (value type, built-in serialization support)
			yield return ["ImmutableSortedSet<int>, empty", ImmutableSortedSet<int>.Empty];
			yield return ["ImmutableSortedSet<int>, 5 items", ImmutableSortedSet.Create(1, 2, 3, 4, 5)];
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

	public class SpeedOptimized : ExternalObjectSerializerTests_ImmutableSortedSetT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Speed };
		}
	}

	public class SizeOptimized : ExternalObjectSerializerTests_ImmutableSortedSetT
	{
		protected override Serializer CreateSerializer()
		{
			return new Serializer { SerializationOptimization = SerializationOptimization.Size };
		}
	}
}
