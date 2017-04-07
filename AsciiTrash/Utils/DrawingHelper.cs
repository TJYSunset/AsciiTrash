using System.Windows;
using Size = System.Drawing.Size;

namespace AsciiTrash.Utils
{
    public static class DrawingHelper
    {
        public static double GetArea(this Rect rect) => rect.Width * rect.Height;

        public static Size Multiply(this Size size, double ratio)
            => new Size((int) (size.Width * ratio), (int) (size.Height * ratio));

        public static Size CorrectAspect(this Size size, Rect rect)
            => new Size(size.Width, (int) (size.Height * rect.Width / rect.Height));
    }
}