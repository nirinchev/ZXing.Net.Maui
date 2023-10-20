using System.Linq;
using Microsoft.Maui.Graphics;

namespace ZXing.Net.Maui;

public static class ZXingNetExtensions
{
    public static BarcodeResult[] ToBarcodeResults(this Result result)
        => new[] { new BarcodeResult
        {
            Raw = result.RawBytes,
            Value = result.Text,
            Format = (BarcodeFormat)(int)result.BarcodeFormat,
            Metadata = result.ResultMetadata?.ToDictionary(md => (MetadataType)md.Key, md => md.Value),
            PointsOfInterest = result.ResultPoints?.Where(p => p is not null).Select(p => new PointF(p.X, p.Y)).ToArray()
        } };

    public static BarcodeResult[] ToBarcodeResults(this Result[] results)
        => results?.Select(result => new BarcodeResult
        {
            Raw = result.RawBytes,
            Value = result.Text,
            Format = (BarcodeFormat)(int)result.BarcodeFormat,
            Metadata = result.ResultMetadata?.ToDictionary(md => (MetadataType)md.Key, md => md.Value),
            PointsOfInterest = result.ResultPoints?.Where(p => p is not null).Select(p => new PointF(p.X, p.Y)).ToArray()
        }).ToArray();
}