///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Caches information about internal object serializers, external object serializers and
	/// types that are used in conjunction with the serializer.
	/// </summary>
	class SerializerCache
	{
		#region Inner Classes

		[Serializable]
		public class AssemblyInfo
		{
			private Guid   mGuid;
			private string mPath;

			public AssemblyInfo() { }

			public AssemblyInfo(Guid guid, string path)
			{
				mGuid = guid;
				mPath = path;
			}

			public Guid Guid
			{
				get => mGuid;
				set => mGuid = value;
			}

			public string Path
			{
				get => mPath;
				set => mPath = value;
			}
		}

		//--------------------------------------------------------------------------------------

		[Serializable]
		public class InternalObjectSerializerInfo
		{
			private string     mFullTypeName;
			private List<Guid> mAssemblyGuids;
			private uint       mSerializerVersion;

			public InternalObjectSerializerInfo()
			{
				mAssemblyGuids = new List<Guid>();
			}

			public InternalObjectSerializerInfo(string fullTypeName, List<Guid> assemblyGuids, uint version)
			{
				mFullTypeName = fullTypeName;
				mAssemblyGuids = assemblyGuids;
				mSerializerVersion = version;
			}

			public string FullTypeName
			{
				get => mFullTypeName;
				set => mFullTypeName = value;
			}

			public List<Guid> AssemblyGuids
			{
				get => mAssemblyGuids;
				set => mAssemblyGuids = value;
			}

			public uint SerializerVersion
			{
				get => mSerializerVersion;
				set => mSerializerVersion = value;
			}
		}

		//--------------------------------------------------------------------------------------

		[Serializable]
		public class ExternalObjectSerializerInfo
		{
			private string mSerializerFullTypeName;
			private Guid   mSerializerAssemblyGuid;
			private string mSerializeeFullTypeName;
			private Guid   mSerializeeAssemblyGuid;

			public ExternalObjectSerializerInfo() { }

			public ExternalObjectSerializerInfo(
				string serializerFullTypeName,
				Guid   serializerAssemblyGuid,
				string serializeeFullTypeName,
				Guid   serializeeAssemblyGuid)
			{
				mSerializerFullTypeName = serializerFullTypeName;
				mSerializerAssemblyGuid = serializerAssemblyGuid;
				mSerializeeFullTypeName = serializeeFullTypeName;
				mSerializeeAssemblyGuid = serializeeAssemblyGuid;
			}

			public string SerializerFullTypeName
			{
				get => mSerializerFullTypeName;
				set => mSerializerFullTypeName = value;
			}

			public string SerializeeFullTypeName
			{
				get => mSerializeeFullTypeName;
				set => mSerializeeFullTypeName = value;
			}

			public Guid SerializerAssemblyGuid
			{
				get => mSerializerAssemblyGuid;
				set => mSerializerAssemblyGuid = value;
			}

			public Guid SerializeeAssemblyGuid
			{
				get => mSerializeeAssemblyGuid;
				set => mSerializeeAssemblyGuid = value;
			}
		}

		//--------------------------------------------------------------------------------------

		[Serializable]
		public class EnumerationInfo
		{
			private string     mFullTypeName;
			private List<Guid> mAssemblyGuids;

			public EnumerationInfo()
			{
				mAssemblyGuids = new List<Guid>();
			}

			public EnumerationInfo(string fullTypeName, List<Guid> assemblyGuids)
			{
				mFullTypeName = fullTypeName;
				mAssemblyGuids = assemblyGuids;
			}

			public string FullTypeName
			{
				get => mFullTypeName;
				set => mFullTypeName = value;
			}

			public List<Guid> AssemblyGuids
			{
				get => mAssemblyGuids;
				set => mAssemblyGuids = value;
			}
		}

		#endregion

		#region Class Variables

		private static readonly LogWriter       sLog                      = LogWriter.Get<SerializerCache>();
		private static readonly Type[]          sConstructorArgumentTypes = { typeof(SerializerArchive) };
		private static readonly object          sSync                     = new object();
		private static volatile SerializerCache sInstance                 = null;

		#endregion

		#region Member Variables

		private readonly Dictionary<Guid, AssemblyInfo>                       mAssemblyGuidToAssemblyInfo;
		private readonly Dictionary<string, AssemblyInfo>                     mAssemblyPathToAssemblyInfo;
		private readonly Dictionary<Type, List<ExternalObjectSerializerInfo>> mTypeToExternalObjectSerializerInfo;
		private readonly Dictionary<Type, InternalObjectSerializerInfo>       mTypeToInternalObjectSerializerInfo;
		private readonly Dictionary<Type, EnumerationInfo>                    mEnumTypeToAssemblyInfo;

		#endregion

		#region Initialization

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerCache"/> class.
		/// </summary>
		private SerializerCache()
		{
			mAssemblyGuidToAssemblyInfo = new Dictionary<Guid, AssemblyInfo>();
			mAssemblyPathToAssemblyInfo = new Dictionary<string, AssemblyInfo>();
			mTypeToExternalObjectSerializerInfo = new Dictionary<Type, List<ExternalObjectSerializerInfo>>();
			mTypeToInternalObjectSerializerInfo = new Dictionary<Type, InternalObjectSerializerInfo>();
			mEnumTypeToAssemblyInfo = new Dictionary<Type, EnumerationInfo>();

			// initialize the cache
			Initialize();
			PrintToLog(LogLevel.Debug);
		}

		#endregion

		#region Singleton Property

		/// <summary>
		/// Gets the singleton instance of the <see cref="SerializerCache"/> class.
		/// </summary>
		public static SerializerCache Instance
		{
			get
			{
				if (sInstance == null)
				{
					lock (sSync)
					{
						if (sInstance == null)
						{
							sInstance = new SerializerCache();
						}
					}
				}

				return sInstance;
			}
		}

		#endregion

		#region Checking whether the Cache Contains Data

		/// <summary>
		/// Checks whether the cache contains data.
		/// </summary>
		/// <returns>
		/// <c>true</c>if the cache contains data; otherwise <c>false</c>.
		/// </returns>
		public bool ContainsData()
		{
			return mTypeToExternalObjectSerializerInfo.Count > 0 || mTypeToInternalObjectSerializerInfo.Count > 0;
		}

		#endregion

		#region Loading an Assembly by its Name

		/// <summary>
		/// Loads an assembly by its full name.
		/// </summary>
		/// <param name="name">Full name of the assembly.</param>
		/// <returns>
		/// The loaded assembly;
		/// <c>null</c>, if the assembly could not be loaded.
		/// </returns>
		public Assembly LoadAssemblyByFullName(string name)
		{
			var assembly = Assembly.Load(name);
			if (assembly != null)
				return assembly;

			/*
			string directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower();
			name = name.ToLower();

			// get all assemblies that match the specified short name
			List<AssemblyInfo> localAssemblyInfos = new List<AssemblyInfo>();
			Dictionary<int, List<AssemblyInfo>> deeperAssemblyInfos = new Dictionary<int, List<AssemblyInfo>>();
			List<AssemblyInfo> otherAssemblyInfos = new List<AssemblyInfo>();
			foreach (KeyValuePair<string, AssemblyInfo> kvp in mAssemblyPathToAssemblyInfo)
			{
				string path = kvp.Value.Path.ToLower();
				if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
				{
					if (System.IO.Path.GetDirectoryName(path) == directory)
					{
						localAssemblyInfos.Add(kvp.Value);
					}
					else if (path.StartsWith(directory))
					{
						string[] split = path.Remove(0, directory.Length+1).Split('\\');
						
						List<AssemblyInfo> list;
						if (!deeperAssemblyInfos.TryGetValue(split.Length, out list)) {
							list = new List<AssemblyInfo>();
							deeperAssemblyInfos.Add(split.Length, list);
						}

						list.Add(kvp.Value);
					}
					else
					{
						otherAssemblyInfos.Add(kvp.Value);
					}
				}
			}

			// check assemblies in the local directory first
			foreach (AssemblyInfo info in localAssemblyInfos)
			{
				try {
					return Assembly.LoadFrom(info.Path);
				} catch (Exception) { }
			}

			// check assemblies below the local directory
			for (int i = 0; deeperAssemblyInfos.Count > 0; i++)
			{
				List<AssemblyInfo> list;
				if (deeperAssemblyInfos.TryGetValue(i, out list))
				{
					foreach (AssemblyInfo info in list) {
						try {
							return Assembly.LoadFrom(info.Path);
						} catch (Exception) { }
					}
					
					deeperAssemblyInfos.Remove(i);
				}
			}

			// check assemblies in other directories
			foreach (AssemblyInfo info in otherAssemblyInfos)
			{
				try {
					return Assembly.LoadFrom(info.Path);
				} catch (Exception) { }
			}
			*/
			return null;
		}

		#endregion

		#region Initializing the Cache

		/// <summary>
		/// Populates the cache.
		/// </summary>
		private void Initialize()
		{
			sLog.Write(LogLevel.Debug, "Initializing cache with existing assemblies...");

			string path = AppDomain.CurrentDomain.BaseDirectory;

			// DLL files
			foreach (string filename in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
			{
				if (!mAssemblyPathToAssemblyInfo.ContainsKey(filename))
				{
					try
					{
						ScanAssembly(filename);
					}
					catch (Exception)
					{
						// swallow...
					}
				}
			}

			// EXE files
			foreach (string filename in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
			{
				if (!mAssemblyPathToAssemblyInfo.ContainsKey(filename))
				{
					try
					{
						ScanAssembly(filename);
					}
					catch (Exception)
					{
						// swallow...
					}
				}
			}
		}


		/// <summary>
		/// Scans the assembly at the specified path (for internal use only).
		/// </summary>
		/// <param name="path">Path of the assembly to scan.</param>
		private void ScanAssembly(string path)
		{
			var assembly = Assembly.LoadFrom(path);
			AddExternalObjectSerializers(assembly);
			AddInternalObjectSerializers(assembly);
			AddEnumerations(assembly);
		}

		/// <summary>
		/// Adds external object serializers stored in the specified assembly to the cache (for internal use only).
		/// </summary>
		/// <param name="assembly">Assembly to scan for external object serializers.</param>
		private void AddExternalObjectSerializers(Assembly assembly)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}

			foreach (var type in types)
			{
				if (type == null)
					continue;

				if (type.IsClass)
				{
					// a class
					object[] attributes = type.GetCustomAttributes(typeof(ExternalObjectSerializerAttribute), false);
					bool attributeOk = attributes.Length > 0;
					bool interfaceOk = typeof(IExternalObjectSerializer).IsAssignableFrom(type);

					if (attributeOk && interfaceOk)
					{
						// class is annotated with the external object serializer attribute and implements the appropriate interface
						foreach (ExternalObjectSerializerAttribute attribute in attributes)
						{
							SetExternalObjectSerializer(type);
						}
					}
					else if (attributeOk || interfaceOk)
					{
						if (!attributeOk)
						{
							// attribute is missing
							sLog.Write(
								LogLevel.Error,
								"Class '{0}' seems to be an external serializer class, but it is not annotated with the '{1}' attribute.",
								type.FullName,
								typeof(ExternalObjectSerializerAttribute).FullName);
						}

						if (!interfaceOk)
						{
							// interface is missing
							sLog.Write(
								LogLevel.Error,
								"Class '{0}' seems to be an external serializer class, but does not implement the '{1}' interface.",
								type.FullName,
								typeof(IExternalObjectSerializer).FullName);
						}
					}
				}
			}
		}


		/// <summary>
		/// Adds internal object serializers stored in the specified assembly to the cache (for internal use only).
		/// </summary>
		/// <param name="assembly">Assembly to scan for internal object serializers.</param>
		private void AddInternalObjectSerializers(Assembly assembly)
		{
			// get types in the assembly
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}

			// check type
			foreach (var type in types)
			{
				if (type == null)
					continue;

				if (type.IsClass || type.IsValueType && !type.IsPrimitive) // class or struct
				{
					// a class
					object[] iosAttributes = type.GetCustomAttributes(typeof(InternalObjectSerializerAttribute), false);
					bool iosAttributeOk = iosAttributes.Length > 0;
					bool interfaceOk = typeof(IInternalObjectSerializer).IsAssignableFrom(type);
					bool constructorOk = type.GetConstructor(BindingFlags.ExactBinding | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, sConstructorArgumentTypes, null) != null;
					var serializeMethodInfo = type.GetMethod("Serialize", new[] { typeof(SerializerArchive), typeof(uint) });
					bool virtualSerializeMethod = serializeMethodInfo != null && serializeMethodInfo.IsVirtual && !serializeMethodInfo.IsFinal;

					if (iosAttributeOk && interfaceOk && constructorOk && !virtualSerializeMethod)
					{
						// class is annotated with the internal object serializer attribute and implements the appropriate interface
						SetInternalObjectSerializer(type);
					}
					else if (iosAttributeOk || interfaceOk) // || constructorOk <-- do not check this, since this will create false alarms for classes taking a SerializerArchive in the constructor
					{
						if (!iosAttributeOk)
						{
							// attribute is missing
							sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it is not annotated with the '{1}' attribute.", type.FullName, typeof(InternalObjectSerializerAttribute).FullName);
						}

						if (!interfaceOk)
						{
							// interface is missing
							sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it does not implement the '{1}' interface.", type.FullName, typeof(IInternalObjectSerializer).FullName);
						}

						if (!constructorOk)
						{
							// serialization constructor is missing
							sLog.Write(LogLevel.Error, "Class '{0}' seems to be an internal serializer class, but it lacks the serialization constructor.", type.FullName);
						}

						if (virtualSerializeMethod)
						{
							// 'Serialize' method is virtual
							sLog.Write(
								LogLevel.Error,
								"Class '{0}' seems to be an internal serializer class, but its 'Serialize' method is virtual which will cause problems when serializing nested classes. You should overwrite the 'Serialize' method in derived classes instead.",
								type.FullName);
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds enumerations stored in the specified assembly to the cache (for internal use only).
		/// </summary>
		/// <param name="assembly">Assembly to scan for enumerations.</param>
		private void AddEnumerations(Assembly assembly)
		{
			// get types in the assembly
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}

			// check type
			foreach (var type in types)
			{
				if (type == null)
					continue;

				if (type.IsEnum)
					SetEnumeration(type);
			}
		}

		#endregion

		#region Printing Cache to the Log

		/// <summary>
		/// Prints information about internal object serializers and external object serializers to the log.
		/// </summary>
		public void PrintToLog(LogLevel level)
		{
			// internal object serializers
			if (mTypeToInternalObjectSerializerInfo.Count > 0)
			{
				sLog.Write(level, "Known Internal Object Serializers:");
				foreach (var kvp in mTypeToInternalObjectSerializerInfo)
				{
					if (kvp.Value.AssemblyGuids.Count > 1)
					{
						var builder = new StringBuilder();
						builder.AppendFormat("-> {0}", kvp.Key.FullName);
						builder.AppendLine();

						for (int i = 0; i < kvp.Value.AssemblyGuids.Count; i++)
						{
							var ai = mAssemblyGuidToAssemblyInfo[kvp.Value.AssemblyGuids[i]];
							builder.AppendFormat("   -> {0}", ai.Path);
							builder.AppendLine();
						}

						sLog.Write(level, builder.ToString());
					}
					else
					{
						var ai = mAssemblyGuidToAssemblyInfo[kvp.Value.AssemblyGuids[0]];
						sLog.Write(level, "-> {0} ({1})", kvp.Key.FullName, ai.Path);
					}
				}
			}
			else
			{
				sLog.Write(level, "Known Internal Object Serializers: <none>");
			}

			// external object serializers
			if (mTypeToExternalObjectSerializerInfo.Count > 0)
			{
				sLog.Write(level, "Known External Object Serializers:");
				foreach (var kvp in mTypeToExternalObjectSerializerInfo)
				{
					foreach (var eosi in kvp.Value)
					{
						var ai1 = mAssemblyGuidToAssemblyInfo[eosi.SerializerAssemblyGuid];
						var ai2 = mAssemblyGuidToAssemblyInfo[eosi.SerializeeAssemblyGuid];
						sLog.Write(level, "-> {0} ({1}) for type {2} ({3})", eosi.SerializerFullTypeName, ai1.Path, eosi.SerializeeFullTypeName, ai2.Path);
					}
				}
			}
			else
			{
				sLog.Write(level, "Known External Object Serializers: <none>");
			}
		}

		#endregion

		#region Internal Object Serializers

		/// <summary>
		/// Checks whether the specified type has an internal object serializer.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="version">Max. supported serializer version.</param>
		/// <returns>
		/// <c>true</c>, if the specified type has an internal object serializer;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool HasInternalObjectSerializer(Type type, out uint version)
		{
			if (mTypeToInternalObjectSerializerInfo.TryGetValue(type, out var info))
			{
				version = info.SerializerVersion;
				return true;
			}

			if (type.IsGenericType)
			{
				var genericTypeDefinition = type.GetGenericTypeDefinition();
				if (mTypeToInternalObjectSerializerInfo.TryGetValue(genericTypeDefinition, out info))
				{
					version = info.SerializerVersion;
					return true;
				}
			}

			version = 0;
			return false;
		}

		#endregion

		#region Setting Internal Object Serializers, External Object Serializers and Enumerations

		/// <summary>
		/// Sets an internal object serializer (for internal use only).
		/// </summary>
		/// <param name="type">Type of the internal object serializer class (same as the serializee).</param>
		private void SetInternalObjectSerializer(Type type)
		{
			// determine the path of the serializer/serializee
			string assemblyPath = type.Assembly.Location;

			// get the assembly information object, if the assembly is already known
			// or create a new information object for the assembly
			// -----------------------------------------------------------------------------------------------------------
			if (!mAssemblyPathToAssemblyInfo.TryGetValue(assemblyPath, out var assemblyInfo))
			{
				assemblyInfo = new AssemblyInfo(Guid.NewGuid(), assemblyPath);
				mAssemblyPathToAssemblyInfo.Add(assemblyInfo.Path, assemblyInfo);
				mAssemblyGuidToAssemblyInfo.Add(assemblyInfo.Guid, assemblyInfo);
			}

			// store information about the internal object serializer type (same as the type to serialize)
			// -----------------------------------------------------------------------------------------------------------
			if (!mTypeToInternalObjectSerializerInfo.TryGetValue(type, out var infos))
			{
				var attributes = type.GetCustomAttributes<InternalObjectSerializerAttribute>(false);
				var versionAttribute = attributes.First();
				uint version = versionAttribute.Version;
				infos = new InternalObjectSerializerInfo(type.FullName, new List<Guid>(), version);
				mTypeToInternalObjectSerializerInfo.Add(type, infos);
			}

			if (infos.AssemblyGuids.Find(other => other == assemblyInfo.Guid) == default)
			{
				infos.AssemblyGuids.Add(assemblyInfo.Guid);
			}
		}


		/// <summary>
		/// Sets an external object serializer.
		/// </summary>
		/// <param name="serializerType">Type of the external object serializer class.</param>
		private void SetExternalObjectSerializer(Type serializerType)
		{
			Serializer.RegisterExternalObjectSerializer(serializerType);

			//// determine the path of the external object serializer
			//string serializerAssemblyPath = serializerType.Assembly.Location;

			//// get the assembly information object, if the assembly is already known
			//// or create a new information object for the assembly 
			//AssemblyInfo serializerAssemblyInfo;
			//if (!mAssemblyPathToAssemblyInfo.TryGetValue(serializerAssemblyPath, out serializerAssemblyInfo))
			//{
			//  serializerAssemblyInfo = new AssemblyInfo(Guid.NewGuid(), serializerAssemblyPath);
			//  mAssemblyPathToAssemblyInfo.Add(serializerAssemblyInfo.Path, serializerAssemblyInfo);
			//  mAssemblyGuidToAssemblyInfo.Add(serializerAssemblyInfo.Guid, serializerAssemblyInfo);
			//}

			//// determine the type the external object serializer is responsible for
			//object[] attributes = serializerType.GetCustomAttributes(typeof(ExternalObjectSerializerAttribute), false);
			//foreach (ExternalObjectSerializerAttribute attribute in attributes)
			//{
			//  Type serializeeType = attribute.TypeToSerialize;
			//  string serializeeAssemblyPath = serializeeType.Assembly.Location;

			//  // get or create an AssemblyInfo object for the assembly containing the type to serialize
			//  AssemblyInfo serializeeAssemblyInfo;
			//  if (!mAssemblyPathToAssemblyInfo.TryGetValue(serializeeAssemblyPath, out serializeeAssemblyInfo))
			//  {
			//    serializeeAssemblyInfo = new AssemblyInfo(Guid.NewGuid(), serializeeAssemblyPath);
			//    mAssemblyPathToAssemblyInfo.Add(serializeeAssemblyInfo.Path, serializeeAssemblyInfo);
			//    mAssemblyGuidToAssemblyInfo.Add(serializeeAssemblyInfo.Guid, serializeeAssemblyInfo);
			//  }

			//  // get information about the type to serialize by its full name (namespace + class name)
			//  // and create an empty record for it, if it does not exist, yet...
			//  List<ExternalObjectSerializerInfo> serializers;
			//  if (!mTypeToExternalObjectSerializerInfo.TryGetValue(serializeeType, out serializers)) {
			//    serializers = new List<ExternalObjectSerializerInfo>();
			//    mTypeToExternalObjectSerializerInfo.Add(serializeeType, serializers);
			//  }

			//  // add serializer, if it is not known, yet...
			//  List<ExternalObjectSerializerInfo> matches = serializers.FindAll((ExternalObjectSerializerInfo other) => { return other.SerializeeAssemblyGuid == serializeeAssemblyInfo.Guid; });
			//  ExternalObjectSerializerInfo match = matches.Find((ExternalObjectSerializerInfo other) => { return other.SerializerAssemblyGuid == serializerAssemblyInfo.Guid; });
			//  if (match == null) {
			//    serializers.Add(new ExternalObjectSerializerInfo(serializerType.FullName, serializerAssemblyInfo.Guid, serializeeType.FullName, serializeeAssemblyInfo.Guid));
			//  }
			//}
		}

		/// <summary>
		/// Sets an enumeration (for internal use only).
		/// </summary>
		/// <param name="type">Enumeration type.</param>
		private void SetEnumeration(Type type)
		{
			// determine the path of the assembly containing the enumeration
			string assemblyPath = type.Assembly.Location;

			// get the assembly information object, if the assembly is already known
			// or create a new information object for the assembly
			// -----------------------------------------------------------------------------------------------------------
			if (!mAssemblyPathToAssemblyInfo.TryGetValue(assemblyPath, out var assemblyInfo))
			{
				assemblyInfo = new AssemblyInfo(Guid.NewGuid(), assemblyPath);
				mAssemblyPathToAssemblyInfo.Add(assemblyInfo.Path, assemblyInfo);
				mAssemblyGuidToAssemblyInfo.Add(assemblyInfo.Guid, assemblyInfo);
			}

			// store information about the enumeration type
			// -----------------------------------------------------------------------------------------------------------

			if (!mEnumTypeToAssemblyInfo.TryGetValue(type, out var infos))
			{
				infos = new EnumerationInfo(type.FullName, new List<Guid>());
				mEnumTypeToAssemblyInfo.Add(type, infos);
			}

			if (infos.AssemblyGuids.Find(other => other == assemblyInfo.Guid) == default)
			{
				infos.AssemblyGuids.Add(assemblyInfo.Guid);
			}
		}

		#endregion

		#region Getting an External Object Serializer

		///// <summary>
		///// Gets an external object serializer for the specified type (if available).
		///// </summary>
		///// <param name="type">Type to get an external object serializer for.</param>
		///// <returns>
		///// Type of the external object serializer for the specified type;
		///// null, if no external object serializer exists for the specified type.
		///// </returns>
		//public IExternalObjectSerializer GetExternalObjectSerializer(Type type)
		//{
		//	string serializeePath = type.Assembly.Location;
		//	List<ExternalObjectSerializerInfo> serializers;
		//	if (mTypeToExternalObjectSerializerInfo.TryGetValue(type, out serializers))
		//	{
		//		for (int i = 0; i < serializers.Count; i++)
		//		{
		//			if (mAssemblyGuidToAssemblyInfo[serializers[i].SerializeeAssemblyGuid].Path == serializeePath)
		//			{
		//				string path = mAssemblyGuidToAssemblyInfo[serializers[i].SerializerAssemblyGuid].Path;
		//				try {
		//					Assembly assembly = Assembly.LoadFrom(path);
		//					return assembly.GetType(serializers[i].SerializerFullTypeName);
		//				} catch (Exception ex) {
		//					sLog.Write(LogLevel.Error, "Getting external object serializer failed ({0}).", ex.Message);
		//				}
		//			}
		//		}
		//	}

		//	return null;
		//}

		#endregion
	}

}
