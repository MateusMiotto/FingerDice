namespace FingerDice.Pages
{
    public partial class GroupPage : ContentPage
    {
        public GroupPage()
        {
            InitializeComponent();
        }

        private void OnLayoutTapped(object sender, TappedEventArgs e)
        {
            var touchPoint = e.GetPosition(MainLayout);

            if (touchPoint != null)
            {
                // Cria um círculo visual
                var circle = new GraphicsView
                {
                    Drawable = new CircleDrawable(),
                    WidthRequest = 60,
                    HeightRequest = 60
                };

                // Posiciona o círculo no local do clique
                AbsoluteLayout.SetLayoutBounds(circle, new Rect(touchPoint.Value.X - 30, touchPoint.Value.Y - 30, 60, 60));
                AbsoluteLayout.SetLayoutFlags(circle, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

                MainLayout.Children.Add(circle);
            }
        }
    }

    // Drawable para desenhar o círculo
    public class CircleDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 4;
            canvas.DrawCircle(dirtyRect.Center.X, dirtyRect.Center.Y, dirtyRect.Width / 2 - 2);
        }
    }
}