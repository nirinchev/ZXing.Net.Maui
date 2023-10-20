using System;
using ZXing.Net.Maui.Readers;

namespace ZXing.Net.Maui
{
	public class CameraFrameBufferEventArgs(PixelBufferHolder pixelBufferHolder) : EventArgs
	{
		public readonly PixelBufferHolder Data = pixelBufferHolder;
	}
}
