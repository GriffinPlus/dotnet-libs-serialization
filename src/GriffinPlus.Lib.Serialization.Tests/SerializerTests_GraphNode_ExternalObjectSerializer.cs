///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[ExternalObjectSerializer(1)]
		public class GraphNode_ExternalObjectSerializer : ExternalObjectSerializer<GraphNode>
		{
			public override void Serialize(SerializationArchive archive, GraphNode obj)
			{
				if (archive.Version == 1)
				{
					archive.Write(obj.Name);
					archive.Write(obj.Next);
					return;
				}

				throw new VersionNotSupportedException(archive);
			}

			public override GraphNode Deserialize(DeserializationArchive archive)
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
