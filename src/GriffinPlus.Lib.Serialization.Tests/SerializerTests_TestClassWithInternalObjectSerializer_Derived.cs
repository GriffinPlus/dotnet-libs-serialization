///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests
{

	public partial class SerializerTests
	{
		[InternalObjectSerializer(1)]
		public class TestClassWithInternalObjectSerializer_Derived : TestClassWithInternalObjectSerializer
		{
			public string   String   { get; set; }
			public DateTime DateTime { get; set; }

			public TestClassWithInternalObjectSerializer_Derived()
			{
				String = "The quick brown fox jumps over the lazy dog";
				DateTime = DateTime.Now;
			}

			public TestClassWithInternalObjectSerializer_Derived(SerializerArchive archive) :
				base(archive.PrepareBaseArchive(typeof(TestClassWithInternalObjectSerializer)))
			{
				if (archive.Version == 1)
				{
					String = archive.ReadString();
					DateTime = archive.ReadDateTime();
				}
				else
				{
					throw new VersionNotSupportedException(archive);
				}
			}

			public new void Serialize(SerializerArchive archive, uint version)
			{
				archive.WriteBaseArchive(this, typeof(TestClassWithInternalObjectSerializer), null);

				if (version == 1)
				{
					archive.Write(String);
					archive.Write(DateTime);
				}
				else
				{
					throw new VersionNotSupportedException(typeof(TestClassWithInternalObjectSerializer_Derived), version);
				}
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = base.GetHashCode();
					hashCode = (hashCode * 397) ^ (String != null ? String.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ DateTime.GetHashCode();
					return hashCode;
				}
			}

			protected bool Equals(TestClassWithInternalObjectSerializer_Derived other)
			{
				return base.Equals(other) &&
				       String == other.String &&
				       DateTime.Equals(other.DateTime);
			}

			public override bool Equals(object obj)
			{
				if (!(obj is TestClassWithInternalObjectSerializer_Derived other)) return false;
				return Equals(other);
			}
		}
	}

}
