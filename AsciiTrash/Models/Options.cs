using CommandLine;
using CommandLine.Text;

namespace AsciiTrash.Models
{
    public class Options
    {
        [Option('i', "input", HelpText = "Path to input image.", Required = true)]
        public string InputPath { get; set; }

        [Option('o', "output", HelpText = "Path to output file.", DefaultValue = @"output.txt")]
        public string OutputPath { get; set; }

        [Option('s', "scale", HelpText = "Scaling ratio of image. For most images a small value is suggested.",
            DefaultValue = 1d)]
        public double Scale { get; set; }

        [Option('e', "encoding", HelpText = "Encoding of output file.", DefaultValue = @"UTF-8")]
        public string Encoding { get; set; }

        [Option('f', "font",
            HelpText =
                "Font family used to calculate glyph information. Not choosing a monospaced one will lead to unexpected behaviors.",
            DefaultValue = @"Consolas")]
        public string FontFamily { get; set; }

        [HelpOption("help")]
        public string GetUsage()
            => HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
    }
}