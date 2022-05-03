///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[ExternalObjectSerializer(typeof(GraphNode), 1)]
		public class GraphNode_ExternalObjectSerializer : IExternalObjectSerializer
		{
			public void Serialize(SerializerArchive archive, uint version, object obj)
			{
				var other = (GraphNode)obj;

				if (version == 1)
				{
					archive.Write(other.Name);
					archive.Write(other.Next);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClassWithExternalObjectSerializer), version);
				}
			}

			public object Deserialize(SerializerArchive archive)
			{
				var obj = new GraphNode();

				if (archive.Version == 1)
				{
					obj.Name = archive.ReadString();
					obj.Next = (List<GraphNode>)archive.ReadObject();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}

				return obj;
			}
		}
	}

}
