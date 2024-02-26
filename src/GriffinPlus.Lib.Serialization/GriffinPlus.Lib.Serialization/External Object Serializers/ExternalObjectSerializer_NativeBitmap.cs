///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-serialization)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.Imaging;

namespace GriffinPlus.Lib.Serialization
{

	/// <summary>
	/// External object serializer for the <see cref="NativeBitmap"/> class.
	/// </summary>
	[ExternalObjectSerializer(1)]
	public unsafe class ExternalObjectSerializer_NativeBitmap : ExternalObjectSerializer<NativeBitmap>
	{
		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="archive">Archive to serialize into.</param>
		/// <param name="bitmap">The bitmap to serialize.</param>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override void Serialize(SerializationArchive archive, NativeBitmap bitmap)
		{
			if (archive.Version == 1)
			{
				archive.Write(bitmap.PixelWidth);
				archive.Write(bitmap.PixelHeight);
				archive.Write(bitmap.DpiX);
				archive.Write(bitmap.DpiY);
				archive.Write(bitmap.Format);
				archive.Write(bitmap.Palette);
				archive.Write(bitmap.BufferStride);
				archive.Write(bitmap.BufferSize);
				archive.Write((void*)bitmap.UnsafeBufferStart, bitmap.BufferSize);
				return;
			}

			throw new VersionNotSupportedException(archive);
		}

		/// <summary>
		/// Deserializes an object.
		/// </summary>
		/// <param name="archive">Archive containing the serialized object.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
		public override NativeBitmap Deserialize(DeserializationArchive archive)
		{
			if (archive.Version == 1)
			{
				int pixelWidth = archive.ReadInt32();
				int pixelHeight = archive.ReadInt32();
				double dpiX = archive.ReadDouble();
				double dpiY = archive.ReadDouble();
				var format = (PixelFormat)archive.ReadObject();
				var palette = (BitmapPalette)archive.ReadObject();
				long stride = archive.ReadInt64();
				long bufferSize = archive.ReadInt64();
				if (sizeof(nint) == 4 && (bufferSize > int.MaxValue || stride > int.MaxValue)) throw new OutOfMemoryException();
				var buffer = NativeBuffer.CreatePageAligned((nint)bufferSize);
				var bitmap = new NativeBitmap(buffer, pixelWidth, pixelHeight, (nint)stride, dpiX, dpiY, format, palette, true);
				archive.ReadBuffer((void*)bitmap.UnsafeBufferStart, bufferSize);
				return bitmap;
			}

			throw new VersionNotSupportedException(archive);
		}
	}

}
