# Griffin+ Serialization

[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/griffinplus/2f589a5e-e2ab-4c08-bee5-5356db2b2aeb/35/master?label=Build)](https://dev.azure.com/griffinplus/DotNET%20Libraries/_build/latest?definitionId=35&branchName=master)
[![Tests (master)](https://img.shields.io/azure-devops/tests/griffinplus/DotNET%20Libraries/35/master?label=Tests)](https://dev.azure.com/griffinplus/DotNET%20Libraries/_build/latest?definitionId=35&branchName=master)
[![NuGet Version](https://img.shields.io/nuget/v/GriffinPlus.Lib.Serialization.svg?label=NuGet%20Version)](https://www.nuget.org/packages/GriffinPlus.Lib.Serialization)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GriffinPlus.Lib.Serialization.svg?label=NuGet%20Downloads)](https://www.nuget.org/packages/GriffinPlus.Lib.Serialization)

## Overview

The *Griffin+ Serialization Library* provides a serializer for almost all .NET objects.

## Features

- Serialized stream includes type metadata: The user does not need to know the type to deserialize
- *Custom Serializers* allow to provide serialization support for almost all .NET objects
  - *Internal Object Serializers* allow to integrate serialization capabilities into own types
  - *External Object Serializers* allow to add serialization capabilities to types that cannot be modified (e.g. framework types or types exported from 3rd party libraries)
  - Support for versioning to address issues that occur when types evolve
  - Support for generic types
  - Custom Serializers are automatically detected when the application spins up
- Cross Platform
  - Support for all common .NET frameworks allowing types to migrate between assemblies
  - Support for interoperating with little endian and big endian machines
- Optimization: The serializer output can be optimized for size or for speed depending on purpose
- Deep Object Copy: The serializer supports to efficiently create deep copies of serializable objects, avoiding to duplicate immutable objects
- Built-In Support for serializing common types
  - Boolean type: `System.Boolean`
  - Character type: `System.Char`
  - String type: `System.String`
  - 8-bit integer types: `System.SByte` and `System.Byte`
  - 16-bit integer types: `System.Int16` and `System.UInt16`
  - 32-bit integer types: `System.Int32` and `System.UInt32`
  - 64-bit integer types: `System.Int64` and `System.UInt64`
  - Floating point types: `System.Single`, `System.Double` and `System.Decimal`
  - Date/Time types: `System.DateTime` and `System.DateTimeOffset`
  - Date-Only type: `System.DateOnly` (.NET 6+ only)
  - Time-Only type: `System.TimeOnly` (.NET 6+ only)
  - Type objects: `System.Type`
  - GUIDs: `System.Guid`
  - Custom Buffers (supports streams and pointers)
  - Types provided by the `GriffinPlus.Lib.Common` package
    - `GriffinPlus.Lib.Imaging.BitmapPalette`
    - `GriffinPlus.Lib.Imaging.Color`
    - `GriffinPlus.Lib.Imaging.NativeBitmap`
    - `GriffinPlus.Lib.Imaging.PixelFormat`
    - `GriffinPlus.Lib.NativeBuffer`
  - Generic collections shipped with the .NET Framework
    - `System.Collection.Generic.Dictionary<TKey,TValue>`
    - `System.Collection.Generic.HashSet<T>`
    - `System.Collection.Generic.List<T>`
    - `System.Collection.Generic.LinkedList<T>`
    - `System.Collection.Generic.Queue<T>`
    - `System.Collection.Generic.SortedDictionary<TKey,TValue>`
    - `System.Collection.Generic.SortedList<TKey,TValue>`
    - `System.Collection.Generic.Stack<T>`
  - Collections that provide a parameterless constructor and implement at least one of the following interfaces (ordered by preference):
    - `System.Collections.Generic.IDictionary<TKey,TValue>`
    - `System.Collections.Generic.IList<T>`
    - `System.Collections.Generic.ICollection<T>`
    - `System.Collections.IDictionary`
    - `System.Collections.IList`
  - Arrays of serializable objects

## Supported Platforms

The library is entirely written in C# using .NET Standard 2.0.

More specific builds for .NET Standard 2.1, .NET Framework 4.6.1, .NET 5.0, .NET 6.0, .NET 7.0 and .NET 8.0 minimize dependencies to framework components and provide optimizations for the different frameworks.

Therefore it should work on the following platforms (or higher):
- .NET Framework 4.6.1
- .NET Core 2/3
- .NET 5/6/7/8
- Mono 5.4
- Xamarin iOS 10.14
- Xamarin Mac 3.8
- Xamarin Android 8.0
- Universal Windows Platform (UWP) 10.0.16299

The library is tested automatically on the following frameworks and operating systems:
- .NET Framework 4.6.1: Tests with library built for .NET Framework 4.6.1 (Windows Server 2022)
- .NET Core 2.2: Tests with library built for .NET Standard 2.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET Core 3.1: Tests with library built for .NET Standard 2.1 (Windows Server 2022 and Ubuntu 22.04)
- .NET 5.0: Tests with library built for .NET 5.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET 6.0: Tests with library built for .NET 6.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET 7.0: Tests with library built for .NET 7.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET 8.0: Tests with library built for .NET 8.0 (Windows Server 2022 and Ubuntu 22.04)

## Usage

### Serializing / Deserializing

#### Step 1: Add the NuGet Package

Add the latest version of the `GriffinPlus.Lib.Serialization` NuGet package to your project.

#### Step 2: Use the Serializer

Add the namespace `GriffinPlus.Lib.Serialization` to your source file.

```csharp
using GriffinPlus.Lib.Serialization;
```

For best performance the `Serializer` class provides the static `Serialize(...)` and `Deserialize(...)` methods to serialize and deserialize data. These methods use an internal serializer pool effectively reducing garbage collection pressure as serializer instances are reused.

```csharp
// object to serialize
var obj = 42;

// serialize the object into a stream
var stream = new MemoryStream();
Serializer.Serialize(stream, obj, context: null, optimization: SerializationOptimization.Speed);

// rewind the stream and deserialize the object
stream.Position = 0;
var copy = Serializer.Deserialize(stream, context: null, useTolerantDeserialization: true);
```

Alternatively, you can create a new `Serializer` instance, configure it to suit your needs and serialize/deserialize your object.

```csharp
// object to serialize
var obj = 42;

// create a new serializer
var serializer = new Serializer
{
    SerializationOptimization = SerializationOptimization.Speed,
    UseTolerantDeserialization = false
};

// serialize the object into a stream
var stream = new MemoryStream();
serializer.Serialize(stream, obj);

// rewind the stream and deserialize the object
stream.Position = 0;
var copy = serializer.Deserialize(stream);
```

The property `SerializationOptimization` allows to influence whether the serializer optimizes for `Size` (most probably when serializing to a file or network connection) or for `Speed` (most probably when staying in-process, e.g. to create a deep copy of an object). The default is to optimize for `Speed`.

The property `UseTolerantDeserialization` determines whether tolerant deserialization is in place. Tolerant deserialization allows to deserialize objects that were serialized on a machine with a different .NET framework. The serializer will try to exactly map to existing types when deserializing. If this fails, it will try to find the type in some other assembly. This enables the serializer to handle type migrations. Different .NET framework versions define even primitive types in different assemblies, so deserializing on some other .NET version would fail, if done without tolerance. As a side effect you can move your own types between assemblies as well. The full type name (namespace + type name) must not change. The default is `false` to avoid unexpected behavior.

#### Step 3a: Add Serialization Support to Own Types (Internal Object Serializer)

Very basic example of a class with an internal object serializer illustrating the parts that are relevant for serialization.

```csharp
[InternalObjectSerializer(1)]
public class MyClass : IInternalObjectSerializer
{
    public int Value { get; }

    protected MyClass(DeserializationArchive archive)
    {
        if (archive.Version == 1)
        {
            Value = archive.ReadInt32();
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }
    }

    void IInternalObjectSerializer.Serialize(SerializationArchive archive)
    {
        if (archive.Version == 1)
        {
            archive.Write(Value);
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }
    }
}
```

A type (class/struct) with an *Internal Object Serializer* has the following characteristics:

- Class annotation: The type is annotated with the `GriffinPlus.Lib.Serialization.InternalObjectSerializer` attribute specifying the maximum version supported by the serializer. The implemented internal object serializer must support all versions from `1` up to the specified version number to allow deserializing older versions as well.
- Interface implementation: The type implements the `GriffinPlus.Lib.Serialization.IInternalObjectSerializer` interface.
- Deserialization constructor: The type provides a special constructor taking a `DeserializationArchive` which acts as an abstract userfriendly interface to the deserialization stream. The constructor is called by the serializer when deserializing an object of this type from a stream. If a serializer version is requested, but not supported, a `GriffinPlus.Lib.Serialization.VersionNotSupportedException` should be thrown. The serializer also takes care of checking the maximum supported version as specified by the `InternalObjectSerializer` attribute. It does not call the deserialization constructor if the requested serializer version is greater than the maximum version specified by the attribute. If you can be sure that the maximum supported version and the versions actually implemented are consistent, you can omit throwing the exception.
- Serialization method: The type provides an implementation of the `IInternalObjectSerializer.Serialize()` method that takes care of writing an object of the type to a stream. The `Serialize()` method takes a `SerializationArchive` exposing an userfriendly abstraction of the serialization stream. The method can be implemented public as well, but it is not recommended as this method is only meaningful in conjunction with the serializer. If a serializer version is requested, but not supported, a `GriffinPlus.Lib.Serialization.VersionNotSupportedException` should be thrown. Same here, you can omit throwing the exception if you are sure that the announced maximum supported serializer version and the actually implemented versions are consistent.

Using an internal object serializer allows to serialize derived classes by delegating serialization/deserialization to the base class. The following example shows a very rudimentary class deriving from the example class above:

```csharp
[InternalObjectSerializer(1)]
public class MyDerivedClass : MyClass, IInternalObjectSerializer
{
    public int OtherValue { get; }

    protected MyDerivedClass(DeserializationArchive archive) :
        base(archive.PrepareBaseArchive())
    {
        if (archive.Version == 1)
        {
            OtherValue = archive.ReadInt32();
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }
    }

    void IInternalObjectSerializer.Serialize(SerializationArchive archive)
    {
        archive.WriteBaseArchive();

        if (archive.Version == 1)
        {
            archive.Write(OtherValue);
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }
    }
}
```

The basic outline is the same as for the base class. The only differences are the delegation to the deserialization constructor and the serialization logic of the base class.

The deserialization constructor of the base class is invoked by the deserialization constructor of the derived class passing a `DeserializationArchive` that is returned by the `PrepareBaseArchive()` method of the `DeserializationArchive` of the derived class. The base class can evolve over time as it deserializes its own data only. It can handle versioning issues on its own, so derived classes do not need to consider them. Each and every class is only responsible for the data it encorporates.

The `Serialize()` method delegates the serialization of base class members by calling `SerializationArchive.WriteBaseArchive()`. This must be the first call in the `Serialize()` method to avoid mixing up the order of serialized data in the output stream.

Internal object serializers **do not** need to be explicitly registered with the serializer as the serializer scans all assemblies for serializers when spinning up. You just need to implement it and it will be used automatically.

#### Step 3b: Add Serialization Support for Other Types (External Object Serializer)

Another option to add serialization support for a type is to implement an external object serializer for it. This is the only way to add serialization support for types that are not under your control. Let's assume the class to serialize is the `MyClass` from the example above, but without its internal object serializer.

```csharp
public class MyClass
{
    public int Value { get; set; }
}
```

An external object serializer for this class could look like the following:

```csharp
[ExternalObjectSerializer(1)]
public class MyClassExternalObjectSerializer : ExternalObjectSerializer<MyClass>
{
    public void Serialize(SerializationArchive archive, MyClass obj)
    {
        if (archive.Version == 1)
        {
            archive.Write(obj.Value);
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }
    }

    public MyClass Deserialize(DeserializationArchive archive)
    {
        MyClass obj = new MyClass();

        if (archive.Version == 1)
        {
            obj.Value = archive.ReadInt32();
        }
        else
        {
            throw new VersionNotSupportedException(archive);
        }

        return obj;
    }
}
```

An *External Object Serializer* class has the following characteristics:

- Class annotation: The external object serializer is annotated with the `GriffinPlus.Lib.Serialization.ExternalObjectSerializer` attribute specifying the maximum supported version. The implemented external object serializer must support all versions from `1` up to the specified version number to allow deserializing older versions as well.
- Base Class: The external object serializer derives from the `GriffinPlus.Lib.Serialization.ExternalObjectSerializer<T>` class. This class is the common base class for all external object serializers. Its generic type argument determines the type the serializer will handle. The type can be a regular type, a constructed generic type, a generic type definition or an interface. While regular and constructed generic types provide support for only the specified type, a generic type definition allow you to implement an external object serializer for a generic type or a type that implements a generic interface. The serializer will take care of infering generic type arguments from the specified type when creating an instance of the external object serializer at runtime.
- Serialization method: The external object serializer overrides the `ExternalObjectSerializer<T>.Serialize()` method taking care of writing an object of the supported type to the serialization stream. If a serializer version is requested, but not supported, a `GriffinPlus.Lib.Serialization.VersionNotSupportedException` should be thrown. If you can be sure that the maximum supported version and the versions actually implemented are consistent, you can omit throwing the exception.
- Deserialization method: The external object serializer overrides the `ExternalObjectSerializer<T>.Deserialize()` method that takes care of reading an object of the supported type from the deserialization stream. Same here, you can omit throwing the exception if you are sure that the announced maximum supported serializer version and the actually implemented versions are consistent.

By nature, an *external object serializer* has only access to public members of the object to serialize/deserialize, while an *internal object serializer* has access to all members. Therefore, an internal object serializer should preferred over an external object serializer, whereever possible.

External object serializers **do not** need to be explicitly registered with the serializer as the serializer scans all assemblies for serializers when spinning up. You just need to implement the external object serializer and it will be used automatically.

## Known Limitations

### No support for cyclic dependencies

The serializer will throw a `GriffinPlus.Lib.Serialization.CyclicDependencyDetectedException` if it detects a cyclic dependency when serializing. If you really need cyclic dependencies in your type you can use a context object to pass references downstream.

### No support for comparers in collections

When serializing collections the serializer will not carry along comparers associated with them. This may lead to unexpected behavior, especially when expecting a deep copy to be identically. The copy of a collection will contain copies of the original collection's elements, but depending on the type of collection the behavior of the collection may differ. As long as collections are used with their default comparer everything will run as expected. Serializing a collection that has been created with a custom comparer is likely to behave differently.