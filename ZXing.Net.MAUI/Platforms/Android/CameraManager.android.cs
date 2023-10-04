using Android.Graphics;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Java.Util.Concurrent;
using AView = Android.Views.View;

namespace ZXing.Net.Maui
{
	internal partial class CameraManager
	{
		Preview cameraPreview;
		ImageAnalysis imageAnalyzer;
		PreviewView previewView;
		IExecutorService cameraExecutor;
		CameraSelector cameraSelector = null;
		ProcessCameraProvider cameraProvider;
		ICamera camera;

		public NativePlatformCameraPreviewView CreateNativeView()
		{
			previewView = new PreviewView(Context.Context);

			return previewView;
		}

		public void Connect()
		{
			var cameraProviderFuture = ProcessCameraProvider.GetInstance(Context.Context);

			cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
			{
				// Used to bind the lifecycle of cameras to the lifecycle owner
				cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();

				// Preview
				cameraPreview = new Preview.Builder().Build();
				cameraPreview.SetSurfaceProvider(previewView.SurfaceProvider);

				// Frame by frame analyze
				imageAnalyzer = new ImageAnalysis.Builder()
					.SetDefaultResolution(new Android.Util.Size(640, 480))
					.SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
					.Build();

				cameraExecutor = Executors.NewSingleThreadExecutor();

				imageAnalyzer.SetAnalyzer(cameraExecutor, new FrameAnalyzer((buffer, size) =>
					FrameReady?.Invoke(this, new CameraFrameBufferEventArgs(new Readers.PixelBufferHolder { Data = buffer, Size = size }))));

				UpdateCamera();

			}), ContextCompat.GetMainExecutor(Context.Context)); //GetMainExecutor: returns an Executor that runs on the main thread.
		}

		public void Disconnect()
		{
			cameraProvider?.Shutdown();
			cameraProvider?.Dispose();
			cameraProvider = null;

			cameraExecutor?.Shutdown();
			cameraExecutor?.Dispose();
			cameraExecutor = null;

			cameraPreview?.Dispose();
			cameraPreview = null;
		}

		public void UpdateCamera()
		{
			if (cameraProvider != null)
			{
				// Unbind use cases before rebinding
				cameraProvider.UnbindAll();

				var cameraLocation = CameraLocation;

				// Select back camera as a default, or front camera otherwise
				if (cameraLocation == CameraLocation.Rear && cameraProvider.HasCamera(CameraSelector.DefaultBackCamera))
					cameraSelector = CameraSelector.DefaultBackCamera;
				else if (cameraLocation == CameraLocation.Front && cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera))
					cameraSelector = CameraSelector.DefaultFrontCamera;
				else
					cameraSelector = CameraSelector.DefaultBackCamera;

				if (cameraSelector == null)
					throw new System.Exception("Camera not found");

				// The Context here SHOULD be something that's a lifecycle owner
				if (Context.Context is AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner)
				{
					camera = cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, cameraPreview, imageAnalyzer);
				}
				else if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is AndroidX.Lifecycle.ILifecycleOwner maLifecycleOwner)
				{
					// if not, this should be sufficient as a fallback
					camera = cameraProvider.BindToLifecycle(maLifecycleOwner, cameraSelector, cameraPreview, imageAnalyzer);
				}
			}
		}

		public void UpdateTorch(bool on)
		{
			camera?.CameraControl?.EnableTorch(on);
		}

		public void Focus(Microsoft.Maui.Graphics.Point point)
		{
			var factory = new SurfaceOrientedMeteringPointFactory(1, 1);
			var meteringPoint = factory.CreatePoint((float)point.X, (float)point.Y);
			var action = new FocusMeteringAction.Builder(meteringPoint, FocusMeteringAction.FlagAf)
				.DisableAutoCancel()
				.Build();

			camera.CameraControl.StartFocusAndMetering(action);
		}

		public void AutoFocus()
		{

		}

		public void Dispose()
		{
			cameraExecutor?.Shutdown();
			cameraExecutor?.Dispose();
		}
	}
}
