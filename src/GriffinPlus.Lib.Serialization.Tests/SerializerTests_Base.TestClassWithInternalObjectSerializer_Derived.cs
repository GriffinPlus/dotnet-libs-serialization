///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib.Serialization.Tests;

public partial class SerializerTests_Base
{
	[InternalObjectSerializer(1)]
	public class TestClassWithInternalObjectSerializer_Derived : TestClassWithInternalObjectSerializer
	{
		public string AnotherString { get; set; }

		public TestClassWithInternalObjectSerializer_Derived()
		{
			AnotherString = "The quick brown fox jumps over the lazy dog";
		}

		public TestClassWithInternalObjectSerializer_Derived(DeserializationArchive archive) :
			base(archive.PrepareBaseArchive())
		{
			if (archive.Version == 1)
			{
				AnotherString = archive.ReadString();
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}

		public new void Serialize(SerializationArchive archive)
		{
			archive.WriteBaseArchive(null);

			// ReSharper disable once InvertIf
			if (archive.Version == 1)
			{
				archive.Write(AnotherString);
				return;
			}

			throw new VersionNotSupportedException(archive);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (AnotherString != null ? AnotherString.GetHashCode() : 0);
				return hashCode;
			}
		}

		protected bool Equals(TestClassWithInternalObjectSerializer_Derived other)
		{
			return base.Equals(other) && AnotherString == other.AnotherString;
		}

		public override bool Equals(object obj)
		{
			return obj is TestClassWithInternalObjectSerializer_Derived other && Equals(other);
		}
	}
}
