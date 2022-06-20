///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using GriffinPlus.Lib.Collections;
using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// A factory creating external object serializers.
	/// </summary>
	public static partial class ExternalObjectSerializerFactory
	{
		private static readonly LogWriter                           sLog                                      = LogWriter.Get(typeof(ExternalObjectSerializerFactory));
		private static readonly object                              sSync                                     = new object();
		private static          List<SerializerInfo>                sRegisteredExternalObjectSerializers      = new List<SerializerInfo>();
		private static          TypeKeyedDictionary<SerializeeInfo> sExternalObjectSerializerInfoBySerializee = new TypeKeyedDictionary<SerializeeInfo>();

		/// <summary>
		/// Gets information about registered external object serializers.
		/// </summary>
		public static IEnumerable<SerializerInfo> RegisteredSerializers => sRegisteredExternalObjectSerializers;

		/// <summary>
		/// Checks whether the specified type is an external object serializer and registers it for use.
		/// </summary>
		/// <param name="type">Possible external object serializer type to register.</param>
		/// <returns>
		/// <c>true</c> if the type has been registered as an external object serializer;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method performs some plausibility checks and writes to the log if it detects that the type could
		/// be an external object serializer, but there is an implementation issue (missing attribute, wrong base class).
		/// </remarks>
		public static bool TryRegisterExternalObjectSerializer(Type type)
		{
			lock (sSync)
			{
				// abort if the external object serializer type has already been registered
				bool exists = sRegisteredExternalObjectSerializers.Find(x => x.SerializerType == type) != null;
				if (exists)
				{
					sLog.Write(
						LogLevel.Debug,
						"Registering external object serializer '{0}' failed (already registered).",
						type.FullName);

					return false;
				}

				// get the ExternalObjectSerializer<> all external object serializers derive from
				var eosBaseType = GetExternalObjectSerializerBaseClass(type);

				// check whether the type is annotated with the attribute for external object serializers
				var eosAttribute = (ExternalObjectSerializerAttribute)type.GetCustomAttributes(typeof(ExternalObjectSerializerAttribute), false).FirstOrDefault();

				// check whether the type has parameterless constructor
				var eosConstructor = type.GetConstructor(
					BindingFlags.ExactBinding | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					Type.DefaultBinder,
					Type.EmptyTypes,
					null);

				// abort if the type does not derive from the base class or attribute
				if (eosAttribute != null && eosBaseType != null && eosConstructor != null)
				{
					// class is annotated with the external object serializer attribute, derives from the appropriate base class
					// and provides a parameterless constructor => everything seems alright!

					// determine the serialized type supported by the external object serializer
					// from the generic type argument of the ExternalObjectSerializer<> class
					var eosSerializeeType = eosBaseType.GenericTypeArguments[0];

					// abort if the serializee is already handled by some other external object serializer
					var otherEos = sRegisteredExternalObjectSerializers.Find(x => x.SerializeeType == eosSerializeeType);
					if (otherEos != null)
					{
						sLog.Write(
							LogLevel.Error,
							"Registering external object serializer '{0}' failed ('{1}' already handles '{2}').",
							type.FullName,
							otherEos.SerializerType.FullName,
							eosSerializeeType.FullName);

						return false;
					}

					// if the external object serializer class is a generic type definition,
					// ensure that all generic type parameters are mapped to the serializee type
					// (otherwise we cannot infer them from the supported serializee type later on)
					if (type.IsGenericTypeDefinition)
					{
						foreach (var eosGenericTypeArgument in type.GetGenericArguments())
						{
							if (!eosSerializeeType.GenericTypeArguments.Contains(eosGenericTypeArgument))
							{
								sLog.Write(
									LogLevel.Error,
									"Registering external object serializer '{0}' failed (the type has generic type parameters that are not used by the handled type '{1}').",
									type.FullName,
									eosSerializeeType.FullName);

								return false;
							}
						}
					}

					// add types the external object serializer supports
					var eosListCopy = new List<SerializerInfo>(sRegisteredExternalObjectSerializers)
					{
						new SerializerInfo(type, eosSerializeeType, eosAttribute.Version)
					};
					eosListCopy.Sort((x, y) => TypeSignificancyComparer.Instance.Compare(x.SerializeeType, y.SerializeeType));
					Thread.MemoryBarrier();
					sRegisteredExternalObjectSerializers = eosListCopy;

					return true;
				}

				if (eosAttribute != null || eosBaseType != null)
				{
					if (sLog.IsLogLevelActive(LogLevel.Error))
					{
						string message = $"Registering external object serializer '{type.FullName}' failed due to an implementation issue.";

						if (eosAttribute == null)
						{
							// attribute is missing
							message += $"\nClass '{type.FullName}' seems to be an external serializer class, but it is not annotated with the '{typeof(ExternalObjectSerializerAttribute).FullName}' attribute.";
						}

						if (eosBaseType == null)
						{
							// base class is missing
							message += $"\nClass '{type.FullName}' seems to be an external serializer class, but it does not derive from '{typeof(ExternalObjectSerializer<>).FullName}'.";
						}

						if (eosConstructor == null)
						{
							// default constructor is missing
							message += $"\nClass '{type.FullName}' seems to be an external serializer class, but it does not have a public parameterless constructor.";
						}

						sLog.Write(LogLevel.Error, message);
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets the external object serializer for the specified type.
		/// </summary>
		/// <param name="type">Type to get an external object serializer for.</param>
		/// <param name="version">Receives the version of the serializer.</param>
		/// <returns>
		/// The external object serializer for the specified type;
		/// <c>null</c>, if the type does not have an external object serializer.
		/// </returns>
		public static IExternalObjectSerializer GetExternalObjectSerializer(Type type, out uint version)
		{
			// check whether a serializer for exactly the specified type is available
			if (sExternalObjectSerializerInfoBySerializee.TryGetValue(type, out var eosi))
			{
				version = eosi.SerializerInfo.Version;
				return eosi.Serializer;
			}

			// there is no external object serializer instance handling the specified type, yet
			// => find the external object serializer matching best...

			lock (sSync)
			{
				// once again, check whether a serializer for exactly the specified type is available
				// (maybe someone else has added the serializer meanwhile...)
				if (sExternalObjectSerializerInfoBySerializee.TryGetValue(type, out eosi))
				{
					version = eosi.SerializerInfo.Version;
					return eosi.Serializer;
				}

				// get a list containing the serializee type and all interfaces it implements
				var possibleSerializeeTypes = GetPossibleSerializeeTypes(type);

				// check the types against the types registered external object serializers can handle
				// (the order of both lists ensures that the most specific types are checked first)
				foreach (var possibleSerializeeType in possibleSerializeeTypes)
				{
					foreach (var eosInfo in sRegisteredExternalObjectSerializers)
					{
						IExternalObjectSerializer eosInstance = null;

						if (possibleSerializeeType == eosInfo.SerializeeType)
						{
							// the serializee exactly matches the serializee supported by the external object serializer
							// => the serializer can handle the type directly
							eosInstance = (IExternalObjectSerializer)FastActivator.CreateInstance(eosInfo.SerializerType);
						}
						else if (possibleSerializeeType.IsConstructedGenericType && eosInfo.SerializeeType.ContainsGenericParameters)
						{
							// the serializee is a generic type and the serializee supported by the external object serializer is a generic type as well
							// => check whether their generic type definitions are the same, so the serializer can be used to handle the type
							var possibleSerializeeTypeDefinition = possibleSerializeeType.GetGenericTypeDefinition();
							var eosSerializeeTypeDefinition = eosInfo.SerializeeType.GetGenericTypeDefinition();
							if (possibleSerializeeTypeDefinition == eosSerializeeTypeDefinition)
							{
								// both generic type definitions are the same
								// => found serializer that can handle the type
								var constructedEosType = eosInfo.SerializerType.MakeGenericType(possibleSerializeeType.GenericTypeArguments);
								eosInstance = (IExternalObjectSerializer)FastActivator.CreateInstance(constructedEosType);
							}
						}

						if (eosInstance != null)
						{
							// found an external object serializer that is capable of handling the specified type
							// => add it to the serializer cache...
							var eosDictCopy = new TypeKeyedDictionary<SerializeeInfo>(sExternalObjectSerializerInfoBySerializee)
							{
								{ type, new SerializeeInfo(type, eosInfo, eosInstance) }
							};
							Thread.MemoryBarrier();
							sExternalObjectSerializerInfoBySerializee = eosDictCopy;

							version = eosInfo.Version;
							return eosInstance;
						}
					}
				}
			}

			version = 0;
			return null;
		}

		/// <summary>
		/// Gets the subtypes of the specified type an external object serializer might be provided for.
		/// </summary>
		/// <param name="type">Type to get possible serializee types for.</param>
		/// <returns>
		/// All subtypes of the specified type an external object serializer may be provided for,
		/// sorted by their significancy.
		/// </returns>
		private static List<Type> GetPossibleSerializeeTypes(Type type)
		{
			// the best match is the an external object serializer for exactly the specified type
			var possibleSerializeeTypes = new List<Type> { type };

			// the specified type may implement interfaces
			// => add all of its interfaces
			var implementedInterfaces = type.FindInterfaces((interfaceType, criteria) => true, null);
			possibleSerializeeTypes.AddRange(implementedInterfaces);

			// sort the list to ensure that the specified type is at the front of the list, followed by implemented interfaces
			// ordered from the most specific interface to the least specific interface
			possibleSerializeeTypes.Sort(TypeSignificancyComparer.Instance);

			return possibleSerializeeTypes;
		}

		/// <summary>
		/// Gets the <see cref="ExternalObjectSerializer{T}"/> base class of the specified external object serializer class.
		/// </summary>
		/// <param name="eosType">External object serializer class from which to retrieve the <see cref="ExternalObjectSerializer{T}"/> base class.</param>
		/// <returns>
		/// The <see cref="ExternalObjectSerializer{T}"/> base class of the specified type;
		/// <c>null</c> if the class does not derive from this base class.
		/// </returns>
		public static Type GetExternalObjectSerializerBaseClass(Type eosType)
		{
			var eosBaseClassType = eosType.BaseType;
			while (eosBaseClassType != null)
			{
				if (eosBaseClassType.IsGenericType && eosBaseClassType.GetGenericTypeDefinition() == typeof(ExternalObjectSerializer<>))
					break;

				eosBaseClassType = eosBaseClassType.BaseType;
			}

			return eosBaseClassType;
		}
	}

}
