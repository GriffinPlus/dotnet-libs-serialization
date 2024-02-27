///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace GriffinPlus.Lib.Caching;

/// <summary>
/// A disk cache for various objects.
/// </summary>
public class ObjectDiskCache : IObjectCache, IDisposable
{
	private static          ObjectDiskCache sDefault                   = null;
	private static readonly object          sDefaultSync               = new();
	private static          string          sDefaultCacheDirectoryPath = null;
	private static readonly Regex           sCacheLockFileRegex        = new(@"^\[OBJ-CACHE\] (?<guid>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$", RegexOptions.Compiled);

	private readonly string         mCacheDirectoryPath;
	private readonly FileStream     mLockFile;
	private readonly AutoResetEvent mRunEvent;
	private readonly Queue<Action>  mThreadQueue;
	private          Thread         mThread;
	private          bool           mTerminateThread;

	/// <summary>
	/// Object cache items by the path of their cache file.
	/// </summary>
	internal readonly Dictionary<string, List<WeakReference>> Items;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObjectDiskCache"/> class.
	/// </summary>
	/// <param name="path">
	/// Path of the cache directory
	/// (<c>null</c> to use the default directory for temporary files).
	/// </param>
	public ObjectDiskCache(string path = null)
	{
		path ??= Path.Combine(Path.GetTempPath(), "ObjectDiskCache");
		mCacheDirectoryPath = Environment.ExpandEnvironmentVariables(path);

		// scan for orphaned cache instance directories
		CleanupCacheDirectory();

		// create and lock a new cache directory
		string cacheDirectoryName = $"[OBJ-CACHE] {Guid.NewGuid():D}";
		ObjectCacheDirectoryPath = Path.Combine(mCacheDirectoryPath, cacheDirectoryName);
		Directory.CreateDirectory(ObjectCacheDirectoryPath);
		string lockFilePath = Path.Combine(ObjectCacheDirectoryPath, "cache.lock");
		mLockFile = new FileStream(lockFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		Items = new Dictionary<string, List<WeakReference>>();

		// start worker thread
		mTerminateThread = false;
		mThreadQueue = new Queue<Action>();
		mRunEvent = new AutoResetEvent(false);
		mThread = new Thread(ThreadProc) { IsBackground = true, Name = "Object Disk Cache Manager" };
		mThread.Start();
	}

	/// <summary>
	/// Finalizes the current object.
	/// </summary>
	~ObjectDiskCache()
	{
		Dispose(false);
	}

	/// <summary>
	/// Disposes the current object releasing managed and native resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
	}

	/// <summary>
	/// Releases occupied resources.
	/// </summary>
	/// <param name="disposing">
	/// <c>true</c> in case of disposal;<br/>
	/// <c>false</c> in case of finalization.
	/// </param>
	public void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		StopWorkerThread();

		// try to delete the files that belong to cached items
		lock (Items)
		{
			foreach (KeyValuePair<string, List<WeakReference>> kvp in Items)
			{
				try { File.Delete(kvp.Key); }
				catch
				{
					/* swallow... */
				}
			}

			Items.Clear();
		}

		// abort if cache directory is not initialized
		if (ObjectCacheDirectoryPath == null)
			return;

		// delete the directory storing cached objects
		try
		{
			Directory.Delete(ObjectCacheDirectoryPath, true);
		}
		catch (Exception)
		{
			// removing directory failed
			// => most probably the lock file is still locked

			// dispose the lock file
			mLockFile?.Dispose();

			// try removing the directory once again
			try { Directory.Delete(ObjectCacheDirectoryPath, true); }
			catch (Exception)
			{
				/* swallow... */
			}
		}
	}

	/// <summary>
	/// Gets the default instance of the object disk cache caching objects in the temporary directory of the current user.
	/// </summary>
	public static ObjectDiskCache Default
	{
		get
		{
			if (sDefault != null)
				return sDefault;

			lock (sDefaultSync)
			{
				sDefault ??= new ObjectDiskCache(sDefaultCacheDirectoryPath);
			}

			return sDefault;
		}
	}

	/// <summary>
	/// Gets or sets the path of the cache directory that is used by default.
	/// </summary>
	public static string DefaultObjectCacheDirectory
	{
		get => sDefaultCacheDirectoryPath;
		set
		{
			lock (sDefaultSync)
			{
				sDefaultCacheDirectoryPath = value;
				sDefault = null; // the cache will be created when the Default property is accessed next time
			}
		}
	}

	/// <summary>
	/// Gets the path of the object cache directory.
	/// </summary>
	internal string ObjectCacheDirectoryPath { get; }

	/// <summary>
	/// Puts a serializable object into the cache.
	/// </summary>
	/// <typeparam name="T">Type of the object to put into the cache (can be its base type).</typeparam>
	/// <param name="obj">Object to put into the cache.</param>
	/// <returns>Cache item keeping track of the object.</returns>
	public IObjectCacheItem<T> Set<T>(T obj) where T : class
	{
		var item = new ObjectDiskCacheItem<T>(this, obj);
		lock (Items)
		{
			string path = item.GetObjectFilePath();
			if (!Items.TryGetValue(path, out List<WeakReference> weakReferences))
			{
				weakReferences = [];
				Items.Add(path, weakReferences);
			}

			weakReferences.Add(new WeakReference(item));
		}

		return item;
	}

	/// <summary>
	/// Entry point for the worker thread.
	/// </summary>
	private void ThreadProc()
	{
		var filesToDelete = new List<string>();
		int lastCheckTickCount = Environment.TickCount;

		while (!mTerminateThread)
		{
			bool set = mRunEvent.WaitOne(5000);

			if (set)
			{
				// execute next actions
				while (true)
				{
					Action action;
					lock (mThreadQueue)
					{
						if (mThreadQueue.Count == 0) break;
						action = mThreadQueue.Dequeue();
					}

					action();
				}
			}

			// skip check, if it is not due...
			if (Environment.TickCount - lastCheckTickCount <= 10000)
				continue;

			// check whether items have been garbage collected
			lock (Items)
			{
				foreach (KeyValuePair<string, List<WeakReference>> kvp in Items)
				{
					for (int i = 0; i < kvp.Value.Count; i++)
					{
						WeakReference weakReference = kvp.Value[i];
						object item = weakReference.Target;
						if (item == null) kvp.Value.RemoveAt(i);
					}

					// delete cache file, if all items referring to it were garbage collected
					if (kvp.Value.Count == 0)
						filesToDelete.Add(kvp.Key);
				}

				// => try to delete the files that belong to collected items
				foreach (string path in filesToDelete)
				{
					Items.Remove(path);
					try { File.Delete(path); }
					catch
					{
						/* swallow... */
					}
				}

				filesToDelete.Clear();
			}

			lastCheckTickCount = Environment.TickCount;
		}
	}

	/// <summary>
	/// Enqueues an action to be executed by the worker thread.
	/// </summary>
	/// <param name="action">Action to be executed by the worker thread.</param>
	internal void EnqueueAction(Action action)
	{
		lock (mThreadQueue) mThreadQueue.Enqueue(action);
		mRunEvent.Set();
	}

	/// <summary>
	/// Tells the worker thread to stop and waits for it to join.
	/// </summary>
	private void StopWorkerThread()
	{
		if (mThread == null) return;
		EnqueueAction(() => { mTerminateThread = true; });
		mThread.Join();
		mThread = null;
	}

	/// <summary>
	/// Scans the cache base directory for orphaned cache instance directories
	/// </summary>
	private void CleanupCacheDirectory()
	{
		try
		{
			foreach (string directoryPath in Directory.GetDirectories(mCacheDirectoryPath))
			{
				string directoryName = Path.GetFileName(directoryPath);
				Match match = sCacheLockFileRegex.Match(directoryName);
				if (!match.Success) continue;
				string cacheLockFilePath = Path.Combine(directoryPath, "cache.lock");
				try
				{
					// try to open the cache lock file
					if (File.Exists(cacheLockFilePath))
					{
						using (File.OpenRead(cacheLockFilePath)) { }
					}

					// opening the cache lock file succeeded
					// => cache directory is not in use anymore
					// => try to remove the directory
					Directory.Delete(directoryPath, true);
				}
				catch
				{
					// most probably the cache.lock file is in use
					// => swallow, we must not remove any files...
				}
			}
		}
		catch
		{
			// some error regarding the cache directory itself occurred
			// => swallow
		}
	}

	/// <summary>
	/// Gets the number of references to the specified object cache file.
	/// </summary>
	/// <param name="path">Path of the object cache file to check.</param>
	/// <returns>Number of references to the specified file.</returns>
	internal int GetFileReferenceCount(string path)
	{
		lock (Items)
		{
			return Items.TryGetValue(path, out List<WeakReference> weakReferences)
				       ? weakReferences.Count
				       : 0;
		}
	}

	/// <summary>
	/// Removes the reference of the current object cache item from the object cache and deletes
	/// the corresponding file, if no other object cache item refers to the file.
	/// </summary>
	/// <param name="item">Object cache item to remove from the object cache.</param>
	/// <returns>
	/// <c>true</c> if the specified item was the last item referring to the file;<br/>
	/// <c>false</c> if the file is shared with other items.
	/// </returns>
	internal bool RemoveItemFromCache<T>(ObjectDiskCacheItem<T> item) where T : class
	{
		lock (Items)
		{
			string path = item.GetObjectFilePath();
			List<WeakReference> weakReferences = Items[path];
			for (int i = 0; i < weakReferences.Count; i++)
			{
				WeakReference weakReference = weakReferences[i];
				if (!ReferenceEquals(weakReference.Target, item)) continue;
				weakReferences.RemoveAt(i);
				if (weakReferences.Count != 0) return false;
				try { File.Delete(path); }
				catch
				{
					/* swallow... */
				}

				Items.Remove(path);
				return true;
			}
		}

		throw new InvalidOperationException("The cache does not contain the specified item.");
	}

	/// <summary>
	/// Adds a reference of the specified object cache item to the object cache.
	/// </summary>
	/// <param name="item">Object cache item to add to the object cache.</param>
	internal void AddItemToCache<T>(ObjectDiskCacheItem<T> item) where T : class
	{
		lock (Items)
		{
			string path = item.GetObjectFilePath();
			if (!Items.TryGetValue(path, out List<WeakReference> weakReferences))
			{
				weakReferences = [];
				Items.Add(path, weakReferences);
			}

			weakReferences.Add(new WeakReference(item));
		}
	}
}
