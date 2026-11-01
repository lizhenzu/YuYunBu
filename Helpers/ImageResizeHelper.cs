using SkiaSharp;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// 읽을 수 없는 사진 형식(HEIC 등)일 때 던진다.
    /// </summary>
    public class UnsupportedImageFormatException : Exception
    {
        public UnsupportedImageFormatException() : base("읽을 수 없는 사진 형식입니다.") { }
    }

    /// <summary>
    /// 사진 줄이기 · 저장 (SkiaSharp).
    /// 2026-11-01: SixLabors.ImageSharp 4.x 가 빌드에 유료 라이선스 키를 요구해서 SkiaSharp 로 바꿈.
    /// </summary>
    public static class ImageResizeHelper
    {
        /// <summary>
        /// 사진을 읽어 EXIF 회전을 바로잡은 비트맵을 돌려준다. 읽지 못하면 UnsupportedImageFormatException.
        /// </summary>
        public static SKBitmap LoadOriented(Stream src)
        {
            // SKCodec 은 탐색 가능한 스트림이 필요하다 (IFormFile 스트림 대비)
            using var ms = new MemoryStream();
            src.CopyTo(ms);
            ms.Position = 0;

            using var codec = SKCodec.Create(ms) ?? throw new UnsupportedImageFormatException();
            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var raw = new SKBitmap(info);
            var result = codec.GetPixels(info, raw.GetPixels());
            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
            {
                raw.Dispose();
                throw new UnsupportedImageFormatException();
            }

            var oriented = ApplyOrigin(raw, codec.EncodedOrigin);
            if (!ReferenceEquals(oriented, raw)) raw.Dispose();
            return oriented;
        }

        public static SKBitmap LoadOriented(string path)
        {
            using var fs = File.OpenRead(path);
            return LoadOriented(fs);
        }

        /// <summary>
        /// 긴 변이 maxEdge 를 넘지 않게 비율을 유지하며 줄인다. 이미 작으면 그대로 복사본을 돌려준다.
        /// </summary>
        public static SKBitmap ResizeMax(SKBitmap src, int maxEdge) => ResizeToFit(src, maxEdge, maxEdge);

        /// <summary>
        /// 가로 maxWidth · 세로 maxHeight 안에 들어가게 비율을 유지하며 줄인다. 키우지는 않는다.
        /// </summary>
        public static SKBitmap ResizeToFit(SKBitmap src, int maxWidth, int maxHeight)
        {
            var ratio = Math.Min((double)maxWidth / src.Width, (double)maxHeight / src.Height);
            if (ratio >= 1.0) return src.Copy();

            var w = Math.Max(1, (int)Math.Round(src.Width * ratio));
            var h = Math.Max(1, (int)Math.Round(src.Height * ratio));
            return src.Resize(new SKImageInfo(w, h, src.ColorType, src.AlphaType), new SKSamplingOptions(SKCubicResampler.Mitchell));
        }

        /// <summary>
        /// jpg 로 저장한다. 투명한 부분(png)은 흰 바탕으로 채운다.
        /// </summary>
        public static void SaveJpeg(SKBitmap bmp, Stream dest, int quality)
        {
            using var flat = new SKBitmap(new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888, SKAlphaType.Opaque));
            using (var canvas = new SKCanvas(flat))
            {
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(bmp, 0, 0);
            }
            Encode(flat, SKEncodedImageFormat.Jpeg, quality, dest);
        }

        public static void SaveJpeg(SKBitmap bmp, string path, int quality)
        {
            using var fs = File.Create(path);
            SaveJpeg(bmp, fs, quality);
        }

        public static void SaveWebp(SKBitmap bmp, string path, int quality)
        {
            using var fs = File.Create(path);
            Encode(bmp, SKEncodedImageFormat.Webp, quality, fs);
        }

        private static void Encode(SKBitmap bmp, SKEncodedImageFormat format, int quality, Stream dest)
        {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(format, quality) ?? throw new InvalidOperationException("사진을 저장하지 못했습니다.");
            data.SaveTo(dest);
        }

        /// <summary>
        /// 휴대폰 사진의 EXIF 방향값대로 돌린다. 돌릴 필요가 없으면 받은 비트맵을 그대로 돌려준다.
        /// </summary>
        private static SKBitmap ApplyOrigin(SKBitmap src, SKEncodedOrigin origin)
        {
            if (origin == SKEncodedOrigin.TopLeft || origin == SKEncodedOrigin.Default) return src;

            var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
                              or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            var w = swap ? src.Height : src.Width;
            var h = swap ? src.Width : src.Height;

            var dst = new SKBitmap(new SKImageInfo(w, h, src.ColorType, src.AlphaType));
            using var canvas = new SKCanvas(dst);
            switch (origin)
            {
                case SKEncodedOrigin.TopRight:    canvas.Scale(-1, 1, w / 2f, 0); break;
                case SKEncodedOrigin.BottomRight: canvas.RotateDegrees(180, w / 2f, h / 2f); break;
                case SKEncodedOrigin.BottomLeft:  canvas.Scale(1, -1, 0, h / 2f); break;
                case SKEncodedOrigin.LeftTop:     canvas.Translate(w, 0); canvas.RotateDegrees(90); canvas.Translate(0, src.Height); canvas.Scale(1, -1); break;
                case SKEncodedOrigin.RightTop:    canvas.Translate(w, 0); canvas.RotateDegrees(90); break;
                case SKEncodedOrigin.RightBottom: canvas.Translate(w, h); canvas.Scale(-1, -1); canvas.Translate(w, 0); canvas.RotateDegrees(90); canvas.Translate(0, src.Height); canvas.Scale(1, -1); break;
                case SKEncodedOrigin.LeftBottom:  canvas.Translate(0, h); canvas.RotateDegrees(-90); break;
            }
            canvas.DrawBitmap(src, 0, 0);
            return dst;
        }
    }
}
