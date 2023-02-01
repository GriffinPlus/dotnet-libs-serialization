# Changelog
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
