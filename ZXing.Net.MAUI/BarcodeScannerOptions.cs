namespace ZXing.Net.Maui;

public record BarcodeReaderOptions

{
	public bool AutoRotate { get; init; }

	public bool TryHarder { get; init; }

	public bool TryInverted { get; init; }

	public BarcodeFormat Formats { get; init; }

	public bool Multiple { get; init; }

	public float WidthCrop { get; init; } = 1;

	public float HeightCrop { get; init; } = 1;

	public bool RotateInput { get; init; }
}
