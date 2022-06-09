///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using System.Diagnostics;

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests_Base
	{
		[DebuggerDisplay("{Name}")]
		public class GraphNode
		{
			public string          Name { get; set; }
			public List<GraphNode> Next { get; set; }

			public GraphNode() { }

			public GraphNode(string name)
			{
				Next = new List<GraphNode>();
			}
		}
	}

}
