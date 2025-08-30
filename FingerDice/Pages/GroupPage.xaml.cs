using FingerDice.Utilities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FingerDice.Pages
{
    public partial class GroupPage : TouchGamePage
    {
        int _groupCount = 2;

        public GroupPage()
        {
            InitializeComponent();
            Initialize(MainLayout, TouchLayer, CountdownLabel);
        }

        protected override void FinishRound()
        {
            SplitIntoGroups();
        }

        protected override int GetSeconds()
        {
            return _groupCount >= 4 ? 5 : 10;
        }

        void SplitIntoGroups()
        {
            if (_pins.Count == 0) return;

            Color[] colors;
            if (_groupCount == 2)
            {
                colors = new[] { Colors.LimeGreen, Colors.Red };
            }
            else
            {
                colors = Enumerable.Range(0, _groupCount)
                    .Select(i => Color.FromHsla(i / (double)_groupCount, 0.75, 0.5))
                    .ToArray();
            }

            var rnd = new Random();
            var shuffled = _pins.OrderBy(_ => rnd.Next()).ToList();

            for (int i = 0; i < shuffled.Count; i++)
            {
                var v = shuffled[i];
                var groupIdx = i % _groupCount;
                var finalColor = colors[groupIdx];

                if (_animNames.TryGetValue(v, out var animName))
                {
                    this.AbortAnimation(animName);
                    _animNames.Remove(v);
                }

                v.Drawable = new SolidCircleDrawable(finalColor);
                v.Invalidate();
            }
        }

        async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            var input = await DisplayPromptAsync("Configurar grupos", "Quantidade de grupos:",
                accept: "OK", cancel: "Cancelar",
                initialValue: _groupCount.ToString(), keyboard: Keyboard.Numeric);

            if (int.TryParse(input, out var n) && n >= 2 && n <= 12)
            {
                _groupCount = n;
                Title = $"Grupos ({_groupCount})";
            }
            else if (input is not null)
            {
                await DisplayAlert("Inválido", "Informe um número entre 2 e 12.", "OK");
            }
        }
    }
}
