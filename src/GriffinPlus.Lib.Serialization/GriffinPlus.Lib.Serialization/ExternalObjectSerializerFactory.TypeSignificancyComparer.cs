///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib.Serialization;

partial class ExternalObjectSerializerFactory
{
	/// <summary>
	/// A comparer that compares two types taking their significancy into account.
	/// The comparer is used to sort type lists to determine the order in which types are checked for serializers.
	/// Classes/structs are most significant, followed by interface types. Generic interface types are considered
	/// more specific than non-generic interface types.
	/// </summary>
	public class TypeSignificancyComparer : IComparer<Type>
	{
		/// <summary>
		/// The singleton instance of the comparer (thread-safe).
		/// </summary>
		public static readonly TypeSignificancyComparer Instance = new();

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

			// classes/structs are less than interfaces
			// => put classes/structs in front of interfaces when sorting, sorted by their name
			if (!x.IsInterface && !y.IsInterface)
				return StringComparer.Ordinal.Compare(x.ToCSharpFormattedString(), y.ToCSharpFormattedString());
			if (x.IsInterface && !y.IsInterface) return 1;
			if (!x.IsInterface && y.IsInterface) return -1;

			// both types are interfaces

			// put generic interfaces in front of non-generic interfaces
			if (x.IsGenericType && !y.IsGenericType) return -1;
			if (!x.IsGenericType && y.IsGenericType) return 1;

			// if one interface is not generic, we can check whether it is extended by the other interface (Mark X1; see below)
			if (!x.IsGenericType && x.IsAssignableFrom(y)) return 1;
			if (!y.IsGenericType && y.IsAssignableFrom(x)) return -1;


			// if both interfaces are generic, we can check which one is more specific by counting the non-generic type parameters
			if (x.IsGenericType && y.IsGenericType)
			{
				int genericParameterCountX = x.GetGenericArguments().Count(t => !t.IsGenericParameter);
				int genericParameterCountY = y.GetGenericArguments().Count(t => !t.IsGenericParameter);
				if (genericParameterCountX > genericParameterCountY) return -1;
				if (genericParameterCountX < genericParameterCountY) return 1;
			}

			// addition to X1 (see above): if the current interface x (e.g., IDictionary<TKey,TValue>) extends another interface with more
			// specified type-parameters (e.g., ICollection<KeyValuePair<TKey,TValue>>) check whether the extended interface (e.g., ICollection<KeyValuePair<TKey,TValue>>)
			// is a specific version of y (e.g., ICollection<T>) by checking the generic type definitions
			if (x.GetInterfaces().Any(t => t.IsGenericType && y.IsGenericType && t.GetGenericTypeDefinition() == y.GetGenericTypeDefinition())) return -1;
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (y.GetInterfaces().Any(t => t.IsGenericType && x.IsGenericType && t.GetGenericTypeDefinition() == x.GetGenericTypeDefinition())) return 1;

			// no further preference
			// => sort by name to ensure the order of types in the list is always the same
			return StringComparer.Ordinal.Compare(x.ToCSharpFormattedString(), y.ToCSharpFormattedString());
		}
	}
}
