using FingerDice.Utilities;
using Microsoft.Maui.Controls;

namespace FingerDice.Pages
{
    public partial class WinnerPage : TouchGamePage
    {
        public WinnerPage()
        {
            InitializeComponent();
            Initialize(MainLayout, TouchLayer, CountdownLabel);
        }

        protected override void FinishRound()
        {
            if (_pins.Count == 0) return;

            var rnd = new Random();
            var winner = _pins[rnd.Next(_pins.Count)];

            foreach (var pin in _pins)
            {
                if (_animNames.TryGetValue(pin, out var animName))
                {
                    this.AbortAnimation(animName);
                    _animNames.Remove(pin);
                }

                if (pin == winner)
                    pin.Drawable = new SolidCircleDrawable(Colors.LimeGreen);
                else
                    pin.Drawable = new SolidCircleDrawable(Colors.Red);

                pin.Invalidate();
            }
        }
    }
}
