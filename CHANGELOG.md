# Changelog
---

## Release v2.1.1

### Other Changes

#### Remove generic type parameter from serialization/deserialization methods using pooled serializers

The generic type parameters do not offer any benefit, so the object to serialize, respectively the deserialized object, is `System.Object` now:

```csharp
public static void Serialize(Stream stream, object obj, object context, SerializationOptimization optimization);
public static object Deserialize(Stream stream, object context, bool useTolerantDeserialization);
```

---
## Release v2.1.0

### Features

#### Serialization/Deserialization using pooled serializers

The `Serializer` class now provides the following static methods to serialize to and deserialize from a stream using pooled serializer instances to reduce pressure on the garbage collection in applications that make heavy use of serializers:

```csharp
public static void Serialize<T>(Stream stream, T obj, object context, SerializationOptimization optimization);
public static T Deserialize<T>(Stream stream, object context, bool useTolerantDeserialization);
```

---
## Release v2.0.5

### Other Changes

#### Add custom serializers

Added external object serializers for the following types in the `GriffinPlus.Lib.Common` package:

- `GriffinPlus.Lib.Imaging.BitmapPalette`
- `GriffinPlus.Lib.Imaging.Color`
- `GriffinPlus.Lib.Imaging.NativeBitmap`
- `GriffinPlus.Lib.Imaging.PixelFormat`
- `GriffinPlus.Lib.NativeBuffer`

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.7

---

## Release v2.0.4

### Other Changes

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.5

---

## Release v2.0.3

### Other Changes

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.4

---

## Release v2.0.2

### Other Changes

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.3

---

## Release v2.0.1

### Bugfixes

#### Add initializing asynchronously

The `Serializer` class can be initialized asynchronously using the `TriggerInit()` method again now. This method has been removed accidently with Release 2.0.0. It allows applications to perform the intialization in the background while starting up without significantly increasing its own startup time.

### Other Changes

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.2

---

## Release v2.0.0

### Breaking Changes

#### Remove the `TypeInfo` class from the `GriffinPlus.Lib.Serialization` package

The `TypeInfo` class has been replaced with the optimized `RuntimeMetadata` class from the `GriffinPlus.Lib.Common` package supporting .NET Framework 4+, .NET Core 2/3 and .NET 5+ properly.

On .NET Framework 4+ the following assemblies are scanned for custom serializers:
- Assemblies in the Global Assembly Cache (GAC)
- Assemblies in the application's base directory or a private bin path below the base directory
- Assemblies that have been generated dynamically

On .NET Core 2/3 and .NET 5+ the following assemblies are scanned for custom serializers:
- Assemblies that are loaded into the default assembly load context
- Assemblies that have been generated dynamically

### Optimizations

#### Optimize type resolution process

At startup only assemblies in the application's base directory are scanned for custom serializers now. This speeds up the startup of the application as only application specific assemblies are scanned for custom serializers. In the rare case that assemblies outside the application's base directory contain custom serializers these assemblies are scanned on demand when objects of types in these assemblies are deserialized.

### Bugfixes

#### Fix exact type resolution

The serializer cached a tolerantly resolved type and used the cache to resolve the type even in cases where exact type resolution was requested.

#### Avoid scanning assemblies for serializers repeatedly

The serializer scanned referenced assemblies multiple times slowing down the startup unnecessarily.

### Other Changes

#### Update dependency on NuGet packages 

- Updated `GriffinPlus.Lib.Common` package to version 3.1.1
- Updated `GriffinPlus.Lib.FastActivator` package to version 1.1.1

#### Add support for testing with other .NET frameworks

- Tests on `net461` tests the library built for `net461`
- Tests on `netcoreapp2.2` tests the library built for `netstandard2.0`
- Tests on `netcoreapp3.1` tests the library built for `netstandard2.1`
- Tests on `net5.0` tests the library built for `net5.0`
- Tests on `net6.0` tests the library built for `net5.0`
- Tests on `net7.0` tests the library built for `net5.0`

---

## Release v1.2.1

### Bugfixes

#### Fix deserialization of reused generic type definition metadata

The serializer failed reconstructing type metadata of generic types with the same generic type definition, but different generic type arguments.

### Other Changes

#### Let `TypeInfo` class log using level `Trace` instead of `Debug`

Log level `Debug'` is usually used to emit information that may be interesting for other developers than the implementer of the class. The messages are about assemblies that are loaded for inspection, nothing another developers should be concerned about. Nevertheless errors that occur while loading an assembly are logged using log level `Debug` to inform other developers that a certain assembly could not be inspected. This is an edge case, but it can happen.

---

## Release v1.2.0

### Features

#### External Object Serializer for `GriffinPlus.Lib.Conversion.IConverter`

The package now contains an external object serializer for converters implementing the `GriffinPlus.Lib.Conversion.IConverter` interface. Actually the serializer does not serialize any object state, but helps to reconstruct converters when serializing/deserializing. Usually converters are stateless, but if you have a converter with some state, a more specific serializer should be implemented.

### Bugfixes

- Fix deserialization of value types.

---

## Release v1.1.0

### Features

#### Object Disk Cache

The `GriffinPlus.Lib.Caching.ObjectDiskCache` class provides an implementation of the [`GriffinPlus.Lib.Caching.IObjectCache`](https://github.com/GriffinPlus/dotnet-libs-common#namespace-griffinpluslibcaching) interface using the serializer to persist objects. The object cache allows to swap serializable objects out to disk to save memory and restore them on demand later on.

---

## Release v1.0.0

First official release.

---
