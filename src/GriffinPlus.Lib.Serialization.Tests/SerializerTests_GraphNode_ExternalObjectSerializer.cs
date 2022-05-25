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
			public void Serialize(SerializationArchive archive, object obj)
			{
				var other = (GraphNode)obj;

				if (archive.Version == 1)
				{
					archive.Write(other.Name);
					archive.Write(other.Next);
					return;
				}

				throw new VersionNotSupportedException(archive);
			}

			public object Deserialize(DeserializationArchive archive)
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
