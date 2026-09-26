using DdsFileTypePlus;
using FreeImageAPI;
using PaintDotNet;

namespace MyTextureConverter
{
    internal sealed record InputFormat(string Name, int Priority, params string[] Extensions)
    {
        public static readonly IReadOnlyList<InputFormat> All = new InputFormat[]
        {
            new("DDS", 0, ".dds"),
            new("TGA", 1, ".tga"),
            new("PSD", 2, ".psd"),
            new("BMP", 3, ".bmp"),
            new("PNG", 4, ".png"),
            new("TIFF", 5, ".tif", ".tiff"),
            new("WEBP", 6, ".webp"),
            new("JPG", 7, ".jpg", ".jpeg"),
            new("GIF", 8, ".gif")
        };

        public static readonly IReadOnlyDictionary<string, int> PriorityByExtension = All
            .SelectMany(f => f.Extensions.Select(e => (Extension: e, f.Priority)))
            .ToDictionary(x => x.Extension, x => x.Priority, StringComparer.OrdinalIgnoreCase);
    }

    internal sealed record OutputFormat(string Name, string Extension, FREE_IMAGE_FORMAT FreeImageFormat,
        FREE_IMAGE_SAVE_FLAGS Flags = FREE_IMAGE_SAVE_FLAGS.DEFAULT, DdsFileFormat? DdsFormat = null,
        bool HasAlpha = true, bool SupportsQuality = false)
    {
        public static readonly IReadOnlyList<OutputFormat> All = new OutputFormat[]
        {
            new("PNG", ".png", FREE_IMAGE_FORMAT.FIF_PNG, FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_SPEED),
            new("TGA", ".tga", FREE_IMAGE_FORMAT.FIF_TARGA),
            new("BMP", ".bmp", FREE_IMAGE_FORMAT.FIF_BMP),
            new("JPG", ".jpg", FREE_IMAGE_FORMAT.FIF_JPEG, HasAlpha: false, SupportsQuality: true),
            new("TIFF", ".tif", FREE_IMAGE_FORMAT.FIF_TIFF, FREE_IMAGE_SAVE_FLAGS.TIFF_LZW),
            new("WEBP", ".webp", FREE_IMAGE_FORMAT.FIF_WEBP, SupportsQuality: true),
            new("PSD", ".psd", FREE_IMAGE_FORMAT.FIF_PSD),
            new("DDS DXT1", ".dds", FREE_IMAGE_FORMAT.FIF_DDS, DdsFormat: DdsFileFormat.BC1),
            new("DDS DXT5", ".dds", FREE_IMAGE_FORMAT.FIF_DDS, DdsFormat: DdsFileFormat.BC3),
            new("DDS BC7", ".dds", FREE_IMAGE_FORMAT.FIF_DDS, DdsFormat: DdsFileFormat.BC7),
            new("DDS BC7 sRGB", ".dds", FREE_IMAGE_FORMAT.FIF_DDS, DdsFormat: DdsFileFormat.BC7Srgb),
            new("DDS BGRA", ".dds", FREE_IMAGE_FORMAT.FIF_DDS, DdsFormat: DdsFileFormat.B8G8R8A8)
        }.Where(f => f.DdsFormat != null || FreeImage.FIFSupportsWriting(f.FreeImageFormat)).ToArray();

        public override string ToString() => Name;
    }

    internal sealed record TextureOptions(OutputFormat Format, int? UpscaleDdsTo, int? MaxSize, bool GenerateMipmaps, int Quality);

    internal static class TextureConverter
    {
        public static void Convert(string source, string target, TextureOptions options)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);

            string temp = Path.ChangeExtension(target, ".tmp" + options.Format.Extension);
            FIBITMAP image = Load(source);
            try
            {
                Resize(ref image, options, IsDds(source));
                Save(image, temp, options);
                File.Move(temp, target, true);
            }
            finally
            {
                FreeImage.Unload(image);
                TryDelete(temp);
            }
        }

        private static bool IsDds(string path) =>
            Path.GetExtension(path).Equals(".dds", StringComparison.OrdinalIgnoreCase);

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        private static void Replace(ref FIBITMAP image, FIBITMAP result, string error)
        {
            if (result.IsNull)
                throw new InvalidDataException(error);
            FreeImage.Unload(image);
            image = result;
        }

        private static FIBITMAP Load(string source)
        {
            FIBITMAP image = LoadWithFreeImage(source);
            if (!image.IsNull)
                return image;

            if (IsDds(source))
                return LoadWithDdsPlugin(source);

            throw new InvalidDataException("формат не распознан или файл повреждён");
        }

        private static FIBITMAP LoadWithFreeImage(string source)
        {
            FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
            FIBITMAP image = FreeImage.LoadEx(source, ref format);
            if (image.IsNull)
                return image;

            try
            {
                if (FreeImage.GetImageType(image) != FREE_IMAGE_TYPE.FIT_BITMAP)
                    Replace(ref image, FreeImage.ConvertToStandardType(image, true), "неподдерживаемый тип изображения");

                Replace(ref image, FreeImage.ConvertTo32Bits(image), "не удалось преобразовать в 32 бита");
                return image;
            }
            catch
            {
                FreeImage.Unload(image);
                throw;
            }
        }

        private static unsafe FIBITMAP LoadWithDdsPlugin(string source)
        {
            using Surface surface = DdsFile.Load(source);
            FIBITMAP image = FreeImage.ConvertFromRawBits((IntPtr)surface.GetRowAddress(0), surface.Width, surface.Height,
                surface.Stride, 32, FreeImage.FI_RGBA_RED_MASK, FreeImage.FI_RGBA_GREEN_MASK, FreeImage.FI_RGBA_BLUE_MASK, true);
            if (image.IsNull)
                throw new InvalidDataException("не удалось прочитать DDS");
            return image;
        }

        private static void Resize(ref FIBITMAP image, TextureOptions options, bool isDds)
        {
            int width = (int)FreeImage.GetWidth(image);
            int height = (int)FreeImage.GetHeight(image);
            int newWidth = width, newHeight = height;

            if (isDds && options.UpscaleDdsTo is int upscaleTo)
            {
                int maxSide = Math.Max(newWidth, newHeight);
                if (maxSide > 0 && maxSide < upscaleTo)
                {
                    int factor = upscaleTo / maxSide;
                    newWidth *= factor;
                    newHeight *= factor;
                }
            }

            if (options.MaxSize is int maxSize)
            {
                while (Math.Max(newWidth, newHeight) > maxSize)
                {
                    newWidth = Math.Max(1, newWidth / 2);
                    newHeight = Math.Max(1, newHeight / 2);
                }
            }

            if (newWidth == width && newHeight == height)
                return;

            FREE_IMAGE_FILTER filter = newWidth > width ? FREE_IMAGE_FILTER.FILTER_BSPLINE : FREE_IMAGE_FILTER.FILTER_CATMULLROM;
            Replace(ref image, FreeImage.Rescale(image, newWidth, newHeight, filter), "не удалось изменить размер");
        }

        private static FREE_IMAGE_SAVE_FLAGS GetSaveFlags(TextureOptions options)
        {
            OutputFormat format = options.Format;
            if (!format.SupportsQuality)
                return format.Flags;
            if (format.FreeImageFormat == FREE_IMAGE_FORMAT.FIF_WEBP && options.Quality >= 100)
                return FREE_IMAGE_SAVE_FLAGS.WEBP_LOSSLESS;
            return (FREE_IMAGE_SAVE_FLAGS)Math.Clamp(options.Quality, 1, 100);
        }

        private static void Save(FIBITMAP image, string path, TextureOptions options)
        {
            OutputFormat format = options.Format;
            if (format.DdsFormat is DdsFileFormat ddsFormat)
            {
                SaveDds(image, path, ddsFormat, options.GenerateMipmaps);
                return;
            }

            FREE_IMAGE_SAVE_FLAGS flags = GetSaveFlags(options);
            if (format.HasAlpha)
            {
                SaveWithFreeImage(image, path, format, flags);
                return;
            }

            FIBITMAP rgb = FreeImage.ConvertTo24Bits(image);
            if (rgb.IsNull)
                throw new InvalidDataException("не удалось преобразовать в 24 бита");
            try
            {
                SaveWithFreeImage(rgb, path, format, flags);
            }
            finally
            {
                FreeImage.Unload(rgb);
            }
        }

        private static void SaveWithFreeImage(FIBITMAP image, string path, OutputFormat format, FREE_IMAGE_SAVE_FLAGS flags)
        {
            if (!FreeImage.SaveEx(image, path, format.FreeImageFormat, flags))
                throw new IOException($"не удалось сохранить {format.Name}");
        }

        private static unsafe void SaveDds(FIBITMAP image, string path, DdsFileFormat format, bool generateMipmaps)
        {
            using var surface = new Surface((int)FreeImage.GetWidth(image), (int)FreeImage.GetHeight(image));
            FreeImage.ConvertToRawBits((IntPtr)surface.GetRowAddress(0), image, surface.Stride, 32,
                FreeImage.FI_RGBA_RED_MASK, FreeImage.FI_RGBA_GREEN_MASK, FreeImage.FI_RGBA_BLUE_MASK, true);

            using FileStream stream = File.Create(path);
            DdsFile.Save(stream, format, DdsErrorMetric.Perceptual, BC7CompressionSpeed.Medium,
                false, generateMipmaps, ResamplingAlgorithm.SuperSampling, surface, null);
        }
    }
}
