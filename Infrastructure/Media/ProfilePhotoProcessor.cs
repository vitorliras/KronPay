using Domain.Exceptions;
using Domain.Services.Users;
using Shared.Localization;
using SkiaSharp;

namespace Infrastructure.Media;

public sealed class ProfilePhotoProcessor : IProfilePhotoProcessor
{
    private const int TargetDimension = 512;
    private const int JpegQuality = 80;

    public ProcessedPhoto Process(byte[] input)
    {
        using var original = SKBitmap.Decode(input)
            ?? throw new DomainException(MessageKeys.InvalidImageFormat);

        var side = Math.Min(original.Width, original.Height);
        var left = (original.Width - side) / 2;
        var top = (original.Height - side) / 2;
        var sourceRect = new SKRectI(left, top, left + side, top + side);
        var destRect = new SKRect(0, 0, TargetDimension, TargetDimension);

        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);

        using var surface = SKSurface.Create(new SKImageInfo(TargetDimension, TargetDimension));
        surface.Canvas.Clear(SKColors.White);
        surface.Canvas.DrawBitmap(original, sourceRect, destRect, sampling);

        using var snapshot = surface.Snapshot();
        using var encoded = snapshot.Encode(SKEncodedImageFormat.Jpeg, JpegQuality)
            ?? throw new DomainException(MessageKeys.InvalidImageFormat);

        return new ProcessedPhoto(encoded.ToArray(), "image/jpeg");
    }
}
