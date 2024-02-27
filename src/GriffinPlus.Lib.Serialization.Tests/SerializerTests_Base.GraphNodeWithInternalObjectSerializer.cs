///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization.Tests;

public partial class SerializerTests_Base
{
	[InternalObjectSerializer(1)]
	internal class GraphNodeWithInternalObjectSerializer : GraphNode, IInternalObjectSerializer
	{
		public GraphNodeWithInternalObjectSerializer(string name) : base(name) { }

		public GraphNodeWithInternalObjectSerializer(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				Name = archive.ReadString();
				Next = (List<GraphNode>)archive.ReadObject();
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}

		void IInternalObjectSerializer.Serialize(SerializationArchive archive)
		{
			// ReSharper disable once InvertIf
			if (archive.Version == 1)
			{
				archive.Write(Name);
				archive.Write(Next);
			}

			throw new VersionNotSupportedException(archive);
		}
	}
}
