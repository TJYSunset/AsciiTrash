using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using AsciiTrash.Models;
using AsciiTrash.Utils;
using CommandLine;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using MoreLinq;
using YamlDotNet.Serialization;

namespace AsciiTrash
{
    public static class Program
    {
        public const string TempDirectory = @".at_temp";

        public static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options)) return;

            // Collect glyph information
            Console.Write("Calculating glyphs thingy...");
            Directory.CreateDirectory(TempDirectory);
            GlyphCollection glyphs;
            var tempPath = Path.Combine(TempDirectory, options.FontFamily + @".yml");
            try
            {
                glyphs = new Deserializer().Deserialize<GlyphCollection>(File.ReadAllText(tempPath, Encoding.UTF8));
            }
            catch
            {
                glyphs = new GlyphCollection(options.FontFamily);
                try
                {
                    File.WriteAllText(tempPath, new Serializer().Serialize(glyphs), Encoding.UTF8);
                }
                catch
                {
                    // Ignored
                }
            }
            Console.WriteLine("Done");

            // Load and preprocess image
            Console.Write("Loading image...");
            var image = (Bitmap) Image.FromFile(options.InputPath, true);
            using (var outStream = new MemoryStream())
            {
                using (var factory = new ImageFactory())
                {
                    factory.Load(image)
                        .Resize(new ResizeLayer(image.Size.Multiply(options.Scale).CorrectAspect(glyphs.Rect),
                            ResizeMode.Stretch))
                        .Saturation(-100)
                        .BackgroundColor(Color.White)
                        .Format(new PngFormat())
                        .Save(outStream);
                }
                image = (Bitmap) Image.FromStream(outStream);
            }
            Console.WriteLine("Done");

            // Parse pixels
            Console.Write("Parsing pixels...");
            var gray = new byte[image.Width, image.Height];
            var palette = new Dictionary<byte, List<Point>>();
            for (var i = 0; i < image.Width; i++)
            for (var j = 0; j < image.Height; j++)
            {
                gray[i, j] = image.GetPixel(i, j).R;

                if (!palette.ContainsKey(gray[i, j]))
                    palette.Add(gray[i, j], new List<Point>());
                palette[gray[i, j]].Add(new Point(i, j));
            }
            Console.WriteLine("Done");

            // Posterize image
            Console.Write("Posterizing...");
            while (palette.Count > GlyphCollection.CharacterRange.Length)
            {
                var mergeSource = palette.MinBy(pair => pair.Value.Count).Key;
                var mergeTarget =
                    palette.Keys.Where(color => color != mergeSource)
                        .MinBy(color => Math.Abs(mergeSource - color));

                palette[mergeSource].ForEach(point => gray[point.X, point.Y] = mergeTarget);
                palette[mergeTarget].AddRange(palette[mergeSource]);
                palette.Remove(mergeSource);
            }
            Console.WriteLine("Done");

            // Cleanup palette
            Console.Write("Cleaning up...");
            byte index = 0;
            var sortedKeys = palette.Keys.OrderBy(color => color).ToDictionary(color => color, color => index++);
            if (palette.Count < glyphs.OrderedCharacters.Length)
            {
                // When palette is extremely small
                var factor = (glyphs.OrderedCharacters.Length - 1) / (double) (index - 1);
                var keys = sortedKeys.Keys.ToList();
                foreach (var pair in keys)
                    sortedKeys[pair] = (byte) Math.Round(sortedKeys[pair] * factor);
            }
            palette.ForEach(pair => pair.Value.ForEach(point => gray[point.X, point.Y] = sortedKeys[pair.Key]));
            Console.WriteLine("Done");

            // Output
            Console.Write("Writing to file...");
            using (var output = new StreamWriter(options.OutputPath, false, Encoding.GetEncoding(options.Encoding)))
            {
                for (var i = 0; i < image.Height; i++)
                {
                    for (var j = 0; j < image.Width; j++)
                        output.Write(glyphs.OrderedCharacters[gray[j, i]]);
                    output.WriteLine();
                }
            }
            Console.WriteLine("Done");
        }
    }
}