///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

using GriffinPlus.Lib.Events;
using GriffinPlus.Lib.Serialization;

namespace GriffinPlus.Lib.Caching
{

	/// <summary>
	/// An item in the <see cref="ObjectDiskCache"/>.
	/// </summary>
	/// <typeparam name="T">Type of object stored in the item.</typeparam>
	public class ObjectDiskCacheItem<T> : IObjectCacheItem<T> where T : class
	{
		private enum ItemState
		{
			InUse,
			SavePending,
			SaveAndDisposePending,
			Disposed
		}

		private readonly ObjectDiskCache mCache;
		private          Guid            mObjectGuid;
		private          bool            mAsync;
		private          WeakReference   mWeakReference;
		private          T               mStrongReference;
		private          bool            mCreateNewWhenModified;
		private          bool            mGetDelayedValueInProgress = false;
		private          ItemState       mState                     = ItemState.InUse;
		private readonly object          mSync                      = new object();

		/// <summary>
		/// Occurs when a property changes.
		/// The event is raised using the synchronization context of the thread registering the event, if possible.
		/// Otherwise the event is raised by a worker thread.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged
		{
			add => PropertyChangedEventManager.RegisterEventHandler(this, value, SynchronizationContext.Current, true);
			remove => PropertyChangedEventManager.UnregisterEventHandler(this, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDiskCacheItem{T}"/> class.
		/// </summary>
		/// <param name="cache">Object disk cache the item belongs to.</param>
		private ObjectDiskCacheItem(ObjectDiskCache cache)
		{
			mCache = cache;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDiskCacheItem{T}"/> class.
		/// </summary>
		/// <param name="cache">Object disk cache the item belongs to.</param>
		/// <param name="obj">Object to keep in the item.</param>
		/// <exception cref="SerializationException">The specified object is not serializable.</exception>
		internal ObjectDiskCacheItem(ObjectDiskCache cache, T obj)
		{
			if (!Serializer.IsSerializable(obj))
				throw new SerializationException("The specified object ({0}) is not serializable.", obj.GetType().FullName);

			lock (mSync)
			{
				mCache = cache;
				mObjectGuid = Guid.NewGuid();
				mStrongReference = obj;
				mWeakReference = obj != null ? new WeakReference(obj) : null;
				mAsync = obj != null && Immutability.IsImmutable(obj.GetType());
				mState = ItemState.SavePending;
				if (mAsync) mCache.EnqueueAction(SaveObject);
				else SaveObject();
			}
		}

		/// <summary>
		/// Disposes the current object cache item removing its object file from disk.
		/// The referenced object is not touched.
		/// </summary>
		public void Dispose()
		{
			lock (mSync)
			{
				if (mObjectGuid == Guid.Empty)
				{
					mStrongReference = null;
					mWeakReference = null;
					mState = ItemState.Disposed;
					return;
				}

				switch (mState)
				{
					case ItemState.InUse:
					{
						mCache.RemoveItemFromCache(this);
						mStrongReference = null;
						mWeakReference = null;
						mState = ItemState.Disposed;
						break;
					}

					case ItemState.SavePending:
					{
						if (mCache.GetFileReferenceCount(GetObjectFilePath()) > 1)
						{
							// there is other cache item referencing the same object cache file and the current item is responsible for saving the item
							// => finish saving and dispose the current item afterwards...
							mState = ItemState.SaveAndDisposePending;
						}
						else
						{
							// the current item is about to save its object, but it is the only item that references the object
							// => it's save to skip saving...
							mCache.RemoveItemFromCache(this);
							mStrongReference = null;
							mWeakReference = null;
							mState = ItemState.Disposed;
						}

						break;
					}

					case ItemState.SaveAndDisposePending:
					case ItemState.Disposed:
						break;
				}
			}
		}

		/// <summary>
		/// Gets or sets the object associated with the cache item.
		/// </summary>
		public T Value
		{
			get
			{
				lock (mSync)
				{
					if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
						throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

					mGetDelayedValueInProgress = false;
					if (mWeakReference == null) return null;
					if (mWeakReference.Target is T obj) return obj;
					obj = LoadObject();
					mWeakReference.Target = obj;
					OnPropertyChanged(nameof(Value), obj);
					OnPropertyChanged(nameof(ValueDelayed), obj);
					return obj;
				}
			}

			set
			{
				if (!Serializer.IsSerializable(value))
					throw new SerializationException("The specified object ({0}) is not serializable.", value.GetType().FullName);

				lock (mSync)
				{
					if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
						throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

					bool hadValue = mWeakReference != null;

					if (value != null)
					{
						if (mWeakReference != null) mWeakReference.Target = value;
						else mWeakReference = new WeakReference(value);
					}
					else
					{
						mWeakReference = null;
					}

					if (mCreateNewWhenModified)
					{
						// finish pending save operation, if the cache file is shared among multiple cache items
						bool finishWritingCacheFile = mState == ItemState.SavePending && mCache.GetFileReferenceCount(GetObjectFilePath()) > 1;
						if (finishWritingCacheFile) SaveObject();

						// change object GUID
						mCache.RemoveItemFromCache(this);
						mObjectGuid = Guid.NewGuid();
						mCache.AddItemToCache(this);
					}

					mStrongReference = value;
					mState = ItemState.SavePending;
					if (mAsync) mCache.EnqueueAction(SaveObject);
					else SaveObject();
					OnPropertyChanged(nameof(Value), value);
					OnPropertyChanged(nameof(ValueDelayed), value);

					bool hasValue = mWeakReference != null;
					if ((hadValue && !hasValue) || (!hadValue && hasValue))
					{
						OnPropertyChanged(nameof(HasValue), value);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the object associated with the cache item.
		/// </summary>
		object IObjectCacheItem.Value
		{
			get => Value;
			set => Value = (T)value;
		}

		/// <summary>
		/// Gets or sets the object associated with the cache item.
		/// Returns <c>null</c> if the object is not in memory, yet, triggers loading the object and raises the
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		public T ValueDelayed
		{
			get
			{
				lock (mSync)
				{
					if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
						throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

					if (mWeakReference == null) return null;
					if (mWeakReference.Target is T obj) return obj;
					if (!mGetDelayedValueInProgress)
					{
						mGetDelayedValueInProgress = true;
						ThreadPool.QueueUserWorkItem(
							x =>
							{
								_ = Value;
							});
					}

					return null;
				}
			}

			set => Value = value;
		}

		/// <summary>
		/// Gets or sets the object associated with the cache item
		/// Returns <c>null</c>, if the object is not in memory, yet, triggers loading the object and raises the
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		object IObjectCacheItem.ValueDelayed
		{
			get => ValueDelayed;
			set => ValueDelayed = (T)value;
		}

		/// <summary>
		/// Gets a value indicating whether the cache item has a value.
		/// </summary>
		public bool HasValue
		{
			get
			{
				lock (mSync)
				{
					if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
						throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

					return mWeakReference != null;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the value of the cache item is still in memory.
		/// </summary>
		public bool IsValueInMemory
		{
			get
			{
				lock (mSync)
				{
					if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
						throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

					return mWeakReference != null && mWeakReference.IsAlive;
				}
			}
		}

		/// <summary>
		/// Gets the type of the object cache item.
		/// </summary>
		public Type Type => typeof(T);

		/// <summary>
		/// Disposes the object by intent and removes the reference to it scheduling it for garbage collection
		/// (it is reloaded from disk on demand as soon as it is accessed).
		/// </summary>
		public void DropObject()
		{
			lock (mSync)
			{
				if (mState == ItemState.SavePending || mState == ItemState.SaveAndDisposePending)
					throw new InvalidOperationException("The cached object is scheduled to be saved, so it cannot be dropped, yet.");

				if (mState == ItemState.InUse)
				{
					if (mWeakReference != null)
					{
						if (mWeakReference.Target is IDisposable obj) obj.Dispose();
						mWeakReference.Target = null;
					}
				}
			}
		}

		/// <summary>
		/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
		/// at the beginning, but as soon as it is changed a new file is created.
		/// </summary>
		/// <returns>Duplicate of the current object cache item.</returns>
		IObjectCacheItem IObjectCacheItem.Dupe()
		{
			return Dupe();
		}

		/// <summary>
		/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
		/// at the beginning, but as soon as it is changed a new file is created.
		/// </summary>
		/// <returns>Duplicate of the current object cache item.</returns>
		IObjectCacheItem<T> IObjectCacheItem<T>.Dupe()
		{
			return Dupe();
		}

		/// <summary>
		/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
		/// at the beginning, but as soon as it is changed a new file is created.
		/// </summary>
		/// <returns>Duplicate of the current object cache item.</returns>
		public ObjectDiskCacheItem<T> Dupe()
		{
			lock (mSync)
			{
				// create new object cache item
				var newObjectCacheItem = new ObjectDiskCacheItem<T>(mCache)
				{
					mAsync = mAsync,
					mObjectGuid = mObjectGuid,
					mStrongReference = null,
					mCreateNewWhenModified = true
				};

				if (mWeakReference != null)
					newObjectCacheItem.mWeakReference = new WeakReference(mWeakReference.Target);

				// associate the object cache item with the corresponding file
				mCache.AddItemToCache(newObjectCacheItem);

				mCreateNewWhenModified = true;
				return newObjectCacheItem;
			}
		}

		/// <summary>
		/// Assigns the the specified object cache item to the current one (the specified item is disposed at the end).
		/// </summary>
		/// <param name="item">Object cache item to assign.</param>
		public void TakeOwnership(IObjectCacheItem item)
		{
			if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
				throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

			if (item.GetType() != typeof(ObjectDiskCacheItem<T>))
				throw new ArgumentException("The item to assign does not have the same type as the current item.");

			var other = (ObjectDiskCacheItem<T>)item;
			if (other.mState == ItemState.Disposed || other.mState == ItemState.SaveAndDisposePending)
				throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

			bool ownLockTaken = false;
			bool otherLockTaken = false;
			try
			{
				while (true)
				{
					Monitor.TryEnter(mSync, ref ownLockTaken);
					Monitor.TryEnter(other.mSync, ref otherLockTaken);
					if (ownLockTaken && otherLockTaken)
					{
						// ensure both cache items are in the same cache
						if (mCache != other.mCache)
							throw new ArgumentException("The specified item does not belong to the same cache as the current item.");

						// retrieve other object in memory, if possible
						// (needed to avoid unnecessary reloading in event handlers fired below...)
						var otherObject = other.mWeakReference.Target as T;

						lock (mCache.Items)
						{
							// delete own file, if no other item references it
							string path = GetObjectFilePath();
							var weakReferences = mCache.Items[path];
							for (int i = 0; i < weakReferences.Count; i++)
							{
								var weakReference = weakReferences[i];
								if (ReferenceEquals(weakReference.Target, this))
								{
									weakReferences.RemoveAt(i);
									if (weakReferences.Count == 0)
									{
										try { File.Delete(path); }
										catch
										{
											/* swallow... */
										}

										mCache.Items.Remove(path);
									}

									break;
								}
							}

							// transfer ownership of the other file
							path = other.GetObjectFilePath();
							foreach (var weakReference in mCache.Items[path])
							{
								if (ReferenceEquals(weakReference.Target, item))
								{
									weakReference.Target = this;
									break;
								}
							}

							// assign members
							mObjectGuid = other.mObjectGuid;
							mAsync = other.mAsync;
							mWeakReference = other.mWeakReference;
							mStrongReference = other.mStrongReference;
							mState = other.mState;
							mCreateNewWhenModified = other.mCreateNewWhenModified;

							// schedule saving the object, if the specified item did not finish saving, yet
							if (other.mState == ItemState.SavePending && other.mAsync)
								mCache.EnqueueAction(SaveObject);

							// dispose item, but do not delete its cache file
							other.mObjectGuid = Guid.Empty;
							other.mState = ItemState.InUse;
							other.Dispose();

							OnPropertyChanged(nameof(HasValue), otherObject);
							OnPropertyChanged(nameof(Value), otherObject);
							OnPropertyChanged(nameof(ValueDelayed), otherObject);
							break;
						}
					}
					else
					{
						// at least one of the locks was not taken
						if (ownLockTaken) Monitor.Exit(mSync);
						if (otherLockTaken) Monitor.Exit(other.mSync);
						ownLockTaken = false;
						otherLockTaken = false;
						Thread.Sleep(1);
					}
				}
			}
			finally
			{
				if (ownLockTaken) Monitor.Exit(mSync);
				if (otherLockTaken) Monitor.Exit(other.mSync);
			}
		}

		/// <summary>
		/// Loads the object from disk (for internal use only).
		/// </summary>
		/// <returns>The loaded object.</returns>
		private T LoadObject()
		{
			lock (mSync)
			{
				if (mState == ItemState.Disposed || mState == ItemState.SaveAndDisposePending)
					throw new ObjectDisposedException(nameof(ObjectDiskCacheItem<T>));

				return Serializer.Deserialize<T>(GetObjectFilePath());
			}
		}

		/// <summary>
		/// Saves the specified object to disk (for internal use only).
		/// </summary>
		private void SaveObject()
		{
			lock (mSync)
			{
				if (mState == ItemState.SavePending || mState == ItemState.SaveAndDisposePending)
				{
					Serializer.Serialize(GetObjectFilePath(), mStrongReference);

					mStrongReference = null;

					if (mState == ItemState.SavePending)
					{
						mState = ItemState.InUse;
					}
					else if (mState == ItemState.SaveAndDisposePending)
					{
						mState = ItemState.InUse;
						Dispose();
					}

					mCreateNewWhenModified = false;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="name">Name of the property that has changed.</param>
		/// <param name="objectToKeepAlive">Object to keep alive until the event has been handled.</param>
		private void OnPropertyChanged([CallerMemberName] string name = null, object objectToKeepAlive = null)
		{
			PropertyChangedEventManager.FireEvent(this, name, objectToKeepAlive);
		}

		/// <summary>
		/// Gets the path of the object cache file.
		/// </summary>
		/// <returns>Path of the object cache file.</returns>
		internal string GetObjectFilePath()
		{
			string filename = $"{mObjectGuid:D}.obj";
			return Path.Combine(mCache.ObjectCacheDirectoryPath, filename);
		}
	}

}
