///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Serialization
{

	partial class ExternalObjectSerializerFactory
	{
		/// <summary>
		/// A comparer that compares two types taking their significancy into account.
		/// The comparer is used to sort type lists to determine the order in which types are checked for serializers.
		/// Classes/structs are most significant, followed by interface types. Generic interface types are considered
		/// more specific than non generic interface types.
		/// </summary>
		public class TypeSignificancyComparer : IComparer<Type>
		{
			/// <summary>
			/// The singleton instance of the comparer (thread-safe).
			/// </summary>
			public static readonly TypeSignificancyComparer Instance = new TypeSignificancyComparer();

			/// <summary>
			/// Compares two <see cref="Type"/> objects and returns a value indicating whether one is less than, equal to,
			/// or greater than the other.
			/// </summary>
			/// <param name="x">The first <see cref="Type"/> to compare.</param>
			/// <param name="y">The second <see cref="Type"/> to compare.</param>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.<br/>
			/// &lt; 0 : <paramref name="x"/> is less than <paramref name="y"/>.<br/>
			/// = 0 : <paramref name="x"/> equals <paramref name="y"/>.<br/>
			/// &gt; 0 : <paramref name="x"/> is greater than <paramref name="y"/>.<br/>
			/// </returns>
			public int Compare(Type x, Type y)
			{
				if (x == y) return 0;
				if (x == null) return 1;
				if (y == null) return -1;

				// classes/structs are less then interfaces
				// => put classes/structs in front of interfaces when sorting, sorted by their name
				if (!x.IsInterface && !y.IsInterface)
					return StringComparer.Ordinal.Compare(x.ToCSharpFormattedString(), y.ToCSharpFormattedString());
				if (x.IsInterface && !y.IsInterface) return 1;
				if (!x.IsInterface && y.IsInterface) return -1;

				// both types are interfaces

				// put generic interfaces in front of non-generic interfaces
				if (x.IsGenericType && !y.IsGenericType) return -1;
				if (!x.IsGenericType && y.IsGenericType) return 1;

				// if one interface is not generic, we we can check whether it is extended by the other interface
				if (!x.IsGenericType && x.IsAssignableFrom(y)) return 1;
				if (!y.IsGenericType && y.IsAssignableFrom(x)) return -1;

				// TODO: Add further comparison steps to fix the order of generic interfaces

				// no further preference
				// => sort by name to ensure the order of types in the list is always the same
				return StringComparer.Ordinal.Compare(x.ToCSharpFormattedString(), y.ToCSharpFormattedString());
			}
		}
	}

}
