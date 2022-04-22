///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using GriffinPlus.Lib.Logging;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

// ReSharper disable InconsistentlySynchronizedField

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// Provides various information about assemblies and types in the current application domain.
	/// </summary>
	public static class AppDomainInfo
	{
		private static readonly LogWriter                                           sLog                     = LogWriter.Get(typeof(AppDomainInfo));
		private static          bool                                                sInitialized             = false;
		private static          bool                                                sInitializing            = false;
		private static readonly object                                              sInitializationSync      = new object();
		private static readonly ReaderWriterLockSlim                                sLock                    = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private static          ImmutableDictionary<Assembly, ImmutableArray<Type>> sTypesByAssembly         = ImmutableDictionary<Assembly, ImmutableArray<Type>>.Empty;
		private static          ImmutableDictionary<Assembly, ImmutableArray<Type>> sExportedTypesByAssembly = ImmutableDictionary<Assembly, ImmutableArray<Type>>.Empty;
		private static          ImmutableDictionary<string, ImmutableArray<Type>>   sTypesByFullName         = ImmutableDictionary<string, ImmutableArray<Type>>.Empty;
		private static          ImmutableDictionary<string, ImmutableArray<Type>>   sExportedTypesByFullName = ImmutableDictionary<string, ImmutableArray<Type>>.Empty;

#if NET5_0_OR_GREATER
		/// <summary>
		/// Triggers initializing the class asynchronously when the module is loaded to keep the delay at start as short as possible.
		/// </summary>
		[ModuleInitializer]
		internal static void ModuleInitializer()
		{
			TriggerInit();
		}
#endif

		/// <summary>
		/// Gets a dictionary mapping assemblies to types stored within them.
		/// The dictionary contains public and non-public types.
		/// </summary>
		public static ImmutableDictionary<Assembly, ImmutableArray<Type>> TypesByAssembly
		{
			get
			{
				Init();
				sLock.EnterReadLock();
				try { return sTypesByAssembly; }
				finally { sLock.ExitReadLock(); }
			}
		}

		/// <summary>
		/// Gets a dictionary mapping assemblies to types stored within them.
		/// The dictionary contains public types only.
		/// </summary>
		public static ImmutableDictionary<Assembly, ImmutableArray<Type>> ExportedTypesByAssembly
		{
			get
			{
				Init();
				sLock.EnterReadLock();
				try { return sExportedTypesByAssembly; }
				finally { sLock.ExitReadLock(); }
			}
		}

		/// <summary>
		/// Gets a dictionary mapping the full name of a type (namespace + type name) to the corresponding <see cref="Type"/> objects.
		/// Multiple assemblies may contain a type with the same full name.
		/// The dictionary contains public and non-public types.
		/// </summary>
		public static ImmutableDictionary<string, ImmutableArray<Type>> TypesByFullName
		{
			get
			{
				Init();
				sLock.EnterReadLock();
				try { return sTypesByFullName; }
				finally { sLock.ExitReadLock(); }
			}
		}

		/// <summary>
		/// Gets a dictionary mapping the full name of a type (namespace + type name) to the corresponding <see cref="Type"/> objects.
		/// Multiple assemblies may contain a type with the same full name.
		/// The dictionary contains public types only.
		/// </summary>
		public static ImmutableDictionary<string, ImmutableArray<Type>> ExportedTypesByFullName
		{
			get
			{
				Init();
				sLock.EnterReadLock();
				try { return sTypesByFullName; }
				finally { sLock.ExitReadLock(); }
			}
		}

		/// <summary>
		/// Initializes the class, if necessary.
		/// </summary>
		public static void Init()
		{
			if (!sInitialized)
			{
				lock (sInitializationSync)
				{
					if (!sInitialized && !sInitializing)
					{
						sInitializing = true;
						InitAssemblyInformation();
						sInitialized = true;
						sInitializing = false;
					}
				}
			}
		}

		/// <summary>
		/// Triggers initializing the class asynchronously, if necessary.
		/// </summary>
		public static void TriggerInit()
		{
			if (!sInitialized && !sInitializing)
			{
				lock (sInitializationSync)
				{
					if (!sInitialized && !sInitializing)
					{
						ThreadPool.QueueUserWorkItem(x => Init());
					}
				}
			}
		}

		/// <summary>
		/// Loads all assemblies in the application's base directory and all referenced assemblies and collects
		/// information about these assemblies and types within them.
		/// </summary>
		private static void InitAssemblyInformation()
		{
			// load all assemblies in the application's base directory recursively
			// (should cover plugin assemblies that may reside in a sub-directory)
			string path = AppDomain.CurrentDomain.BaseDirectory;
			var regex = new Regex(@"\.(exe|dll)$", RegexOptions.IgnoreCase);
			foreach (string filename in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
			{
				if (regex.IsMatch(filename))
				{
					try
					{
						sLog.Write(LogLevel.Debug, "Loading assembly in application directory ({0})...", filename);
						Assembly.LoadFrom(filename);
					}
					catch (Exception ex)
					{
						sLog.Write(LogLevel.Debug, "Loading assembly ({0}) failed.\nError: {1}.", filename, ex.Message);
					}
				}
			}

			// load all assemblies referenced by already loaded assemblies
			var processedAssemblies = new HashSet<Assembly>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				LoadReferencedAssemblies(assembly, processedAssemblies);
			}

			// register a handler to get notified when a new assembly is loaded
			AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoadEvent;

			// scan for types in loaded assemblies and referenced assemblies recursively
			sLock.EnterWriteLock();
			try
			{
				var typesByAssembly = sTypesByAssembly.ToDictionary(x => x.Key, x => x.Value.ToArray());
				var exportedTypesByAssembly = sExportedTypesByAssembly.ToDictionary(x => x.Key, x => x.Value.ToArray());
				var typesByFullName = sTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());
				var exportedTypesByFullName = sExportedTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InspectAssemblyAndCollectInformation(
						assembly,
						typesByAssembly,
						exportedTypesByAssembly,
						typesByFullName,
						exportedTypesByFullName);
				}

				sTypesByAssembly = typesByAssembly.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sExportedTypesByAssembly = exportedTypesByAssembly.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sTypesByFullName = typesByFullName.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sExportedTypesByFullName = exportedTypesByFullName.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
			}
			finally
			{
				sLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Loads all assemblies referenced by the specified assembly.
		/// </summary>
		/// <param name="assembly">Assembly whose references to load.</param>
		/// <param name="processedAssemblies">Set of already processed assemblies.</param>
		private static void LoadReferencedAssemblies(Assembly assembly, HashSet<Assembly> processedAssemblies = null)
		{
			// create a new set of loaded assemblies, if it was not specified (first run)
			if (processedAssemblies == null) processedAssemblies = new HashSet<Assembly>();

			// abort, if the specified assembly was already processed to avoid processing assemblies multiple times
			if (processedAssemblies.Contains(assembly))
				return;

			// remember that the assembly was processed
			processedAssemblies.Add(assembly);

			// load references of the assembly
			foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
			{
				sLog.Write(LogLevel.Debug, "Loading referenced assembly ({0})...", referencedAssemblyName);

				try
				{
					var referencedAssembly = Assembly.Load(referencedAssemblyName);
					LoadReferencedAssemblies(referencedAssembly, processedAssemblies);
				}
				catch (Exception ex)
				{
					sLog.Write(LogLevel.Debug, "Loading referenced assembly ({0}) failed.\nError: {1}.", referencedAssemblyName, ex.Message);
				}
			}
		}

		/// <summary>
		/// Inspects the specified assembly and collects information to expose.
		/// </summary>
		/// <param name="assembly">Assembly to inspect.</param>
		/// <param name="typesByAssembly">A dictionary mapping assemblies to public and non-public types stored within them.</param>
		/// <param name="exportedTypesByAssembly">A dictionary mapping assemblies to public types stored within them.</param>
		/// <param name="typesByFullName">
		/// A dictionary mapping the full name of public and non-public types (namespace + type name) to the corresponding
		/// <see cref="Type"/> objects.
		/// </param>
		/// <param name="exportedTypesByFullName">
		/// A dictionary mapping the full name of public types (namespace + type name) to the corresponding
		/// <see cref="Type"/> objects.
		/// </param>
		private static void InspectAssemblyAndCollectInformation(
			Assembly                       assembly,
			Dictionary<Assembly, Type[]>   typesByAssembly,
			Dictionary<Assembly, Type[]>   exportedTypesByAssembly,
			Dictionary<string, List<Type>> typesByFullName,
			Dictionary<string, List<Type>> exportedTypesByFullName)
		{
			// the executing thread should held the lock for writing as this modifies internal data
			Debug.Assert(sLock.IsWriteLockHeld);

			if (!typesByAssembly.TryGetValue(assembly, out var types))
			{
				// scan the assembly for types
				try { types = assembly.GetTypes(); }
				catch (ReflectionTypeLoadException ex) { types = ex.Types; }

				// add types to the table mapping assemblies to types defined in them
				types = types.Where(x => x != null).ToArray();
				typesByAssembly.Add(assembly, types);
				exportedTypesByAssembly.Add(assembly, types.Where(x => x.IsPublic).ToArray());

				// update the table mapping type names to type objects
				foreach (var type in types)
				{
					Debug.Assert(type.FullName != null, "type.FullName != null");
					if (!typesByFullName.TryGetValue(type.FullName, out var list))
					{
						list = new List<Type>();
						typesByFullName.Add(type.FullName, list);
					}

					list.Add(type);

					if (type.IsPublic)
					{
						if (!exportedTypesByFullName.TryGetValue(type.FullName, out list))
						{
							list = new List<Type>();
							exportedTypesByFullName.Add(type.FullName, list);
						}

						list.Add(type);
					}
				}
			}
		}

		/// <summary>
		/// Is called when a new assembly is loaded into the current application domain.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">Information about the loaded assembly.</param>
		private static void OnAssemblyLoadEvent(object sender, AssemblyLoadEventArgs args)
		{
			// the lock should not be held by the executing thread (this would mean that the assembly
			// was loaded during processing which is highly undesirable and may cause deadlocks)
			Debug.Assert(!sLock.IsReadLockHeld);
			Debug.Assert(!sLock.IsWriteLockHeld);

			// log that the assembly was loaded
			sLog.Write(
				LogLevel.Debug,
				"Assembly ({0}) was loaded after the initial scan, adding information for it...",
				args.LoadedAssembly.Location);

			// load all referenced assemblies to ensure all assemblies are loaded before beginning inspection
			// (new loaded assemblies trigger the AssemblyLoad event for every loaded assembly, so there is no need
			// to consider loaded referenced assemblies here)
			LoadReferencedAssemblies(args.LoadedAssembly);

			// inspect assembly and collect information about it
			sLock.EnterWriteLock();
			try
			{
				var typesByAssembly = sTypesByAssembly.ToDictionary(x => x.Key, x => x.Value.ToArray());
				var exportedTypesByAssembly = sExportedTypesByAssembly.ToDictionary(x => x.Key, x => x.Value.ToArray());
				var typesByFullName = sTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());
				var exportedTypesByFullName = sExportedTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());

				InspectAssemblyAndCollectInformation(
					args.LoadedAssembly,
					typesByAssembly,
					exportedTypesByAssembly,
					typesByFullName,
					exportedTypesByFullName);

				sTypesByAssembly = typesByAssembly.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sExportedTypesByAssembly = exportedTypesByAssembly.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sTypesByFullName = typesByFullName.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
				sExportedTypesByFullName = exportedTypesByFullName.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray());
			}
			finally
			{
				sLock.ExitWriteLock();
			}
		}
	}

}
