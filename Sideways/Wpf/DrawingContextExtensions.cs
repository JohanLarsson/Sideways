namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public static class DrawingContextExtensions
    {
        public static void DrawText(this DrawingContext drawingContext, string text, Typeface typeface, double renderingEmSize, Point baselineOrigin, Brush brush, DpiScale dpiScale)
        {
            if (!typeface.TryGetGlyphTypeface(out var glyphTypeface))
            {
                throw new InvalidOperationException("No glyphtypeface found");
            }

            var glyphIndexes = new ushort[text.Length];
            var advanceWidths = new double[text.Length];

            for (var n = 0; n < text.Length; n++)
            {
                glyphIndexes[n] = glyphTypeface.CharacterToGlyphMap[text[n]];
                advanceWidths[n] = glyphTypeface.AdvanceWidths[glyphTypeface.CharacterToGlyphMap[text[n]]] * renderingEmSize;
            }

            var glyphRun = new GlyphRun(
                glyphTypeface: glyphTypeface,
                bidiLevel: 0,
                isSideways: false,
                renderingEmSize: renderingEmSize,
                pixelsPerDip: (float)dpiScale.PixelsPerDip,
                glyphIndices: glyphIndexes,
                baselineOrigin: baselineOrigin + new Vector(0, renderingEmSize),
                advanceWidths: advanceWidths,
                glyphOffsets: null,
                characters: null,
                deviceFontName: null,
                clusterMap: null,
                caretStops: null,
                language: null);
            drawingContext.DrawGlyphRun(brush, glyphRun);
        }
    }
}
