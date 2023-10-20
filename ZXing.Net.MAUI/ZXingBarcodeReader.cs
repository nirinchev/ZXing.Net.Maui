using System.Buffers;

namespace ZXing.Net.Maui.Readers;

public class ZXingBarcodeReader : IBarcodeReader
{
	private readonly BarcodeReaderGeneric zxingReader = new();

	private BarcodeReaderOptions options;

	public BarcodeReaderOptions Options
	{

		get => options ??= new BarcodeReaderOptions();
		set
		{
			options = value ?? new BarcodeReaderOptions();
			zxingReader.Options.PossibleFormats = options.Formats.ToZXingList();
			zxingReader.Options.TryHarder = options.TryHarder;
			zxingReader.AutoRotate = options.AutoRotate;
			zxingReader.Options.TryInverted = options.TryInverted;
		}
	}

	public BarcodeResult[] Decode(PixelBufferHolder image)
	{
		var w = (int)image.Size.Width;
		var h = (int)image.Size.Height;

		LuminanceSource ls = default;

		var left = (int)(w * (1 - this.Options.WidthCrop) / 2);
		var croppedWidth = (int)(w * this.Options.WidthCrop);
		var top = (int)(h * (1 - this.Options.HeightCrop) / 2);
		var croppedHeight = (int)(h * this.Options.HeightCrop);

#if ANDROID
		var buffer = ArrayPool<byte>.Shared.Rent(w * h);
		image.Data.Get(buffer, 0, w * h).Dispose();

		var cropped = ArrayPool<byte>.Shared.Rent(croppedWidth * croppedHeight);

		PopulateOutput(buffer, cropped, w, left, top, croppedWidth, croppedHeight, this.Options.RotateInput);

		var width = this.Options.RotateInput ? croppedHeight : croppedWidth;
		var height = this.Options.RotateInput ? croppedWidth : croppedHeight;

		ls = new PlanarYUVLuminanceSource(cropped, width, height, 0, 0, width, height, false);
#elif MACCATALYST || IOS
		ls = new CVPixelBufferBGRA32LuminanceSource(image.Data, w, h).crop(left, top, croppedWidth, croppedHeight);
#endif

		try
		{
			return Options.Multiple
				? zxingReader.DecodeMultiple(ls)?.ToBarcodeResults()
				: zxingReader.Decode(ls)?.ToBarcodeResults();
		}
		finally
		{
			image.Data.Dispose();
#if ANDROID
			ArrayPool<byte>.Shared.Return(buffer);
			ArrayPool<byte>.Shared.Return(cropped);
#endif
		}
	}

#if ANDROID
	private static void PopulateOutput(byte[] input, byte[] output, int bufferWidth, int left, int top, int width, int height, bool rotate)
	{
		var offset = top * bufferWidth + left;

		if (rotate)
		{
			for (var x = 0; x < width; x++)
			{
				for (var y = height - 1; y >= 0; y--)
				{
					var index = offset + y * bufferWidth + x;
					output[x * height + y] = input[index];
				}
			}
		}
		else
		{
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					// If we're rotating
					output[y * width + x] = input[offset + y * bufferWidth + x];
				}
			}
		}
	}
#endif
}