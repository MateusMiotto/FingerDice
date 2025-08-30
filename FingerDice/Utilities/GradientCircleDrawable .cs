using FingerDice.Utilities;
using Microsoft.Maui.Graphics;

namespace FingerDice.Utilities
{
    public class GradientCircleDrawable : IDrawable
    {
        public Color C1 { get; }
        public Color C2 { get; }
        public double Phase { get; set; }
        public GradientCircleDrawable(Color c1, Color c2) { C1 = c1; C2 = c2; }

        public void Draw(ICanvas canvas, RectF r)
        {
            canvas.Antialias = true;

            var angle = Phase * 360.0;
            var rad = (float)(Math.PI * angle / 180.0);
            var ux = (float)Math.Cos(rad);
            var uy = (float)Math.Sin(rad);

            var cx = r.Center.X; var cy = r.Center.Y; var half = r.Width / 2f;
            var p1 = new PointF(cx - ux * half, cy - uy * half);
            var p2 = new PointF(cx + ux * half, cy + uy * half);

            var grad = new LinearGradientPaint
            {
                StartPoint = p1,
                EndPoint = p2,
                GradientStops = new[]
                {
                new PaintGradientStop(0f, C1),
                new PaintGradientStop(1f, C2),
            }
            };

            canvas.SetFillPaint(grad, r);
            canvas.FillCircle(r.Center.X, r.Center.Y, r.Width / 2f);     // preenchido

            canvas.StrokeColor = Colors.Black.WithAlpha(0.15f);          // contorno leve
            canvas.StrokeSize = 2;
            canvas.DrawCircle(r.Center.X, r.Center.Y, r.Width / 2f - 1);
        }
    }


}