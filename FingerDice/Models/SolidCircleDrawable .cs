using FingerDice.Utilities;
using Microsoft.Maui.Graphics;

namespace FingerDice.Utilities
{
    public class SolidCircleDrawable : IDrawable
    {
        public Color Fill { get; set; }
        public SolidCircleDrawable(Color fill) => Fill = fill;

        public void Draw(ICanvas canvas, RectF r)
        {
            canvas.Antialias = true;
            canvas.FillColor = Fill;
            canvas.FillCircle(r.Center.X, r.Center.Y, r.Width / 2f);
            canvas.StrokeColor = Colors.Black.WithAlpha(0.15f);
            canvas.StrokeSize = 2;
            canvas.DrawCircle(r.Center.X, r.Center.Y, r.Width / 2f - 1);
        }
    }
}