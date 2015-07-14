namespace AutoSpacing
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;

    using SharpDX.Direct3D9;

    #endregion

    internal class MDrawing
    {
        private static readonly Dictionary<int, Font> Fonts = new Dictionary<int, Font>();

        private static readonly HashSet<Line> Lines = new HashSet<Line>();

        static MDrawing()
        {
            {
                Drawing.OnPreReset += OnDrawingPreReset;
                Drawing.OnPostReset += OnDrawingPostReset;
            }
        }

        public static System.Drawing.Font GetFont(int fontSize)
        {
            Font font;
            {
                if (!Fonts.TryGetValue(fontSize, out font))
                {
                    font = new Font(
                        Drawing.Direct3DDevice, 
                        new FontDescription
                            {
                                FaceName = "Calibri", 
                                Height = fontSize, 
                                OutputPrecision = FontPrecision.Default, 
                                Quality = FontQuality.Default
                            });
                    Fonts[fontSize] = font;
                }
                else
                {
                    if (font == null || font.IsDisposed)
                    {
                        Fonts.Remove(fontSize);
                        GetFont(fontSize);
                    }
                }
            }

            return font;
        }

        public static Line GetLine(int width = -1)
        {
            Line line;
            {
                line = new Line(Drawing.Direct3DDevice);
                if (width >= 0)
                {
                    line.Width = width;
                }

                Lines.Add(line);
            }

            return line;
        }

        private static void OnDrawingPostReset(EventArgs args)
        {
            {
                foreach (var font in Fonts.Where(font => font.Value != null && !font.Value.IsDisposed))
                {
                    font.Value.OnResetDevice();
                }

                foreach (var line in Lines.Where(line => line != null && !line.IsDisposed))
                {
                    line.OnResetDevice();
                }
            }
        }

        private static void OnDrawingPreReset(EventArgs args)
        {
            {
                foreach (var font in Fonts.Where(font => font.Value != null && !font.Value.IsDisposed))
                {
                    font.Value.OnLostDevice();
                }

                foreach (var line in Lines.Where(line => line != null && !line.IsDisposed))
                {
                    line.OnLostDevice();
                }
            }
        }
    }
}