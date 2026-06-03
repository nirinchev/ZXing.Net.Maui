using AndroidX.Camera.Core;
using Java.Nio;
using Microsoft.Maui.Graphics;
using System;

namespace ZXing.Net.Maui
{
	internal class FrameAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
	{
		readonly Action<ByteBuffer, Size> frameCallback;

		public FrameAnalyzer(Action<ByteBuffer, Size> callback)
		{
			frameCallback = callback;
		}

		public void Analyze(IImageProxy image)
		{
			var buffer = image.GetPlanes()[0].Buffer;

			var s = new Size(image.Width, image.Height);

			frameCallback?.Invoke(buffer, s);

			image.Close();
		}

		// CameraX 1.4+ added these as default methods on ImageAnalysis.Analyzer.
		// The .NET binding surfaces them as abstract, so an unimplemented FrameAnalyzer
		// throws AbstractMethodError at runtime when CameraX queries them.
		public Android.Util.Size DefaultTargetResolution => null;

		// 0 = ImageAnalysis.Analyzer.COORDINATE_SYSTEM_ORIGINAL (the Java default)
		public int TargetCoordinateSystem => 0;

		public void UpdateTransform(Android.Graphics.Matrix matrix) { }
	}
}
