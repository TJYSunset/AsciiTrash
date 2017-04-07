using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AsciiTrash.Utils;
using YamlDotNet.Serialization;

namespace AsciiTrash.Models
{
    public sealed class GlyphCollection
    {
        public const string CharacterRange =
            @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public const char BaseCharacter = 'A';

        private const int LeftToRight = 0;
        private const double EmSize = 1;
        private static readonly Point Origin = new Point(0, 0);

        public GlyphCollection(string family)
        {
            GlyphTypeface gtypeface;
            var typeface =
                new Typeface(new FontFamily(family), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            if (!typeface.TryGetGlyphTypeface(out gtypeface))
                throw new ArgumentException("Error getting glyph typeface", nameof(family));

            var map = CharacterRange.ToDictionary(character => character,
                character => gtypeface.CharacterToGlyphMap[character]);
            var width = gtypeface.AdvanceWidths[map[BaseCharacter]];
            Rect = new GlyphRun(gtypeface, LeftToRight, false, EmSize, 1, new[] {map[BaseCharacter]}, Origin,
                new[] {width}, null, null, null, null, null, null).ComputeAlignmentBox();

            OrderedCharacters = CharacterRange.OrderByDescending(character =>
            {
                var run = new GlyphRun(gtypeface, LeftToRight, false, EmSize, 1, new[] {map[character]}, Origin,
                    new[] {width}, null, null, null, null, null, null);
                return run.BuildGeometry().GetArea() / run.ComputeAlignmentBox().GetArea();
            }).ToArray();
        }

        [YamlMember(Alias = "chars")]
        public char[] OrderedCharacters { get; private set; }

        [YamlMember(Alias = "rect")]
        public Rect Rect { get; private set; }
    }
}