///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Serialization.Tests
{

	/// <summary>
	/// Tests for a <see cref="Serializer"/> that optimizes for size.
	/// </summary>
	public class SerializerTests_SizeOptimized : SerializerTests_Base
	{
		/// <summary>
		/// Creates an instance of the <see cref="Serializer"/> class and configures it for the test.
		/// </summary>
		/// <returns>The <see cref="Serializer"/> instance to test.</returns>
		protected override Serializer CreateSerializer()
		{
			var serializer = new Serializer
			{
				SerializationOptimization = SerializationOptimization.Size
			};

			return serializer;
		}
	}

}
